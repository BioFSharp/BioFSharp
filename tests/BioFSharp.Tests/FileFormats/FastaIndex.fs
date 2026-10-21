namespace BioFSharp.Tests.FileFormats

module FastaIndexTests =
    
    open BioFSharp.FileFormats.FastaIndex
    open Expecto

    let private seq1Entry = FastaIndexEntry.create "seq1" 23L 26L 10L 11L

    let fileFormatTests =
        testList "FastaIndex" [
            test "create" {
                Expect.equal seq1Entry.Name "seq1" "Name was not equal"
                Expect.equal seq1Entry.Length 23L "Length was not equal"
                Expect.equal seq1Entry.Offset 26L "Offset was not equal"
                Expect.equal seq1Entry.LineBases 10L "LineBases was not equal"
                Expect.equal seq1Entry.LineWidth 11L "LineWidth was not equal"
            }
            test "toLine" {
                let actual = FastaIndexEntry.toLine seq1Entry
                Expect.equal actual "seq1\t23\t26\t10\t11" ".fai line was not correctly formatted"
            }
            test "ofLine" {
                let actual = FastaIndexEntry.ofLine "seq1\t23\t26\t10\t11"
                Expect.equal actual seq1Entry ".fai line was not correctly parsed"
            }
            test "ofLine with UniProt name" {
                let actual = FastaIndexEntry.ofLine "sp|P69905|HBA_HUMAN\t142\t88\t74\t75"
                Expect.equal actual (FastaIndexEntry.create "sp|P69905|HBA_HUMAN" 142L 88L 74L 75L) ".fai line was not correctly parsed"
            }
            test "ofLine with offset beyond 32 bit" {
                let actual = FastaIndexEntry.ofLine "chrY\t57227415\t3099750718\t60\t61"
                Expect.equal actual.Offset 3099750718L "Offset larger than Int32.MaxValue was not parsed"
            }
            test "toLine and ofLine round trip" {
                let actual = seq1Entry |> FastaIndexEntry.toLine |> FastaIndexEntry.ofLine
                Expect.equal actual seq1Entry "Entry changed in round trip"
            }
            test "ofLine fails on wrong number of columns" {
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "seq1\t23\t26\t10" |> ignore) "4 columns should fail"
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "seq1\t23\t26\t10\t11\t12" |> ignore) "6 columns (FASTQ index) should fail"
            }
            test "ofLine fails on invalid numbers" {
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "seq1\t-23\t26\t10\t11" |> ignore) "Negative number should fail"
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "seq1\tabc\t26\t10\t11" |> ignore) "Text should fail"
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "seq1\t23 \t26\t10\t11" |> ignore) "Spaces should fail"
            }
            test "ofLine fails on empty name" {
                Expect.throwsT<System.IO.InvalidDataException> (fun () -> FastaIndexEntry.ofLine "\t23\t26\t10\t11" |> ignore) "Empty name should fail"
            }
        ]
        