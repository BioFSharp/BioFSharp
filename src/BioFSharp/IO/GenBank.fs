namespace BioFSharp.IO

open System.Collections.Generic
open FSharpAux
open FSharpAux.IO
open BioFSharp
open BioFSharp.FileFormats.GenBank
open System.IO

module GenBank =
    
    ///Functions for reading a GenBank file
    ///Represents the possible sections in a GenBank file
    type private CurrentSection = 
        Meta = 1 |Reference = 2| Feature = 3 |Origin = 4 |End = 5

    ///Token representing lines of a GenBank file for parsing purposes
    type private Token =
    ///Represents the lines ranking highest in hierarchy. These lines are not idented, and are parsed as key,value pair
    | Section of string*string
    ///Represents the lines ranking second in hierarchy. These lines are idented, but contain a key,value pair
    | Member of string*string
    ///Represents the lines ranking third in hierarchy. Features are only present in the features section of a GenBank file.
    ///These lines are idented and dont have a key.
    | Feature of string
    ///Represents the lines ranking lowest in hierarchy. These lines are idented and dont have a key. This union case indicates that the 
    ///value contained belongs to the next highest ranking line in hierarchy.
    | Value of string
    
    ///Splits the input string at a specific position (pos) returns two substrings of it, one sarting at (start) and
    ///ending at (pos), the other starting at (pos) and containing the rest of the string
    let private subStr start pos (str:string) =
        if start+pos < str.Length then 
            str.Substring(start,pos),(str.Substring(pos))
        else
            str,""

    ///Returns true if the input string is idented, otherwise returns false
    let private isIdent (s:string) = s.StartsWith " " 
    ///Retursn true if the input string is empty after being trimmed of whitespace, otherwise returns false
    let private isEmpty (s:string) =
        s.Trim() = "" 
        
    ///Returns a CurrentSection depending on an input key. Returns the input currentSection if the key does not indicate that the section changes.
    let private getCurrentSection (current: CurrentSection) (key : string) =
        match key.Trim() with
        |"REFERENCE"    -> CurrentSection.Reference
        |"FEATURES"     -> CurrentSection.Feature
        |"ORIGIN"       -> CurrentSection.Origin
        |_              -> current
        

    ///Regular expression for parsing a feature key,value pair from a string
    let private featureRegexPattern = @"/(?<qualifierName>.+)="+  @"(?<qualifierValue>.+)"
    
    ///Regular expression for parsing a feature key,value pair that contains no value from a string
    let private featureRegexPattern2 = @"/(?<qualifierName>.+)"

    ///returns an key value pair for a feature from an input string. If the input string does not contain an "=" sign, the value belongs
    ///to the previous line and the function EXPLAIN THIS LATER
    let private parseFeatureQualifier (s: string) =
        if s.Contains "=" then
            match s with 
            | Regex.Active.RegexGroups featureRegexPattern qual -> (qual.[0].["qualifierName"].Value),(qual.[0].["qualifierValue"].Value)
            | _ -> ".","."
        else
            match s with 
            | Regex.Active.RegexGroups featureRegexPattern2 qual -> (qual.[0].["qualifierName"].Value),"."
            | _ -> ".","."

    ///Assigns a string to its corresponding token type.
    let private lexer (sectionType:CurrentSection) (str:string) =
        
        let splitPos =
            match sectionType with
            |CurrentSection.Meta        ->  12
            |CurrentSection.Reference   ->  12
            |CurrentSection.Feature     ->  21
            |CurrentSection.Origin      ->  10
            |_ -> 0
        
        let (k,v) = subStr 0 splitPos str
        let cs = getCurrentSection sectionType k
        if isIdent k && not (isEmpty k) then
            cs,(Token.Member (k.Trim(),v.Trim()))
        elif isEmpty k then
            if cs = CurrentSection.Feature then
                if v.Trim().StartsWith "/" then
                    cs,(Token.Feature (v.Trim()))
                else
                    cs,(Token.Value (v.Trim()))
            else
                cs,(Token.Value (v.Trim()))
        else
            cs,(Token.Section (k.Trim(),v.Trim()))
        
    ///Iterates over an input sequence of strings and returns a sequence containing the corresponding token for each entry.
    let private tokenizer (input:seq<string>) =
        let en = input.GetEnumerator()
        let rec loop (sectionType:CurrentSection) =
            seq {
                match en.MoveNext() with
                | false -> ()
                | true -> 
                    let stype,token = lexer sectionType en.Current
                    yield (stype,token)
                    yield! loop stype
                }
        loop CurrentSection.Meta


    ///Iterates over an input sequence of tokens and adds the corresponding GenBankItems to a dictionary. The returned dictionary represents a GenBank file.
    let private parser (originSequenceConverter:seq<char> -> #seq<'SequenceItem>) (input:seq<CurrentSection*Token>) =
        
        let dict = new Dictionary<string,GenBankItem<'SequenceItem>>()
        let en = input.GetEnumerator()
        
        let rec loop sec k v (ref:(string*string) list) (refList:(string*string)list list) featType featBs featQual (qname:string) (featQualList:FeatureQualifier list) (feat:Feature list) (origin:string) (isBs:bool) =
            if en.MoveNext() then
                let nextSec, nextToken = en.Current
                match sec,nextSec with
                |CurrentSection.Meta,nextSec 
                    ->  match nextSec with
                        |CurrentSection.Meta
                            ->  match nextToken with
                                |Member (k',v') when k' ="ORGANISM"
                                    ->  dict.Add(k,(GenBankItem.Value v))
                                        loop nextSec k' (v'.PadRight(67)) ref refList featType featBs featQual qname featQualList feat origin isBs
                                |Section (k',v') |Member (k',v')
                                    ->  dict.Add(k,(GenBankItem.Value v))
                                        loop nextSec k' v' ref refList featType featBs featQual qname featQualList feat origin isBs
                                |Token.Value v' 
                                    ->  loop nextSec k (v+v') ref refList featType featBs featQual qname featQualList feat origin isBs
                                |_  ->  ()
                        |_  ->  dict.Add(k, GenBankItem<'SequenceItem>.Value v)
                                match nextToken with
                                |Section (k',v')
                                    -> loop nextSec k' v' ref refList featType featBs featQual qname featQualList feat origin isBs
                                |_  -> ()
                    
                |CurrentSection.Reference,nextSec
                    ->  match nextSec with
                        |CurrentSection.Reference
                            ->  match nextToken with
                                |Section (k',v') 
                                    ->  loop sec k' v' [] (((k,v)::ref)::refList) featType featBs featQual qname featQualList feat origin isBs
                                |Member (k',v')
                                    ->  loop sec k' v' ((k,v)::ref) refList featType featBs featQual qname featQualList feat origin isBs
                                |Token.Value v'
                                    -> loop nextSec k (v+v') ref refList featType featBs featQual qname featQualList feat origin isBs
                                |_  -> ()
                        |_  ->  dict.Add("REFERENCES",References ((((k,v)::ref)::refList) |> List.rev |> List.map (fun x -> List.rev x)))
                                match nextToken with
                                |Section (k',v')
                                    ->  loop nextSec k' v' ref refList featType featBs featQual qname featQualList feat origin isBs
                                |_  ->  ()
        
                |CurrentSection.Feature,nextSec
                    ->  match nextSec with 
                        |CurrentSection.Feature when (k="FEATURES")
                            ->  match nextToken with 
                                |Member (k',v')
                                    -> loop nextSec k' v' ref refList k' v' featQual qname featQualList feat origin true
                                |_  -> ()
                        |CurrentSection.Feature 
                            ->  match nextToken with
                                |Member (k',v')
                                    -> loop nextSec k' v' ref refList k' v' "" "" [] (((Feature.create featType featBs (List.rev((FeatureQualifier.create qname featQual)::(featQualList)))::feat))) origin true 
                                |Feature f
                                    ->  let qualName,featureQualifier = parseFeatureQualifier f
                                        loop nextSec k v ref refList featType featBs featureQualifier qualName (if qname = "" then featQualList else ((FeatureQualifier.create qname featQual )::featQualList) ) feat origin false
                                |Token.Value v'
                                    ->  if isBs then
                                            loop nextSec k v' ref refList featType (featBs+v') featQual qname featQualList feat origin true
                                        else
                                            loop nextSec k v' ref refList featType featBs (featQual+v') qname featQualList feat origin false
                                |_  -> ()
                        |_ ->   dict.Add("FEATURES",Features (List.rev (((Feature.create featType featBs (List.rev((FeatureQualifier.create qname featQual)::(featQualList)))::feat)))))
                                match nextToken with
                                |Section (k',v')
                                    ->  loop nextSec k' v' ref refList featType featBs featQual qname featQualList feat origin false
                                |_  ->  ()
        
                |CurrentSection.Origin,nextSec
                    ->  match nextSec with
                        |CurrentSection.Origin
                            -> match nextToken with
                                |Member (k',v')
                                    ->  loop nextSec k' v' ref refList featType featBs featQual qname featQualList feat (origin+v) false
                                |_ ->dict.Add("ORIGIN",GenBankItem.Sequence((String.toCharArray (origin+v)) |> originSequenceConverter))
                        |_  ->  dict.Add("ORIGIN",GenBankItem.Sequence((String.toCharArray (origin+v)) |> originSequenceConverter))
                |_ -> ()
        if en.MoveNext() then
            match (snd en.Current) with
            |Section (k,v) ->
                loop (fst en.Current) k v [] [] "." "." "" "" [] [] "" true
            |_ -> ()
            dict
        else 
            dict

    /// <summary>
    /// 
    /// </summary>
    /// <param name="originSequenceConverter"></param>
    /// <param name="lines"></param>
    let readLines (originSequenceConverter: seq<char>-> #seq<'SequenceItem>) (lines: seq<string>) : Dictionary<string,GenBankItem<'SequenceItem>> =
        lines
        |> tokenizer
        |> parser originSequenceConverter
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="originSequenceConverter"></param>
    /// <param name="filePath"></param>
    let read (originSequenceConverter: seq<char>-> #seq<'SequenceItem>) (filePath:string) : Dictionary<string,GenBankItem<'SequenceItem>> =
        Seq.fromFile filePath
        |> readLines originSequenceConverter

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="gb"></param>
    let write (originSequenceConverter: seq<'SequenceItem> -> seq<char>) (filePath:string) (gb : Dictionary<string,GenBankItem<'SequenceItem>>) = 
        gb
        |> toLines originSequenceConverter
        |> fun l -> File.WriteAllLines(filePath,l)
