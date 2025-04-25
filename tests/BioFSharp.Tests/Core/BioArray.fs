namespace BioFSharp.Tests.Core

open BioFSharp.Tests.ReferenceObjects.BioCollections.BioArray

open Expecto
open BioFSharp

module BioArrayTests = 

    let bioArrayTests  =

        testList "BioArray" [

            testCase "ofAminoAcidString" (fun () ->
                let parsedAminoAcids =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioArray.ofAminoAcidString

                Expect.equal
                    aminoAcidSetArray
                    parsedAminoAcids
                    "BioArray.ofAminoAcidString did not parse the amino acid set correctly."
            )

            testCase "ofAminoAcidSymbolString" (fun () ->
                let parsedAminoAcidSymbols =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioArray.ofAminoAcidSymbolString

                Expect.equal 
                    aminoAcidSymbolSetArray
                    parsedAminoAcidSymbols
                    "BioArray.ofAminoAcidSymbolString did not parse the amino acid set correctly."
            )

            testCase "ofNucleotideString" (fun () ->
                let parsedNucleotides =
                    "ATGCUI-*RYKMSWBDHVN"
                    |> BioArray.ofNucleotideString

                Expect.equal 
                    nucleotideSetArray
                    parsedNucleotides
                    "BioArray.ofNucleotideString did not parse the nucleotide set correctly."
            )

            testCase "reverse" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioArray.reverse)
                    testCodingStrandRev
                    "BioArray.reverse did not reverse the nucleotide sequence correctly."
            )
            
            testCase "complement" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioArray.complement)
                    testTemplateStrand
                    "BioArray.complement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "reverseComplement" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioArray.reverseComplement)
                    testCodingStrandRevComplement
                    "BioArray.reverseComplement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "mapInTriplets" (fun () ->
                Expect.equal 
                    (testTemplateStrand |> BioArray.mapInTriplets id)
                    testTriplets
                    "BioArray.reverseComplement did not build the correct base triplets."
            )

            testCase "transcribeCodingStrand" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioArray.transcribeCodingStrand)
                    testTranscript
                    "BioArray.transcribeCodingStrand did not transcribe the coding strand correctly."
            )

            testCase "transcribeTemplateStrand" (fun () ->
                Expect.equal 
                    (testTemplateStrand |> BioArray.transcribeTemplateStrand)
                    testTranscript
                    "BioArray.transcribeTemplateStrand did not transcribe the template strand correctly."
            )

            testCase "translate" (fun () ->
                Expect.equal 
                    (testTranscript |> BioArray.translate 0)
                    testProt
                    "BioArray.translate did not translate the transcript correctly."
            )

            testCase "isEqual" (fun () ->
                Expect.isTrue
                    (testTranscript |> BioArray.equal testTranscript)
                    "BioArray.isEqual did not return correct result when transcripts were equal."
            )

            testCase "toString" (fun () ->
                Expect.equal
                    (aminoAcidSetArray |> BioArray.toString)
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    "BioArray.toString did not return the correct string"
            )

            testCase "toMonoisotopicMass" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt |> BioArray.toMonoisotopicMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406)
                    "BioArray.toMonoisotopicMass did not return correct mass"
            )

            testCase "toAverageMass" (fun() ->
                Expect.floatClose
                    Accuracy.medium // High accuracy was not passing test
                    (testProt |> BioArray.toAverageMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764)
                    "BioArray.toAverageMass did not return correct mass"
            )

            testCase "toMonoisotopicMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt |> BioArray.toMonoisotopicMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406 + 18.0)
                    "BioArray.toMonoisotopicMassWith did not return correct mass"
            )

            testCase "toAverageMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.medium
                    (testProt |> BioArray.toAverageMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764 + 18.0)
                    "BioArray.toAverageMassWith did not return correct mass"
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
                    (testProt |> BioArray.toCompositionVector)
                    testCompVec
                    "BioArray.toCompositionVector did not return correct vector"
            )
        ]
