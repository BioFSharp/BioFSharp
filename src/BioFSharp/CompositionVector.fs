namespace BioFSharp

/// <summary>
/// Provides methods to compute composition vectors from biological sequences.
/// </summary>
type CompositionVector = 

    /// <summary>
    /// Computes the absolute frequency of specified bioitems within a sequence.
    /// </summary>
    /// <param name="letter">Array of bioitems (e.g., amino acids or nucleotides) whose frequency should be computed.</param>
    /// <param name="sequence">Sequence of bioitems to analyze.</param>
    /// <returns>
    /// A map from each bioitem to its absolute count within the sequence.
    /// </returns>
    static member inline bioItemFrequency<^a when ^a :> IBioItem and ^a : (static member  op_Explicit : ^a -> int) and ^a : comparison> (letter:array<^a>) (sequence: array<^a>) =
        let cvec = BioArray.toCompositionVector sequence
        letter
        |> Seq.map (fun l -> l,cvec.[(int l) - 65])
        |> Map.ofSeq


    /// <summary>
    /// Computes the relative frequency (probability) of specified bioitems within a sequence.
    /// </summary>
    /// <param name="letter">Array of bioitems (e.g., amino acids or nucleotides) whose relative frequency should be computed.</param>
    /// <param name="sequence">Sequence of bioitems to analyze.</param>
    /// <returns>
    /// A map from each bioitem to its relative frequency (probability) within the sequence.
    /// </returns>
    static member inline bioItemProbability<^a when ^a :> IBioItem and ^a : (static member  op_Explicit : ^a -> int) and ^a : comparison> (letter:array<^a>) (sequence: array<^a>) =
        let cvec = BioArray.toRelCompositionVector sequence
        letter
        |> Seq.map (fun l -> l,cvec.[(int l) - 65])
        |> Map.ofSeq


    /// <summary>
    /// Converts an array of bioitems into its absolute composition vector.
    /// </summary>
    /// <param name="input">Array of bioitems.</param>
    /// <returns>An array representing the count of each bioitem.</returns>
    static member inline ofArray input =
        BioArray.toCompositionVector input


    /// <summary>
    /// Converts a list of bioitems into its absolute composition vector.
    /// </summary>
    /// <param name="input">List of bioitems.</param>
    /// <returns>An array representing the count of each bioitem.</returns>
    static member inline ofList input =
        BioList.toCompositionVector input


    /// <summary>
    /// Converts a sequence of bioitems into its absolute composition vector.
    /// </summary>
    /// <param name="input">Sequence of bioitems.</param>
    /// <returns>An array representing the count of each bioitem.</returns>
    static member inline ofSeq input =
        BioSeq.toCompositionVector input
