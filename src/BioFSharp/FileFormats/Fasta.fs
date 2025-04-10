namespace BioFSharp.FileFormats

open BioFSharp

module Fasta =

    /// A FastaItem represents a sequence record in a fasta formatted file, consisting of header line and sequence
    type FastaItem<'SequenceItem> = {
        Header    : string
        Sequence  : seq<'SequenceItem>
    } with
        /// <summary>
        /// Creates a fasta item representing a single sequence record in a fasta formatted file.
        /// </summary>
        /// <param name="header">The header line of the sequence record</param>
        /// <param name="sequence">The sequence</param>
        static member inline create (header: string) (sequence: #seq<'SequenceItem>) =
            { Header = header; Sequence = sequence }

        /// <summary>
        /// Converts the FastaItem to a tagged sequence.
        ///
        /// The header line is converted to a tag type of choice using the provided converter function.
        /// </summary>
        /// <param name="headerConverter">A function to convert header to tag</param>
        /// <param name="fsa">the fasta item to convert</param>
        static member inline toTaggedSequence (headerConverter: string -> 'Tag) (fsa:FastaItem<'SequenceItem>) : TaggedSequence<'Tag, 'SequenceItem>=
            BioFSharp.TaggedSequence.create (headerConverter fsa.Header) fsa.Sequence

        /// <summary>
        /// Creates a fasta item from a tagged sequence.
        /// </summary>
        /// <param name="tagConverter"></param>
        /// <param name="taggedSeq">the tagged sequence to convert</param>
        static member inline ofTaggedSequence (tagConverter: 'Tag -> string) (sequenceItemConverter: 'TaggedSequenceItem -> 'SequenceItem) (taggedSeq:TaggedSequence<'Tag,'TaggedSequenceItem>): FastaItem<'SequenceItem> =
            FastaItem.create
                (tagConverter taggedSeq.Tag)
                (taggedSeq.Sequence |> Seq.map sequenceItemConverter)

        /// <summary>
        /// Converts the FastaItem to a sequence of lines.
        ///
        /// The header line is prefixed with '>' and the sequence is split into chunks of 80 characters.
        /// </summary>
        /// <param name="sequenceItemConverter">A function to convert sequence items to characters</param>
        /// <param name="fsa">the fasta item to convert</param>
        static member toLines (sequenceItemConverter: 'SequenceItem -> char) (fsa:FastaItem<'SequenceItem>) =
            let toChunks (length:int) (source: seq<'SequenceItem>) (head:string)=    
                let ie = source.GetEnumerator()
                let mutable sourceIsEmpty = ref false
                let builder = System.Text.StringBuilder(length)        
                seq {
                    yield sprintf ">%s" head
                    while ie.MoveNext () do                
                        builder.Append(sequenceItemConverter ie.Current) |> ignore
                        for x in 2 .. length do
                            if ie.MoveNext() then
                                builder.Append(sequenceItemConverter ie.Current) |> ignore
                            else
                                sourceIsEmpty.Value <- true                
            
                        match sourceIsEmpty.Value with
                        | false -> // writer builder
                                    yield (builder.ToString())
                                    builder.Clear() |> ignore
                        | true  -> yield (builder.ToString())
                    }
            toChunks 80 fsa.Sequence fsa.Header

        /// <summary>
        /// Converts the FastaItem to a string.
        ///
        /// The header line is prefixed with '>' and the sequence is split into chunks of 80 characters per line.
        /// </summary>
        /// <param name="sequenceItemConverter">A function to convert sequence items to characters</param>
        /// <param name="fsa">the fasta item to convert</param>
        static member toString (sequenceItemConverter: 'SequenceItem -> char) (fsa:FastaItem<'SequenceItem>) =
            fsa
            |> FastaItem.toLines sequenceItemConverter
            |> String.concat System.Environment.NewLine
