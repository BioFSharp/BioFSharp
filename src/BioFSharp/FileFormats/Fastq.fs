namespace BioFSharp.FileFormats

open System
open FSharpAux
open FSharpAux.IO

module Fastq =

    /// FastqItem record contains header, sequence, qualityheader, qualitysequence of one entry
    type FastqItem<'SequenceItem,'QualitySequenceItem> = {
        Header    : string
        Sequence  : seq<'SequenceItem>
        QualityHeader : string;
        QualitySequence : seq<'QualitySequenceItem>
    } with
        /// Creates FastqItem with header line, sequence, qualityHeader and qualitySequence
        static member inline create (header: string) (sequence: #seq<'SequenceItem>) (qualityHeader:string) (qualitySequence: #seq<'QualitySequenceItem>) : FastqItem<'SequenceItem,'QualitySequenceItem> =
            { Header = header; Sequence = sequence; QualityHeader = qualityHeader; QualitySequence = qualitySequence}

        /// <summary>
        /// Converts the FastaItem to a sequence of lines.
        ///
        /// The header line is prefixed with '>' and the sequence is split into chunks of 80 characters.
        /// </summary>
        /// <param name="sequenceItemConverter">A function to convert sequence items to characters</param>
        /// <param name="fsa">the fasta item to convert</param>
        static member toLines (sequenceItemConverter: 'SequenceItem -> char) (qualitySequenceItemConverter: 'QualitySequenceItem -> char) (fsq:FastqItem<'SequenceItem,'QualitySequenceItem>) =
            let builder = System.Text.StringBuilder()
            seq {
                yield sprintf "@%s" fsq.Header
                fsq.Sequence |> Seq.iter (fun si -> builder.Append(sequenceItemConverter si) |> ignore)
                yield (builder.ToString())
                builder.Clear() |> ignore
                yield sprintf "+%s" fsq.QualityHeader
                fsq.QualitySequence |> Seq.iter (fun qi -> builder.Append(qualitySequenceItemConverter qi) |> ignore)
                yield (builder.ToString())
                builder.Clear() |> ignore
            }

        /// <summary>
        /// Converts the FastaItem to a string.
        ///
        /// The header line is prefixed with '>' and the sequence is split into chunks of 80 characters per line.
        /// </summary>
        /// <param name="sequenceItemConverter">A function to convert sequence items to characters</param>
        /// <param name="fsa">the fasta item to convert</param>
        static member toString (sequenceItemConverter: 'SequenceItem -> char) (qualitySequenceItemConverter: 'QualitySequenceItem -> char) (fsq:FastqItem<'SequenceItem,'QualitySequenceItem>) =
            fsq
            |> FastqItem.toLines sequenceItemConverter qualitySequenceItemConverter
            |> String.concat System.Environment.NewLine