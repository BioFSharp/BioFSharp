namespace BioFSharp.IO

open System.Collections.Generic
open System.IO
open System.Text
open BioFSharp.FileFormats.FastaIndex

/// Functions to read and write FASTA indexfiles
module FastaIndex =

    /// One line of a file as bytes
    type private ByteLine = {
        /// Byte position of the first byte of the line in the file
        Start    : int64
        /// Bytes of the line without the final \n
        Content  : byte []
        /// False for the last line of a file that does not end with \n
        HasBreak : bool
    }

    /// The sequence that is currently scanned
    type private ScanState = {
        Name      : string
        Offset    : int64
        Length    : int64
        LineBases : int64
        LineWidth : int64
        /// Number of sequence lines read so far
        Lines     : int
        /// Only blank lines follow until the next header
        Ended     : bool
    }

    let private readByteLines (path: string) : seq<ByteLine> =
        seq {
            use stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536)
            let position = ref 0L
            let content = ResizeArray<byte>()
            let finished = ref false
            while not finished.Value do
                content.Clear()
                let mutable hasBreak = false
                let mutable reading = true
                while reading do
                    let b = stream.ReadByte()
                    if b = -1 then reading <- false
                    elif b = int '\n' then
                        hasBreak <- true
                        reading <- false
                    else content.Add(byte b)
                if content.Count = 0 && not hasBreak then
                    finished.Value <- true
                else
                    let line = { Start = position.Value; Content = content.ToArray(); HasBreak = hasBreak }
                    position.Value <- line.Start + int64 line.Content.Length + (if hasBreak then 1L else 0L)
                    yield line
        }

    let private isSpace (b: byte) =
        b = byte ' ' || b = byte '\t' || b = byte '\r' || b = 11uy || b = 12uy

    let private isBase (b: byte) =
        b > byte ' ' && b < 127uy

    let private parseName (content: byte []) : string =
        let start =
            seq { 1 .. content.Length - 1 }
            |> Seq.tryFind (fun i -> not (isSpace content.[i]))
            |> Option.defaultValue content.Length
        let stop =
            seq { start .. content.Length - 1 }
            |> Seq.tryFind (fun i -> isSpace content.[i])
            |> Option.defaultValue content.Length
        Encoding.UTF8.GetString(content, start, stop - start)

    /// Creates the index of the given FASTA file by reading it once as bytes. The result can be saved with FastaIndex.write
    /// Fails if a line is longer than the first line of its sequence, sequence data follows a shorter line, the last sequence is empty, or the file contains no sequence
    /// Empty sequences are left out and if the names are the same, only the first one is kept
    let create (fastaPath: string) : FastaIndexEntry [] =
        let entries = ResizeArray<FastaIndexEntry>()
        let names = HashSet<string>()

        let fail (lineNumber: int) (message: string) : 'T =
            raise (InvalidDataException (sprintf "Incorrect FASTA format for indexing (line %i): %s" lineNumber message))

        let finish (state: ScanState) =
            if state.Lines > 0 && names.Add state.Name then
                entries.Add(FastaIndexEntry.create state.Name state.Length state.Offset state.LineBases state.LineWidth)

        let processLine (current: ScanState option) (lineNumber: int, line: ByteLine) : ScanState option =
            let content = line.Content
            let width = int64 content.Length + 1L
            let bases = content |> Array.sumBy (fun b -> if isBase b then 1L else 0L)
            let isBlank = content.Length = 0 || (content.Length = 1 && content.[0] = byte '\r' && line.HasBreak)
            if content.Length = 1 && content.[0] = byte '>' && not line.HasBreak then
                current
            elif content.Length > 0 && content.[0] = byte '>' then
                current |> Option.iter finish
                Some {
                    Name = parseName content
                    Offset = line.Start + width
                    Length = 0L
                    LineBases = 0L
                    LineWidth = 0L
                    Lines = 0
                    Ended = false
                }
            else
                match current with
                | None ->
                    if isBlank then None
                    else fail lineNumber "sequence data before the first header"
                | Some state when state.Ended ->
                    if isBlank then current
                    else fail lineNumber (sprintf "sequence '%s' continues after a shorter line or a blank line" state.Name)
                | Some state when state.Lines = 0 ->
                    if content.Length = 0 then Some { state with Ended = true }
                    else Some { state with Length = bases; LineBases = bases; LineWidth = width; Lines = 1 }
                | Some state ->
                    if width = state.LineWidth then
                        Some { state with Length = state.Length + bases; Lines = state.Lines + 1 }
                    elif width > state.LineWidth then
                        fail lineNumber (sprintf "line is longer than the first line of sequence '%s'" state.Name)
                    else
                        Some { state with Length = state.Length + bases; Lines = state.Lines + 1; Ended = true }

        let last =
            readByteLines fastaPath
            |> Seq.mapi (fun i line -> i + 1, line)
            |> Seq.fold processLine None

        match last with
        | Some state when state.Lines = 0 ->
            raise (InvalidDataException (sprintf "Incorrect FASTA format for indexing: the last sequence '%s' is empty" state.Name))
        | Some state -> finish state
        | None -> ()

        if entries.Count = 0 then
            raise (InvalidDataException "Incorrect FASTA format for indexing: the file contains no sequence")

        entries.ToArray()

