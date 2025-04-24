namespace BioFSharp.IO

open BioFSharp.FileFormats.PDB
open System.IO

module PDB =

    module ActivePatterns =

        let (|ATOM|_|) (line:string) = 
            if line.StartsWith "ATOM" then
                let serialNumber                = line.[6..10]  .Trim() |> int
                let name                        = line.[12..15] |> AtomName.ofString
                let alternateLocationIndicator  = line.[16..16] .Trim() 
                let residueName                 = line.[17..19] .Trim() 
                let chainIdentifier             = line.[21..21] .Trim() 
                let residueSequenceNumber       = line.[22..25] .Trim() |> int
                let residueInsertionCode        = line.[26..26] .Trim() 
                let x                           = line.[30..37] .Trim() |> float
                let y                           = line.[38..45] .Trim() |> float
                let z                           = line.[46..53] .Trim() |> float
                let occupancy                   = line.[54..59] .Trim() |> float
                let temperatureFactor           = line.[60..65] .Trim() |> float
                let segmentIdentifier           = line.[72..75] .Trim() 
                let elementSymbol               = line.[76..77] .Trim() 
                let charge                      = line.[78..79] .Trim() 
                Some (
                    Atom.create(
                        serialNumber              ,
                        name                      ,
                        alternateLocationIndicator,
                        residueName               ,
                        chainIdentifier           ,
                        residueSequenceNumber     ,
                        residueInsertionCode      ,
                        x                         ,
                        y                         ,
                        z                         ,
                        occupancy                 ,
                        temperatureFactor         ,
                        segmentIdentifier         ,
                        elementSymbol             ,
                        charge
                    )
                )
            else
                None

        let (|HETATM|_|) (line:string) = 
            if line.StartsWith "HETATM" then
                let serialNumber                = line.[6..10]  .Trim() |> int
                let name                        = line.[12..15] |> AtomName.ofString
                let alternateLocationIndicator  = line.[16..16] .Trim() 
                let residueName                 = line.[17..19] .Trim() 
                let chainIdentifier             = line.[21..21] .Trim() 
                let residueSequenceNumber       = line.[22..25] .Trim() |> int
                let residueInsertionCode        = line.[26..26] .Trim() 
                let x                           = line.[30..37] .Trim() |> float
                let y                           = line.[38..45] .Trim() |> float
                let z                           = line.[46..53] .Trim() |> float
                let occupancy                   = line.[54..59] .Trim() |> float
                let temperatureFactor           = line.[60..65] .Trim() |> float
                let segmentIdentifier           = line.[72..75] .Trim() 
                let elementSymbol               = line.[76..77] .Trim() 
                let charge                      = line.[78..79] .Trim() 
                Some (
                    HetAtom.create(
                        serialNumber              ,
                        name                      ,
                        alternateLocationIndicator,
                        residueName               ,
                        chainIdentifier           ,
                        residueSequenceNumber     ,
                        residueInsertionCode      ,
                        x                         ,
                        y                         ,
                        z                         ,
                        occupancy                 ,
                        temperatureFactor         ,
                        segmentIdentifier         ,
                        elementSymbol             ,
                        charge
                    )
                )
            else
                None

        let (|TER|_|) (line:string) = 
        
            let emptyToOpt (str: string) =
                if str.Trim().Length = 0 then None else Some str

            if line.StartsWith "TER" then
                let serialNumber            = line.[6..10]  |> emptyToOpt |> Option.map int
                let residueName             = line.[17..19] |> emptyToOpt 
                let chainIdentifier         = line.[21..21] |> emptyToOpt 
                let residueSequenceNumber   = line.[22..25] |> emptyToOpt |> Option.map int
                let residueInsertionCode    = line.[27..27] |> emptyToOpt 

                Some (
                    Terminator.create(
                        ?SerialNumber         = serialNumber         ,
                        ?ResidueName          = residueName          ,
                        ?ChainIdentifier      = chainIdentifier      ,
                        ?ResidueSequenceNumber= residueSequenceNumber,
                        ?ResidueInsertionCode = residueInsertionCode 
                    )
                )
            else
                None

        
        let (|DBREF|_|) (line:string) = 
            let emptyToOpt (str: string) =
                if str.Trim().Length = 0 then None else Some str

            if line.StartsWith "DBREF " then
                let idCode          = line[7..10].Trim()
                let chainIdentifier = line[12] |> string
                let seqBegin        = line[14..17].Trim() |> int
                let insertBegin     = line[18] |> string |> emptyToOpt 
                let seqEnd          = line[20..23].Trim() |> int
                let insertEnd       = line[24] |> string |> emptyToOpt 
                let database        = line[26..31].Trim()
                let dbAccession     = line[33..40].Trim()
                let dbIdCode        = line[42..54].Trim()
                let dbSeqBegin      = line[55..59].Trim() |> int
                let iDBnsBeg        = line[60] |> string |> emptyToOpt 
                let dbSeqEnd        = line[62..66].Trim() |> int
                let dbInsEnd        = line[67] |> string |> emptyToOpt 

                Some (
                    DBREF.create(
                        idCode          = idCode,
                        chainIdentifier = chainIdentifier,
                        seqBegin        = seqBegin,
                        seqEnd          = seqEnd,
                        database        = database,
                        dbAccession     = dbAccession,
                        dbIdCode        = dbIdCode,
                        dbSeqBegin      = dbSeqBegin,
                        dbSeqEnd        = dbSeqEnd,
                        ?insertBegin    = insertBegin,
                        ?insertEnd      = insertEnd,
                        ?iDBnsBeg       = iDBnsBeg,
                        ?dbInsEnd       = dbInsEnd
                    )       
                )
            else
                None

    let tryParseCoordinateLine (line:string) =
        match line with
        | ActivePatterns.ATOM      a -> Some (Coordinate.Atom a)
        | ActivePatterns.HETATM    h -> Some (Coordinate.HetAtom h)
        | ActivePatterns.TER       t -> Some (Coordinate.Terminator t)
        | _ -> None

    let readDBREFs (path: string) =
        path
        |> File.ReadAllLines 
        |> Seq.choose (fun x -> match x with | ActivePatterns.DBREF d -> Some d | _ -> None)

    let readCoordinates (path: string) =
        path
        |> File.ReadAllLines 
        |> Seq.choose tryParseCoordinateLine