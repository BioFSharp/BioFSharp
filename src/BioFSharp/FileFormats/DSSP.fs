namespace BioFSharp.FileFormats

open FSharpAux
open BioFSharp
open BioFSharp.BioItemConverters

module DSSP =

    type DSSPLine = {
        ResidueIndex            : int
        ResidueName             : string
        InsertionCode           : string
        ChainId                 : string
        AminoAcid               : string
        SecondaryStructure      : SecondaryStructure
        AccessibleSurface       : int
        NH_O_1_Relidx           : int
        NH_O_1_Energy           : float
        O_NH_1_Relidx           : int
        O_NH_1_Energy           : float
        NH_O_2_Relidx           : int
        NH_O_2_Energy           : float
        O_NH_2_Relidx           : int
        O_NH_2_Energy           : float
        TCO                     : float
        KAPPA                   : float
        ALPHA                   : float 
        PHI                     : float
        PSI                     : float 
        X_CA                    : float
        Y_CA                    : float
        Z_CA                    : float
        Chain                   : string
    
    } with
        static member create residueindex residuename insertioncode chainid aminoacid secondarystructure accessiblesurface nh_o_1_relidx nh_o_1_energy o_nh_1_relidx o_nh_1_energy nh_o_2_relidx nh_o_2_energy o_nh_2_relidx o_nh_2_energy tco kappa alpha phi psi x_ca y_ca z_ca chain =
            {
                ResidueIndex        = residueindex      
                ResidueName         = residuename       
                InsertionCode       = insertioncode     
                ChainId             = chainid           
                AminoAcid           = aminoacid         
                SecondaryStructure  = secondarystructure
                AccessibleSurface   = accessiblesurface 
                NH_O_1_Relidx       = nh_o_1_relidx     
                NH_O_1_Energy       = nh_o_1_energy     
                O_NH_1_Relidx       = o_nh_1_relidx     
                O_NH_1_Energy       = o_nh_1_energy     
                NH_O_2_Relidx       = nh_o_2_relidx     
                NH_O_2_Energy       = nh_o_2_energy     
                O_NH_2_Relidx       = o_nh_2_relidx     
                O_NH_2_Energy       = o_nh_2_energy     
                TCO                 = tco               
                KAPPA               = kappa             
                ALPHA               = alpha             
                PHI                 = phi               
                PSI                 = psi               
                X_CA                = x_ca              
                Y_CA                = y_ca              
                Z_CA                = z_ca              
                Chain               = chain             
            }

    let toAASequence (dssp:seq<DSSPLine>) =
        dssp
        |> Seq.choose (fun x -> x.AminoAcid |> char |> AminoAcids.oneLetterToOption)
        |> Array.ofSeq

    let toStructureSequence (dssp:seq<DSSPLine>) =
        dssp
        |> Seq.map (fun dssp -> dssp.SecondaryStructure)
        |> Array.ofSeq    

    let toSequenceFeatures (dssp:seq<DSSPLine>) =
        dssp
        |> Seq.map (fun x -> x.SecondaryStructure)
        |> Seq.indexed
        |> Seq.chunkBy (snd >> SecondaryStructure.toString)
        |> Seq.map (fun (structure,stretch) ->
            SequenceFeature.create(
                "DSSP Structure",
                (stretch |> Seq.minBy fst |> fst),
                (stretch |> Seq.maxBy fst |> fst),
                (char structure)
            )
        )

    let format (dssp:seq<DSSPLine>) =
        dssp
        |> fun x -> 
            let seq = 
                x
                |> Seq.map (fun x -> x.AminoAcid)
                |> String.concat ""
                |> BioArray.ofAminoAcidString
            
            let features = ["DSSP Structure", toSequenceFeatures x |> List.ofSeq] |> Map.ofList
        
            AnnotatedSequence.create "dssp" seq features
        
        |> AnnotatedSequence.format

        |> fun s -> $"""{System.Environment.NewLine}
DSSP (Define Secondary Structure of Proteins) result summary.

Seconary structure keys:
- H = α-helix
- B = residue in isolated β-bridge
- E = extended strand, participates in β ladder
- G = 3-helix (310 helix)
- I = 5 helix (π-helix)
- T = hydrogen bonded turn
- S = bend

{s}{System.Environment.NewLine}"""