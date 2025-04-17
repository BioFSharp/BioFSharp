namespace BioFSharp.FileFormats

module BlastQueries =

    open BlastHits

    type BlastQuery =
        | NoHits of string 
        | Hits   of string * BlastHit list


    let getQueryId (bq:BlastQuery) =
        match bq with 
        | NoHits qid    -> qid
        | Hits (qid,_)  -> qid

    let getBlastHits (bq:BlastQuery) =
        match bq with 
        | NoHits _    -> []
        | Hits (_,bh) -> bh

    // Returns the best hit from blast query.
    let tryGetBestHit (bq:BlastQuery) =
        match bq with 
        | NoHits _    -> None
        | Hits (_,bh) -> 
            match bh with 
            | h::t -> Some  h
            | []   -> None

    let mapHits mapping (bq:BlastQuery) =
        match bq with 
        | NoHits _    -> bq
        | Hits (qid,bh) -> 
            BlastQuery.Hits(qid,List.map mapping bh)

    let filterHits predicate (bq:BlastQuery) =
        match bq with 
        | NoHits _    -> bq
        | Hits (qid,bh) -> 
            let fHits = List.filter predicate bh
            match fHits with
            | [] -> BlastQuery.NoHits qid
            | _  -> BlastQuery.Hits(qid,fHits)