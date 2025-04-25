namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.Mass

module MassTests = 

    let massTests =
        testList "Mass" [

            testCase "test_toMz" <| fun () ->
                let mass = 1000.  
                let res  = toMZ mass 3.
                let exp  = 334.3406098
                Expect.floatClose Accuracy.high res exp "Mass to Mz conversion failed."

            testCase "test_toMass" <| fun () ->
                let mz   = 334.3406098
                let res  = ofMZ mz 3.
                let exp  = 1000.
                Expect.floatClose Accuracy.high res exp "Mz to Mass conversion failed."
                    
            testCase "test_accuracy" <| fun () ->
                let mass            = 1000.003
                let referenceMass   = 1000.001
                let res             = accuracy mass referenceMass 
                let exp             = 1.999998
                Expect.floatClose Accuracy.high res exp "Accuracy calculation failed."

            testCase "test_deltaMassByPpm" <| fun () ->
                let mass            = 1000.
                let ppm             = 100.
                let res             = deltaMassByPpm ppm mass 
                let exp             = 0.1
                Expect.floatClose Accuracy.high res exp "Delta mass calculation failed."

            testCase "test_rangePpm" <| fun () ->
                let mass            = 1000.
                let ppm             = 100.
                let res             = rangePpm ppm mass
                let exp             = (999.9,1000.1)
                Expect.equal res exp "Range of ppm calculation failed"
        ]
   