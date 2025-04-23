namespace BioFSharp.Tests.FileFormats

module FastqTests =

    open BioFSharp
    open BioFSharp.FileFormats.Fastq

    open BioFSharp.Tests.ReferenceObjects.Fastq
    open Expecto

    let fileFormatTests =
        testList "Fastq" [
            test "toLines" {
                let actual = expectedFastqItem |> FastqItem.toLines BioItem.symbol phred33ToChar
                let expected = testString_one_entry.Trim().Split(System.Environment.NewLine) // trim because lines will have no EOL at the end
                Expect.sequenceEqual actual expected "fastqitem string was not correctly formatted"
            }
            test "toString" {
                let actual = expectedFastqItem |> FastqItem.toString BioItem.symbol phred33ToChar
                let expected = testString_one_entry.Trim() // trim because lines will have no EOL at the end
                Expect.sequenceEqual actual expected "Fastaitem string was not correctly formatted"
            }
        ]