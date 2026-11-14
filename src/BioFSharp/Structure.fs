namespace BioFSharp

open BioFSharp.FileFormats.PDB
open BioFSharp.IO

module Structure =

    type Residue = {
        Name            : string
        SequenceNumber  : int
        InsertionCode   : string
        IsHetero        : bool
        Atoms           : Atom []
    }

    type Chain = {
        Identifier  : string
        Residues    : Residue []
    }

    type Model = {
        Number  : int
        Chains  : Chain []
    }

    type Structure = {
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

    let private toResidues (atoms: (bool * Atom) []) =
        atoms
        |> Array.groupBy (fun (_, a) -> a.ResidueSequenceNumber, a.ResidueInsertionCode, a.ResidueName)
        |> Array.map (fun ((number, insertionCode, name), group) ->
            {
                Name            = name
                SequenceNumber  = number
                InsertionCode   = insertionCode
                IsHetero        = fst group.[0]
                Atoms           = group |> Array.map snd
            }
        )

    let private toChains (atoms: (bool * Atom) []) =
        atoms
        |> Array.groupBy (fun (_, a) -> a.ChainIdentifier)
        |> Array.map (fun (identifier, group) ->
            {
                Identifier  = identifier
                Residues    = toResidues group
            }
        )

    let ofCoordinates (coordinates: seq<Coordinate>) =
        let models = ResizeArray<int * ResizeArray<bool * Atom>>()
        let current () =
            if models.Count = 0 then models.Add((1, ResizeArray()))
            snd models.[models.Count - 1]
        for coordinate in coordinates do
            match coordinate with
            | Coordinate.Model number   -> models.Add((number, ResizeArray()))
            | Coordinate.Atom a         -> current().Add((false, a))
            | Coordinate.HetAtom h      -> current().Add((true, ofHetAtom h))
            | Coordinate.Terminator _
            | Coordinate.EndModel       -> ()
        {
            Models =
                models.ToArray()
                |> Array.map (fun (number, atoms) ->
                    {
                        Number  = number
                        Chains  = toChains (atoms.ToArray())
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


    let private residueCoordinates (residue: Residue) =
        residue.Atoms
        |> Array.map (fun a ->
            if residue.IsHetero then Coordinate.HetAtom (toHetAtom a)
            else Coordinate.Atom a
        )

    let private terminator (residue: Residue) =
        let last = residue.Atoms.[residue.Atoms.Length - 1]
        Coordinate.Terminator (
            Terminator.create(
                SerialNumber            = last.SerialNumber + 1,
                ResidueName             = residue.Name,
                ChainIdentifier         = last.ChainIdentifier,
                ResidueSequenceNumber   = residue.SequenceNumber,
                ?ResidueInsertionCode   = (if residue.InsertionCode = "" then None else Some residue.InsertionCode)
            )
        )

    let private modelCoordinates (model: Model) =
        let polymerEnd (chain: Chain) =
            chain.Residues |> Array.tryFindIndexBack (fun r -> not r.IsHetero)
        let polymers =
            model.Chains
            |> Array.collect (fun chain ->
                match polymerEnd chain with
                | Some i ->
                    Array.append
                        (chain.Residues.[..i] |> Array.collect residueCoordinates)
                        [| terminator chain.Residues.[i] |]
                | None -> [||]
            )
        let heteros =
            model.Chains
            |> Array.collect (fun chain ->
                let start = match polymerEnd chain with Some i -> i + 1 | None -> 0
                chain.Residues.[start..]
            )
            |> Array.sortBy (fun residue -> residue.Atoms.[0].SerialNumber)
            |> Array.collect residueCoordinates
        Array.append polymers heteros

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

    let write (path: string) (structure: Structure) =
        structure
        |> toCoordinates
        |> Array.map Coordinate.toString
        |> fun lines -> System.IO.File.WriteAllLines(path, lines)

    let read (path: string) =
        path
        |> PDB.readCoordinates
        |> ofCoordinates

    let distance (a: Atom) (b: Atom) =
        let dx = a.X - b.X
        let dy = a.Y - b.Y
        let dz = a.Z - b.Z
        sqrt (dx * dx + dy * dy + dz * dz)

    let atoms (model: Model) =
        model.Chains
        |> Array.collect (fun chain -> chain.Residues |> Array.collect (fun residue -> residue.Atoms))

    module NeighborSearch =

        let bruteForce (cutoff: float) (atoms: Atom []) =
            [|
                for i in 0 .. atoms.Length - 1 do
                    for j in i + 1 .. atoms.Length - 1 do
                        if distance atoms.[i] atoms.[j] <= cutoff then
                            yield i, j
            |]

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
            
    let residues (model: Model) =
        model.Chains
        |> Array.collect (fun chain -> chain.Residues)

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

    let angle (a: Atom) (b: Atom) (c: Atom) =
        let ba = vector b a
        let bc = vector b c
        let cosine = dot ba bc / sqrt (dot ba ba * dot bc bc)
        cosine |> max -1.0 |> min 1.0 |> acos |> toDegrees

    let dihedral (a: Atom) (b: Atom) (c: Atom) (d: Atom) =
        let b1 = vector a b
        let b2 = vector b c
        let b3 = vector c d
        let n1 = cross b1 b2
        let n2 = cross b2 b3
        let y = sqrt (dot b2 b2) * dot b1 n2
        let x = dot n1 n2
        atan2 y x |> toDegrees
        
    type BackboneTorsions = {
        Residue : Residue
        Phi     : float option
        Psi     : float option
        Omega   : float option
    }

    let private tryAtom (name: string) (residue: Residue) =
        residue.Atoms
        |> Array.tryFind (fun atom -> (AtomName.toString atom.Name).Trim() = name)

    let backboneTorsions (chain: Chain) =
        let backbone =
            chain.Residues
            |> Array.choose (fun residue ->
                match tryAtom "N" residue, tryAtom "CA" residue, tryAtom "C" residue with
                | Some n, Some ca, Some c -> Some (residue, n, ca, c)
                | _ -> None
            )
        let bonded i =
            let (_, _, _, c) = backbone.[i]
            let (_, n, _, _) = backbone.[i + 1]
            distance c n <= 2.0
        backbone
        |> Array.mapi (fun i (residue, n, ca, c) ->
            let previous = if i > 0 && bonded (i - 1) then Some backbone.[i - 1] else None
            let next = if i < backbone.Length - 1 && bonded i then Some backbone.[i + 1] else None
            {
                Residue = residue
                Phi     = previous |> Option.map (fun (_, _, _, previousC) -> dihedral previousC n ca c)
                Psi     = next |> Option.map (fun (_, nextN, _, _) -> dihedral n ca c nextN)
                Omega   = next |> Option.map (fun (_, nextN, nextCA, _) -> dihedral ca c nextN nextCA)
            }
        )
        
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
        