namespace BioFSharp

type SecondaryStructureFormat =
    | DSSP
    | Stride
    | NoFormat

///compromise summary of secondary structure, intended to approximate crystallographers' intuition, based on columns 19-38, which are the principal result of DSSP analysis of the atomic coordinates.
type SecondaryStructure =
    /// α-helix
    | AlphaHelix        of StructureInfo: string * Format: SecondaryStructureFormat
    /// residue in isolated β-bridge
    | IsolatedBetaBridge of StructureInfo: string * Format: SecondaryStructureFormat
    /// extended strand, participates in β ladder
    | BetaSheet         of StructureInfo: string * Format: SecondaryStructureFormat
    /// 3-helix (310 helix)
    | Helix310          of StructureInfo: string * Format: SecondaryStructureFormat
    /// 5 helix (π-helix)
    | PiHelix           of StructureInfo: string * Format: SecondaryStructureFormat
    /// hydrogen bonded turn
    | Turn              of StructureInfo: string * Format: SecondaryStructureFormat
    /// bend
    | Bend              of StructureInfo: string * Format: SecondaryStructureFormat
    ///
    | NoStructure       of StructureInfo: string * Format: SecondaryStructureFormat

    static member toString (s: SecondaryStructure) =
        match s with
        | AlphaHelix         _ -> "H"
        | IsolatedBetaBridge _ -> "B"
        | BetaSheet          _ -> "E"
        | Helix310           _ -> "G"
        | PiHelix            _ -> "I"
        | Turn               _ -> "T"
        | Bend               _ -> "S"
        | NoStructure       (_, SecondaryStructureFormat.Stride) -> "C"
        | NoStructure       (_, _) -> " "

    static member isHelical (s: SecondaryStructure) =
        match s with
        | AlphaHelix _ | Helix310 _ | PiHelix _ -> true
        | _ -> false

    static member isSheet (s: SecondaryStructure) =
        match s with
        | IsolatedBetaBridge _ | BetaSheet _ -> true
        | _ -> false

    static member isNoStructure (s: SecondaryStructure) =
        match s with
        | NoStructure _ -> true
        | _ -> false

    static member ofString (secondaryStructureFormat:SecondaryStructureFormat) (str:string) =

        let identifier = str.[0]
        
        match (secondaryStructureFormat, identifier) with 
        | _,'H'       -> AlphaHelix           (str.Trim(), secondaryStructureFormat)
        | SecondaryStructureFormat.DSSP,'B' | SecondaryStructureFormat.Stride,'b' | SecondaryStructureFormat.Stride,'B' 
        | SecondaryStructureFormat.NoFormat,'B' | SecondaryStructureFormat.NoFormat,'b' | SecondaryStructureFormat.NoFormat,'B' 
            -> IsolatedBetaBridge   (str.Trim(), secondaryStructureFormat)
        | _,'E'       -> BetaSheet            (str.Trim(), secondaryStructureFormat)
        | _,'G'       -> Helix310             (str.Trim(), secondaryStructureFormat)
        | _,'I'       -> PiHelix              (str.Trim(), secondaryStructureFormat)
        | _,'T'       -> Turn                 (str.Trim(), secondaryStructureFormat)
        | _,'S'       -> Bend                 (str.Trim(), secondaryStructureFormat)
        | SecondaryStructureFormat.DSSP,' ' | SecondaryStructureFormat.Stride,'C' 
        | SecondaryStructureFormat.NoFormat,' ' | SecondaryStructureFormat.NoFormat,'C' 
            -> NoStructure          (str.Trim(), secondaryStructureFormat)
        | _ -> failwith $"{str} does not start with a valid secondary structure code. Valid codes: H, B, E, G, I, T, S, ' ', C but got '%c{str.[0]}'"
