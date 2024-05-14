namespace BioFSharp.Interactive

module Formatters =

    open System
    open Microsoft.DotNet.Interactive.Formatting
    open BioFSharp
    open BioFSharp.IO

    let formatAminoAcid (item: AminoAcids.AminoAcid) (writer: IO.TextWriter) =
        let pretty = FSIPrinters.prettyPrintBioItem item
        writer.Write(pretty)

    let formatBioCollection (bioCollection: seq<IBioItem>) (writer: IO.TextWriter) =
        let pretty = $"<pre>{FSIPrinters.prettyPrintBioCollection bioCollection}</pre>"
        writer.Write(pretty)

    let registerAll() =
        Formatter.Register<AminoAcids.AminoAcid>(
            formatAminoAcid,
            "text/html"
        )
        Formatter.Register<seq<IBioItem>>(
            formatBioCollection,
            "text/html"
        )