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
            