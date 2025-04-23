namespace BioFSharp.Tests.IO

module FastaTests =

    open BioFSharp
    open BioFSharp.FileFormats.Fasta
    open BioFSharp.IO

    open BioFSharp.Tests.ReferenceObjects.Fasta
    open Expecto

    let IOTests =
        testList "Fasta" [
            test "readLines" {
                let actual = testString_one_entry.Split(System.Environment.NewLine) |> Fasta.readLines BioArray.ofAminoAcidString |> Seq.item 0
                let expected = expectedFastaItem
                Expect.equal actual.Header expected.Header "Header was not equal"
                Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
            }
            test "read" {
                Expect.isTrue (System.IO.File.Exists("./fixtures/one_entry.fasta")) ""
                let actual = Fasta.read BioArray.ofAminoAcidString "./fixtures/one_entry.fasta" |> Seq.item 0
                let expected = expectedFastaItem
                Expect.equal actual.Header expected.Header "Header was not equal"
                Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
            }
            test "write" {
                let testOutPath = "./fixtures/one_entry_tmp.fasta"
                System.IO.File.Delete(testOutPath)
                Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
                Fasta.write BioItem.symbol testOutPath [expectedFastaItem]
                Expect.isTrue (System.IO.File.Exists(testOutPath)) "file was not written"
                let actual = System.IO.File.ReadAllText(testOutPath)
                let expected = testString_one_entry
                Expect.equal actual expected "Fastaitem string was not correctly formatted"
                System.IO.File.Delete(testOutPath)
                Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
            }
            test "readLines (2 entries)" {
                let actual = testString_two_entries.Split(System.Environment.NewLine) |> Fasta.readLines BioArray.ofAminoAcidString
                let expected = expectedFastaItem
                let item0 = actual |> Seq.item 0
                let item1 = actual |> Seq.item 1
                Expect.equal item0.Header expected.Header "Header was not equal (entry 0)"
                Expect.sequenceEqual item0.Sequence expected.Sequence "Sequence was not equal (entry 0)"
                Expect.equal item1.Header expected.Header "Header was not equal (entry 1)"
                Expect.sequenceEqual item1.Sequence expected.Sequence "Sequence was not equal (entry 1)"
            }
            test "read (2 entries)" {
                Expect.isTrue (System.IO.File.Exists("./fixtures/two_entries.fasta")) ""
                let actual = Fasta.read BioArray.ofAminoAcidString "./fixtures/two_entries.fasta"
                let expected = expectedFastaItem
                let item0 = actual |> Seq.item 0
                let item1 = actual |> Seq.item 1
                Expect.equal item0.Header expected.Header "Header was not equal (entry 0)"
                Expect.sequenceEqual item0.Sequence expected.Sequence "Sequence was not equal (entry 0)"
                Expect.equal item1.Header expected.Header "Header was not equal (entry 1)"
                Expect.sequenceEqual item1.Sequence expected.Sequence "Sequence was not equal (entry 1)"
            }
            test "write (2 entries)" {
                let testOutPath = "./fixtures/two_entries_tmp.fasta"
                System.IO.File.Delete(testOutPath)
                Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
                Fasta.write BioItem.symbol testOutPath [expectedFastaItem; expectedFastaItem]
                Expect.isTrue (System.IO.File.Exists(testOutPath)) "file was not written"
                let actual = System.IO.File.ReadAllText(testOutPath)
                let expected = testString_two_entries
                Expect.equal actual expected "Fastaitem string was not correctly formatted"
                System.IO.File.Delete(testOutPath)
                Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
            }
        
        ]