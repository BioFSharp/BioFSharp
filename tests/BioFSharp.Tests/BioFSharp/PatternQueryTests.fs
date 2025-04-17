module PatternQuery

open BioFSharp
open BioFSharp.IO

open Nucleotides
open Algorithm.PatternQuery

open Expecto
open FSharpAux.IO

let shineDalgarno = [|A;G;G;A;G;G;T|] //"AGGAGGT"

let sequences = 
    TestingUtils.readEmbeddedDocument "GCA_000019425.1_first230000_genomic.fna"
    |> Fasta.readLines BioArray.ofNucleotideString
    |> Seq.toArray

[<Tests>]
let PatternQuery =
    testList "Fuzzy" [
        testCase "Zero missmatches counts" (fun() ->
            let tmp = sequences.[0].Sequence :?> BioArray.BioArray<_>
            let numMatches = FuzzyMatching.search 0 shineDalgarno tmp |> Seq.length    
            Expect.equal                
                numMatches
                6
                "FuzzyMatching.search did not find the right number of matches without missmatches."
        )
        
        testCase "Zero missmatches" (fun() ->
            let tmp = sequences.[0].Sequence :?> BioArray.BioArray<_>
            let matches = 
                FuzzyMatching.search 0 shineDalgarno tmp
                |> List.map (fun s_index -> tmp.[s_index..s_index+shineDalgarno.Length])
            Expect.sequenceEqual               
                matches
                (Seq.initInfinite (fun _ -> shineDalgarno))
                "FuzzyMatching.search did not find the right matches without missmatches."
        )


    ]