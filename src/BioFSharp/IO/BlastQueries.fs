namespace BioFSharp.IO

open FSharpAux
open FSharpAux.IO
open BioFSharp.FileFormats
open DynamicObj

module BlastQueries =

    /// Reads BlastQuery from file enumerator 
    let readLines (lines: seq<string>) =

        // Conditon of grouping lines
        let sameGroup l =             
            //not (String.length l = 0 || l.[0] <> '#')
            not (String.length l = 0 || l.StartsWith("# BLA") |> not)

        let rec listRevWithRank rank (list: BlastHits.BlastHit list) acc=
            match list with
            | [] -> acc
            | [x] -> x.Rank<-rank
                     x::acc
            | head::tail -> 
                head.Rank<-rank
                listRevWithRank (rank-1) tail (head::acc)

        // Matches grouped lines and concatenates them    
        let rec record  (query:string) (fields:string array) (hitList: BlastHits.BlastHit list) (lines:string list)=
            match lines with
            | line::tail when line.StartsWith "# Query"  -> record (line.Remove(0,9).Trim()) fields hitList tail
            | line::tail when line.StartsWith "# Fields" -> record query (line.Remove(0,10).Split(',')) hitList tail
            | line::tail when line.StartsWith "#"        -> record query fields hitList tail
            | item::tail ->
                let split = item.Split('\t')
                let bh = BlastHits.BlastHit (split.[0],split.[1],-1)
                for i=2 to split.Length-1 do
                    bh |> DynObj.setProperty (fields.[i].Trim()) split.[i]
                record query fields (bh::hitList) tail 
            | [] -> match hitList with
                    | _::_ -> BlastQueries.BlastQuery.Hits (query , listRevWithRank hitList.Length hitList [])
                    | []   -> BlastQueries.BlastQuery.NoHits query
        
        // main
        lines
        |> Seq.groupWhen sameGroup 
        |> Seq.map (fun l -> 
            let l' = List.ofSeq l
            record "" [||] [] l' )


    /// Reads BlastQuery from file.
    let read (filePath: string) =
        FileIO.readFile filePath
        |> readLines