﻿namespace BioFSharp.FileFormats

open System
open System.IO
open FSharpAux
open FSharpAux.IO

module PDB =

    //!! https://www.wwpdb.org/documentation/file-format-content/format33

    type AtomName = {
        ChemicalSymbol: string
        RemotenessIndicator: string
        BranchDesignator: string
    } with
        static member toString (an: AtomName) =
            $"""{an.ChemicalSymbol.PadLeft(2,' ')}{an.RemotenessIndicator.PadRight(1,' ')}{an.BranchDesignator.PadRight(1,' ')}"""
        static member ofString (str:string) =
            {ChemicalSymbol = str.[0..1]; RemotenessIndicator = str.[2..2]; BranchDesignator = str.[3..3]}

    /// atomic coordinate record containing the X,Y,Z orthogonal � coordinates for atoms in standard residues (amino acids and nucleic acids).
    type Atom = {
        //Col 7-11
        SerialNumber                : int
        //Col 13-16
        Name                        : AtomName
        //Col 17
        AlternateLocationIndicator  : string
        //Col 18-20
        ResidueName                 : string
        //Col 22
        ChainIdentifier             : string
        //Col 23-26
        ResidueSequenceNumber       : int
        //Col 27
        ResidueInsertionCode        : string
        //Col 31-38
        X                           : float
        //Col 39-46
        Y                           : float
        //Col 47-54
        Z                           : float
        //Col 55-60
        Occupancy                   : float
        //Col 61-66
        TemperatureFactor           : float
        //Col 73-76
        SegmentIdentifier           : string
        //Col 77-78
        ElementSymbol               : string
        //Col 79-80
        Charge                      : string
    } with
        static member create 
            (
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
            ) = 
                {
                    SerialNumber                = serialNumber              
                    Name                        = name                      
                    AlternateLocationIndicator  = alternateLocationIndicator
                    ResidueName                 = residueName               
                    ChainIdentifier             = chainIdentifier           
                    ResidueSequenceNumber       = residueSequenceNumber     
                    ResidueInsertionCode        = residueInsertionCode      
                    X                           = x                         
                    Y                           = y                         
                    Z                           = z                         
                    Occupancy                   = occupancy                 
                    TemperatureFactor           = temperatureFactor         
                    SegmentIdentifier           = segmentIdentifier         
                    ElementSymbol               = elementSymbol             
                    Charge                      = charge
                }

        static member toString (atom: Atom) =
            let serialNumber              = $"{atom.SerialNumber}".PadLeft(5,' ')          // 7-11  -> SerialNumber                 right
            let name                      = AtomName.toString atom.Name
            let alternateLocationIndicator= $"{atom.AlternateLocationIndicator}".PadLeft(1,' ') // 17    -> AlternateLocationIndicator
            let residueName               = $"{atom.ResidueName}".PadLeft(3,' ')                // 18-20 -> ResidueName                  right
            let chainIdentifier           = $"{atom.ChainIdentifier}".PadLeft(1,' ')            // 22    -> ChainIdentifier           
            let residueSequenceNumber     = $"{atom.ResidueSequenceNumber}".PadLeft(4,' ')      // 23-26 -> ResidueSequenceNumber        right
            let residueInsertionCode      = $"{atom.ResidueInsertionCode}".PadLeft(1,' ')       // 27    -> ResidueInsertionCode      
            let x                         = $"%.3f{atom.X}".PadLeft(8,' ')                      // 31-38 -> X                            right
            let y                         = $"%.3f{atom.Y}".PadLeft(8,' ')                      // 39-46 -> Y                            right
            let z                         = $"%.3f{atom.Z}".PadLeft(8,' ')                      // 47-54 -> Z                            right
            let occupancy                 = $"%.2f{atom.Occupancy}".PadLeft(6,' ')              // 55-60 -> Occupancy                    right
            let temperatureFactor         = $"%.2f{atom.TemperatureFactor}".PadLeft(6,' ')      // 61-66 -> TemperatureFactor            right
            let segmentIdentifier         = $"{atom.SegmentIdentifier}".PadRight(4,' ')         // 73-76 -> SegmentIdentifier            left
            let elementSymbol             = $"{atom.ElementSymbol}".PadLeft(2,' ')              // 77-78 -> ElementSymbol                right
            let charge                    = $"{atom.Charge}".PadLeft(2,' ')                     // 79-80 -> Charge                    
            $"""ATOM  {serialNumber} {name}{alternateLocationIndicator}{residueName} {chainIdentifier}{residueSequenceNumber}{residueInsertionCode}   {x}{y}{z}{occupancy}{temperatureFactor}      {segmentIdentifier}{elementSymbol}{charge}""".PadRight(80, ' ')

    /// atomic coordinate record containing the X,Y,Z orthogonal � coordinates for atoms in nonstandard residues. Nonstandard residues include inhibitors, cofactors, ions, and solvent. The only functional difference from ATOM records is that HETATM residues are by default not connected to other residues. Note that water residues should be in HETATM records.
    type HetAtom = {
        //Col 7-11
        SerialNumber                : int
        //Col 13-16
        Name                        : AtomName
        //Col 17
        AlternateLocationIndicator  : string
        //Col 18-20
        ResidueName                 : string
        //Col 22
        ChainIdentifier             : string
        //Col 23-26
        ResidueSequenceNumber       : int
        //Col 27
        ResidueInsertionCode        : string
        //Col 31-38
        X                           : float
        //Col 39-46
        Y                           : float
        //Col 47-54
        Z                           : float
        //Col 55-60
        Occupancy                   : float
        //Col 61-66
        TemperatureFactor           : float
        //Col 73-76
        SegmentIdentifier           : string
        //Col 77-78
        ElementSymbol               : string
        //Col 79-80
        Charge                      : string
    } with
        static member create 
            (
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
            ) = 
                {
                    SerialNumber                = serialNumber              
                    Name                        = name                      
                    AlternateLocationIndicator  = alternateLocationIndicator
                    ResidueName                 = residueName               
                    ChainIdentifier             = chainIdentifier           
                    ResidueSequenceNumber       = residueSequenceNumber     
                    ResidueInsertionCode        = residueInsertionCode      
                    X                           = x                         
                    Y                           = y                         
                    Z                           = z                         
                    Occupancy                   = occupancy                 
                    TemperatureFactor           = temperatureFactor         
                    SegmentIdentifier           = segmentIdentifier         
                    ElementSymbol               = elementSymbol             
                    Charge                      = charge
                }

        static member toString (atom: HetAtom) =
            let serialNumber              = $"{atom.SerialNumber}".PadLeft(5,' ')          // 7-11  -> SerialNumber                 right
            let name                      = AtomName.toString atom.Name
            let alternateLocationIndicator= $"{atom.AlternateLocationIndicator}".PadLeft(1,' ') // 17    -> AlternateLocationIndicator
            let residueName               = $"{atom.ResidueName}".PadLeft(3,' ')                // 18-20 -> ResidueName                  right
            let chainIdentifier           = $"{atom.ChainIdentifier}".PadLeft(1,' ')            // 22    -> ChainIdentifier           
            let residueSequenceNumber     = $"{atom.ResidueSequenceNumber}".PadLeft(4,' ')      // 23-26 -> ResidueSequenceNumber        right
            let residueInsertionCode      = $"{atom.ResidueInsertionCode}".PadLeft(1,' ')       // 27    -> ResidueInsertionCode      
            let x                         = $"%.3f{atom.X}".PadLeft(8,' ')                      // 31-38 -> X                            right
            let y                         = $"%.3f{atom.Y}".PadLeft(8,' ')                      // 39-46 -> Y                            right
            let z                         = $"%.3f{atom.Z}".PadLeft(8,' ')                      // 47-54 -> Z                            right
            let occupancy                 = $"%.2f{atom.Occupancy}".PadLeft(6,' ')              // 55-60 -> Occupancy                    right
            let temperatureFactor         = $"%.2f{atom.TemperatureFactor}".PadLeft(6,' ')      // 61-66 -> TemperatureFactor            right
            let segmentIdentifier         = $"{atom.SegmentIdentifier}".PadRight(4,' ')         // 73-76 -> SegmentIdentifier            left
            let elementSymbol             = $"{atom.ElementSymbol}".PadLeft(2,' ')              // 77-78 -> ElementSymbol                right
            let charge                    = $"{atom.Charge}".PadLeft(2,' ')                     // 79-80 -> Charge                    
            $"""HETATM{serialNumber} {name}{alternateLocationIndicator}{residueName} {chainIdentifier}{residueSequenceNumber}{residueInsertionCode}   {x}{y}{z}{occupancy}{temperatureFactor}      {segmentIdentifier}{elementSymbol}{charge}""".PadRight(80, ' ')

    type Terminator =
        {
        // Col 7-11#
        SerialNumber            : int option
        // Col 18-20
        ResidueName             : string option
        // Col 22
        ChainIdentifier         : string option
        // Col 23-26
        ResidueSequenceNumber   : int option
        // Col 27
        ResidueInsertionCode    : string option
        } with
            static member create 
                (
                    ?SerialNumber         ,
                    ?ResidueName          ,
                    ?ChainIdentifier      ,
                    ?ResidueSequenceNumber,
                    ?ResidueInsertionCode 
                ) =
                    {
                        SerialNumber            = SerialNumber         
                        ResidueName             = ResidueName          
                        ChainIdentifier         = ChainIdentifier      
                        ResidueSequenceNumber   = ResidueSequenceNumber
                        ResidueInsertionCode    = ResidueInsertionCode 
                    }

            static member toString (ter:Terminator) =
                let serialNumber            = $"""{ter.SerialNumber           |> Option.map string |> Option.defaultValue ""}""".PadLeft(5,' ')
                let residueName             = $"""{ter.ResidueName            |> Option.defaultValue ""}""".PadLeft(3,' ')
                let chainIdentifier         = $"""{ter.ChainIdentifier        |> Option.defaultValue ""}""".PadLeft(1,' ')
                let residueSequenceNumber   = $"""{ter.ResidueSequenceNumber  |> Option.map string |> Option.defaultValue ""}""".PadLeft(4,' ')
                let residueInsertionCode    = $"""{ter.ResidueInsertionCode   |> Option.defaultValue ""}""".PadLeft(1,' ')

                $"""TER   {serialNumber}      {residueName} {chainIdentifier}{residueSequenceNumber}{residueInsertionCode}""".PadRight(80, ' ')

    [<RequireQualifiedAccess>]
    type Coordinate =
        | Atom of Atom
        | HetAtom of HetAtom
        | Terminator of Terminator

        static member toString (c:Coordinate) =
            match c with
            | Atom       c -> c |> Atom.toString
            | HetAtom    c -> c |> HetAtom.toString
            | Terminator c -> c |> Terminator.toString

    ///The DBREF record provides cross-reference links between PDB sequences (what appears in SEQRES record) and a corresponding database sequence. 
    type DBREF = {
        IdCode: string
        ChainIdentifier: string
        SeqBegin: int
        InsertBegin: string option
        SeqEnd: int
        InsertEnd: string option
        Database: string
        DbAccession: string
        DbIdCode: string
        DbSeqBegin: int
        IDBnsBeg: string option
        DbSeqEnd: int
        DbInsEnd: string option
    } with
        static member create(
            idCode,
            chainIdentifier,
            seqBegin,
            seqEnd,
            database,
            dbAccession,
            dbIdCode,
            dbSeqBegin,
            dbSeqEnd,
            ?insertBegin,
            ?insertEnd,
            ?iDBnsBeg,
            ?dbInsEnd
        ) =
            {
                IdCode          = idCode
                ChainIdentifier = chainIdentifier
                SeqBegin        = seqBegin
                InsertBegin     = insertBegin
                SeqEnd          = seqEnd
                InsertEnd       = insertEnd
                Database        = database
                DbAccession     = dbAccession
                DbIdCode        = dbIdCode
                DbSeqBegin      = dbSeqBegin
                IDBnsBeg        = iDBnsBeg
                DbSeqEnd        = dbSeqEnd
                DbInsEnd        = dbInsEnd
            }
