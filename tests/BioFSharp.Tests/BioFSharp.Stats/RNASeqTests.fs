module RNASeqTests

open BioFSharp.Stats
open Expecto


let testSeq = seq { for i in 1. .. 2. -> ("stringtest"+ i.ToString(),(i,i))}
let testgeneID = seq { "stringtest1";  "stringtest2"}
let testLength = seq {1.; 2.}
let testCount = seq {1.;2.}
let testInSeq = Seq.map3 (fun id gl gc ->  RNASeq.RNASeqInput.Create id gl gc) testgeneID testLength testCount

let resultRPKM= seq {("stringtest1", 333333333.3333333); ("stringtest2",333333333.3333333)}
let resultTPM= seq {("stringtest1", 500000.); ("stringtest2", 500000.)}
let RPKMres = Seq.map (fun (id,rpkm) ->  RNASeq.NormalizedCounts.Create id rpkm RNASeq.NormalizationMethod.RPKM) resultRPKM
let TPMres = Seq.map (fun (id,tpm) ->  RNASeq.NormalizedCounts.Create id tpm RNASeq.NormalizationMethod.TPM) resultTPM
[<Tests>]
let RNASeqTests =
    
    testList "RNASeqTests" [
        testCase "RPKM" (fun _ ->
            Expect.sequenceEqual
                (RNASeq.rpkms testInSeq)
                //|> Array.ofSeq)
                (RPKMres)
                //|> Array.ofSeq)
                "RPKM did not return correct Sequence"
        )
        testCase "TPM" (fun _ ->
            Expect.sequenceEqual 
                (RNASeq.tpms testInSeq) 
                //|> Array.ofSeq)
                (TPMres)
                //|> Array.ofSeq)
                "TPM did not return correct Sequence"
        )        
    ]