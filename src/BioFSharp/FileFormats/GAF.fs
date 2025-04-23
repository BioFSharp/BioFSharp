namespace BioFSharp.FileFormats

open System
open FSharpAux
open FSharpAux.IO

module GAF =
    
    type GAFEntry = {
        Database            : string
        DbObjectID          : string
        DbObjectSymbol      : string
        Qualifier           : string []
        GoTerm              : string
        DbReference         : string []
        Evidence            : string
        WithFrom            : string []
        Aspect              : string
        DbObjectName        : string
        DbObjectSynonym     : string []
        DbObjectType        : string
        Taxon               : string []
        Date                : System.DateTime
        AssignedBy          : string
        AnnotationExtension : string [] option
        GeneProductFormId   : string option
    } with
        static member create (str:string) version2 =  
            let split = str.Split([|'\t'|])
            { 
                Database            = split.[0]
                DbObjectID          = split.[1]
                DbObjectSymbol      = split.[2]
                Qualifier           = split.[3].Split([|'|'|])
                GoTerm              = split.[4]
                DbReference         = split.[5].Split([|'|'|])
                Evidence            = split.[6]
                WithFrom            = split.[7].Split([|'|'|])
                Aspect              = split.[8]
                DbObjectName        = split.[9]
                DbObjectSynonym     = split.[10].Split([|'|'|])
                DbObjectType        = split.[11]
                Taxon               = split.[12].Split([|'|'|])
                Date                = System.DateTime.ParseExact(split.[13],"yyyyMMdd",null).Date
                AssignedBy          = split.[14]
                AnnotationExtension = if version2 then Some (split.[15].Split([|','|])) else None
                GeneProductFormId   = if version2 then Some  split.[16]                 else None
            }

        static member toString (isVersion2:bool) (entry: GAFEntry) : string =
            [
                entry.Database          
                entry.DbObjectID
                entry.DbObjectSymbol
                entry.Qualifier         |> String.concat "|"
                entry.GoTerm
                entry.DbReference       |> String.concat "|"
                entry.Evidence
                entry.WithFrom          |> String.concat "|"
                entry.Aspect
                entry.DbObjectName
                entry.DbObjectSynonym   |> String.concat "|"
                entry.DbObjectType
                entry.Taxon             |> String.concat "|"
                entry.Date.ToString("yyyyMMdd")
                entry.AssignedBy    
                entry.AnnotationExtension |> fun x -> if isVersion2 then x.Value |> String.concat "," else ""   //adds additional tabs to file if version is <2
                entry.GeneProductFormId   |> fun x -> if isVersion2 then x.Value else ""                        //adds additional tabs to file if version is <2
            ]
            |> String.concat "\t"
    
    type GAF = {
        Header  : seq<string>
        Entries : seq<GAFEntry>
    } with
        static member create header entries =
            { Header = header; Entries = entries }

        static member toLines (gaf: GAF) =
            let isVersion2 = 
                (gaf.Header |> Seq.item 0).StartsWith("!gaf-version: 2")
            Seq.append 
                gaf.Header 
                (gaf.Entries |> Seq.map (GAFEntry.toString isVersion2))

        static member toString (gaf: GAF) =
            gaf
            |> GAF.toLines
            |> String.concat System.Environment.NewLine