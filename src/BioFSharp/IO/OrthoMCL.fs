namespace BioFSharp.IO

open BioFSharp.FileFormats
open FSharpAux.IO

module OrthoMCL =

    let read (filePath: string) =
        let csvReader = SchemaReader.Csv.CsvReader<OrthoMCL>(SchemaMode=SchemaReader.Csv.Exact)
        csvReader.ReadFile(filePath,'\t',false)
