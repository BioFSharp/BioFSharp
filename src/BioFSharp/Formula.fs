﻿namespace BioFSharp

open System.Text.RegularExpressions
open FSharpAux

///Contains functionality for working with molecules as a formula of their elements and formulas of biologically relevant molecules
module Formula =
        
       
    let private REGEX_ELEMENT_SYM = new Regex(@"(?<element>[A-Z][a-z]*)(?:[ ({})]*)(?<number>[0-9.]*)", RegexOptions.Compiled);
    

    let private merge (a : Map<'a, float>) (b : Map<'a, float>) (f : 'a -> float * float -> 'b) =
        a
        |> Map.fold (fun acc k v' -> 
                        match acc |> Map.tryFind k with 
                        | Some v -> Map.add k (f k (v, v')) acc
                        | None   -> Map.add k (f k (0., v')) acc 
                    ) b
                

    /// Type abreviation for Map<Elements.Element,float>
    type Formula      = Map<Elements.Element,float>

    /// Empty formula
    let emptyFormula : Formula = Map.empty 

    /// Returns Formula as string
    let toString (f:Formula) =
        seq { for e in f do yield sprintf "%s%.2f" (Elements.getMainIsotope e.Key).AtomicSymbol e.Value } |> String.concat " "

    /// adds two formula
    let add (f1:Formula) (f2:Formula) :Formula =
        merge (f1) (f2) (fun _ (v, v') -> v + v')

    // adds two formula
    //let (+)  (f1:Formula) (f2:Formula) = add f1 f2

    /// substracts f1 from f2
    let substract (f1:Formula) (f2:Formula) :Formula =
        merge (f1) (f2)  (fun _ (v, v') -> v - v')

    // substract two formula
    //let (-)  (f1:Formula) (f2:Formula) = substract f1 f2

    /// Returns average mass of sum formula
    let averageMass (f:Formula) =
        f |> Seq.sumBy (fun elem -> (Elements.getMainIsotope elem.Key).RelAtomicMass * elem.Value)

    /// Returns monoisotopic mass of sum formula
    let monoisoMass (f:Formula) =
        f |> Seq.sumBy (fun elem -> (Elements.getMainIsotope elem.Key).Mass * elem.Value)
    
    /// Lables all elements of a certain kind within a formula
    let replaceElement (f:Formula) (unlabled:Elements.Element) (labled:Elements.Element) =
        let result : Formula = 
            f 
            |> Seq.map (fun (keyValue) -> if keyValue.Key = unlabled then (labled,keyValue.Value) else (keyValue.Key,keyValue.Value) )
            |> Map.ofSeq
        result
    
    /// Lables a given number of elements of a certain kind within a formula
    let replaceNumberOfElement (f:Formula) (unlabled:Elements.Element) (labled:Elements.Element) (number:float) : Formula =
        let result : Formula = 
            f 
            |> Seq.map (fun (keyValue) -> if keyValue.Key = unlabled then (keyValue.Key,keyValue.Value - number) else (keyValue.Key,keyValue.Value) )            
            |> Map.ofSeq
        result.Add(labled,number)
    
    ///Retruns true if the formula contains the element of interest. 
    let contains (ele:Elements.Element) (f:Formula)  = 
        Map.containsKey ele f

    ///Returns the number of occurences of the element of interest.
    let count (ele:Elements.Element) (f:Formula) =
        match Map.tryFind ele f with 
        | Some n -> n 
        | None   -> 0.
      
    ///Returns the number of occurences of the element with the given symbolic representation.
    let countBySym (eleSym:string) (f:Formula) =
        f 
        |> Map.tryPick (fun e num -> if eleSym = Elements.getAtomicSymbol e then Some num else None)
        |> Option.defaultValue 0.

    // TODO: (NO)2 
    /// Parse formula string and returns formula type
    let parseFormulaString (strFormula:string) =
        let matches = REGEX_ELEMENT_SYM.Matches (strFormula)
        // regex results as seq
        let ms = seq {for i = 0 to matches.Count - 1 do yield matches.[i]}
        let msItems = ms |> Seq.map ( fun g -> 
                let elem = Elements.Table.ofSymbol((g.Groups.["element"].Value))
                let n    = (g.Groups.["number"].Value)
                (elem, (if n <> "" then float(n) else 1.)) )
        let result : Formula =
            msItems 
            |> Seq.fold (fun acc (key,value) -> if acc.ContainsKey(key) then acc.Add(key,(value + acc.[key])) else acc.Add(key,value) ) Map.empty
        result
                   
    ///Contains formulas for amino acids, nucleotides and biologically relevant anorganic molecules
    module Table =

        let CO  = parseFormulaString "CO"
        let CO2 = parseFormulaString "CO2"
        let OH  = parseFormulaString "OH"  //
        let H2O = parseFormulaString "H2O"
        let NH  = parseFormulaString "NH"  //
        let NH2 = parseFormulaString "NH2"
        let NH3 = parseFormulaString "NH3"


        //Amino acid formulas minus H20   
        ///Alanine
        let Ala = parseFormulaString "C3H5ON"
        ///Cysteine
        let Cys = parseFormulaString "C3H5ONS"  
        ///Asparagic Acid
        let Asp = parseFormulaString "C4H5O3N" 
        ///Glutamic Acid
        let Glu = parseFormulaString "C5H7O3N"   
        ///Phenylalanine
        let Phe = parseFormulaString "C9H9ON"    
        ///Glycine
        let Gly = parseFormulaString "C2H3ON"  
        ///Histidine
        let His = parseFormulaString "C6H7ON3"   
        ///Isoleucine
        let Ile = parseFormulaString "C6H11ON" 
        ///Lysine
        let Lys = parseFormulaString "C6H12ON2"  
        ///Leucine
        let Leu = parseFormulaString "C6H11ON"   
        ///Methionine
        let Met = parseFormulaString "C5H9ONS"   
        ///Asparagine
        let Asn = parseFormulaString "C4H6O2N2" 
        ///Pyrrolysine
        let Pyl = parseFormulaString "C12H19N3O2"
        ///Proline
        let Pro = parseFormulaString "C5H7ON"    
        ///GLutamine
        let Gln = parseFormulaString "C5H8O2N2"  
        ///Arginine
        let Arg = parseFormulaString "C6H12ON4"  
        ///Serine
        let Ser = parseFormulaString "C3H5O2N"   
        ///Threonine
        let Thr = parseFormulaString "C4H7O2N"   
        ///Selenocysteine
        let Sec = parseFormulaString "C3H5NOSe"  
        ///Valine
        let Val = parseFormulaString "C5H9ON"
        ///Tryptophane
        let Trp = parseFormulaString "C11H10ON2" 
        ///Tyrosine
        let Tyr = parseFormulaString "C9H9O2N"                      
        ///Ambiguity: Unknown amino acid                                        
        let Xaa = parseFormulaString "C2H3N1O1"  
        ///Ambiguity: Leucine or Isoleucine
        let Xle = parseFormulaString "C6H11N1O1" 
        ///Ambiguity: Glutamine or Glutamic Acid
        let Glx = parseFormulaString "C5H7N1O3" 
        ///Ambiguity: Asparagine or Asparagic Acid
        let Asx = parseFormulaString "C4H5N1O3"  


        //Nucleotide formulas minus H20   
        /// A : Adenine
        let A = parseFormulaString "C10H13N5O4"
        /// T : Thymidine (only DNA)
        let T = parseFormulaString "C10H14N2O5"
        /// G : Guanine
        let G = parseFormulaString "C10H13N5O5"
        /// C : Cytosine
        let C = parseFormulaString "C9H13N3O5"
        /// U : Uracil    (only RNA)
        let U = parseFormulaString "C4H4N2O2"
        /// I : Inosine   (only RNA)
        let I = parseFormulaString "C10H12N4O5"
