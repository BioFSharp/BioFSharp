module FastqTests

open BioFSharp
open BioFSharp.Nucleotides
open BioFSharp.FileFormats.Fastq
open BioFSharp.IO
open Expecto

let testString_one_entry = """@SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
GGGTGATGGCCGCTGCCGATGGCGTCAAATCCCACCAAGTTACCCTTAACAACTTAAGGGTTTTCAAATAGA
+SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
IIIIIIIIIIIIIIIIIIIIIIIIIIIIII9IG9ICIIIIIIIIIIIIIIIIIIIIDIIIIIII>IIIIII/
"""                         .ReplaceLineEndings()

let testString_two_entries = """@SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
GGGTGATGGCCGCTGCCGATGGCGTCAAATCCCACCAAGTTACCCTTAACAACTTAAGGGTTTTCAAATAGA
+SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
IIIIIIIIIIIIIIIIIIIIIIIIIIIIII9IG9ICIIIIIIIIIIIIIIIIIIIIDIIIIIII>IIIIII/
@SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
GGGTGATGGCCGCTGCCGATGGCGTCAAATCCCACCAAGTTACCCTTAACAACTTAAGGGTTTTCAAATAGA
+SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72
IIIIIIIIIIIIIIIIIIIIIIIIIIIIII9IG9ICIIIIIIIIIIIIIIIIIIIIDIIIIIII>IIIIII/
"""                           .ReplaceLineEndings()

/// get Phred quality score (Phred+33)
let charToPhred33 (s:seq<char>) = s |> Seq.map (fun c -> int c - 33)

/// encode Phred quality score (Phred+33)
let phred33ToChar (q:int) =
    char (q + 33)

let expectedFastqItem =
    FastqItem.create
        "SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72"
        [G;G;G;T;G;A;T;G;G;C;C;G;C;T;G;C;C;G;A;T;G;G;C;G;T;C;A;A;A;T;C;C;C;A;C;C;A;A;G;T;T;A;C;C;C;T;T;A;A;C;A;A;C;T;T;A;A;G;G;G;T;T;T;T;C;A;A;A;T;A;G;A]
        "SRR001666.1 071112_SLXA-EAS1_s_7:5:1:817:345 length=72"
        [40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;24;40;38;24;40;34;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;40;35;40;40;40;40;40;40;40;29;40;40;40;40;40;40;14]

let fastqTests =
    testList "Fastq" [
        test "readLines" {
            let actual = testString_one_entry.Split(System.Environment.NewLine) |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.item 0
            let expected = expectedFastqItem
            Expect.equal actual.Header expected.Header "Header was not equal"
            Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
        }
        test "toLines" {
            let actual = expectedFastqItem |> FastqItem.toLines BioItem.symbol phred33ToChar
            let expected = testString_one_entry.Trim().Split(System.Environment.NewLine) // trim because lines will have no EOL at the end
            Expect.sequenceEqual actual expected "fastqitem string was not correctly formatted"
        }
        test "read" {
            Expect.isTrue (System.IO.File.Exists("./fixtures/one_entry.fastq")) ""
            let actual = Fastq.read BioArray.ofNucleotideString charToPhred33 "./fixtures/one_entry.fastq" |> Seq.item 0
            let expected = expectedFastqItem
            Expect.equal actual.Header expected.Header "Header was not equal"
            Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
        }
        //test "write" {
        //    let testOutPath = "./fixtures/one_entry_tmp.fastq"
        //    System.IO.File.Delete(testOutPath)
        //    Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
        //    Fastq.write BioItem.symbol phred33ToChar testOutPath [expectedfastqItem]
        //    Expect.isTrue (System.IO.File.Exists(testOutPath)) "file was not written"
        //    let actual = System.IO.File.ReadAllText(testOutPath)
        //    let expected = testString_one_entry
        //    Expect.equal actual expected "fastqitem string was not correctly formatted"
        //    System.IO.File.Delete(testOutPath)
        //    Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
        //}
        test "readLines (2 entries)" {
            let actual = testString_two_entries.Split(System.Environment.NewLine) |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
            let expected = expectedFastqItem
            let item0 = actual |> Seq.item 0
            let item1 = actual |> Seq.item 1
            Expect.equal item0.Header expected.Header "Header was not equal (entry 0)"
            Expect.sequenceEqual item0.Sequence expected.Sequence "Sequence was not equal (entry 0)"
            Expect.equal item1.Header expected.Header "Header was not equal (entry 1)"
            Expect.sequenceEqual item1.Sequence expected.Sequence "Sequence was not equal (entry 1)"
        }
        test "read (2 entries)" {
            Expect.isTrue (System.IO.File.Exists("./fixtures/two_entries.fastq")) ""
            let actual = Fastq.read BioArray.ofNucleotideString charToPhred33 "./fixtures/two_entries.fastq"
            let expected = expectedFastqItem
            let item0 = actual |> Seq.item 0
            let item1 = actual |> Seq.item 1
            Expect.equal item0.Header expected.Header "Header was not equal (entry 0)"
            Expect.sequenceEqual item0.Sequence expected.Sequence "Sequence was not equal (entry 0)"
            Expect.equal item1.Header expected.Header "Header was not equal (entry 1)"
            Expect.sequenceEqual item1.Sequence expected.Sequence "Sequence was not equal (entry 1)"
        }
        //test "write (2 entries)" {
        //    let testOutPath = "./fixtures/two_entries_tmp.fastq"
        //    System.IO.File.Delete(testOutPath)
        //    Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
        //    Fastq.write BioItem.symbol testOutPath [expectedfastqItem; expectedfastqItem]
        //    Expect.isTrue (System.IO.File.Exists(testOutPath)) "file was not written"
        //    let actual = System.IO.File.ReadAllText(testOutPath)
        //    let expected = testString_two_entries
        //    Expect.equal actual expected "fastqitem string was not correctly formatted"
        //    System.IO.File.Delete(testOutPath)
        //    Expect.isFalse (System.IO.File.Exists(testOutPath)) "test output not clean"
        //}
        
    ]