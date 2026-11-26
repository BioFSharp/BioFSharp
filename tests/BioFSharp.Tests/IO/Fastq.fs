namespace BioFSharp.Tests.IO

module FastqTests =

    open BioFSharp
    open BioFSharp.Nucleotides
    open BioFSharp.FileFormats.Fastq
    open BioFSharp.IO

    open BioFSharp.Tests.ReferenceObjects.Fastq
    open Expecto

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

    let IOTests =
        testList "Fastq" [
            test "readLines" {
                let actual = testString_one_entry.Split(System.Environment.NewLine) |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.item 0
                let expected = expectedFastqItem
                Expect.equal actual.Header expected.Header "Header was not equal"
                Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
            }
            test "read" {
                Expect.isTrue (System.IO.File.Exists("./fixtures/one_entry.fastq")) ""
                let actual = Fastq.read BioArray.ofNucleotideString charToPhred33 "./fixtures/one_entry.fastq" |> Seq.item 0
                let expected = expectedFastqItem
                Expect.equal actual.Header expected.Header "Header was not equal"
                Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
            }
            ptest "write" {
                ()
            }
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
            ptest "write (2 entries)" {
                ()
            }
            test "readLines with # and ; quality characters" {
                // FASTQ string that starts with # and ;
                let mockFastq = "@read1\nACGT\n+\n#;II"
                let lines = mockFastq.Split('\n')

                // Pass the lines into the parser
                let actual = 
                    lines 
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 
                    |> Seq.item 0

                // The parser shouldn't strip the quality line and should correctly parse the scores.
                // ASCII: # = 35, ; = 59, I = 73. 
                // Phred+33: 35-33=2, 59-33=26, 73-33=40.
                let expectedQual = [2; 26; 40; 40]
                
                Expect.equal actual.Header "read1" "Header was not equal"
                Expect.sequenceEqual actual.QualitySequence expectedQual "Quality sequence stripped or incorrect"
            }
            test "readLines starting with '@', '#', ';' and '+'" {
                let actual =
                    "@read1\nACGT\n+\n@III\n@read2\nACGT\n+\n#III\n@read3\nACGT\n+read3\n;III\n@read4\nACGT\n+\n+III".Split('\n')
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> List.ofSeq
                Expect.equal (actual |> List.map (fun r -> r.Header)) ["read1"; "read2"; "read3"; "read4"] "All four records should be parsed"
                Expect.sequenceEqual actual[0].QualitySequence [31; 40; 40; 40] "'@' is Phred+33 score 31"
                Expect.sequenceEqual actual[1].QualitySequence [2; 40; 40; 40] "'#' is Phred+33 score 2"
                Expect.sequenceEqual actual[2].QualitySequence [26; 40; 40; 40] "';' is Phred+33 score 26"
                Expect.sequenceEqual actual[3].QualitySequence [10; 40; 40; 40] "'+' is Phred+33 score 10"
                Expect.equal actual[2].QualityHeader "read3" "Quality header should be kept"
            }
            test "readLines with wrapped records" {
                let actual =
                    "@wrapped1\nACG\nTAC\n+\n@@I\nIII\n@wrapped2\nGG\n+\nII".Split('\n')
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> List.ofSeq
                Expect.equal actual.Length 2 "Two records should be parsed"
                Expect.sequenceEqual actual[0].Sequence [A; C; G; T; A; C] "Wrapped sequence lines should be joined"
                Expect.sequenceEqual actual[0].QualitySequence [31; 31; 40; 40; 40; 40] "Wrapped quality lines should be joined"
                Expect.equal actual[1].Header "wrapped2" "Second record should start after the first quality string"
            }
            test "readLines with zero-length read" {
                let actual =
                    "@empty\n\n+\n\n@read2\nAC\n+\nII".Split('\n')
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> List.ofSeq
                Expect.equal actual.Length 2 "Two records should be parsed"
                Expect.isEmpty actual[0].Sequence "First record should have an empty sequence"
                Expect.isEmpty actual[0].QualitySequence "First record should have an empty quality string"
                Expect.equal actual[1].Header "read2" "Second record should be parsed after the empty one"
                Expect.sequenceEqual actual[1].Sequence [A; C] "Second sequence should be preserved"
                Expect.sequenceEqual actual[1].QualitySequence [40; 40] "Second quality string should be preserved"
            }
            test "readLines with zero-length read at end of input" {
                let actual =
                    [| "@empty"; ""; "+"; "" |]
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> Seq.exactlyOne
                Expect.isEmpty actual.Sequence "Sequence should be empty"
                Expect.isEmpty actual.QualitySequence "Quality string should be empty"
            }
            test "readLines with blank lines and CRLF line endings" {
                let actual =
                    "\r\n@read1\r\nAC\r\n+\r\nII\r\n\r\n@read2\r\nGT\r\n+\r\nII\r\n".Split('\n')
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> List.ofSeq
                Expect.equal (actual |> List.map (fun r -> r.Header)) ["read1"; "read2"] "Blank lines and '\\r' should be ignored"
                Expect.sequenceEqual actual[1].Sequence [G; T] "Sequence was not equal"
            }
            test "readLines with empty input" {
                let actual = [||] |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                Expect.isEmpty actual "Empty input should give no records"
            }
            test "readLines fails when header is missing" {
                Expect.throwsT<System.IO.InvalidDataException>
                    (fun () -> [| "read1"; "ACGT"; "+"; "IIII" |] |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.iter ignore)
                    "A record without '@' should fail"
            }
            test "readLines fails when '+' line is missing" {
                Expect.throwsT<System.IO.InvalidDataException>
                    (fun () -> [| "@read1"; "ACGT" |] |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.iter ignore)
                    "A record without '+' should fail"
            }
            test "readLines fails when quality is shorter than sequence" {
                Expect.throwsT<System.IO.InvalidDataException>
                    (fun () -> [| "@read1"; "ACGT"; "+"; "II" |] |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.iter ignore)
                    "Truncated quality should fail"
            }
            test "readLines fails when quality is longer than sequence" {
                Expect.throwsT<System.IO.InvalidDataException>
                    (fun () -> [| "@read1"; "ACGT"; "+"; "IIIIII" |] |> Fastq.readLines BioArray.ofNucleotideString charToPhred33 |> Seq.iter ignore)
                    "Too long quality should fail"
            }
            test "readLines only parses the records that are requested" {
                let first =
                    [| "@read1"; "AC"; "+"; "II"; "broken" |]
                    |> Fastq.readLines BioArray.ofNucleotideString charToPhred33
                    |> Seq.head
                Expect.equal first.Header "read1" "First record should be readable before the later one"
            }
            
        ]