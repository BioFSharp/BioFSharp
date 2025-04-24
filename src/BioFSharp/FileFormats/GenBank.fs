namespace BioFSharp.FileFormats

///functions for reading and writing GenBank files
module GenBank =
      
    open System
    open FSharpAux
    open FSharpAux.IO    
    open System.Collections.Generic
    open BioFSharp

    ///Represents a single feature Qualifier and its value from the FEATURES section of a Genbank file. Features can contain
    ///Information about genes and gene products, as well as regions of biological significance reported in the sequence
    type FeatureQualifier = {
        ///Key of the Feature
        Name:string;
        ///Value of the Feature
        Value:string
    } with
        static member create name value = {Name=name;Value=value} 

    ///Represents a single feature from the FEATURES section of a GenBank file. Features can contain
    ///Information about genes and gene products, as well as regions of biological significance reported in the sequence
    type Feature = {
        ///Type of the Feature
        Type        : string;
        ///Location of the feature in the sequence
        BaseSpan    : string;
        ///A List of feature Qualifiers and their values associated with this feature
        Qualifiers  : FeatureQualifier list
    } with
        static member create t bs qual = {Type = t; BaseSpan=bs; Qualifiers=qual}

    /// Represents any Item a GenBank file can contain as a union case. The result of parsing a genBank file will be a dictionary containing this type.
    type GenBankItem<'SequenceItem> =
        ///Any value contained in the meta section of a GenBank file. 
        | Value of string
        ///All references contained in a GenBank file is seperate entries in a list.
        | References of (string*string) list list
        ///All features contained in a GenBank file as seperate entries in a list
        | Features of Feature list
        ///The origin section of a GenBank file
        | Sequence of seq<'SequenceItem>

        ///Returns all references of a GenBank file representation
    let getReferences (gb:Dictionary<string,GenBankItem<'SequenceItem>>) =
        if gb.ContainsKey("REFERENCES") then
            match gb.["REFERENCES"] with 
            |References r
                ->  r
            |_  ->  []
        else
            []
    
    ///Returns all features of a GenBank file representation
    let getFeatures (gb:Dictionary<string,GenBankItem<'SequenceItem>>) =
        if gb.ContainsKey("FEATURES") then
            match gb.["FEATURES"] with 
            |Features f
                ->  f
            |_  ->  []
        else
            []
    
    ///Returns all features of a specific type of a GenBank file representation
    let getFeaturesWithType (featureType:string) (gb:Dictionary<string,GenBankItem<'SequenceItem>>) =
        if gb.ContainsKey("FEATURES") then
            match gb.["FEATURES"] with
            |Features f ->  [for i in f do if i.Type = featureType then yield i]
            |_          ->  printfn "unexpected type at key FEATURES. Result is empty"
                            []
        else
            []
    
    ///Returns the Origin of a GenBank file representation
    let getOrigin (gb:Dictionary<string,GenBankItem<'SequenceItem>>) =
        if gb.ContainsKey("ORIGIN") then
            match gb.["ORIGIN"] with 
            |Sequence o   ->  o
            |_          ->  failwith "No Origin found"
        else
             failwith "No Origin found"
    
    ///Returns all Values of the meta section of a Genbank file representation
    let getValues (gb:Dictionary<string,GenBankItem<'SequenceItem>>) = 
        [for i in gb do
            match i.Value with
            |GenBankItem.Value v    -> yield i.Key,v
            |_          -> ()
            ]
    
    ///Returns a GenBank item at the specified key, if it exists in the dictionary
    let tryGetItem (key:string) (gb:Dictionary<string,GenBankItem<'SequenceItem>>) =
        if gb.ContainsKey key then
            Some gb.[key]
        else
            None

    ///constructs a sequence of strings in the right formatting (including identation of the key and the position for splitting key/value in the file) 
    ///from input key and value.
    let internal formatKV (key:string) (value:string) split ident =

        let rec loop i splitSeq (words:string array) (line:string) lineList = 
            if i<words.Length then
                if (line.Length + words.[i].Length) < 80 then
                    loop (i+1) splitSeq words (line+words.[i]) lineList
                else
                    loop (i+1) splitSeq words (splitSeq+words.[i]) (line::lineList)
            else List.rev (line::lineList)
        
        let splitSeq = 
            [|for i=0 to split-1 do yield ' '|] |> String.fromCharArray
            
        let k' = key.PadLeft(key.Length+ident)
        let k = k'.PadRight(split)

        if value.Length < (80-split) then
            let line = k+value
            seq{yield line}
        elif not (value.Contains " ") && not (value.Contains ",") then
            let x = 
                value 
                |> String.toCharArray
                |> Seq.chunkBySize (79-split) 
                |> Seq.mapi (fun i x -> if i=0 then k+(String.fromCharArray x) else splitSeq + (String.fromCharArray x))
            x
        else 
            let words = 
                value 
                |> String.toCharArray
                |> Seq.groupAfter (fun x -> if x = ' ' || x = ',' then true else false)
                |> Seq.toArray
                |> Array.map (fun x -> List.toArray x |> String.fromCharArray)
            seq{ yield! loop 0 splitSeq words k []}

    ///creates a GenBank file at the specified path, taking a converter function for the origin sequence of the file 
    let toLines (originSequenceConverter: seq<'SequenceItem> -> seq<char>) (gb : Dictionary<string,GenBankItem<'SequenceItem>>) = 
        seq{
            for kv in gb do
                let k,gbi = kv.Key,kv.Value
        
                match gbi,k with
                    |GenBankItem.Value v,"ORGANISM" 
                        ->  yield! (formatKV k v 12 2) |> Seq.mapi (fun i x -> if i=0 then "  " + x.Trim() else x)
                    |GenBankItem.Value v,_          
                        ->  yield! (formatKV k v 12 0)
                    |References r,_       
                        ->  for i in r do
                                for k',v' in i do
                                    match k' with
                                    |"REFERENCE"-> yield! formatKV k' v' 12 0
                                    |"PUBMED"   -> yield! formatKV k' v' 12 3
                                    |_          -> yield! formatKV k' v' 12 2
                    |Features f,_                   
                        ->  yield "FEATURES             Location/Qualifiers"
                            for feat in f do
                                let k',bs = feat.Type,feat.BaseSpan
                                yield! formatKV k' bs 21 5
                                for qual in feat.Qualifiers do
                                    yield! formatKV "                    " ("/"+qual.Name+"="+qual.Value) 21 0
                    |GenBankItem.Sequence o,_
                        ->  yield "ORIGIN      "
                            let charSeq = 
                                originSequenceConverter o
                                |> Seq.chunkBySize 60
                                |> Seq.map (fun x -> Seq.chunkBySize 10 x)
                                |> Seq.map (fun x -> x |> Seq.foldi (fun i acc elem -> if not (i=5) then acc + (String.fromCharArray elem) + " " else  acc + (String.fromCharArray elem)) "")
                                |> Seq.mapi (fun i x -> let k = string ((i*60) + 1)
                                                        let k' = k.PadLeft(9)
                                                        k' + " " + x)
                            yield! charSeq
                            yield "//"
            }

    let toString (originSequenceConverter: seq<'SequenceItem> -> seq<char>) (gb : Dictionary<string,GenBankItem<'SequenceItem>>) = 
        gb
        |> toLines originSequenceConverter
        |> String.concat System.Environment.NewLine