namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp

module TaggedSequenceTests =

    let taggedSequencetests =
        testList "TaggedSequence" [
            
            let t   = "Numbers"
            let s   = [1..100]  
            let ts  = {Tag=t;Sequence=s}

            testCase "create" <| fun () ->
                let ts' = TaggedSequence.create t s
                Expect.equal ts' ts "Record initialization via function differs from initialization via record expression. Check parameter order of 'create'"

            testCase "test_mapTag" <| fun () ->
                let t'    = ts.Tag.ToLower()
                let ts'   = TaggedSequence.mapTag (fun (t:string) -> t.ToLower() ) ts
                Expect.equal ts'.Tag t' "'mapTag' does not alter the value of the field 'Tag' as expected."

            testCase "test_mapSequence" <| fun () ->
                let t'    = ts.Sequence |> Seq.map ((*) (-1))
                let ts'   = TaggedSequence.mapSequence (Seq.map ((*) (-1))) ts
                Expect.sequenceEqual ts'.Sequence t' "'mapSequence' does not alter the value of the field 'Sequence' as expected."
        ]