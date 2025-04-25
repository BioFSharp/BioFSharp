namespace BioFSharp.Tests.ReferenceObjects.BioCollections

module BioSeq =

    open BioFSharp

    open AminoAcids
    
    let aminoAcidSetArray : BioSeq.BioSeq<AminoAcid> = 
        seq [|Ala;Cys;Asp;Glu;Phe;Gly;His;Ile;Lys;Leu;Met;Asn;Pyl;Pro;Gln;Arg;Ser;Thr;Sec;Val;Trp;Tyr;Xaa;Xle;Glx;Asx;Gap;Ter|]

    let aminoAcidSymbolSetArray : BioSeq.BioSeq<AminoAcidSymbols.AminoAcidSymbol> = 
        aminoAcidSetArray |> Seq.map AminoAcidSymbols.aminoAcidSymbol

    let testProt : BioSeq.BioSeq<AminoAcid> = 
        seq [|Met;Val;Leu|]

    open Nucleotides

    let nucleotideSetArray : BioSeq.BioSeq<Nucleotide> = 
        seq [|A;T;G;C;U;I;Gap;Ter;R;Y;K;M;S;W;B;D;H;V;N|]

    let testCodingStrand : BioSeq.BioSeq<Nucleotides.Nucleotide> = 
        seq [|A;T;G;G;T;A;C;T;G;A;C|]

    let testCodingStrandRev : BioSeq.BioSeq<Nucleotides.Nucleotide> = 
        testCodingStrand |> Seq.rev

    let testCodingStrandRevComplement : BioSeq.BioSeq<Nucleotides.Nucleotide> = 
        seq [|G;T;C;A;G;T;A;C;C;A;T|]  

    let testTemplateStrand : BioSeq.BioSeq<Nucleotides.Nucleotide> = 
        seq [|T;A;C;C;A;T;G;A;C;T;G|]

    let testTranscript : BioSeq.BioSeq<Nucleotides.Nucleotide> = 
        seq [|A;U;G;G;U;A;C;U;G;A;C|]

    let testTriplets =
        seq [|(T,A,C);(C,A,T);(G,A,C)|]

module BioArray =

    let aminoAcidSetArray = BioSeq.aminoAcidSetArray |> Array.ofSeq
    let aminoAcidSymbolSetArray = BioSeq.aminoAcidSymbolSetArray |> Array.ofSeq
    let testProt = BioSeq.testProt |> Array.ofSeq
    let nucleotideSetArray = BioSeq.nucleotideSetArray |> Array.ofSeq
    let testCodingStrand = BioSeq.testCodingStrand |> Array.ofSeq
    let testCodingStrandRev = BioSeq.testCodingStrandRev |> Array.ofSeq
    let testCodingStrandRevComplement = BioSeq.testCodingStrandRevComplement |> Array.ofSeq
    let testTemplateStrand = BioSeq.testTemplateStrand |> Array.ofSeq
    let testTranscript = BioSeq.testTranscript |> Array.ofSeq
    let testTriplets = BioSeq.testTriplets |> Array.ofSeq

module BioList =

    let aminoAcidSetArray = BioSeq.aminoAcidSetArray |> List.ofSeq
    let aminoAcidSymbolSetArray = BioSeq.aminoAcidSymbolSetArray |> List.ofSeq
    let testProt = BioSeq.testProt |> List.ofSeq
    let nucleotideSetArray = BioSeq.nucleotideSetArray |> List.ofSeq
    let testCodingStrand = BioSeq.testCodingStrand |> List.ofSeq
    let testCodingStrandRev = BioSeq.testCodingStrandRev |> List.ofSeq
    let testCodingStrandRevComplement = BioSeq.testCodingStrandRevComplement |> List.ofSeq
    let testTemplateStrand = BioSeq.testTemplateStrand |> List.ofSeq
    let testTranscript = BioSeq.testTranscript |> List.ofSeq
    let testTriplets = BioSeq.testTriplets |> List.ofSeq