namespace BioFSharp.IO

open BioFSharp.FileFormats
open BioFSharp.CLIArgs.Blast
open FSharpAux.IO

module CustomBlastResult =

    let read (cAttributes:seq<OutputCustom>) separator filePath =
        let headerLine = cAttributes |> Seq.map OutputCustom.toCLIString |> String.concat (separator.ToString())
        let csvReader = SchemaReader.Csv.CsvReader<CustomBlastResult>(SchemaMode=SchemaReader.Csv.Fill)
        csvReader.ReadFile(filePath,separator,headerLine)
