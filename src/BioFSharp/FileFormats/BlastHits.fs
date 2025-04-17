namespace BioFSharp.FileFormats

open DynamicObj

module BlastHits =

    type BlastHit(queryId:string, subjectId:string,rank:int) =
        inherit DynamicObj ()
        //interface ITrace with
            // Implictit ITrace    
        member val QueryId = queryId with get,set
        member val SubjectId = subjectId with get,set
        member val Rank = rank with get,set

    // Returns subject id from BlastHit.
    let getQueryId (bh:BlastHit) =
        bh.QueryId

    // Returns query id from BlastHit.
    let getSubjectId (bh:BlastHit) =
        bh.SubjectId

    // Returns rank from BlastHit.
    let getRank (bh:BlastHit) =
        bh.Rank

    // Returns query length from BlastHit. If not set default is -1.
    let getQueryLength (bh:BlastHit) =
        //bh?``subject length``
        match bh.TryGetTypedPropertyValue<string>("query length") with 
        | Some v -> int v
        | None   -> -1


    // Returns subject length from BlastHit. If not set default is -1.
    let getSubjectLength (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("subject length") with 
        | Some v -> int v
        | None   -> -1


    // Returns alignment length from BlastHit. If not set default is -1.
    let getAlignmentLength (bh:BlastHit) =

        match bh.TryGetTypedPropertyValue<string>("alignment length") with 
        | Some v -> int v
        | None   -> -1


    // Returns number of mismatches from BlastHit. If not set default is -1.
    let getMismatches (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("mismatches") with 
        | Some v -> int v
        | None   -> -1


    // Returns number of identical matches from BlastHit. If not set default is -1.
    let getIdentical (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("identical") with 
        | Some v -> int v
        | None   -> -1


    // Returns number of positive matches from BlastHit. If not set default is -1.
    let getPositives (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("positives") with 
        | Some v -> int v
        | None   -> -1



    // Returns evalue from BlastHit. If not set default is -1.
    let getEValue (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("evalue") with 
        | Some v -> int v
        | None   -> -1

    // Returns bit score from BlastHit. If not set default is -1.
    let getBitScore (bh:BlastHit) =
        match bh.TryGetTypedPropertyValue<string>("bit score") with 
        | Some v -> int v
        | None -> -1


    let lengthSimilarity (bh:BlastHit) =
        let ql = getQueryLength bh |> float
        let sl = getSubjectLength bh |> float
        min ql sl / max ql sl

    let subjectQuerySimilarity (bh:BlastHit) =
        let al = getAlignmentLength bh |> float
        let il = getIdentical bh |> float
        min al il / max al il
