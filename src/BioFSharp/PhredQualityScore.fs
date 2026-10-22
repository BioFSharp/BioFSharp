namespace BioFSharp

/// Functions to encode, decode, and evaluate Phred quality scores
module Phred =

    /// ASCII encoding of Phred quality scores
    [<RequireQualifiedAccess>]
    type Encoding =
        | Phred33
        | Phred64

        /// Returns the ASCII value that represents Q0
        member this.Offset =
            match this with
            | Phred33 -> 33
            | Phred64 -> 64

        /// Returns the highest quality score that can be encoded as an ASCII character
        member this.MaxScore = int '~' - this.Offset

    /// Returns the quality score of the given ASCII character
    let toScore (encoding: Encoding) (c: char) : int =
        let score = int c - encoding.Offset
        if score < 0 || score > encoding.MaxScore then
            // Fails if the character is not valid for the encoding
            invalidArg (nameof c) (sprintf "'%c' (ASCII %i) is not a valid %A quality character" c (int c) encoding)
        score

    /// Returns the ASCII character of the given quality score
    let ofScore (encoding: Encoding) (score: int) : char =
        if score < 0 || score > encoding.MaxScore then
            // Fails if the score cannot be encoded
            invalidArg (nameof score) (sprintf "Q%i cannot be encoded as %A (valid: Q0 to Q%i)" score encoding encoding.MaxScore)
        char (score + encoding.Offset)

    /// Returns the quality scores of the given ASCII characters.
    /// Can be used as quality converter: Fastq.read BioArray.ofNucleotideString (Phred.toScores Phred.Encoding.Phred33) path
    let toScores (encoding: Encoding) (chars: seq<char>) : int [] =
        chars |> Seq.map (toScore encoding) |> Array.ofSeq

    /// Returns the ASCII string of the given quality scores
    let ofScores (encoding: Encoding) (scores: seq<int>) : string =
        scores |> Seq.map (ofScore encoding) |> Array.ofSeq |> System.String

    /// Returns the probability that a base call with the given quality score is wrong: P = 10^(-Q/10)
    let toErrorProbability (score: int) : float =
        if score < 0 then invalidArg (nameof score) (sprintf "Q%i is negative" score)
        10. ** (-(float score) / 10.)

    /// Returns the nearest quality score of the given error probability: Q = -10 * log10(P)
    let ofErrorProbability (probability: float) : int =
        // The probability must be greater than 0 and at most 1
        if not (probability > 0. && probability <= 1.) then
            invalidArg (nameof probability) (sprintf "%f is not greater than 0 and at most 1" probability)
        int (System.Math.Round(-10. * log10 probability))

    /// Returns the arithmetic mean of the given quality scores
    /// Use meanByErrorProbability for the mean error rate
    let mean (scores: seq<int>) : float =
        scores |> Seq.averageBy float

    /// Returns the quality score that corresponds to the mean error probability of the given quality scores
    let meanByErrorProbability (scores: seq<int>) : float =
        -10. * log10 (scores |> Seq.averageBy toErrorProbability)

    /// Returns the sum of the error probabilities of the given quality scores
    let expectedErrors (scores: seq<int>) : float =
        scores |> Seq.sumBy toErrorProbability

    /// Returns the fraction of quality scores that are at least the given threshold
    let fractionAtLeast (threshold: int) (scores: seq<int>) : float =
        scores |> Seq.averageBy (fun score -> if score >= threshold then 1. else 0.)

    /// Guesses the encoding of the given quality characters by the lowest character
    /// Returns None if chars is empty or contains characters outside '!' to '~'
    let tryDetectEncoding (chars: seq<char>) : Encoding option =
        let range =
            chars
            |> Seq.fold (fun range c ->
                match range with
                | None -> Some (c, c)
                | Some (lowest, highest) -> Some (min lowest c, max highest c)
            ) None
        match range with
        | Some (lowest, highest) when lowest >= '!' && highest <= '~' ->
            if lowest < '@' then Some Encoding.Phred33 else Some Encoding.Phred64
        | _ -> None
        