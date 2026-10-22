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

        // Raises an error that includes the line number where parsing failed
        let fail (lineNumber: int) (message: string) : 'T =
            raise (InvalidDataException (sprintf "Incorrect FASTQ format (line %i): %s" lineNumber message))

        seq {
            use enumerator = lines.GetEnumerator()
            let lineNumber = ref 0

            // Returns the next line without whitespace or None at the end
            let nextLine () =
                if enumerator.MoveNext() then
                    lineNumber.Value <- lineNumber.Value + 1
                    Some (enumerator.Current.TrimEnd())
                else
                    None

            // Skips blank lines
            let rec nextHeader () =
                match nextLine () with
                | Some "" -> nextHeader ()
                | other   -> other

            let finished = ref false
            while not finished.Value do
                match nextHeader () with
                | None -> finished.Value <- true
                | Some headerLine ->
                    let recordStart = lineNumber.Value
                    if not (headerLine.StartsWith "@") then
                        fail recordStart "the header did not start with '@'"

                    // Require at least one sequence line, even for an empty read.
                    let sequence = StringBuilder()
                    let rec readSequence hasSequenceLine =
                        match nextLine () with
                        | None -> fail lineNumber.Value (sprintf "record starting at line %i ended before the '+' seperator" recordStart)
                        | Some l when l.StartsWith "+" ->
                            if not hasSequenceLine then
                                fail lineNumber.Value (sprintf "record starting at line %i is missing its sequence line before the '+' separator" recordStart)
                            l.Substring 1
                        | Some l ->
                            sequence.Append l |> ignore
                            readSequence true
                    let qualityHeader = readSequence false

                    // Consume at least one quality line before yielding, even for an empty read.
                    let quality = StringBuilder()
                    let mutable hasQualityLine = false
                    while not hasQualityLine || quality.Length < sequence.Length do
                        match nextLine () with
                        | None when not hasQualityLine -> fail lineNumber.Value (sprintf "record starting at line %i ended before its quality line" recordStart)
                        | None -> fail lineNumber.Value (sprintf "record starting at line %i ended before its quality string was complete (%i of %i characters)" recordStart quality.Length sequence.Length)
                        | Some l ->
                            quality.Append l |> ignore
                            hasQualityLine <- true
                    if quality.Length <> sequence.Length then
                        fail lineNumber.Value (sprintf "record starting at line %i has a quality string of length %i but a sequence of length %i" recordStart quality.Length sequence.Length)

                    // Matches grouped lines and aggregates to a fastq record
                    yield FastqItem.create
                        (headerLine.Substring 1)
                        (sequenceConverter (sequence.ToString()))
                        qualityHeader
                        (qualitySequenceConverter (quality.ToString()))
        }
          
    /// Reads FastqItem from FastQ format file. Converter and qualityConverter determines type of sequence by converting seq<char> -> type
    let read converter qualityConverter filePath =
        FileIO.readFile filePath
        |> readLines converter qualityConverter

    /// Reads FastqItem from GZip format file. Converter and qualityConverter determines type of sequence by converting seq<char> -> type
    let readGZip converter qualityConverter filePath =
        FileIO.readFileGZip filePath
        |> readLines converter qualityConverter
        