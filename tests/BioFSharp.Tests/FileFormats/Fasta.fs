namespace BioFSharp.Tests.FileFormats

module FastaTests =

    open BioFSharp
    open BioFSharp.FileFormats.Fasta

    open BioFSharp.Tests.ReferenceObjects.Fasta
    open Expecto

    let fileFormatTests =
        testList "Fasta" [
            test "toLines" {
                let actual = expectedFastaItem |> FastaItem.toLines BioItem.symbol
                let expected = testString_one_entry.Trim().Split(System.Environment.NewLine) // trim because lines will have no EOL at the end
                Expect.sequenceEqual actual expected "Fastaitem string was not correctly formatted"
            }
            test "toString" {
                let actual = expectedFastaItem |> FastaItem.toString BioItem.symbol
                let expected = testString_one_entry.Trim() // trim because lines will have no EOL at the end
                Expect.sequenceEqual actual expected "Fastaitem string was not correctly formatted"
            }
        ]