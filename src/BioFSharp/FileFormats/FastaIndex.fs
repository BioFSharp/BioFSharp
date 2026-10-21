namespace BioFSharp.FileFormats

open System
open System.Globalization
open System.IO

module FastaIndex =

    /// One line of a FASTA index as created by samtools faidx
    type FastaIndexEntry = {
        /// Name of the sequence: the header up to the first whitespace
        Name      : string
        /// Number of bases of the sequence
        Length    : int64
        /// Byte offset of the first base of the sequence in the FASTA file
        Offset    : int64
        /// Number of bases per line
        LineBases : int64
        /// Number of bytes per line, including the line break
        LineWidth : int64
    } with
        /// Creates a FastaIndexEntry from the given values
        static member create (name: string) (length: int64) (offset: int64) (lineBases: int64) (lineWidth: int64) : FastaIndexEntry =
            { Name = name; Length = length; Offset = offset; LineBases = lineBases; LineWidth = lineWidth }

        /// Returns the separated .fai line
        static member toLine (entry: FastaIndexEntry) : string =
            sprintf "%s\t%i\t%i\t%i\t%i" entry.Name entry.Length entry.Offset entry.LineBases entry.LineWidth

        /// Returns the entry of the .fai line
        static member ofLine (line: string) : FastaIndexEntry =
            let parse (column: string) (value: string) =
                match Int64.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture) with
                | true, number -> number
                | false, _ -> raise (InvalidDataException (sprintf "Incorrect .fai format: %s '%s' is not a non-negative integer in line '%s'" column value line))
            match line.Split('\t') with
            | [| name; length; offset; lineBases; lineWidth |] when name <> "" ->
                FastaIndexEntry.create
                    name
                    (parse "LENGTH" length)
                    (parse "OFFSET" offset)
                    (parse "LINEBASES" lineBases)
                    (parse "LINEWIDTH" lineWidth)
            | _ ->
                raise (InvalidDataException (sprintf "Incorrect .fai format: expected 5 tab-separated columns with a name in line '%s'" line))
                