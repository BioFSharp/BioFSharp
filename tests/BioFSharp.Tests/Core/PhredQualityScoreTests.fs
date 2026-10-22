namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.IO
open BioFSharp.Tests.ReferenceObjects.Fastq

module PhredQualityScoreTests =

    let phredTests =
        testList "Phred" [

            testCase "toScore_Phred33" <| fun () ->
                let res = ['!'; '+'; '5'; '?'; 'I'; '~'] |> List.map (Phred.toScore Phred.Encoding.Phred33)
                let exp = [0; 10; 20; 30; 40; 93]
                Expect.equal res exp "Phred+33 characters were converted incorrectly."

            testCase "toScore_Phred64" <| fun () ->
                let res = ['@'; 'J'; 'T'; '^'; 'h'; '~'] |> List.map (Phred.toScore Phred.Encoding.Phred64)
                let exp = [0; 10; 20; 30; 40; 62]
                Expect.equal res exp "Phred+64 characters were converted incorrectly."

            testCase "toScore_invalidCharacter" <| fun () ->
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.toScore Phred.Encoding.Phred33 ' ' |> ignore) "Character below '!' was accepted as Phred+33."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.toScore Phred.Encoding.Phred64 '?' |> ignore) "Character below '@' was accepted as Phred+64."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.toScore Phred.Encoding.Phred33 '\127' |> ignore) "Character above '~' was accepted as Phred+33."

            testCase "ofScore_toScore_roundtrip" <| fun () ->
                for encoding in [Phred.Encoding.Phred33; Phred.Encoding.Phred64] do
                    for score in 0 .. encoding.MaxScore do
                        let res = score |> Phred.ofScore encoding |> Phred.toScore encoding
                        Expect.equal res score $"Round trip failed for Q{score} in {encoding}."

            testCase "ofScore_invalidScore" <| fun () ->
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.ofScore Phred.Encoding.Phred33 -1 |> ignore) "Negative score was encoded."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.ofScore Phred.Encoding.Phred33 94 |> ignore) "Q94 was encoded as Phred+33."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.ofScore Phred.Encoding.Phred64 63 |> ignore) "Q63 was encoded as Phred+64."

            testCase "toScores" <| fun () ->
                let res = Phred.toScores Phred.Encoding.Phred33 "II9IC#"
                let exp = [|40; 40; 24; 40; 34; 2|]
                Expect.equal res exp "Quality string was converted incorrectly."

            testCase "ofScores" <| fun () ->
                let res = Phred.ofScores Phred.Encoding.Phred33 [40; 40; 24; 40; 34; 2]
                let exp = "II9IC#"
                Expect.equal res exp "Quality scores were encoded incorrectly."

            testCase "toScores_fastqQualityConverter" <| fun () ->
                let res =
                    testString_one_entry.Split(System.Environment.NewLine)
                    |> Fastq.readLines BioArray.ofNucleotideString (Phred.toScores Phred.Encoding.Phred33)
                    |> Seq.head
                Expect.sequenceEqual res.QualitySequence expectedFastqItem.QualitySequence "Quality scores of FASTQ record were converted incorrectly."

            testCase "toErrorProbability" <| fun () ->
                let res = [0; 10; 20; 30; 40] |> List.map Phred.toErrorProbability
                let exp = [1.; 0.1; 0.01; 0.001; 0.0001]
                List.iter2 (fun r e -> Expect.floatClose Accuracy.high r e "Error probability was calculated incorrectly.") res exp

            testCase "toErrorProbability_negativeScore" <| fun () ->
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.toErrorProbability -1 |> ignore) "Negative score was accepted."

            testCase "ofErrorProbability" <| fun () ->
                let res = [1.; 0.1; 0.05; 0.001] |> List.map Phred.ofErrorProbability
                let exp = [0; 10; 13; 30]
                Expect.equal res exp "Quality score was calculated incorrectly."

            testCase "ofErrorProbability_invalidProbability" <| fun () ->
                for p in [0.; -0.1; 1.5; nan] do
                    Expect.throwsT<System.ArgumentException> (fun () -> Phred.ofErrorProbability p |> ignore) $"Probability {p} was accepted."

            testCase "ofErrorProbability_toErrorProbability_roundtrip" <| fun () ->
                for score in 0 .. 93 do
                    let res = score |> Phred.toErrorProbability |> Phred.ofErrorProbability
                    Expect.equal res score $"Round trip failed for Q{score}."

            testCase "mean" <| fun () ->
                let res = Phred.mean [10; 30; 40; 40]
                let exp = 30.
                Expect.floatClose Accuracy.high res exp "Mean quality was calculated incorrectly."

            testCase "meanByErrorProbability" <| fun () ->
                let res = Phred.meanByErrorProbability [10; 30]
                let exp = -10. * log10 ((0.1 + 0.001) / 2.)
                Expect.floatClose Accuracy.high res exp "Mean quality by error probability was calculated incorrectly."
                Expect.isLessThan res (Phred.mean [10; 30]) "Mean quality by error probability should be lower than the arithmetic mean."

            testCase "expectedErrors" <| fun () ->
                let res = Phred.expectedErrors [10; 20; 30]
                let exp = 0.111
                Expect.floatClose Accuracy.high res exp "Expected errors were calculated incorrectly."

            testCase "expectedErrors_empty" <| fun () ->
                Expect.floatClose Accuracy.high (Phred.expectedErrors []) 0. "Expected errors of no bases should be 0."

            testCase "fractionAtLeast" <| fun () ->
                let res = Phred.fractionAtLeast 30 [20; 30; 40; 10]
                let exp = 0.5
                Expect.floatClose Accuracy.high res exp "Q30 fraction was calculated incorrectly."

            testCase "metrics_empty" <| fun () ->
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.mean [] |> ignore) "Mean of no scores was calculated."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.meanByErrorProbability [] |> ignore) "Mean by error probability of no scores was calculated."
                Expect.throwsT<System.ArgumentException> (fun () -> Phred.fractionAtLeast 30 [] |> ignore) "Fraction of no scores was calculated."

            testCase "tryDetectEncoding" <| fun () ->
                Expect.equal (Phred.tryDetectEncoding "II9IC#") (Some Phred.Encoding.Phred33) "Phred+33 was not detected."
                Expect.equal (Phred.tryDetectEncoding "hhTB@") (Some Phred.Encoding.Phred64) "Phred+64 was not detected."
                Expect.equal (Phred.tryDetectEncoding "") None "Empty input should not be detected."
                Expect.equal (Phred.tryDetectEncoding "II\tII") None "Invalid characters should not be detected."
        ]
