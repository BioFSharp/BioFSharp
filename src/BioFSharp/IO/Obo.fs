namespace BioFSharp.IO

open BioFSharp
open BioFSharp.FileFormats.Obo

open System
open FSharpAux
open FSharpAux.IO

module Obo =

    let internal xrefRegex = 
        System.Text.RegularExpressions.Regex("""(?<xrefName>^([^"{])*)(\s?)(?<xrefDescription>\"(.*?)\")?(\s?)(?<xrefModifiers>\{(.*?)}$)?""")

    let parseDBXref (v:string) =
        let matches = xrefRegex.Match(v.Trim()).Groups

        {
            Name = matches.Item("xrefName").Value
            Description = matches.Item("xrefDescription").Value
            Modifiers = matches.Item("xrefModifiers").Value
        }

    let internal synonymRegex = 
        System.Text.RegularExpressions.Regex("""(?<synonymText>^\"(.*?)\"){1}(\s?)(?<synonymScope>(EXACT|BROAD|NARROW|RELATED))?(\s?)(?<synonymDescription>\w*)(\s?)(?<dbxreflist>\[(.*?)\])?""")

    let parseSynonym (scopeFromDeprecatedTag:TermSynonymScope option) (line:int) (v:string) =
        let matches = synonymRegex.Match(v.Trim()).Groups
        {
            Text = matches.Item("synonymText").Value
            Scope =
                match scopeFromDeprecatedTag with
                |Some scope -> scope
                |_ ->   matches.Item("synonymScope").Value
                        |> TermSynonymScope.ofString line
            TypeName = matches.Item("synonymDescription").Value
            DBXrefs =
                let tmp = matches.Item("dbxreflist").Value
                match tmp.Replace("[","").Replace("]","") with
                | "" -> []
                | dbxrefs ->
                    dbxrefs.Split(',')
                    |> Array.map parseDBXref
                    |> Array.toList
        }

    ///<summary>
    /// Parses a OBO flat file and returns a sequence of OboTerm terms.
    ///
    /// Note that all other stanzas are ignored.
    ///</summary>
    ///<param name="verbose">If true, prints warnings for unknown tags.</param>
    ///<param name="input">The input sequence of strings representing the lines of the OBO flat file.</param>
    let readTermLines verbose (input:seq<string>)  =         
        /// Parses a [term] item in a recusive function
        let rec parseSingleOboTerm (en:Collections.Generic.IEnumerator<string>) lineNumber 
            id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
            intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
            propertyValues builtIn createdBy creationDate =   

            if en.MoveNext() then                
                let split = en.Current.Split([|": "|], System.StringSplitOptions.None)
                match split.[0] with
                | "id"              -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        v name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate            
        
                | "name"            -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        id v isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "is_anonymous"    ->
                    parseSingleOboTerm en (lineNumber + 1)
                        id name true altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "alt_id"              -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous (v::altIds) definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "def"              -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds v comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "comment"             -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition v subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "subset"              -> 
                    let v = split.[1..] |> String.concat ": "
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment (v::subsets) synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | synonymTag when synonymTag.Contains("synonym")              -> 
                    let scope =
                        match synonymTag with
                        | "exact_synonym"   -> Some Exact
                        | "narrow_synonym"  -> Some Narrow
                        | "broad_synonym"   -> Some Broad
                        | _                 -> None
                    let v = parseSynonym scope lineNumber (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets (v::synonyms) xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "xref" | "xref_analog" | "xref_unk" -> 
                    let v = (split.[1..] |> String.concat ": ") |> parseDBXref
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms (v::xrefs) isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "is_a"              -> 
                    let v = (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs (v::isA)
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "intersection_of"              -> 
                    let v = (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        (v::intersectionOf) unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "union_of"              -> 
                    let v = (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf (v::unionOf) disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate
                    
                | "disjoint_from"              -> 
                    let v = (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf (v::disjointFrom) relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate
                    
                | "relationship"              -> 
                    let v = (split.[1..] |> String.concat ": ")
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom (v::relationships) isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate

                | "is_obsolete"             -> 
                    let v = ((split.[1..] |> String.concat ": ").Trim()) 
                    let v' = v = "true"

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships v' replacedby consider 
                        propertyValues builtIn createdBy creationDate            

                | "replaced_by"             -> 
                    let v = (split.[1..] |> String.concat ": ")

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete (v::replacedby) consider 
                        propertyValues builtIn createdBy creationDate


                | "consider" | "use_term"            -> 
                    let v = (split.[1..] |> String.concat ": ")

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby (v::consider)
                        propertyValues builtIn createdBy creationDate


                | "builtin"             -> 
                    let v = ((split.[1..] |> String.concat ": ").Trim()) 
                    let v' = v = "true"

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues v' createdBy creationDate

                | "property_value"             -> 
                    let v = (split.[1..] |> String.concat ": ")

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        (v::propertyValues) builtIn createdBy creationDate

                | "created_by"             -> 
                    let v = (split.[1..] |> String.concat ": ")

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn v creationDate


                | "creation_date"             -> 
                    let v = (split.[1..] |> String.concat ": ")

                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy v

                | "" -> 
                    lineNumber,
                    OboTerm.create 
                        id 
                        name 
                        isAnonymous 
                        (altIds |> List.rev) 
                        definition comment 
                        (subsets        |> List.rev)
                        (synonyms       |> List.rev)
                        (xrefs          |> List.rev)
                        (isA            |> List.rev)
                        (intersectionOf |> List.rev)
                        (unionOf        |> List.rev)
                        (disjointFrom   |> List.rev)
                        (relationships  |> List.rev)
                        isObsolete 
                        (replacedby     |> List.rev)
                        (consider       |> List.rev)
                        (propertyValues |> List.rev)
                        builtIn 
                        createdBy creationDate

                | unknownTag -> 
                    if verbose then printfn "[WARNING@L %i]: Found term tag <%s> that does not fit OBO flat file specifications 1.4. Skipping it..." lineNumber unknownTag
                    parseSingleOboTerm en (lineNumber + 1)
                        id name isAnonymous altIds definition comment subsets synonyms xrefs isA 
                        intersectionOf unionOf disjointFrom relationships isObsolete replacedby consider 
                        propertyValues builtIn createdBy creationDate
                                   
            else
                // Maybe check if id is empty
                lineNumber,
                OboTerm.create 
                    id 
                    name 
                    isAnonymous 
                    (altIds |> List.rev) 
                    definition comment 
                    (subsets        |> List.rev)
                    (synonyms       |> List.rev)
                    (xrefs          |> List.rev)
                    (isA            |> List.rev)
                    (intersectionOf |> List.rev)
                    (unionOf        |> List.rev)
                    (disjointFrom   |> List.rev)
                    (relationships  |> List.rev)
                    isObsolete 
                    (replacedby     |> List.rev)
                    (consider       |> List.rev)
                    (propertyValues |> List.rev)
                    builtIn 
                    createdBy creationDate
                //failwithf "Unexcpected end of file."

        //parseTermDef
        let rec parseSingleTermDef (en:Collections.Generic.IEnumerator<string>) id name isTransitive isCyclic =     
            if en.MoveNext() then                
                let split = en.Current.Split([|": "|], System.StringSplitOptions.None)
                match split.[0] with
                | "id"            -> parseSingleTermDef en (split.[1..] |> String.concat ": ") name isTransitive isCyclic
                | "name"          -> parseSingleTermDef en id (split.[1..] |> String.concat ": ") isTransitive isCyclic
        
        
                | "is_transitive" -> parseSingleTermDef en id name (split.[1..] |> String.concat ": ") isCyclic
                | "is_cyclic"     -> parseSingleTermDef en id name isTransitive (split.[1..] |> String.concat ": ")
                | ""              -> OboTermDef.create id name isTransitive isCyclic
                      
                | _               -> parseSingleTermDef en id name isTransitive isCyclic
            else
                // Maybe check if id is empty
                OboTermDef.create id name isTransitive isCyclic
                //failwithf "Unexcpected end of file."

        let en = input.GetEnumerator()
        let rec loop (en:System.Collections.Generic.IEnumerator<string>) lineNumber =
            seq {
                match en.MoveNext() with
                | true ->             
                    match en.Current with
                    | "[Term]"    -> let lineNumber,parsedTerm = (parseSingleOboTerm en lineNumber "" "" false [] "" "" [] [] [] [] [] [] [] [] false [] [] [] false "" "")
                                     yield parsedTerm
                                     yield! loop en lineNumber
                    | _ -> yield! loop en (lineNumber+1)
                | false -> ()
            }
        loop en 1

    let readTerms (verbose:bool) (filePath: string) =
        filePath
        |> System.IO.File.ReadLines
        |> readTermLines verbose