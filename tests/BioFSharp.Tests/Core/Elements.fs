namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.Elements

module ElementsTests = 
    let elementsTests =

        testList "Elements" [

            testCase "test_createMonoIsotopic" <| fun () ->
                let Na = { 
                    Symbol = "Na"
                    X      = Isotopes.Table.Na23
                    Xcomp  = Isotopes.Table.Na23.NatAbundance
                    Root   = Isotopes.Table.Na23.NatAbundance 
                    }
                let res = createMono "Na" (Isotopes.Table.Na23,Isotopes.Table.Na23.NatAbundance) 
                Expect.equal res Na "Record initialization via function differs from initialization via record expression. Check parameter order of 'createMono'"

            testCase "test_createDiIsotopic" <| fun () ->
                let H = { 
                    Symbol = "H" 
                    X      = Isotopes.Table.H1
                    Xcomp  = Isotopes.Table.H1.NatAbundance
                    X1     = Isotopes.Table.H2
                    X1comp = Isotopes.Table.H2.NatAbundance
                    Root   = (-1. * Isotopes.Table.H1.NatAbundance / Isotopes.Table.H2.NatAbundance)
                    }
                let res = createDi "H" (Isotopes.Table.H1,Isotopes.Table.H1.NatAbundance) (Isotopes.Table.H2,Isotopes.Table.H2.NatAbundance) 
                Expect.equal res H "Record initialization via function differs from initialization via record expression. Check parameter order of 'createDi'" 

            let (rootO,rootO') =
                System.Numerics.Complex(-0.0926829268292669, 22.0592593273255),
                System.Numerics.Complex(-0.0926829268292696, -22.0592593273255)
            let O = { 
                Symbol = "O"
                X      = Isotopes.Table.O16
                Xcomp  = Isotopes.Table.O16.NatAbundance
                X1     = Isotopes.Table.O17
                X1comp = Isotopes.Table.O17.NatAbundance
                X2     = Isotopes.Table.O18
                X2comp = Isotopes.Table.O18.NatAbundance
                Root   = (rootO,rootO')
                }
            let O' = createTri "O" (Isotopes.Table.O16,Isotopes.Table.O16.NatAbundance) (Isotopes.Table.O17,Isotopes.Table.O17.NatAbundance) (Isotopes.Table.O18,Isotopes.Table.O18.NatAbundance)

            testCase "createTriIsotopic - firstReal" <| fun () ->
                Expect.floatClose Accuracy.high (fst O.Root).Real (fst O'.Root).Real ""
            testCase "createTriIsotopic - secondReal" <| fun () ->
                Expect.floatClose Accuracy.high (snd O.Root).Real (snd O'.Root).Real ""
            testCase "createTriIsotopic - firstImaginary" <| fun () ->
                Expect.floatClose Accuracy.high (fst O.Root).Imaginary (fst O'.Root).Imaginary ""
            testCase "createTriIsotopic - secondImaginary" <| fun () ->
                Expect.floatClose Accuracy.high (snd O.Root).Imaginary (snd O'.Root).Imaginary ""
            testCase "createTriIsotopic - initialization" <| fun () ->
                Expect.equal  O O' "Record initialization via function differs from initialization via record expression. Check parameter order of 'createTri'"                       
            
            let O   = createTri "O" (Isotopes.Table.O16,Isotopes.Table.O16.NatAbundance) (Isotopes.Table.O17,Isotopes.Table.O17.NatAbundance) (Isotopes.Table.O18,Isotopes.Table.O18.NatAbundance)
            let O'  = {O with Symbol = "N"}
            let O'' = {O with Xcomp = nan}

            testCase "triIsotopicCompare - negative" <| fun () ->
                Expect.notEqual O O' "Expected equality, because the member 'CompareTo' checks for equality of the record field Symbol"                           

            testCase "triIsotopicCompare - positive" <| fun () ->
                Expect.equal O O'' "Expected equality, because the member 'CompareTo' checks for equality of the record field Symbol"                             

            testCase "getMainIsotope - Mono" <| fun () ->
                let iso = getMainIsotope Table.Na
                let exp = Isotopes.Table.Na23
                Expect.equal iso exp ""                           

            testCase "getMainIsotope - Di" <| fun () ->
                let iso = getMainIsotope Table.H
                let exp = Isotopes.Table.H1
                Expect.equal iso exp ""                           

            testCase "getMainIsotope - Tri" <| fun () ->
                let iso = getMainIsotope Table.O
                let exp = Isotopes.Table.O16
                Expect.equal iso exp ""                           

            testCase "getMainIsotope - Multi" <| fun () ->                         
                let iso = getMainIsotope Table.Zn
                let exp = Isotopes.Table.Zn64
                Expect.equal iso exp ""                                               

            testCase "getMainXComp - Mono" <| fun () ->
                let abu = getMainXComp Table.Na
                let exp = Isotopes.Table.Na23.NatAbundance
                Expect.floatClose Accuracy.high abu exp ""                           

            testCase "Di" <| fun () ->
                let abu = getMainXComp Table.H
                let exp = Isotopes.Table.H1.NatAbundance
                Expect.floatClose Accuracy.high abu exp ""                           

            testCase "Tri" <| fun () ->
                let abu = getMainXComp Table.O
                let exp = Isotopes.Table.O16.NatAbundance
                Expect.floatClose Accuracy.high abu exp ""                           

            testCase "Multi" <| fun () ->                         
                let abu = getMainXComp Table.Zn
                let exp = Isotopes.Table.Zn64.NatAbundance
                Expect.floatClose Accuracy.high abu exp ""                                               

            testCase "getSinglePhiL - Mono" <| fun () ->
                let res = getSinglePhiL Table.Na 1000000. 2.
                let exp = 1000000.0
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiL - Di" <| fun () ->
                let res = getSinglePhiL Table.H 1000000. 2.
                let exp = 0.01322804227
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiL - Tri" <| fun () ->
                let res = getSinglePhiL Table.O 1000000. 2.
                let exp = -4109.842165
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiL - Multi" <| fun () ->                         
                let res = getSinglePhiL Table.Zn 1000000. 2.
                Expect.isTrue (nan.Equals(res)) ""                                              

            testCase "getSinglePhiM - Mono" <| fun () ->
                let res = getSinglePhiM Table.Na 1000000. 2.
                let exp = 1892.042007
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiM - Di" <| fun () ->
                let res = getSinglePhiM Table.H 1000000. 2.
                let exp = 0.0528309132
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiM - Tri" <| fun () ->
                let res = getSinglePhiM Table.O 1000000. 2.
                let exp = -4109.842165
                Expect.floatClose Accuracy.high res exp ""                           

            testCase "getSinglePhiM - Multi" <| fun () ->                         
                let res = getSinglePhiM Table.Zn 1000000. 2.
                Expect.isTrue (nan.Equals(res)) ""

            testCase "test_getAtomicSymbol" <| fun () ->
                let naS'    = "Na"
                let na      =  Mono (createMono naS' (Isotopes.Table.Na23,Isotopes.Table.Na23.NatAbundance) )
                let naS    = getAtomicSymbol na
                Expect.equal naS naS' "Symbols are not equal." 

            testCase "test_ofSymbol" <| fun () ->
                let H   = Elements.Table.ofSymbol "H" 
                let H'  = Elements.Table.H
                Expect.equal H H' "Symbols are not equal." 
                  
        ]