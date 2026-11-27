namespace BioFSharp

open System.Collections.Generic
open BioFSharp.FileFormats.PDB
open BioFSharp.IO

/// Functions to group PDB coordinates into a structure hierarchy and to compute distances, contacts, angles, and superpositions on it
module Structure =

    /// <summary>
    /// A residue and its atoms.
    /// </summary>
    type Residue = {
        /// Residue name, e.g. "ALA" or "HOH"
        Name            : string
        /// Residue sequence number (columns 23-26)
        SequenceNumber  : int
        /// Residue insertion code (column 27)
        /// empty if there is none
        InsertionCode   : string
        /// True if the residue was read from HETATM records
        IsHetero        : bool
        /// True if the residue belongs to the polymer of its chain, i.e.
        /// Modified residues such as selenomethionine (MSE) can be both hetero and part of the polymer
        IsPolymer       : bool
        /// Atoms of the residue in file order
        /// HETATM records are stored as atoms
        Atoms           : Atom []
    }

    /// <summary>
    /// A chain and its residues in order of first appearance
    /// </summary>
    type Chain = {
        /// Chain identifier (column 22)
        Identifier  : string
        /// Residues of the chain in order of first appearance
        Residues    : Residue []
    }

    /// <summary>
    /// One model of a structure, e.g. one conformation of an NMR ensemble
    /// </summary>
    type Model = {
        /// Model serial number.
        /// Coordinates without MODEL records belong to model 1
        Number  : int
        /// Chains of the model in order of first appearance
        Chains  : Chain []
    }

    /// <summary>
    /// All models of a structure.
    /// </summary>
    type Structure = {
        /// Models in file order
        Models  : Model []
    }

    let private ofHetAtom (h: HetAtom) : Atom =
        {
            SerialNumber                = h.SerialNumber
            Name                        = h.Name
            AlternateLocationIndicator  = h.AlternateLocationIndicator
            ResidueName                 = h.ResidueName
            ChainIdentifier             = h.ChainIdentifier
            ResidueSequenceNumber       = h.ResidueSequenceNumber
            ResidueInsertionCode        = h.ResidueInsertionCode
            X                           = h.X
            Y                           = h.Y
            Z                           = h.Z
            Occupancy                   = h.Occupancy
            TemperatureFactor           = h.TemperatureFactor
            SegmentIdentifier           = h.SegmentIdentifier
            ElementSymbol               = h.ElementSymbol
            Charge                      = h.Charge
        }

    let private toResidues (terminated: bool) (atoms: (bool * bool * Atom) []) =
        let residues =
            atoms
            |> Array.groupBy (fun (_, _, a) -> a.ResidueSequenceNumber, a.ResidueInsertionCode, a.ResidueName)
            |> Array.map (fun ((number, insertionCode, name), group) ->
                let (isHetero, afterTerminator, _) = group.[0]
                {
                    Name            = name
                    SequenceNumber  = number
                    InsertionCode   = insertionCode
                    IsHetero        = isHetero
                    IsPolymer       = not afterTerminator
                    Atoms           = group |> Array.map (fun (_, _, a) -> a)
                }
            )
        if terminated then
            residues
        else
            let polymerEnd = residues |> Array.tryFindIndexBack (fun r -> not r.IsHetero) |> Option.defaultValue -1
            residues |> Array.mapi (fun i r -> { r with IsPolymer = i <= polymerEnd })

    let private toChains (terminatedChains: HashSet<string>) (atoms: (bool * bool * Atom) []) =
        atoms
        |> Array.groupBy (fun (_, _, a) -> a.ChainIdentifier)
        |> Array.map (fun (identifier, group) ->
            {
                Identifier  = identifier
                Residues    = toResidues (terminatedChains.Contains identifier) group
            }
        )

    /// <summary>
    /// Groups PDB coordinates into a structure of models, chains, residues and atoms.
    /// Coordinates before the first MODEL record belong to model 1.
    /// Atoms are grouped by chain identifier, also when a chain continues later in the input (e.g. waters listed after other chains).
    /// Residues are grouped by sequence number, insertion code, and residue name.
    /// HETATM records become atoms of residues with IsHetero set to true.
    /// Residues before the TER record of their chain are part of the polymer (IsPolymer).
    /// For a chain without TER record, the residues up to the last non-hetero residue are part of the polymer (IsPolymer).
    /// ENDMDL records are not stored. Atoms with alternate locations are all kept.
    /// </summary>
    /// <param name="coordinates">The coordinates to group, e.g. from PDB.readCoordinates</param>
    /// <returns>The structure</returns>
    let ofCoordinates (coordinates: seq<Coordinate>) =
        let models = ResizeArray<int * ResizeArray<bool * bool * Atom> * HashSet<string>>()
        let current () =
            if models.Count = 0 then models.Add((1, ResizeArray(), HashSet()))
            models.[models.Count - 1]
        let add (isHetero: bool) (atom: Atom) =
            let (_, atoms, terminatedChains) = current ()
            atoms.Add((isHetero, terminatedChains.Contains atom.ChainIdentifier, atom))
        for coordinate in coordinates do
            match coordinate with
            | Coordinate.Model number   -> models.Add((number, ResizeArray(), HashSet()))
            | Coordinate.Atom a         -> add false a
            | Coordinate.HetAtom h      -> add true (ofHetAtom h)
            | Coordinate.Terminator t   ->
                let (_, atoms, terminatedChains) = current ()
                match t.ChainIdentifier with
                | Some chain -> terminatedChains.Add chain |> ignore
                | None when atoms.Count > 0 ->
                    let (_, _, last) = atoms.[atoms.Count - 1]
                    terminatedChains.Add last.ChainIdentifier |> ignore
                | None -> ()
            | Coordinate.EndModel       -> ()
        {
            Models =
                models.ToArray()
                |> Array.map (fun (number, atoms, terminatedChains) ->
                    {
                        Number  = number
                        Chains  = toChains terminatedChains (atoms.ToArray())
                    }
                )
        }

    let private toHetAtom (a: Atom) : HetAtom =
        {
            SerialNumber                = a.SerialNumber
            Name                        = a.Name
            AlternateLocationIndicator  = a.AlternateLocationIndicator
            ResidueName                 = a.ResidueName
            ChainIdentifier             = a.ChainIdentifier
            ResidueSequenceNumber       = a.ResidueSequenceNumber
            ResidueInsertionCode        = a.ResidueInsertionCode
            X                           = a.X
            Y                           = a.Y
            Z                           = a.Z
            Occupancy                   = a.Occupancy
            TemperatureFactor           = a.TemperatureFactor
            SegmentIdentifier           = a.SegmentIdentifier
            ElementSymbol               = a.ElementSymbol
            Charge                      = a.Charge
        }

    let private residueCoordinates (chain: Chain) (residue: Residue) =
        residue.Atoms
        |> Array.map (fun a ->
            let atom =
                { a with
                    ResidueName             = residue.Name
                    ChainIdentifier         = chain.Identifier
                    ResidueSequenceNumber   = residue.SequenceNumber
                    ResidueInsertionCode    = residue.InsertionCode }
            if residue.IsHetero then Coordinate.HetAtom (toHetAtom atom)
            else Coordinate.Atom atom
        )

    let private terminator (chain: Chain) (residue: Residue) =
        let last = residue.Atoms.[residue.Atoms.Length - 1]
        Coordinate.Terminator (
            Terminator.create(
                SerialNumber            = last.SerialNumber + 1,
                ResidueName             = residue.Name,
                ChainIdentifier         = chain.Identifier,
                ResidueSequenceNumber   = residue.SequenceNumber,
                ?ResidueInsertionCode   = (if residue.InsertionCode = "" then None else Some residue.InsertionCode)
            )
        )

    let private modelCoordinates (model: Model) =
        let polymers =
            model.Chains
            |> Array.collect (fun chain ->
                let polymer = chain.Residues |> Array.filter (fun r -> r.IsPolymer)
                if polymer.Length = 0 then [||]
                else
                    Array.append
                        (polymer |> Array.collect (residueCoordinates chain))
                        [| terminator chain polymer.[polymer.Length - 1] |]
            )
        let others =
            model.Chains
            |> Array.collect (fun chain ->
                chain.Residues
                |> Array.filter (fun r -> not r.IsPolymer)
                |> Array.map (fun r -> chain, r)
            )
            |> Array.sortBy (fun (_, residue) -> residue.Atoms.[0].SerialNumber)
            |> Array.collect (fun (chain, residue) -> residueCoordinates chain residue)
        Array.append polymers others

    /// <summary>
    /// Converts a structure back to PDB coordinates.
    /// For each chain, the polymer residues (IsPolymer) are written and followed by a TER record for the last of them.
    /// The remaining residues of all chains follow, ordered by the serial number of their first atom.
    /// The chain identifier and the residue name, number and insertion code of each atom and TER record are taken from the enclosing chain and residue, so edits to them are written consistently.
    /// MODEL and ENDMDL records are only written when the structure has more than one model.
    /// </summary>
    /// <param name="structure">The structure to convert</param>
    /// <returns>The coordinates of the structure</returns>
    let toCoordinates (structure: Structure) =
        if structure.Models.Length = 1 then
            modelCoordinates structure.Models.[0]
        else
            structure.Models
            |> Array.collect (fun model ->
                Array.concat [
                    [| Coordinate.Model model.Number |]
                    modelCoordinates model
                    [| Coordinate.EndModel |]
                ]
            )

    /// <summary>
    /// Writes a structure as PDB coordinate records to a file.
    /// Only coordinate records are written
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="structure">The structure to write</param>
    let write (path: string) (structure: Structure) =
        structure
        |> toCoordinates
        |> Array.map Coordinate.toString
        |> fun lines -> System.IO.File.WriteAllLines(path, lines)

    /// <summary>
    /// Reads the coordinate records of a PDB file and groups them into a structure of models, chains, residues and atoms.
    /// </summary>
    /// <param name="path">Path to a PDB formatted file</param>
    /// <returns>The structure</returns>
    /// <exception cref="System.FormatException">If a coordinate record contains a field that cannot be parsed</exception>
    let read (path: string) =
        path
        |> PDB.readCoordinates
        |> ofCoordinates

    /// <summary>
    /// Computes the distance of two atoms.
    /// </summary>
    /// <param name="a">The first atom</param>
    /// <param name="b">The second atom</param>
    /// <returns>The distance in the unit of the coordinates (Angstrom for PDB files)</returns>
    let distance (a: Atom) (b: Atom) =
        let dx = a.X - b.X
        let dy = a.Y - b.Y
        let dz = a.Z - b.Z
        sqrt (dx * dx + dy * dy + dz * dz)

    /// <summary>
    /// Returns all atoms of a model, ordered by chain and residue.
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The atoms of the model</returns>
    let atoms (model: Model) =
        model.Chains
        |> Array.collect (fun chain -> chain.Residues |> Array.collect (fun residue -> residue.Atoms))

    /// Functions to find all pairs of atoms within a distance
    module NeighborSearch =

        /// <summary>
        /// Finds all pairs of atoms within a cutoff distance by comparing every atom with every other atom.
        /// The runtime grows with the square of the number of atoms.
        /// Use grid for large structures.
        /// </summary>
        /// <param name="cutoff">The maximum distance, inclusive</param>
        /// <param name="atoms">The atoms to search</param>
        /// <returns>Pairs of indices (i, j) into atoms with i &lt; j, sorted by i and then j</returns>
        let bruteForce (cutoff: float) (atoms: Atom []) =
            [|
                for i in 0 .. atoms.Length - 1 do
                    for j in i + 1 .. atoms.Length - 1 do
                        if distance atoms.[i] atoms.[j] <= cutoff then
                            yield i, j
            |]

        /// <summary>
        /// Finds all pairs of atoms within a cutoff distance using a grid of cubes (cell list).
        /// Atoms are sorted into cubes with the cutoff as side length, and atoms in the same or neighboring cubes are compared.
        /// Returns the same pairs as bruteForce.
        /// </summary>
        /// <param name="cutoff">The maximum distance, inclusive</param>
        /// <param name="atoms">The atoms to search</param>
        /// <returns>Pairs of indices (i, j) into atoms with i &lt; j, sorted by i and then j</returns>
        /// <exception cref="System.ArgumentException">If the cutoff is not greater than 0</exception>
        let grid (cutoff: float) (atoms: Atom []) =
            if cutoff <= 0. then invalidArg (nameof cutoff) "The cutoff must be greater than 0."
            let cell (a: Atom) =
                struct (int (floor (a.X / cutoff)), int (floor (a.Y / cutoff)), int (floor (a.Z / cutoff)))
            let cells = System.Collections.Generic.Dictionary<struct (int * int * int), ResizeArray<int>>()
            for i in 0 .. atoms.Length - 1 do
                let key = cell atoms.[i]
                match cells.TryGetValue key with
                | true, members -> members.Add i
                | _ -> cells.[key] <- ResizeArray [ i ]
            let pairs = ResizeArray<int * int>()
            for i in 0 .. atoms.Length - 1 do
                let struct (x, y, z) = cell atoms.[i]
                for dx in -1 .. 1 do
                    for dy in -1 .. 1 do
                        for dz in -1 .. 1 do
                            match cells.TryGetValue(struct (x + dx, y + dy, z + dz)) with
                            | true, members ->
                                for j in members do
                                    if j > i && distance atoms.[i] atoms.[j] <= cutoff then
                                        pairs.Add((i, j))
                            | _ -> ()
            pairs.ToArray() |> Array.sortBy (fun (i, j) -> int64 i * int64 atoms.Length + int64 j)

    /// <summary>
    /// Returns all residues of a model, ordered by chain.
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The residues of the model</returns>
    let residues (model: Model) =
        model.Chains
        |> Array.collect (fun chain -> chain.Residues)

    /// <summary>
    /// Finds all pairs of residues that have atoms within a cutoff distance.
    /// Neighboring residues in a chain are always in contact since they are bonded.
    /// </summary>
    /// <param name="atomFilter">Selects the atoms to use, e.g. only alpha carbons, or no hydrogens and waters</param>
    /// <param name="cutoff">The maximum distance, inclusive</param>
    /// <param name="model">The model</param>
    /// <returns>Pairs of indices (i, j) into Structure.residues model with i &lt; j, sorted</returns>
    let residueContacts (atomFilter: Atom -> bool) (cutoff: float) (model: Model) =
        let residues = residues model
        let selected =
            residues
            |> Array.mapi (fun r residue ->
                residue.Atoms
                |> Array.filter atomFilter
                |> Array.map (fun atom -> r, atom)
            )
            |> Array.concat
        selected
        |> Array.map snd
        |> NeighborSearch.grid cutoff
        |> Array.map (fun (i, j) -> fst selected.[i], fst selected.[j])
        |> Array.filter (fun (a, b) -> a <> b)
        |> Array.distinct
        |> Array.sort

    /// <summary>
    /// Computes the contact map of a model: matrix is true where two residues have atoms within a cutoff distance.
    /// </summary>
    /// <param name="atomFilter">Selects the atoms to use, e.g. only alpha carbons, or no hydrogens and waters</param>
    /// <param name="cutoff">The maximum distance, inclusive</param>
    /// <param name="model">The model</param>
    /// <returns>An n x n matrix for the n residues of Structure.residues model</returns>
    let contactMap (atomFilter: Atom -> bool) (cutoff: float) (model: Model) =
        let n = (residues model).Length
        let map = Array2D.create n n false
        for (a, b) in residueContacts atomFilter cutoff model do
            map.[a, b] <- true
            map.[b, a] <- true
        map

    let private vector (a: Atom) (b: Atom) =
        b.X - a.X, b.Y - a.Y, b.Z - a.Z

    let private dot (x1, y1, z1) (x2, y2, z2) : float =
        x1 * x2 + y1 * y2 + z1 * z2

    let private cross (x1, y1, z1) (x2, y2, z2) : float * float * float =
        y1 * z2 - z1 * y2, z1 * x2 - x1 * z2, x1 * y2 - y1 * x2

    let private toDegrees (radians: float) =
        radians * 180.0 / System.Math.PI

    /// <summary>
    /// Computes the angle at atom b between the bonds to atom a and atom c.
    /// </summary>
    /// <param name="a">The first atom</param>
    /// <param name="b">The atom at the vertex of the angle</param>
    /// <param name="c">The third atom</param>
    /// <returns>The angle in degrees, from 0 to 180</returns>
    let angle (a: Atom) (b: Atom) (c: Atom) =
        let ba = vector b a
        let bc = vector b c
        let cosine = dot ba bc / sqrt (dot ba ba * dot bc bc)
        cosine |> max -1.0 |> min 1.0 |> acos |> toDegrees

    /// <summary>
    /// Computes the torsion (dihedral) angle of four atoms around the bond between atom b and atom c.
    /// The angle is positive for a clockwise rotation from the bond a-b to the bond c-d, looking from b to c (IUPAC convention).
    /// Returns 0 if three of the atoms are on a straight line.
    /// </summary>
    /// <param name="a">The first atom</param>
    /// <param name="b">The second atom</param>
    /// <param name="c">The third atom</param>
    /// <param name="d">The fourth atom</param>
    /// <returns>The torsion angle in degrees, from -180 to 180</returns>
    let dihedral (a: Atom) (b: Atom) (c: Atom) (d: Atom) =
        let b1 = vector a b
        let b2 = vector b c
        let b3 = vector c d
        let n1 = cross b1 b2
        let n2 = cross b2 b3
        let y = sqrt (dot b2 b2) * dot b1 n2
        let x = dot n1 n2
        atan2 y x |> toDegrees

    /// <summary>
    /// The backbone torsion angles of a residue.
    /// </summary>
    type BackboneTorsions = {
        /// The residue
        Residue : Residue
        /// Torsion angle C(i-1)-N-CA-C in degrees, None at the chain start and after a chain break
        Phi     : float option
        /// Torsion angle N-CA-C-N(i+1) in degrees, None at the chain end and before a chain break
        Psi     : float option
        /// Torsion angle CA-C-N(i+1)-CA(i+1) of the peptide bond to the next residue in degrees, None at the chain end and before a chain break
        Omega   : float option
    }

    let private tryAtom (name: string) (residue: Residue) =
        residue.Atoms
        |> Array.tryFind (fun atom -> (AtomName.toString atom.Name).Trim() = name)

    /// <summary>
    /// Computes the backbone torsion angles phi, psi, and omega for each polymer residue of a chain (IsPolymer).
    /// The neighbors of a residue are the polymer residues before and after it.
    /// Two neighbors count as connected when the distance of C to the N of the next residue is at most 2.0 Angstrom.
    /// Each angle is computed when the four atoms it needs are present: phi needs C of the previous residue and N, CA, C;
    /// psi needs N, CA, C, and N of the next residue;
    /// omega needs CA, C and N, CA of the next residue.
    /// Use the first atom for atoms with alternate locations
    /// </summary>
    /// <param name="chain">The chain</param>
    /// <returns>The backbone torsions of each polymer residue, in chain order</returns>
    let backboneTorsions (chain: Chain) =
        let polymer = chain.Residues |> Array.filter (fun r -> r.IsPolymer)
        let atom name i = tryAtom name polymer.[i]
        let bonded i =
            match atom "C" i, atom "N" (i + 1) with
            | Some c, Some n -> distance c n <= 2.0
            | _ -> false
        polymer
        |> Array.mapi (fun i residue ->
            let hasPrevious = i > 0 && bonded (i - 1)
            let hasNext = i < polymer.Length - 1 && bonded i
            let torsion (atoms: Atom option list) =
                match atoms with
                | [ Some a; Some b; Some c; Some d ] -> Some (dihedral a b c d)
                | _ -> None
            {
                Residue = residue
                Phi     = if hasPrevious then torsion [ atom "C" (i - 1); atom "N" i; atom "CA" i; atom "C" i ] else None
                Psi     = if hasNext then torsion [ atom "N" i; atom "CA" i; atom "C" i; atom "N" (i + 1) ] else None
                Omega   = if hasNext then torsion [ atom "CA" i; atom "C" i; atom "N" (i + 1); atom "CA" (i + 1) ] else None
            }
        )

    /// <summary>
    /// Computes the root-mean-square deviation of two matching atom arrays
    /// </summary>
    /// <param name="a">The first atoms</param>
    /// <param name="b">The second atoms, where b[i] corresponds to a[i]</param>
    /// <returns>The RMSD in the unit of the coordinates (Angstrom for PDB files)</returns>
    /// <exception cref="System.ArgumentException">If the arrays differ in length or are empty</exception>
    let rmsd (a: Atom []) (b: Atom []) =
        if a.Length <> b.Length then invalidArg (nameof b) "Both atom arrays must have the same length."
        if a.Length = 0 then invalidArg (nameof a) "The atom arrays must not be empty."
        let sum = Array.map2 (fun x y -> let d = distance x y in d * d) a b |> Array.sum
        sqrt (sum / float a.Length)

    let private centroid (atoms: Atom []) =
        Array.averageBy (fun (a: Atom) -> a.X) atoms, Array.averageBy (fun (a: Atom) -> a.Y) atoms, Array.averageBy (fun (a: Atom) -> a.Z) atoms

    let private largestEigenvector (matrix: float [,]) =
        let a = Array2D.copy matrix
        let v = Array2D.init 4 4 (fun i j -> if i = j then 1.0 else 0.0)
        let offDiagonal () =
            seq { for i in 0 .. 3 do for j in i + 1 .. 3 -> a.[i, j] * a.[i, j] } |> Seq.sum
        let mutable sweep = 0
        while sweep < 50 && offDiagonal () > 1e-22 do
            for p in 0 .. 2 do
                for q in p + 1 .. 3 do
                    if a.[p, q] <> 0.0 then
                        let theta = (a.[q, q] - a.[p, p]) / (2.0 * a.[p, q])
                        let t = (if theta >= 0.0 then 1.0 else -1.0) / (abs theta + sqrt (theta * theta + 1.0))
                        let c = 1.0 / sqrt (t * t + 1.0)
                        let s = t * c
                        for k in 0 .. 3 do
                            let akp = a.[k, p]
                            let akq = a.[k, q]
                            a.[k, p] <- c * akp - s * akq
                            a.[k, q] <- s * akp + c * akq
                        for k in 0 .. 3 do
                            let apk = a.[p, k]
                            let aqk = a.[q, k]
                            a.[p, k] <- c * apk - s * aqk
                            a.[q, k] <- s * apk + c * aqk
                        for k in 0 .. 3 do
                            let vkp = v.[k, p]
                            let vkq = v.[k, q]
                            v.[k, p] <- c * vkp - s * vkq
                            v.[k, q] <- s * vkp + c * vkq
            sweep <- sweep + 1
        let largest = [| 0 .. 3 |] |> Array.maxBy (fun i -> a.[i, i])
        Array.init 4 (fun k -> v.[k, largest])

    /// <summary>
    /// Moves and rotates atoms onto matching reference atoms so that their RMSD is minimal.
    /// Uses the quaternion method (Horn, 1987): the optimal rotation is the eigenvector of the largest eigenvalue of a 4 x 4 matrix
    /// Only proper rotations are used, so a structure is never mirrored.
    /// </summary>
    /// <param name="reference">The atoms to superpose onto</param>
    /// <param name="mobile">The atoms to move, where mobile[i] corresponds to reference[i]</param>
    /// <returns>A copy of the mobile atoms with moved coordinates</returns>
    /// <exception cref="System.ArgumentException">If the arrays differ in length or are empty</exception>
    let superpose (reference: Atom []) (mobile: Atom []) =
        if reference.Length <> mobile.Length then invalidArg (nameof mobile) "Both atom arrays must have the same length."
        if reference.Length = 0 then invalidArg (nameof reference) "The atom arrays must not be empty."
        let (rx, ry, rz) = centroid reference
        let (mx, my, mz) = centroid mobile
        let mutable sxx, sxy, sxz, syx, syy, syz, szx, szy, szz = 0., 0., 0., 0., 0., 0., 0., 0., 0.
        for i in 0 .. reference.Length - 1 do
            let x, y, z = mobile.[i].X - mx, mobile.[i].Y - my, mobile.[i].Z - mz
            let x', y', z' = reference.[i].X - rx, reference.[i].Y - ry, reference.[i].Z - rz
            sxx <- sxx + x * x'
            sxy <- sxy + x * y'
            sxz <- sxz + x * z'
            syx <- syx + y * x'
            syy <- syy + y * y'
            syz <- syz + y * z'
            szx <- szx + z * x'
            szy <- szy + z * y'
            szz <- szz + z * z'
        let n =
            array2D [
                [ sxx + syy + szz; syz - szy;        szx - sxz;        sxy - syx        ]
                [ syz - szy;       sxx - syy - szz;  sxy + syx;        szx + sxz        ]
                [ szx - sxz;       sxy + syx;        -sxx + syy - szz; syz + szy        ]
                [ sxy - syx;       szx + sxz;        syz + szy;        -sxx - syy + szz ]
            ]
        let q = largestEigenvector n
        let q0, q1, q2, q3 = q.[0], q.[1], q.[2], q.[3]
        let r =
            array2D [
                [ q0*q0 + q1*q1 - q2*q2 - q3*q3; 2.0 * (q1*q2 - q0*q3);         2.0 * (q1*q3 + q0*q2)         ]
                [ 2.0 * (q1*q2 + q0*q3);         q0*q0 - q1*q1 + q2*q2 - q3*q3; 2.0 * (q2*q3 - q0*q1)         ]
                [ 2.0 * (q1*q3 - q0*q2);         2.0 * (q2*q3 + q0*q1);         q0*q0 - q1*q1 - q2*q2 + q3*q3 ]
            ]
        mobile
        |> Array.map (fun atom ->
            let x, y, z = atom.X - mx, atom.Y - my, atom.Z - mz
            { atom with
                X = r.[0, 0] * x + r.[0, 1] * y + r.[0, 2] * z + rx
                Y = r.[1, 0] * x + r.[1, 1] * y + r.[1, 2] * z + ry
                Z = r.[2, 0] * x + r.[2, 1] * y + r.[2, 2] * z + rz }
        )