namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.Formula

module FormulaTests = 

    let formulaTests =

        testList "Formula" [

            testCase "toString" <| fun () ->
                let res = "H2O"  |> parseFormulaString |> toString
                Expect.equal res "H2.00 O1.00" "Parsing of formula string failed."

            testCase "add" <| fun () ->
                let f1  = "H2"  |> parseFormulaString 
                let f2  = "O"   |> parseFormulaString 
                let res = add f1 f2 |> toString 
                Expect.equal res "H2.00 O1.00" "Addition of formulas failed."

            testCase "substract - basic" <| fun () ->
                let f1  = "H2O"  |> parseFormulaString 
                let f2  = "H4O2" |> parseFormulaString 
                let res = substract f1 f2 |> toString 
                Expect.equal res "H2.00 O1.00" "Substraction of formulas failed"
            testCase "substract - neg" <| fun () ->
                let f1  = "H2O"   |> parseFormulaString 
                let f2  = emptyFormula 
                let res = substract f1 f2 |> toString 
                Expect.equal res  "H-2.00 O-1.00" "Substraction of formulas failed"

            testCase "substract - Zero" <| fun () ->
                let f1  = "H2O" |> parseFormulaString 
                let f2  = "H2O" |> parseFormulaString 
                let res = substract f1 f2 |> toString 
                Expect.equal res "H0.00 O0.00" "Substraction of formulas failed"

            testCase "averageMass" <| fun () ->
                let res = "H2O" |> parseFormulaString |> Formula.averageMass 
                let exp = 18.015294
                Expect.floatClose Accuracy.high res exp "Substraction of formulas failed"

            testCase "monoisoMass" <| fun () ->
                let res = "H2O" |> parseFormulaString |> Formula.monoisoMass 
                let exp = 18.01056468
                Expect.floatClose Accuracy.high res exp "Substraction of formulas failed"            
            

            let f = "N10" |> parseFormulaString 
            let labeledf  = Formula.replaceElement f Elements.Table.N Elements.Table.Heavy.N15 

            testCase "replaceElement - isReplaced" <| fun () ->
                let res = Map.containsKey Elements.Table.N labeledf
                Expect.isFalse res "Element was not removed."     

            testCase "replaceElement - newElementInserted" <| fun () ->
                let res = Map.containsKey Elements.Table.Heavy.N15 labeledf
                Expect.isTrue res "Element was not replaced."

            testCase "replaceElement - NumberIsNotChanged" <| fun () ->
                let res  = Map.find Elements.Table.N f
                let res' = Map.find Elements.Table.Heavy.N15 labeledf
                        
                Expect.floatClose Accuracy.high res res' "Element number has changed."


            let f = "N10" |> parseFormulaString 
            let labeledf  = Formula.replaceNumberOfElement f Elements.Table.N Elements.Table.Heavy.N15 4.

            testCase "replaceNumberOfElement - isNotReplaced" <| fun () ->
                let res = Map.containsKey Elements.Table.N labeledf
                Expect.isTrue res "Element was removed."        

            testCase "replaceNumberOfElement - newElementInserted" <| fun () ->
                let res = Map.containsKey Elements.Table.Heavy.N15 labeledf
                Expect.isTrue res "Element was not replaced."

            testCase "replaceNumberOfElement - stoichiomentry - old" <| fun () ->
                let res = Map.find Elements.Table.N labeledf
                let exp = 6. 
                Expect.floatClose Accuracy.high res exp "Element number not correct."
            testCase "replaceNumberOfElement - stoichiomentry - new" <| fun () ->
                let res = Map.find Elements.Table.Heavy.N15 labeledf
                let exp = 4. 
                Expect.floatClose Accuracy.high res exp "Element number not correct."


            let f = "N10" |> parseFormulaString  

            testCase "contains- positive" <| fun () ->
                let res = contains Elements.Table.N f 
                Expect.isTrue res ""

            testCase "contains - negative" <| fun () ->
                let res = contains Elements.Table.H f 
                Expect.isFalse res ""

            let f = "N10" |> parseFormulaString  

            testCase "count - positive" <| fun () ->
                let res = count Elements.Table.N f
                let exp = 10. 
                Expect.floatClose Accuracy.high (res) (exp) "Element number not correct."

            testCase "count - negative" <| fun () ->
                let res = count Elements.Table.H f
                let exp = 0.
                Expect.floatClose Accuracy.high (res) (exp) "Element number not correct."

            let f = "N10" |> parseFormulaString  

            testCase "countBySym - positive" <| fun () ->
                let res = countBySym "N" f 
                let exp = 10. 
                Expect.floatClose Accuracy.high (res) (exp) "Element number not correct."

            testCase "countBySym - negative" <| fun () ->
                let res = countBySym "H" f
                let exp = 0.
                Expect.floatClose Accuracy.high (res) (exp) "Element number not correct."
    
    ]