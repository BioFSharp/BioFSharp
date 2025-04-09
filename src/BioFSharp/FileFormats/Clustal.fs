namespace BioFSharp.FileFormats

open BioFSharp

///Contains functions for reading clustal alignment files
module Clustal = 

    //Header of file and info on conservation of sequences
    type AlignmentInfo = {Header : seq<char>; ConservationInfo : seq<char>}

    ///Checks if the header of a parsed clustal alignment matches the clustal file conventions
    let hasClustalFileHeader (alignment:Alignment.Alignment<TaggedSequence<string,char>,AlignmentInfo>) = 
        let en = alignment.MetaData.Header.GetEnumerator()
        let rec loop i =
            match en.MoveNext() with
            | false -> 
                false
            | true ->      
                let symbol = en.Current
                if symbol = "CLUSTAL".[i] || symbol = "clustal".[i] then 
                    if i = 6 then 
                        true
                    else 
                        loop (i+1)
                else 
                    false     
        loop 0