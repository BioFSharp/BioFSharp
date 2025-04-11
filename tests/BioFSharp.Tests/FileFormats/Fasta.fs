module FastaTests

open BioFSharp
open BioFSharp.AminoAcids
open BioFSharp.AminoAcidSymbols
open BioFSharp.FileFormats.Fasta
open BioFSharp.IO
open Expecto

let testString_one_entry = """>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
"""                         .ReplaceLineEndings()

let testString_two_entries = """>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
"""                           .ReplaceLineEndings()
    

let expectedFastaItem =
    FastaItem.create
        "gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]"
        (
            [
                Leu;Cys;Leu;Tyr;Thr;His;Ile;Gly;Arg;Asn;Ile;Tyr;Tyr;Gly;Ser;Tyr;Leu;Tyr;Ser;Glu
                Thr;Trp;Asn;Thr;Gly;Ile;Met;Leu;Leu;Leu;Ile;Thr;Met;Ala;Thr;Ala;Phe;Met;Gly;Tyr
                Val;Leu;Pro;Trp;Gly;Gln;Met;Ser;Phe;Trp;Gly;Ala;Thr;Val;Ile;Thr;Asn;Leu;Phe;Ser
                Ala;Ile;Pro;Tyr;Ile;Gly;Thr;Asn;Leu;Val;Glu;Trp;Ile;Trp;Gly;Gly;Phe;Ser;Val;Asp
                Lys;Ala;Thr;Leu;Asn;Arg;Phe;Phe;Ala;Phe;His;Phe;Ile;Leu;Pro;Phe;Thr;Met;Val;Ala
                Leu;Ala;Gly;Val;His;Leu;Thr;Phe;Leu;His;Glu;Thr;Gly;Ser;Asn;Asn;Pro;Leu;Gly;Leu
                Thr;Ser;Asp;Ser;Asp;Lys;Ile;Pro;Phe;His;Pro;Tyr;Tyr;Thr;Ile;Lys;Asp;Phe;Leu;Gly
                Leu;Leu;Ile;Leu;Ile;Leu;Leu;Leu;Leu;Leu;Leu;Ala;Leu;Leu;Ser;Pro;Asp;Met;Leu;Gly
                Asp;Pro;Asp;Asn;His;Met;Pro;Ala;Asp;Pro;Leu;Asn;Thr;Pro;Leu;His;Ile;Lys;Pro;Glu
                Trp;Tyr;Phe;Leu;Phe;Ala;Tyr;Ala;Ile;Leu;Arg;Ser;Val;Pro;Asn;Lys;Leu;Gly;Gly;Val
                Leu;Ala;Leu;Phe;Leu;Ser;Ile;Val;Ile;Leu;Gly;Leu;Met;Pro;Phe;Leu;His;Thr;Ser;Lys
                His;Arg;Ser;Met;Met;Leu;Arg;Pro;Leu;Ser;Gln;Ala;Leu;Phe;Trp;Thr;Leu;Thr;Met;Asp
                Leu;Leu;Thr;Leu;Thr;Trp;Ile;Gly;Ser;Gln;Pro;Val;Glu;Tyr;Pro;Tyr;Thr;Ile;Ile;Gly
                Gln;Met;Ala;Ser;Ile;Leu;Tyr;Phe;Ser;Ile;Ile;Leu;Ala;Phe;Leu;Pro;Ile;Ala;Gly;Xaa
                Ile;Glu;Asn;Tyr
            ]
        )

let fastaTests =
    testList "Fasta" [
        test "readLines" {
            let actual = testString_one_entry.Split(System.Environment.NewLine) |> Fasta.readLines BioArray.ofAminoAcidString |> Seq.item 0
            let expected = expectedFastaItem
            Expect.equal actual.Header expected.Header "Header was not equal"
            Expect.sequenceEqual actual.Sequence expected.Sequence "Sequence was not equal"
        }
        test "toLines" {
            let actual = expectedFastaItem |> FastaItem.toLines BioItem.symbol
            let expected = testString_one_entry.Trim().Split(System.Environment.NewLine) // trim because lines will have no EOL at the end
            Expect.sequenceEqual actual expected "Fastaitem string was not correctly formatted"
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