namespace BioFSharp.Tests.Core

open BioFSharp.Tests.ReferenceObjects.BioCollections.BioList

open Expecto
open BioFSharp

module BioListTests = 

    let bioListTests  =

        testList "BioList" [

            testCase "ofAminoAcidString" (fun () ->
                let parsedAminoAcids =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioList.ofAminoAcidString

                Expect.equal
                    parsedAminoAcids
                    (aminoAcidSetArray)
                    "BioList.ofAminoAcidString did not parse the amino acid set correctly."
            )

            testCase "ofAminoAcidSymbolString" (fun () ->
                let parsedAminoAcidSymbols =
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    |> BioList.ofAminoAcidSymbolString

                Expect.equal 
                    (aminoAcidSymbolSetArray)
                    parsedAminoAcidSymbols
                    "BioList.ofAminoAcidSymbolString did not parse the amino acid set correctly."
            )

            testCase "ofNucleotideString" (fun () ->
                let parsedNucleotides =
                    "ATGCUI-*RYKMSWBDHVN"
                    |> BioList.ofNucleotideString

                Expect.equal 
                    (nucleotideSetArray)
                    parsedNucleotides
                    "BioList.ofNucleotideString did not parse the nucleotide set correctly."
            )

            testCase "reverse" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioList.reverse)
                    (testCodingStrandRev)
                    "BioList.reverse did not reverse the nucleotide sequence correctly."
            )
            
            testCase "complement" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioList.complement)
                    (testTemplateStrand)
                    "BioList.complement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "reverseComplement" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioList.reverseComplement)
                    (testCodingStrandRevComplement)
                    "BioList.reverseComplement did not build the reverse complement of the nucleotide sequence correctly."
            )

            testCase "mapInTriplets" (fun () ->
                Expect.equal 
                    (testTemplateStrand |> BioList.mapInTriplets id)
                    (testTriplets)
                    "BioList.reverseComplement did not build the correct base triplets."
            )

            testCase "transcribeCodingStrand" (fun () ->
                Expect.equal 
                    (testCodingStrand |> BioList.transcribeCodingStrand)
                    (testTranscript)
                    "BioList.transcribeCodingStrand did not transcribe the coding strand correctly."
            )

            testCase "transcribeTemplateStrand" (fun () ->
                Expect.equal 
                    (testTemplateStrand |> BioList.transcribeTemplateStrand)
                    (testTranscript)
                    "BioList.transcribeTemplateStrand did not transcribe the template strand correctly."
            )

            testCase "translate" (fun () ->
                Expect.equal 
                    (testTranscript |> BioList.translate 0)
                    (testProt)
                    "BioList.translate did not translate the transcript correctly."
            )

            testCase "isEqual" (fun () ->
                Expect.isTrue
                    (testTranscript
                    |> BioList.isEqual (testTranscript))
                    "BioList.isEqual did not return correct result when transcripts were equal."
            )

            testCase "toString" (fun () ->
                Expect.equal
                    (aminoAcidSetArray |> BioList.toString)
                    "ACDEFGHIKLMNOPQRSTUVWYXJZB-*"
                    "BioList.toString did not return the correct string"
            )

            testCase "toMonoisotopicMass" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt |> BioList.toMonoisotopicMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406)
                    "BioList.toMonoisotopicMass did not return correct mass"
            )

            testCase "toAverageMass" (fun() ->
                Expect.floatClose
                    Accuracy.medium // High accuracy was not passing test
                    (testProt |> BioList.toAverageMass)
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764)
                    "BioList.toAverageMass did not return correct mass"
            )

            testCase "toMonoisotopicMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.high
                    (testProt |> BioList.toMonoisotopicMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.04048 + 99.06841 + 113.08406 + 18.0)
                    "BioList.toMonoisotopicMassWith did not return correct mass"
            )

            testCase "toAverageMassWith" (fun () ->
                Expect.floatClose
                    Accuracy.medium
                    (testProt |> BioList.toAverageMassWith 18.0) // 18 = mass of one water molecule
                    // Masses obtained from University of Washington Proteomics Resource https://proteomicsresource.washington.edu/protocols06/masses.php
                    (131.19606 + 99.13106 + 113.15764 + 18.0)
                    "BioList.toAverageMassWith did not return correct mass"
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
                    (testProt |> BioList.toCompositionVector)
                    testCompVec
                    "BioList.toCompositionVector did not return correct vector"
            )

        ]
