namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp.Isotopes

module IsotopesTests = 

    let isotopesTests =

        testList "Isotopes" [

            testCase "test_createIsotope" <| fun () ->
                let H1 = { 
                    AtomicSymbol  =  "H" 
                    AtomicNumberZ = 1
                    MassNumber    = 1
                    Mass          = 1.00782503207
                    NatAbundance  = 0.999885
                    RelAtomicMass = 1.007947
                    }
                let res = create "H" 1 1 1.00782503207 0.999885 1.007947
                Expect.equal res H1 "Record initialization via function differs from initialization via record expression. Check parameter order of 'create'" 
        ]