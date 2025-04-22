namespace BioFSharp

/// Conversion functions to and from BioItems
module BioItemConverters =

    module AminoAcids =

        /// <summary>
        /// Converter function to return Some AminoAcid if the input character is a valid one-letter code.
        /// Returns None if the input is not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some AminoAcid for valid AA codes, None otherwise</returns>
        let oneLetterToOption (character:char) =
            let pac = AminoAcids.parseOneLetterCode character
            match pac with
            | AminoAcids.ParsedAminoAcidChar.StandardCodes  (aa)         -> Some aa
            | AminoAcids.ParsedAminoAcidChar.AmbiguityCodes (aa)         -> Some aa 
            | AminoAcids.ParsedAminoAcidChar.GapTer         (aa)         -> Some aa 
            | _ -> None

        /// <summary>
        /// Converter function to return Some AminoAcid if the input characters are a valid three-letter code.
        /// Returns None if the input is not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input characters to convert</param>
        /// <returns>Some AminoAcid for valid AA codes, None otherwise</returns>
        let threeLettersToOption (characters:#seq<char>) =
            let pac = AminoAcids.parseThreeLetterCode characters
            match pac with
            | AminoAcids.ParsedAminoAcidChar.StandardCodes  (aa)         -> Some aa
            | AminoAcids.ParsedAminoAcidChar.AmbiguityCodes (aa)         -> Some aa 
            | AminoAcids.ParsedAminoAcidChar.GapTer         (aa)         -> Some aa 
            | _ -> None

        /// <summary>
        /// Converter function to return Some AminoAcid if the input character is a valid one-letter code for standard or ambiguous amino acids, but no Gap or tertminator.
        /// Returns None if the input is a Gap, a Terminator, or not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some AminoAcid for valid AA codes, None otherwise</returns>
        let oneLetterToOptionWithoutGapTer (character:char) =
            let pac = AminoAcids.parseOneLetterCode character
            match pac with
            | AminoAcids.ParsedAminoAcidChar.StandardCodes  (aa)
            | AminoAcids.ParsedAminoAcidChar.AmbiguityCodes (aa) -> Some aa 
            | _  -> None

        /// <summary>
        /// Converter function to return Some AminoAcid if the input character is a valid one-letter code for standard amino acids.
        /// Returns None if the input an ambiguous code (e.g., 'X', 'Z', 'B', 'J'), or not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some AminoAcid for valid AA codes, None otherwise</returns>
        let oneLetterToOptionStandard (character:char) =
            let pac = AminoAcids.parseOneLetterCode character
            match pac with
            | AminoAcids.ParsedAminoAcidChar.StandardCodes  (aa)         -> Some aa
            | AminoAcids.ParsedAminoAcidChar.GapTer         (aa)         -> Some aa 
            | _ -> None

        /// <summary>
        /// Converter function to return Some AminoAcid if the input character is a valid one-letter code for standard amino acids, but no Gap or tertminator.
        /// Returns None if the input an ambiguous code (e.g., 'X', 'Z', 'B', 'J'), a Gap, a Terminator, or not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some AminoAcid for valid AA codes, None otherwise</returns>
        let oneLetterToOptionStandardWithoutGapTer (character:char) =
            let pac = AminoAcids.parseOneLetterCode character
            match pac with
            | AminoAcids.ParsedAminoAcidChar.StandardCodes (aa) -> Some aa
            | _  -> None

    module Nucleotides =

        /// <summary>
        /// Converter function to return Some Nucleotide if the input character is a valid one-letter code.
        /// Returns None if the input is not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some Nucleotide for valid nucleotide codes, None otherwise</returns>
        let oneLetterToOption (character:char) =
            let pnc = Nucleotides.charToParsedNucleotideChar character
            match pnc with
            | Nucleotides.ParsedNucleotideChar.StandardCodes    (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.Standard_DNAonly (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.Standard_RNAonly (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.AmbiguityCodes   (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.GapTer           (n) -> Some n 
            | Nucleotides.ParsedNucleotideChar.NoNucChar (_)        -> None              

        /// <summary>
        /// Converter function to return Some Nucleotide if the input character is a valid one-letter code for standard nucleotides.
        /// Returns None if the input an ambiguous code (e.g.,'R','Y','K','M','S','W','B','D','H','V','N'), or not a valid one-letter code.
        /// </summary>
        /// <param name="character">The input character to convert</param>
        /// <returns>Some Nucleotide for valid nucleotide codes, None otherwise</returns>
        let oneLetterToOptionStandard (character:char) =
            let pnc = Nucleotides.charToParsedNucleotideChar character
            match pnc with
            | Nucleotides.ParsedNucleotideChar.StandardCodes    (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.Standard_DNAonly (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.Standard_RNAonly (n) -> Some n
            | Nucleotides.ParsedNucleotideChar.GapTer           (n) -> Some n
            | _ -> None
