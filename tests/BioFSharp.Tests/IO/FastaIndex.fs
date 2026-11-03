namespace BioFSharp.Tests.IO

module FastaIndexTests =

    open System
    open System.IO
    open BioFSharp.IO
    open BioFSharp.FileFormats.FastaIndex
    open Expecto

    let private testString_three_entries = ">seq1 first test sequence\nACGTACGTAC\nGTACGTACGT\nACG\n>seq2\nTTTTTGGGGG\nCCCCCAAAAA\n>seq3 short\nNNNN\n"

    let private testString_protein = ">sp|P69905|HBA_HUMAN Hemoglobin subunit alpha OS=Homo sapiens OX=9606 GN=HBA1 PE=1 SV=2\nMVLSPADKTNVKAAWGKVGAHAGEYGAEALERMFLSFPTTKTYFPHFDLSHGSAQVKGHGKKVADALTNAVAHV\nDDMPNALSALSDLHAHKLRVDPVNFKLLSHCLLVTLAAHLPAEFTPAVHASLDKFLASVSTVLTSKYR\n"

    let private expectedEntries =
        [|
            FastaIndexEntry.create "seq1" 23L 26L 10L 11L
            FastaIndexEntry.create "seq2" 20L 58L 10L 11L
            FastaIndexEntry.create "seq3" 4L 92L 4L 5L
        |]

    let private expectedEntries_crlf =
        [|
            FastaIndexEntry.create "seq1" 23L 27L 10L 12L
            FastaIndexEntry.create "seq2" 20L 63L 10L 12L
            FastaIndexEntry.create "seq3" 4L 100L 4L 6L
        |]

    let private expectedEntry_protein = FastaIndexEntry.create "sp|P69905|HBA_HUMAN" 142L 88L 74L 75L

    let private createOfString (content: string) =
        let path = Path.GetTempFileName()
        try
            File.WriteAllText(path, content)
            FastaIndex.create path
        finally
            File.Delete path
            
    let private readOfString (content: string) =
        let path = Path.GetTempFileName()
        try
            File.WriteAllText(path, content)
            FastaIndex.read path
        finally
            File.Delete path

    let IOTests =
        testList "FastaIndex" [
            test "create" {
                let actual = createOfString testString_three_entries
                Expect.equal actual expectedEntries "Index was not equal"
            }
            test "create (CRLF)" {
                let actual = createOfString (testString_three_entries.Replace("\n", "\r\n"))
                Expect.equal actual expectedEntries_crlf "Index was not equal"
            }
            test "create (protein)" {
                let actual = createOfString testString_protein
                Expect.equal actual [| expectedEntry_protein |] "Index was not equal"
            }
            test "create (names)" {
                Expect.equal (createOfString ">x\tdesc\nACGT\n").[0].Name "x" "Name was not equal (tab)"
                Expect.equal (createOfString ">   x  desc\nACGT\n").[0].Name "x" "Name was not equal (leading spaces)"
                Expect.equal (createOfString ">\nACGT\n").[0].Name "" "Name was not equal (no name)"
            }
            test "create (no final line break)" {
                Expect.equal (createOfString ">x\nACGTACGT\nACG") [| FastaIndexEntry.create "x" 11L 3L 8L 9L |] "Index was not equal"
                Expect.equal (createOfString ">x\nACGT") [| FastaIndexEntry.create "x" 4L 3L 4L 5L |] "Index was not equal (one line)"
                Expect.equal (createOfString ">x\r\nACGT\r\nAC") [| FastaIndexEntry.create "x" 6L 4L 4L 6L |] "Index was not equal (CRLF)"
                Expect.equal (createOfString ">x\nACGT\n>") [| FastaIndexEntry.create "x" 4L 3L 4L 5L |] "Index was not equal ('>' at the end)"
            }
            test "create (blank lines)" {
                Expect.equal (createOfString "\n\n>x\nACGT\n") [| FastaIndexEntry.create "x" 4L 5L 4L 5L |] "Index was not equal (before first header)"
                Expect.equal (createOfString ">x\nACGT\n\n>y\nGGGG\n") [| FastaIndexEntry.create "x" 4L 3L 4L 5L; FastaIndexEntry.create "y" 4L 12L 4L 5L |] "Index was not equal (between entries)"
                Expect.equal (createOfString ">x\nACGT\nAC\n\n\n") [| FastaIndexEntry.create "x" 6L 3L 4L 5L |] "Index was not equal (at the end)"
            }
            test "create (empty sequences)" {
                Expect.equal (createOfString ">empty\n>x\nACGT\n") [| FastaIndexEntry.create "x" 4L 10L 4L 5L |] "Index was not equal"
                Expect.equal (createOfString ">x\n\n>y\nAC\n") [| FastaIndexEntry.create "y" 2L 7L 2L 3L |] "Index was not equal (blank line)"
            }
            test "create (duplicate names)" {
                Expect.equal (createOfString ">a\nAC\n>a\nGG\n") [| FastaIndexEntry.create "a" 2L 3L 2L 3L |] "Index was not equal"
                Expect.equal (createOfString ">a\nAC\n>b\nGG\n>a\nTTTT\n") [| FastaIndexEntry.create "a" 2L 3L 2L 3L; FastaIndexEntry.create "b" 2L 9L 2L 3L |] "Index was not equal (3 entries)"
                Expect.equal (createOfString ">a\n>a\nAC\n") [| FastaIndexEntry.create "a" 2L 6L 2L 3L |] "Index was not equal (empty first entry)"
            }
            test "create (whitespace in sequence lines)" {
                Expect.equal (createOfString ">x\nACGT ACGT\nACGTACGTA\n") [| FastaIndexEntry.create "x" 17L 3L 8L 10L |] "Index was not equal (space)"
                Expect.equal (createOfString ">x\nACGT \nACGT \n") [| FastaIndexEntry.create "x" 8L 3L 4L 6L |] "Index was not equal (trailing space)"
                Expect.equal (createOfString ">x\nAC\tGT\nACGT\n") [| FastaIndexEntry.create "x" 8L 3L 4L 6L |] "Index was not equal (tab)"
            }
            test "create (line widths)" {
                Expect.equal (createOfString ">x\nAC GT\nACGTA\n") [| FastaIndexEntry.create "x" 9L 3L 4L 6L |] "Index was not equal (same width)"
                Expect.equal (createOfString ">x\r\nACGT\r\nACGT\n") [| FastaIndexEntry.create "x" 8L 4L 4L 6L |] "Index was not equal (narrower last line)"
            }
            test "create fails (longer line)" {
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\nACGTAC\n" |> ignore) "Indexing did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\nACGT  \n" |> ignore) "Indexing did not fail (trailing spaces)"
            }
            test "create fails (sequence after shorter line)" {
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\nAC\nAC\n" |> ignore) "Indexing did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\n\nACGT\n" |> ignore) "Indexing did not fail (blank line)"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\nAC\n\r" |> ignore) "Indexing did not fail (carriage return)"
            }
            test "create fails (data before first header)" {
                Expect.throwsT<InvalidDataException> (fun () -> createOfString "; comment\n>x\nACGT\n" |> ignore) "Indexing did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString "  \n>x\nACGT\n" |> ignore) "Indexing did not fail (spaces)"
            }
            test "create fails (empty last sequence)" {
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\n>empty\n" |> ignore) "Indexing did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString ">x\nACGT\n>empty" |> ignore) "Indexing did not fail (no final line break)"
            }
            test "create fails (no sequences)" {
                Expect.throwsT<InvalidDataException> (fun () -> createOfString "" |> ignore) "Indexing did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> createOfString "\n\n" |> ignore) "Indexing did not fail (blank lines)"
            }
            test "create fails (missing file)" {
                Expect.throwsT<FileNotFoundException> (fun () -> FastaIndex.create (Path.Combine(Path.GetTempPath(), "missing.fa")) |> ignore) "Indexing did not fail"
            }
            test "write" {
                let path = Path.GetTempFileName()
                try
                    FastaIndex.write path expectedEntries
                    let actual = File.ReadAllBytes path
                    let expected = Text.Encoding.ASCII.GetBytes "seq1\t23\t26\t10\t11\nseq2\t20\t58\t10\t11\nseq3\t4\t92\t4\t5\n"
                    Expect.equal actual expected "Index file was not equal"
                finally
                    File.Delete path
            }
            test "write (no entries)" {
                let path = Path.GetTempFileName()
                try
                    FastaIndex.write path [||]
                    Expect.isEmpty (File.ReadAllBytes path) "Index file was not empty"
                finally
                    File.Delete path
            }
            test "read" {
                let actual = readOfString "seq1\t23\t26\t10\t11\nseq2\t20\t58\t10\t11\nseq3\t4\t92\t4\t5\n"
                Expect.equal actual expectedEntries "Index was not equal"
            }
            test "read (CRLF)" {
                let actual = readOfString "seq1\t23\t26\t10\t11\r\nseq2\t20\t58\t10\t11\r\nseq3\t4\t92\t4\t5\r\n"
                Expect.equal actual expectedEntries "Index was not equal"
            }
            test "read (no final line break)" {
                let actual = readOfString "seq1\t23\t26\t10\t11\nseq2\t20\t58\t10\t11\nseq3\t4\t92\t4\t5"
                Expect.equal actual expectedEntries "Index was not equal"
            }
            test "read (empty name)" {
                let actual = readOfString "\t4\t2\t4\t5\nb\t4\t10\t4\t5\n"
                Expect.equal actual [| FastaIndexEntry.create "" 4L 2L 4L 5L; FastaIndexEntry.create "b" 4L 10L 4L 5L |] "Index was not equal"
            }
            test "read (duplicate names)" {
                let actual = readOfString "a\t4\t3\t4\t5\na\t4\t11\t4\t5\nb\t4\t11\t4\t5\n"
                Expect.equal actual [| FastaIndexEntry.create "a" 4L 3L 4L 5L; FastaIndexEntry.create "b" 4L 11L 4L 5L |] "Index was not equal"
            }
            test "read (empty file)" {
                Expect.isEmpty (readOfString "") "Index was not empty"
            }
            test "read fails (blank line)" {
                Expect.throwsT<InvalidDataException> (fun () -> readOfString "a\t4\t3\t4\t5\n\nb\t4\t11\t4\t5\n" |> ignore) "Reading did not fail"
                Expect.throwsT<InvalidDataException> (fun () -> readOfString "a\t4\t3\t4\t5\n\n" |> ignore) "Reading did not fail (at the end)"
            }
            test "read fails (invalid line)" {
                Expect.throwsT<InvalidDataException> (fun () -> readOfString "a\t4\t3\t4\n" |> ignore) "Reading did not fail"
            }
            test "read fails (missing file)" {
                Expect.throwsT<FileNotFoundException> (fun () -> FastaIndex.read (Path.Combine(Path.GetTempPath(), "missing.fai")) |> ignore) "Reading did not fail"
            }
            test "write and read" {
                let path = Path.GetTempFileName()
                try
                    let entries = createOfString testString_protein
                    FastaIndex.write path entries
                    let actual = FastaIndex.read path
                    Expect.equal actual entries "Index was not equal"
                finally
                    File.Delete path
            }
        ]
        