namespace BioFSharp.IO

open FSharpAux
open FSharpAux.IO
open BioFSharp.FileFormats.Fastq
open System.IO
open System.Text
open System.Collections.Generic

/// Functions to read and write fastq formatted files
module Fastq =

    /// Maps each lines from an entry to FastqItem
    let readLines (sequenceConverter: seq<char>-> #seq<'SequenceItem>) (qualitySequenceConverter: seq<char>-> #seq<'QualitySequenceItem>) (lines: seq<string>) : seq<FastqItem<'SequenceItem,'QualitySequenceItem>> =

        // Conditon of grouping lines
        let same_group l =             
            not (String.length l = 0 || l.[0] <> '@')

        // Matches grouped lines and aggregates to a single fasta record
        let parseRecord (l: string list) = 
            match l with
            | [] -> raise (System.IO.InvalidDataException "Incorrect FASTQ format: input was empty")
            | (h:string) :: (s:string) :: (qh:string) :: (qs:string) :: _  ->
                if not (h.StartsWith("@")) then raise (System.IO.InvalidDataException "Incorrect FASTQ format: record header did start with '@'")
                if not (qh.StartsWith("+")) then raise (System.IO.InvalidDataException "Incorrect FASTQ format: quality header did start with '+'")
                let header = h.Remove(0,1)
                let sequence = sequenceConverter s
                let qualityHeader = qh.Remove(0,1)
                let qualitySequence = qualitySequenceConverter qs
                FastqItem.create header sequence qualityHeader qualitySequence
            | _ -> raise (System.IO.InvalidDataException "Incorrect FASTQ format: record did not consist of 4 lines")
    
        lines
        |> Seq.filter (fun (l:string) -> not (l.StartsWith ";" || l.StartsWith "#"))
        |> Seq.groupWhen same_group 
        |> Seq.map (fun l -> parseRecord (List.ofSeq l))
          
    /// Reads FastqItem from FastQ format file. Converter and qualityConverter determines type of sequence by converting seq<char> -> type
    let read converter qualityConverter filePath =
        FileIO.readFile filePath
        |> readLines converter qualityConverter

    /// Reads FastqItem from GZip format file. Converter and qualityConverter determines type of sequence by converting seq<char> -> type
    let readGZip converter qualityConverter filePath =
        FileIO.readFileGZip filePath
        |> readLines converter qualityConverter
