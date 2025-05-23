﻿namespace BioFSharp.FileFormats

open System.IO
open System.Text
open BioFSharp

///Contains functions for parsing phylogenetic trees from the Newick format
module Newick = 
    
    ///Read file as sequence of characters
    let private readFile (file:string) =   
        seq {
            use textReader = new StreamReader(file, Encoding.Default)
            while not textReader.EndOfStream do
                yield textReader.Read() |> char}

    //---Reader---//

    (*    Lexer    *)
    ///Type used by the lexer
    type private Token = 
        | OpenBranch
        | CloseBranch
        | NextNode
        | NextLine
        | Separator
        | EndTree
        | ID of char
        | Distance of char

    let private lexer (lastToken:Token) (symbol:char) = 
        match lastToken,symbol with 
        | _ , '\013' | _ , '\010' -> NextLine
        | _ , ',' -> NextNode
        | _ , '(' -> OpenBranch
        | _ , ')' -> CloseBranch
        | _ , ':' -> Separator
        | _ , ';' -> EndTree
        | Distance _ , c | Separator, c -> Distance c
        | _, c -> ID c

    (*    Tokenizer    *)
    let private tokenizer (input:seq<_>) = 
        let en = input.GetEnumerator()
        let rec loop lastToken = 
            seq {
                match en.MoveNext() with
                | false -> ()
                | true -> 
                    match lexer lastToken en.Current with
                    | NextLine -> yield NextLine; yield! loop lastToken
                    | cToken -> yield cToken; yield! loop cToken
                }
        loop OpenBranch

    (*    Parser    *)
    ///Parses a seq of tokens to a PhylTree
    let private parser (converter : string -> 'Distance) (input:seq<Token>) : PhylogeneticTree<string*'Distance> = 
        let en = input.GetEnumerator()
        let sbID,sbDist = StringBuilder(),StringBuilder()
        ///Reduces tree to a tuple of its info
        let cutDown (tree:PhylogeneticTree<string*'Distance>) = 
            match tree with | PhylogeneticTree.Branch (x,y) -> x
        let mutable iOpen,iClosed = 0,0
        let rec loop() =
            ///This function is called when a new branch is opened, it recursively creates a list of nodes until the corresponding ')' is reached
            let rec createBranch treeList = 
                match loop() with
                |tree,false -> createBranch (tree::treeList) //Node finished, last symbol was ','
                |tree,true -> (tree::treeList) |> List.rev  //Node finished, last symbol was ')'
            match en.MoveNext() with
            |false -> 
                //When the enumeration ends even though the tree is not finished, an exception is raised
                if iOpen = iClosed then 
                    failwith (sprintf "Tree not complete: finished enumeration after as many open as closed nodes. Probably the semicolon at the end is missing")
                else
                    failwith (sprintf "Tree not complete: finished enumeration after %i opened but only %i closed branches." iOpen iClosed)
            |true ->
                match en.Current with
                //createBranch is called to create childrenlist, afterwards the 'nodeinfo' is obtained by creating a tree with 'loop' and reducing it to 'info' afterwards with 'cutdown'
                | OpenBranch -> 
                    iOpen <- iOpen + 1
                    let children = createBranch []
                    let treeInfo,isFinished = loop()
                    PhylogeneticTree.Branch(cutDown treeInfo, children),isFinished
                //distancevalue is added to stringbuilder, iteration is continued
                | Distance c -> 
                    sbDist.Append(c) |> ignore
                    loop()
                //namevalue is added to stringbuilder, iteration is continued
                | ID c -> 
                    sbID.Append(c) |> ignore
                    loop()
                //name is obtained from stringbuilder, distance is obtained from stringbuilder and converted; tree is built from these infos and branchclosed boolean true is returned
                | CloseBranch -> 
                    iClosed <- iClosed + 1
                    let dist,id = sbDist.ToString(),sbID.ToString()
                    (sbDist.Clear(),sbID.Clear()) |> ignore
                    PhylogeneticTree.Branch((id,converter dist),[]),true
                //name is obtained from stringbuilder, distance is obtained from stringbuilder and converted; tree is built from these info and branchclosed boolean false is returned
                | NextNode -> 
                    let dist,id = sbDist.ToString(),sbID.ToString()
                    (sbDist.Clear(),sbID.Clear()) |> ignore
                    PhylogeneticTree.Branch((id,converter dist),[]),false
                //name is obtained from stringbuilder, distance is obtained from stringbuilder and converted; tree is built from these infos and branchclosed boolean true is returned
                | EndTree ->
                    let dist,id = sbDist.ToString(),sbID.ToString()
                    (sbDist.Clear(),sbID.Clear()) |> ignore
                    PhylogeneticTree.Branch((id,converter dist),[]),true
                //ignored
                | Separator -> 
                    loop()
                //ignored
                | NextLine -> 
                    loop()
        fst (loop())

    ///Returns a PhylTree of file. Converter is used to create a distancevalue from a string
    let read (converter : string -> 'Distance) (path: string) : PhylogeneticTree<string*'Distance> =
        path
        |> readFile
        |> tokenizer
        |> parser converter

     //---Writer---//

    ///Creates a NewickTree file of PhylTree. nodeConverter is used to split the distanceInfo and the name of a node, because they are parsed separately. First result of the tuple is name, second is distance.
    let write (nodeConverter: 'T -> string * string) (path:string) (tree: PhylogeneticTree<'T>) = 
        let rec loop tree =
            seq {
                match tree with
                | PhylogeneticTree.Leaf nodeInfo ->
                    let name,distance = nodeConverter nodeInfo
                    yield name + ":" + (distance)
                | PhylogeneticTree.Branch ((nodeInfo), nl) ->
                    let nodeInfo = 
                        match nodeConverter nodeInfo with
                        | (name, "") -> name
                        | (name, distance) -> name + ":" + (distance)                        
                    yield "(" + "\013" +  "\010"
                    let l = nl.Length
                    for i = 0 to l-2 do
                        yield! loop nl.[i]
                        yield "," + "\013" +  "\010"
                    yield! loop nl.[l-1]
                    yield ")" + "\013" + "\010"
                    yield nodeInfo
            }
        let s = loop tree
        use sw = (new System.IO.StreamWriter(path))
        sw.AutoFlush <- true
        Seq.iter (fun (i:string) -> sw.Write(i)) s
        sw.Write (";")
