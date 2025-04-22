namespace BioFSharp

///This module contains the BioSeq type and its according functions. The BioSeq type is a sequence of objects using the IBioItem interface
module BioSeq =

    open FSharpAux
    open BioFSharp.BioItemConverters

    ///Sequence of objects using the IBioItem interface
    type BioSeq<[<EqualityConditionalOn; ComparisonConditionalOn >] 'TBioItem when 'TBioItem :> IBioItem> = seq<'TBioItem>

    /// Generates AminoAcid sequence of one-letter-code string using given OptionConverter
    let ofAminoAcidStringWithOptionConverter (converter: char -> AminoAcids.AminoAcid option) (s:#seq<char>) : BioSeq<AminoAcids.AminoAcid> =
        s |> Seq.choose converter

    /// Generates AminoAcid sequence of one-letter-code raw string
    let ofAminoAcidString (s:#seq<char>) : BioSeq<AminoAcids.AminoAcid> =          
        s |> ofAminoAcidStringWithOptionConverter AminoAcids.oneLetterToOption

    /// Generates AminoAcid sequence of one-letter-code raw string
    let ofThreeLetterAminoAcidString (s:#seq<char>) : BioSeq<AminoAcids.AminoAcid> =          
        s 
        |> Seq.chunkBySize 3
        |> Seq.choose AminoAcids.threeLettersToOption

    /// Generates AminoAcidSymbol sequence of one-letter-code raw string
    let ofAminoAcidSymbolString (s:#seq<char>) : BioSeq<_> =          
        s
        |> Seq.choose (AminoAcidSymbols.parseChar >> snd)

    /// Generates nucleotide sequence of one-letter-code string using given OptionConverter
    let ofNucleotideStringWithOptionConverter (converter: char -> Nucleotides.Nucleotide option) (s:#seq<char>) : BioSeq<Nucleotides.Nucleotide> =
        s |> Seq.choose converter
        
    /// Generates nucleotide sequence of one-letter-code raw string
    let ofNucleotideString (s:#seq<char>) : BioSeq<Nucleotides.Nucleotide>  =             
        s |> ofNucleotideStringWithOptionConverter Nucleotides.oneLetterToOption

    ///Active pattern which returns a base triplet
    let private (|Triplet|_|) (en:System.Collections.Generic.IEnumerator<_>) = 
        if en.MoveNext () then                
            let n1 = en.Current
            if en.MoveNext () then
                let n2 = en.Current
                if en.MoveNext () then
                    Some((n1,n2,en.Current))
                else
                    None
            else
                None
        else
            None

    /// <summary>
    /// Builds a new collection whose elements are the result of applying
    /// the given function to each triplet of the collection.
    ///
    /// If the input sequence is not divisible into triplets, the last elements are ignored, and the result is built from the truncated sequence ending with the last valid triplet.
    /// </summary>
    /// <param name="mapping">The function to apply on each triplet</param>
    /// <param name="input">The input sequence</param>
    let mapInTriplets f (input:seq<'a>) =
        let sourceIsEmpty = ref false    
        seq {   
            use en = input.GetEnumerator()
            while not(sourceIsEmpty.Value) do                
            match en with
            | Triplet t -> yield (f t)                                                              
            | _         -> sourceIsEmpty.Value <- true                               
        }

    /// Create the reverse DNA or RNA strand. For example, the sequence "ATGC" is converted to "CGTA"
    let reverse (nucs:seq<Nucleotides.Nucleotide>) : BioSeq<Nucleotides.Nucleotide> = 
        nucs |> Seq.rev

    /// Create the complement DNA or cDNA (from RNA) strand. For example, the sequence "ATGC" is converted to "TACG"
    let complement (nucs:seq<Nucleotides.Nucleotide>) : BioSeq<Nucleotides.Nucleotide> = 
        nucs |> Seq.map Nucleotides.complement

    /// Create the reverse complement strand meaning antiparallel DNA strand or the cDNA (from RNA) respectivly. For example, the sequence "ATGC" is converted to "GCAT". "Antiparallel" combines the two functions "Complement" and "Inverse".
    let reverseComplement (nucs:seq<Nucleotides.Nucleotide>) : BioSeq<Nucleotides.Nucleotide> = 
        nucs |> Seq.map Nucleotides.complement |> Seq.rev

    //  Replace T by U
    /// Transcribe a given DNA coding strand (5'-----3')
    let transcribeCodingStrand (nucs:seq<Nucleotides.Nucleotide>) : BioSeq<Nucleotides.Nucleotide> = 
        nucs |> Seq.map (fun nuc -> Nucleotides.replaceTbyU nuc)
        
    /// Transcribe a given DNA template strand (3'-----5')
    let transcribeTemplateStrand (nucs:seq<Nucleotides.Nucleotide>) : BioSeq<Nucleotides.Nucleotide> = 
        nucs |> Seq.map (fun nuc -> Nucleotides.replaceTbyU (Nucleotides.complement nuc))

    /// translates nucleotide sequence to aminoacid sequence    
    let translate (nucleotideOffset:int) (rnaSeq:seq<Nucleotides.Nucleotide>) : BioSeq<AminoAcids.AminoAcid> =         
        if (nucleotideOffset < 0) then
            raise (System.ArgumentException(sprintf "Nucleotide offset %i < 0 is invalid" nucleotideOffset))                
        rnaSeq
        |> Seq.skip nucleotideOffset
        |> mapInTriplets Nucleotides.lookupBytes

    /// Compares the elemens of two sequence
    let equal (a: BioSeq<'TBioItem>) (b: BioSeq<'TBioItem>) =
        0 = Seq.compareWith 
            (fun elem1 elem2 ->
                if elem1 = elem2 then 0    
                else 1) a b 

    /// Returns string of one-letter-code
    let toString (bs:seq<#IBioItem>) =
        new string [|for c in bs -> BioItem.symbol c|]         

    /// Returns formula
    let toFormula (bs:seq<#IBioItem>) =
        bs |> Seq.fold (fun acc item -> Formula.add acc (BioItem.formula item)) Formula.emptyFormula

    /// Returns monoisotopic mass of the given sequence
    let toMonoisotopicMass (bs:seq<#IBioItem>) =
        bs |> Seq.sumBy BioItem.monoisoMass

    /// Returns average mass of the given sequence
    let toAverageMass (bs:seq<#IBioItem>) =
        bs |> Seq.sumBy BioItem.averageMass

    /// Returns monoisotopic mass of the given sequence and initial value (e.g. H2O) 
    let toMonoisotopicMassWith (state) (bs:seq<#IBioItem>) =
        bs |> Seq.fold (fun massAcc item -> massAcc + BioItem.monoisoMass item) state

    /// Returns average mass of the given sequence and initial value (e.g. H2O) 
    let toAverageMassWith (state) (bs:seq<#IBioItem>) =
        bs |> Seq.fold (fun massAcc item -> massAcc + BioItem.averageMass item) state

    /// Returns a function to calculate the monoisotopic mass of the given sequence !memoization
    let initMonoisoMass<'a when 'a :> IBioItem> : (seq<'a> -> float) =        
        let memMonoisoMass =
            Memoization.memoizeP (BioItem.formula >> Formula.monoisoMass)
        (fun bs -> 
            bs 
            |> Seq.sumBy memMonoisoMass)

    /// Returns a function to calculate the average mass of the given sequence !memoization
    let initAverageMass<'a when 'a :> IBioItem> : (seq<'a> -> float) =
        let memAverageMass =
            Memoization.memoizeP (BioItem.formula >> Formula.averageMass)
        (fun bs -> 
            bs 
            |> Seq.sumBy memAverageMass)

    /// Returns a function to calculate the monoisotopic mass of the given sequence and initial value (e.g. H2O) !memoization
    let initMonoisoMassWith<'a when 'a :> IBioItem> (state:float) : (seq<'a> -> float)  =        
        let memMonoisoMass =
            Memoization.memoizeP (BioItem.formula >> Formula.monoisoMass)
        (fun bs -> 
            bs |> Seq.fold (fun massAcc item -> massAcc + memMonoisoMass item) state)

    /// Returns a function to calculate the average mass of the given sequence and initial value (e.g. H2O) !memoization
    let initAverageMassWith<'a when 'a :> IBioItem> (state:float) : (seq<'a> -> float) =
        let memAverageMass =
            Memoization.memoizeP (BioItem.formula >> Formula.averageMass)
        (fun bs -> 
            bs |> Seq.fold (fun massAcc item -> massAcc + memAverageMass item) state)

    ///Creates an array with information about the abundacies of the distinct BioItems by converting the symbol of the BioItem to an integer and incrementing the given integer. To decrease the size of the resulting array by still having a fast performance, all indices are shifted by 65. Therefore to call the abundancy of a given BioItem, use "Resultcompositionvector.[(BioItem.symbol bioitem) - 65]"
    let toCompositionVector (input:BioSeq<_>)  =
        let compVec = Array.zeroCreate 26
        input
        |> Seq.iter (fun a ->                         
                            let index = (int (BioItem.symbol a)) - 65
                            compVec.[index] <- compVec.[index] + 1)
        compVec   
