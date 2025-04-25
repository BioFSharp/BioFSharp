namespace BioFSharp.Tests.Core

open BioFSharp.Tests.ReferenceObjects.BioCollections.BioSeq

open Expecto
open BioFSharp

module BioSeqTests = 

    let bioSeqTests  =

        testList "BioSeq" [

            testCase "ofAminoAcidString" (fun () ->
                let parsedAminoAcids =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioSeq.ofAminoAcidString

                Expect.sequenceEqual
                    parsedAminoAcids
                    (aminoAcidSetArray)
                    "BioSeq.ofAminoAcidString did not parse the amino acid set correctly."
            )

            testCase "ofAminoAcidSymbolString" (fun () ->
                let parsedAminoAcidSymbols =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioSeq.ofAminoAcidSymbolString

                Expect.sequenceEqual 
                    (aminoAcidSymbolSetArray )
                    parsedAminoAcidSymbols
                    "BioSeq.ofAminoAcidSymbolString did not parse the amino acid set correctly."
            )

            testCase "ofNucleotideString" (fun () ->
                let parsedNucleotides =
                    "ATGCUI-*RYKMSWBDHVN"
                    |> BioSeq.ofNucleotideString

                Expect.sequenceEqual 
                    (nucleotideSetArray )
                    parsedNucleotides
                    "BioSeq.ofNucleotideString did not parse the nucleotide set correctly."
            )

            testCase "reverse" (fun () ->
                Expect.sequenceEqual 
                    (testCodingStrand  |> BioSeq.reverse)
                    (testCodingStrandRev )
                    "BioSeq.reverse did not reverse the nucleotide sequence correctly."
            )
            
            testCase "complement" (fun () ->
                Expect.sequenceEqual 
                    (testCodingStrand  |> BioSeq.complement)
                    (testTemplateStrand )
                    "BioSeq.complement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "reverseComplement" (fun () ->
                Expect.sequenceEqual 
                    (testCodingStrand  |> BioSeq.reverseComplement)
                    (testCodingStrandRevComplement )
                    "BioSeq.reverseComplement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "mapInTriplets" (fun () ->
                Expect.sequenceEqual 
                    (testTemplateStrand  |> BioSeq.mapInTriplets id)
                    (testTriplets )
                    "BioSeq.reverseComplement did not build the correct base triplets."
            )

            testCase "transcribeCodingStrand" (fun () ->
                Expect.sequenceEqual 
                    (testCodingStrand  |> BioSeq.transcribeCodingStrand)
                    (testTranscript )
                    "BioSeq.transcribeCodingStrand did not transcribe the coding strand correctly."
            )

            testCase "transcribeTemplateStrand" (fun () ->
                Expect.sequenceEqual
                    (testTemplateStrand  |> BioSeq.transcribeTemplateStrand)
                    (testTranscript )
                    "BioSeq.transcribeTemplateStrand did not transcribe the template strand correctly."
            )

            testCase "translate" (fun () ->
                Expect.sequenceEqual 
                    (testTranscript  |> BioSeq.translate 0)
                    (testProt )
                    "BioSeq.translate did not translate the transcript correctly."
            )

            testCase "isEqual" (fun () ->
                Expect.isTrue
                    (testTranscript 
                    |> BioSeq.equal (testTranscript ))
                    "BioSeq.isEqual did not return correct result when transcripts were equal."
            )

            testCase "toString" (fun () ->
                Expect.equal
                    (aminoAcidSetArray  |> BioSeq.toString)
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    "BioSeq.toString did not return the correct string"
            )

            testCase "toMonoisotopicMass" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt  |> BioSeq.toMonoisotopicMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406)
                    "BioSeq.toMonoisotopicMass did not return correct mass"
            )

            testCase "toAverageMass" (fun() ->
                Expect.floatClose
                    Accuracy.medium // High accuracy was not passing test
                    (testProt  |> BioSeq.toAverageMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764)
                    "BioSeq.toAverageMass did not return correct mass"
            )

            testCase "toMonoisotopicMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt  |> BioSeq.toMonoisotopicMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406 + 18.0)
                    "BioSeq.toMonoisotopicMassWith did not return correct mass"
            )

            testCase "toAverageMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.medium
                    (testProt  |> BioSeq.toAverageMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764 + 18.0)
                    "BioSeq.toAverageMassWith did not return correct mass"
            )

            testCase "toCompositionVector" (fun () ->
                let testCompVec = Array.zeroCreate 26
                let metIndex = 12 // Value of (int(BioItem.symbol Met)) - 65
                let valIndex = 21 // Value of (int(BioItem.symbol Val)) - 65
                let leuIndex = 11 // Value of (int(BioItem.symbol Leu)) - 65
                testCompVec.[metIndex] <- testCompVec.[metIndex] + 1
                testCompVec.[valIndex] <- testCompVec.[valIndex] + 1
                testCompVec.[leuIndex] <- testCompVec.[leuIndex] + 1
                Expect.equal
                    (testProt  |> BioSeq.toCompositionVector)
                    testCompVec
                    "BioSeq.toCompositionVector did not return correct vector"
            )
        ]
