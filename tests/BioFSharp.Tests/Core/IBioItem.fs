namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.Mass

module IBioItem = 

    let iBioItemTests =

        testList "IBioItem" [

            testCase "name" <| fun () ->
                let res = BioItem.name AminoAcids.Ala  
                let exp  = "Alanine"
                Expect.equal res exp "Incorrect name returned."

            testCase "symbol" <| fun () ->
                let res = BioItem.symbol AminoAcids.Ala  
                let exp  = 'A'
                Expect.equal res exp "Incorrect symbol returned."
                  
            testCase "formula" <| fun () ->
                let res = BioItem.formula AminoAcids.Ala  
                let exp = Formula.Table.Ala
                Expect.equal res exp "Incorrect formula returned."
            
            testCase "isTerminator" <| fun () ->
                let res = BioItem.isTerminator AminoAcids.Ala  
                Expect.isFalse res ""

            testCase "isGap" <| fun () ->
                let res = BioItem.isGap AminoAcids.Ala  
                Expect.isFalse res ""

            testCase "monoIsoMass" <| fun () ->
                let res = BioItem.monoisoMass AminoAcids.Ala  
                let exp = 71.03711378
                Expect.floatClose Accuracy.high res exp "Monoisotopic mass of alanin was calculated incorrectly."

            testCase "averageMass" <| fun () ->
                let res = BioItem.averageMass AminoAcids.Ala  
                let exp = 71.078175
                Expect.floatClose Accuracy.high res exp "Average mass of alanin was calculated incorrectly."

            testCase "monoIsoMassWithMem - calculation" <| fun () ->
                let massF = BioItem.initMonoisoMassWithMem  
                let res = massF AminoAcids.Ala 
                let exp = 71.03711378
                Expect.floatClose Accuracy.high res exp "Monoisotopic mass of alanin was calculated incorrectly."

            testCase "monoIsoMassWithMem - speed" <| fun () ->
                let massF = BioItem.initMonoisoMassWithMem  
                let f1 () = [for i = 0 to 100 do yield massF AminoAcids.Ala] 
                let f2 () = [for i = 0 to 100 do yield BioItem.monoisoMass AminoAcids.Ala] 
                Expect.isFasterThan  f1 f2 ""
                
            testCase "averageMassWithMem - calculation" <| fun () ->
                let massF = BioItem.initAverageMassWithMem  
                let res = massF AminoAcids.Ala 
                let exp = 71.078175
                Expect.floatClose Accuracy.high res exp "Average mass of alanin was calculated incorrectly."

            testCase "averageMassWithMem - speed" <| fun () ->
                let massF = BioItem.initAverageMassWithMem
                let f1 () = [for i = 0 to 100 do yield massF AminoAcids.Ala] 
                let f2 () = [for i = 0 to 100 do yield BioItem.averageMass AminoAcids.Ala] 
                Expect.isFasterThan  f1 f2 ""

            testCase "monoIsoMassWithMemP - calculation" <| fun () ->
                let massF = BioItem.initMonoisoMassWithMemP  
                let res = massF AminoAcids.Ala 
                let exp = 71.03711378
                Expect.floatClose Accuracy.high res exp "Monoisotopic mass of alanin was calculated incorrectly."

            testCase "monoIsoMassWithMemP - speed" <| fun () ->
                let massF = BioItem.initMonoisoMassWithMemP  
                let f1 () = [for i = 0 to 100 do yield massF AminoAcids.Ala] 
                let f2 () = [for i = 0 to 100 do yield BioItem.monoisoMass AminoAcids.Ala] 
                Expect.isFasterThan  f1 f2 ""

            testCase "averageMassWithMemP - calculation" <| fun () ->
                let massF = BioItem.initAverageMassWithMemP  
                let res = massF AminoAcids.Ala 
                let exp = 71.078175
                Expect.floatClose Accuracy.high res exp "Average mass of alanin was calculated incorrectly."

            testCase "averageMassWithMemP - speed" <| fun () ->
                let massF = BioItem.initAverageMassWithMemP  
                let f1 () = [for i = 0 to 100 do yield massF AminoAcids.Ala] 
                let f2 () = [for i = 0 to 100 do yield BioItem.averageMass AminoAcids.Ala] 
                Expect.isFasterThan  f1 f2 ""
        ]