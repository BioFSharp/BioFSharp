namespace BioFSharp.Tests.Core

open Expecto
open BioFSharp
open BioFSharp.IsotopicDistribution

module IsotopicDistributionTests =    

    let isotopicDistributionTests =
        testList "IsotopicDistribution" [

            let rbclPeptide = "LTYYTPDYVVR" |> BioList.ofAminoAcidString

            testCase "MIDA - native" <| fun () ->
                let f = BioList.toFormula rbclPeptide
                let resMz,resI = 
                    (MIDA.ofFormula (MIDA.normalizeByProbSum) 0.01 (exp(-150.)) 2) f
                    |> List.take 10        
                    |> List.unzip
                let expMz,expI = 
                    [(686.35113, 0.4399410927); (686.8526273, 0.3467444671);
                    (687.3540283, 0.1511531361); (687.8466896, 7.808205088e-06);
                    (687.8553813, 0.04733265321); (688.3452095, 7.844567519e-08);
                    (688.3514551, 0.0001487690513); (688.3567659, 0.01165838331);
                    (688.3636913, 2.557290565e-10); (688.8468601, 5.75226667e-08);]
                    |> List.unzip
                let mzEqual = TestingUtils.floatsClose Accuracy.high resMz expMz 
                let intensitiesEqual = TestingUtils.floatsClose Accuracy.high resI expI
                Expect.isTrue (mzEqual && intensitiesEqual ) "Isotopic distribution is not predicted correctly."           

            testCase "MIDA - heavyNitrogen" <| fun () ->
                let f = 
                    BioList.toFormula rbclPeptide 
                    |> fun f -> Formula.replaceElement f Elements.Table.N Elements.Table.Heavy.N15
                let resMz,resI = 
                    (MIDA.ofFormula (MIDA.normalizeByProbSum) 0.01 (exp(-20.)) 2) f    
                    |> List.unzip
                let expMz,expI = 
                    [(691.3363288, 7.844567955e-08); (691.8348719, 7.865483286e-06);
                        (692.3334028, 0.0005401034423); (692.8319433, 0.02289511018);
                        (692.8381101, 2.385842864e-06); (693.3305243, 0.4565473545);
                        (693.3367837, 0.0001410612366); (693.8321728, 0.3309329837);
                        (693.8370007, 0.0002143438977); (693.8416492, 8.441523553e-08);
                        (694.3337061, 0.1360967899); (694.342768, 2.389782534e-08);
                        (694.8351733, 0.04057623846); (695.3365961, 0.009685161862);
                        (695.3418529, 2.702654225e-06); (695.8379911, 0.00195231858);
                        (696.3393613, 0.0003429970119); (696.8407131, 5.368619035e-05);
                        (696.8468557, 3.733582685e-09); (697.3420478, 7.597770518e-06);
                        (697.3471893, 2.085938082e-09); (697.8433701, 9.832927444e-07);
                        (698.3446502, 1.142143697e-07); (698.8459295, 9.189067252e-09)]
                    |> List.unzip
                let mzEqual = TestingUtils.floatsClose Accuracy.high resMz expMz 
                let intensitiesEqual = TestingUtils.floatsClose Accuracy.high resI expI
                Expect.isTrue (mzEqual && intensitiesEqual ) "Isotopic distribution is not predicted correctly."           

            testCase "Brain - native" <| fun () ->
                let f = BioList.toFormula rbclPeptide
                let resI = IsotopicDistribution.BRAIN.ofFormula 10 f
                let expI = 
                    [0.4447133896; 0.3504035102; 0.1527121741; 0.04781885845; 0.01192430934;
                        0.002500506597; 0.000455935203; 7.392927856e-05; 1.083278323e-05;
                        1.451714164e-06]
                let intensitiesEqual = TestingUtils.floatsClose Accuracy.high resI expI
                Expect.isTrue (intensitiesEqual) "Isotopic distribution is not predicted correctly."           
                
        ]


