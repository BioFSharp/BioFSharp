namespace BioFSharp.IO

open FSharpAux
open FSharpAux.IO
open BioFSharp.FileFormats.Fasta
open System.IO
open System.Text

/// Functions to read and write fasta formatted files
module Fasta =

    // Conditon of grouping lines
    let private same_group l =             
        not (String.length l = 0 || l.[0] <> '>')
    
    /// <summary>
    /// Creates a sequence of FastaItems by parsing the input line per line.
    ///
    /// The passed converter function is used to convert the sequence of each record to the desired type.
    ///
    /// Lines starting with '#' or ';' are ignored.
    /// </summary>
    /// <param name="sequenceConverter">Function to convert the sequence of each record to the desired type</param>
    /// <param name="lines">The input to parse in the form of strings each representing a line of a fasta formatted file</param>
    /// <returns>Sequence of FastaItems</returns>
    /// <exception cref="System.IO.InvalidDataException">If the input is not in the correct fasta format</exception>
    let readLines (sequenceConverter: seq<char>-> #seq<'SequenceItem>) (lines: seq<string>) : seq<FastaItem<'SequenceItem>> =
        // Matches grouped lines and aggregates to a single fasta record
        let parseRecord (l: string list) = 
            match l with
            | [] -> raise (System.IO.InvalidDataException "Incorrect FASTA format: no header line starting with '>' in the input.")
            | (h:string) :: t when h.StartsWith ">" ->
                let header = h.Remove(0,1)
                let sequence = (Seq.concat t) |> sequenceConverter
                FastaItem.create header sequence
            | h :: _ -> raise (System.IO.InvalidDataException "Incorrect FASTA format: no header line starting with '>' in the input.")
    
        lines
        |> Seq.filter (fun (l:string) -> not (l.StartsWith ";" || l.StartsWith "#"))
        |> Seq.groupWhen same_group 
        |> Seq.map (fun l -> parseRecord (List.ofSeq l))

    /// <summary>
    /// Creates a sequence of FastaItems by parsing the input line per line.
    ///
    /// The passed converter function is used to convert the sequence of each record to the desired type.
    ///
    /// Lines starting with '#' or ';' are ignored.
    /// </summary>
    /// <param name="sequenceConverter">Function to convert the sequence of each record to the desired type</param>
    /// <param name="filePath">Path to a fasta formatted file</param>
    /// <returns>Sequence of FastaItems</returns>
    /// <exception cref="System.IO.InvalidDataException">If the input is not in the correct fasta format</exception>
    let read (sequenceConverter: seq<char>-> #seq<'SequenceItem>) (filePath: string) =
        FileIO.readFile filePath
        |> readLines sequenceConverter

    /// <summary>
    /// Creates a sequence of FastaItems by parsing the input line per line.
    ///
    /// The passed converter function is used to convert the sequence of each record to the desired type.
    ///
    /// Lines starting with '#' or ';' are ignored.
    /// </summary>
    /// <param name="sequenceConverter">Function to convert the sequence of each record to the desired type</param>
    /// <param name="filePath">Path to a gzip compressed fasta formatted file</param>
    /// <returns>Sequence of FastaItems</returns>
    /// <exception cref="System.IO.InvalidDataException">If the input is not in the correct fasta format</exception>
    let readGzip (sequenceConverter: seq<char>-> #seq<'SequenceItem>) (filePath: string) =
        FileIO.readFileGZip filePath
        |> readLines sequenceConverter

    /// <summary>
    /// Writes a sequence of FastaItem to a stream.
    ///
    /// The passed converter function is used to convert each item in the sequence of each record to the desired type.
    ///
    /// The passed stream stays open and is not disposed after writing to it.
    /// If you want to reuse the stream (e.g. you are not writing to a file stream but a memory stream that gets used afterwards)
    /// you have to reset the position with `stream.Seek(0L, SeekOrigin.Begin)`
    /// </summary>
    /// <param name="sequenceItemConverter">Function to convert each item of the sequence of each record to the desired type</param>
    /// <param name="stream">The stream to write to</param>
    /// <param name="data">A sequence of fasta items to write to the input stream</param>
    let writeToStream (sequenceItemConverter: 'SequenceItem -> char) (stream:Stream) (data:seq<FastaItem<'SequenceItem>>) =
        let toChunks (w:System.IO.StreamWriter) (length:int) (source: seq<'SequenceItem>) =    
            use ie = source.GetEnumerator()
            let sourceIsEmpty = ref false
            let builder = System.Text.StringBuilder(length)
            let rec loop () = 
                if ie.MoveNext () then 
                    builder.Append(sequenceItemConverter ie.Current) |> ignore
                    for x in 2 .. length do
                        if ie.MoveNext() then
                            builder.Append(sequenceItemConverter ie.Current) |> ignore
                        else
                            sourceIsEmpty.Value <- true                
                
                    match sourceIsEmpty.Value with
                    | false -> // writer builder
                        w.WriteLine(builder.ToString())
                        builder.Clear() |> ignore
                        loop ()
                    | true  -> 
                        w.WriteLine(builder.ToString())
                        ()
                else
                    w.Flush()
            loop ()

        use sWriter = new System.IO.StreamWriter(stream,UTF8Encoding(false,true),4096,true)
        data
        |> Seq.iter (fun (i:FastaItem<_>) ->
            sWriter.WriteLine(">" + i.Header)
            toChunks sWriter 80 i.Sequence
        ) 

    /// <summary>
    /// Writes a sequence of FastaItem to a stream.
    ///
    /// The passed converter function is used to convert each item in the sequence of each record to the desired type.
    ///
    /// If the file already exists, it is overwritten.
    /// </summary>
    /// <param name="sequenceItemConverter">Function to convert each item of the sequence of each record to the desired type</param>
    /// <param name="filePath">The file to write to</param>
    /// <param name="data">A sequence of fasta items to write to the input file path</param>
    let write (sequenceItemConverter: 'SequenceItem -> char) (filePath:string) (data:seq<FastaItem<'SequenceItem>>) =
        let file = new FileStream(filePath,FileMode.Create)
        writeToStream sequenceItemConverter file data
        file.Dispose()

    // translate this from C# docs
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream.write?view=net-8.0
    //let writeGZip (sequenceItemConverter: 'SequenceItem -> char) (filePath:string) (data:seq<FastaItem<'SequenceItem>>) = ...

    /// <summary>
    /// Writes a sequence of FastaItem to a stream.
    ///
    /// The passed converter function is used to convert each item in the sequence of each record to the desired type.
    ///
    /// If the file already exists, it is appended.
    /// </summary>
    /// <param name="sequenceItemConverter">Function to convert each item of the sequence of each record to the desired type</param>
    /// <param name="filePath">The file to write to</param>
    /// <param name="data">A sequence of fasta items to write to the input file path</param>
    let append (toString:'T -> char) (filePath:string) (data:seq<FastaItem<#seq<'T>>>) =
        let file = new FileStream(filePath,FileMode.Append)
        writeToStream toString file data   
        file.Dispose()

    // translate this from C# docs
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream.write?view=net-8.0
    //let appendGZip (sequenceItemConverter: 'SequenceItem -> char) (filePath:string) (data:seq<FastaItem<'SequenceItem>>) = ...