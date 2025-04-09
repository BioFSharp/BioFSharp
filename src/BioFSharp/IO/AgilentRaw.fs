namespace BioFSharp.IO

open FSharpAux.IO.SchemaReader
open FSharpAux.IO.SchemaReader.Csv
open BioFSharp.FileFormats.AgilentRaw

module AgilentRaw =

    /// Reads agilent raw data from file
    let read (filePath: string) =
        let reader = new CsvReader<AgilentDataRaw>(SchemaMode = Csv.Fill)
        reader.ReadFile(filePath, '\t', true,SkipLinesBeforeHeader = 9)