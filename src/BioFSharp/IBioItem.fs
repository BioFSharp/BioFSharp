﻿namespace BioFSharp

open System
open FSharpAux

///Marker interface for BioItem base.
//[<StructuralEquality;StructuralComparison>]
type IBioItem =    
    ///Display name of the bio item, e.g. "Alanine"
    abstract member Name     : string
    ///Symbol of the bio item, e.g. 'A' for alanine
    abstract member Symbol   : char    
    ///Chemical formula of the bio item represented as Formula
    abstract member Formula  : Formula.Formula    
    ///Indicator wether the bio item represents a sequence terminator
    abstract member isTerminator : bool         
    ///Indicator the bio item represents a sequence gap
    abstract member isGap        : bool    

///Lexer Tags for parsing BioItems
type NcbiParsingType = 
    | StandardCodes  = 10y 
    | AmbiguityCodes = 20y
    | GapTer         = 30y  
    | NoAAChar       = 0y

/// Basic functions on IBioItems interface
module BioItem = 

    /// Returns the display name of the bio item
    let inline name (bItem:#IBioItem) =
        bItem.Name
    
    /// Returns the symbol of the bio item
    let inline symbol (bItem:#IBioItem) =
        bItem.Symbol

    /// Returns the formula of the bio item
    let inline formula  (bItem:#IBioItem) =
        bItem.Formula

    /// Returns true if the bio item represents a sequence terminator
    let inline isTerminator  (bItem:#IBioItem) =
        bItem.isTerminator

    /// Returns true if the bio item represents a sequence gap
    let inline isGap  (bItem:#IBioItem) =
        bItem.isGap
    

    /// Returns the monoisotopic mass of a bio item (without H20)
    let inline monoisoMass<'a when 'a :> IBioItem> = 
        formula >> Formula.monoisoMass            

    /// Returns the average mass of a bio item  (without H20)
    let inline averageMass<'a when 'a :> IBioItem> = 
        formula >> Formula.averageMass
        
    /// Returns a function to calculate the monoisotopic mass of a bio item with memoization !Attention! Not thread safe!
    let initMonoisoMassWithMem<'a when 'a :> IBioItem> =
        let monoisoMass' = formula >> Formula.monoisoMass
        Memoization.memoize (fun a -> monoisoMass' a)

    /// Returns a function to calculate the average mass of a bio item with memoization !Attention! Not thread safe!
    let initAverageMassWithMem<'a when 'a :> IBioItem> =
        let averageMass' = formula >> Formula.averageMass
        Memoization.memoize (fun a -> averageMass' a)
    
    /// Returns a function to calculate the monoisotopic mass of a bio item with memoization
    let initMonoisoMassWithMemP<'a when 'a :> IBioItem> =
        let monoisoMass' = formula >> Formula.monoisoMass
        Memoization.memoizeP (fun a -> monoisoMass' a)

    /// Returns a function to calculate the average mass of a bio item with memoization
    let initAverageMassWithMemP<'a when 'a :> IBioItem> = 
        let averageMass' = formula >> Formula.averageMass
        Memoization.memoizeP (fun a -> averageMass' a)


///// Type abbreviation for converting char to Bioitem
//type bioItemConverter<'a when 'a :> IBioItem> = char -> 'a option
//
//
///// Type abbreviation for converting char to optional Bioitem
//type bioItemOptionConverter<'a when 'a :> IBioItem> = char -> 'a option
