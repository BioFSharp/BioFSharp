namespace BioFSharp.FileFormats

open BioFSharp

module Stride =

    //TO-DO replace after merge and release of FSharpAux PR: 
    module internal Seq =
        let chunkBy (projection: 'T -> 'Key) (source: _ seq) = 
            seq {
                use e = source.GetEnumerator ()
                if e.MoveNext () then
                    let mutable g = projection e.Current
                    let mutable members = ResizeArray ()
                    members.Add e.Current
                    while e.MoveNext () do
                        let key = projection e.Current
                        if g = key then 
                            members.Add e.Current
                        else
                            yield g, members |> Seq.cast<'T>
                            g <- key
                            members <- ResizeArray ()
                            members.Add e.Current
                    yield g, members |> Seq.cast<'T>
            }

    type StrideLine = {
        ResidueIndex            : int
        OrdinalResidueIndex     : int
        ResidueName             : string
        ChainId                 : string
        AminoAcid               : AminoAcids.AminoAcid
        SecondaryStructure      : SecondaryStructure
        AccessibleSurface       : float
        PHI                     : float
        PSI                     : float 
    } with
        static member create residueindex ordinalresidueindex residuename chainid aminoacid secondarystructure accessiblesurface phi psi =
            {
                ResidueIndex        = residueindex      
                OrdinalResidueIndex = ordinalresidueindex      
                ResidueName         = residuename       
                ChainId             = chainid           
                AminoAcid           = aminoacid         
                SecondaryStructure  = secondarystructure
                AccessibleSurface   = accessiblesurface 
                PHI                 = phi               
                PSI                 = psi               
            }

    let toAASequence (stride:seq<StrideLine>) =
        stride
        |> Seq.map (fun x -> x.AminoAcid)
        |> Array.ofSeq

    let toStructureSequence (stride:seq<StrideLine>) =
        stride
        |> Seq.map (fun stride -> stride.SecondaryStructure)
        |> Array.ofSeq    

    let toSequenceFeatures (stride:seq<StrideLine>) =
        stride
        |> Seq.map (fun x -> x.SecondaryStructure)
        |> Seq.indexed
        |> Seq.chunkBy (snd >> SecondaryStructure.toString)
        |> Seq.map (fun (structure,stretch) ->
            SequenceFeature.create(
                "Stride Structure",
                (stretch |> Seq.minBy fst |> fst),
                (stretch |> Seq.maxBy fst |> fst),
                (char structure)
            )
        )

    let format (stride:seq<StrideLine>) =
        stride
        |> fun x -> 
            let seq = 
                x
                |> Seq.map (fun x -> x.AminoAcid)

            let features = ["STRIDE Structure", toSequenceFeatures x |> List.ofSeq] |> Map.ofList
        
            AnnotatedSequence.create "stride" seq features
        
        |> AnnotatedSequence.format
        |> fun s -> $"""{System.Environment.NewLine}
STRIDE result summary.

Seconary structure keys:
- H = α-helix
- B = residue in isolated β-bridge
- E = extended strand, participates in β ladder
- G = 3-helix (310 helix)
- I = 5 helix (π-helix)
- T = hydrogen bonded turn
- S = bend

{s}{System.Environment.NewLine}"""