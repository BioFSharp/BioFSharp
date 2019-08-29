(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I @"../../bin/BioFSharp/net47/"
#I @"../../bin/BioFSharp.BioDB/net45/"
#I @"../../bin/BioFSharp.ImgP/net47"
#I @"../../bin/BioFSharp.IO/net47/"
#I @"../../bin/BioFSharp.Parallel/net47/"
#I @"../../bin/BioFSharp.Stats/net47/"
#I @"../../bin/BioFSharp.Vis/net47/"
#r @"../../lib/Formatting/FSharp.Plotly.dll"
#r "BioFSharp.dll"
#r "BioFSharp.IO.dll"
#r "FSharpAux.dll"
open BioFSharp
open FSharp.Plotly
open FSharpAux

(**
<table class="HeadAPI">
<td class="Head"><h1>BioCollections</h1></td>
<td class="API">
    <a id="APILink" href="https://csbiology.github.io/BioFSharp/reference/biofsharp-bioseq.html" >&#128194;View BioSeq documentation</a>
</td>
</table>
Analogous to the build-in collections BioFSharp provides BioSeq, BioList and BioArray for individual collection specific optimized operations. 
The easiest way to create them are the `ofBioItemString` -functions
*)


let s1 = "PEPTIDE" |> BioSeq.ofAminoAcidString 
let s2 = "PEPTIDE" |> BioList.ofAminoAcidSymbolString 
let s3 = "TAGCAT"  |> BioArray.ofNucleotideString 

(***do-not-eval***)
///Peptide represented as a Bioseq
"PEPTIDE" |> BioSeq.ofAminoAcidString 

///Peptide represented as a BioList
"PEPTIDE"|> BioList.ofAminoAcidSymbolString 

///Nucleotide sequence represented as a BioArray
"TAGCAT" |> BioArray.ofNucleotideString 

(**Resulting BioSeq containing our peptide:*)
(*** include-value:s1 ***)
(**Resulting BioList containing our peptide:*)
(*** include-value:s2 ***)
(**Resulting BioArray containing our oligonucleotide:*)
(*** include-value:s3 ***)

(**
##Basics
Some functions which might be needed regularly are defined to work with nucleotides and amino acids:
*)

let myPeptide = "PEPTIDE" |> BioSeq.ofAminoAcidString 

(*** include-value:myPeptide ***)

let myPeptideFormula = BioSeq.toFormula myPeptide |> Formula.toString 

(*** include-value:myPeptideFormula ***)

let myPeptideMass = BioSeq.toAverageMass myPeptide 

(*** include-value:myPeptideMass ***)

(**
##AminoAcids
###Digestion
BioFSharp also comes equipped with a set of tools aimed at cutting apart amino acid sequences. To demonstrate the usage, we'll throw some `trypsin` at the small RuBisCO subunit of _Arabidopos thaliana_:  
In the first step, we define our input sequence and the protease we want to use.
*)

let RBCS = 
    """MASSMLSSATMVASPAQATMVAPFNGLKSSAAFPATRKANNDITSITSNGGRVNCMQVWP
    PIGKKKFETLSYLPDLTDSELAKEVDYLIRNKWIPCVEFELEHGFVYREHGNSPGYYDGR
    YWTMWKLPLFGCTDSAQVLKEVEECKKEYPNAFIRIIGFDNTRQVQCISFIAYKPPSFT""" 
    |> BioArray.ofAminoAcidString

let trypsin = Digestion.Table.getProteaseBy "Trypsin"

(**
With these two things done, digesting the protein is a piece of cake. For doing this, just use the `digest` function.  
*)
let digestedRBCS = Digestion.BioArray.digest trypsin 0 RBCS 

(**
<br>
<button type="button" class="btn" data-toggle="collapse" data-target="#digested">Show/Hide digestion result</button>
<div id="digested" class="collapse digested">
<pre>
val digestedRBCS : Digestion.DigestedPeptide [] =
  [|{ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 1;
     MissCleavageEnd = 28;
     PepSequence =
      [Met; Ala; Ser; Ser; Met; Leu; Ser; Ser; Ala; Thr; Met; Val; Ala; Ser;
       Pro; Ala; Gln; Ala; Thr; Met; Val; Ala; Pro; Phe; Asn; Gly; Leu; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 29;
     MissCleavageEnd = 37;
     PepSequence = [Ser; Ser; Ala; Ala; Phe; Pro; Ala; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 38;
     MissCleavageEnd = 38;
     PepSequence = [Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 39;
     MissCleavageEnd = 52;
     PepSequence =
      [Ala; Asn; Asn; Asp; Ile; Thr; Ser; Ile; Thr; Ser; Asn; Gly; Gly; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 53;
     MissCleavageEnd = 64;
     PepSequence =
      [Val; Asn; Cys; Met; Gln; Val; Trp; Pro; Pro; Ile; Gly; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 65;
     MissCleavageEnd = 65;
     PepSequence = [Lys];}; {ProteinID = 0;
                             MissCleavages = 0;
                             MissCleavageStart = 66;
                             MissCleavageEnd = 66;
                             PepSequence = [Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 67;
     MissCleavageEnd = 83;
     PepSequence =
      [Phe; Glu; Thr; Leu; Ser; Tyr; Leu; Pro; Asp; Leu; Thr; Asp; Ser; Glu;
       Leu; Ala; Lys];}; {ProteinID = 0;
                          MissCleavages = 0;
                          MissCleavageStart = 84;
                          MissCleavageEnd = 90;
                          PepSequence = [Glu; Val; Asp; Tyr; Leu; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 91;
     MissCleavageEnd = 92;
     PepSequence = [Asn; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 93;
     MissCleavageEnd = 108;
     PepSequence =
      [Trp; Ile; Pro; Cys; Val; Glu; Phe; Glu; Leu; Glu; His; Gly; Phe; Val;
       Tyr; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 109;
     MissCleavageEnd = 120;
     PepSequence =
      [Glu; His; Gly; Asn; Ser; Pro; Gly; Tyr; Tyr; Asp; Gly; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 121;
     MissCleavageEnd = 126;
     PepSequence = [Tyr; Trp; Thr; Met; Trp; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 127;
     MissCleavageEnd = 140;
     PepSequence =
      [Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp; Ser; Ala; Gln; Val; Leu; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 141;
     MissCleavageEnd = 146;
     PepSequence = [Glu; Val; Glu; Glu; Cys; Lys];}; {ProteinID = 0;
                                                      MissCleavages = 0;
                                                      MissCleavageStart = 147;
                                                      MissCleavageEnd = 147;
                                                      PepSequence = [Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 148;
     MissCleavageEnd = 155;
     PepSequence = [Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 156;
     MissCleavageEnd = 163;
     PepSequence = [Ile; Ile; Gly; Phe; Asp; Asn; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 164;
     MissCleavageEnd = 180;
     PepSequence =
      [Gln; Val; Gln; Cys; Ile; Ser; Phe; Ile; Ala; Tyr; Lys; Pro; Pro; Ser;
       Phe; Thr; Gly];}|]
</pre>
<button type="button" class="btn" data-toggle="collapse" data-target="#digested">Hide again</button>  

</div>
<br>
In reality, proteases don't always completely cut the protein down. Instead, some sites stay intact and should be considered for in silico analysis. 
This can easily be done with the `concernMissCleavages` function. It takes the minimum and maximum amount of misscleavages you want to have and also the digested protein. As a result you get all possible combinations arising from this information.
*)
let digestedRBCS' = Digestion.BioArray.concernMissCleavages 0 2 digestedRBCS

(**
<button type="button" class="btn" data-toggle="collapse" data-target="#digested2">Show/Hide digestion result</button>
<div id="digested2" class="collapse digested2">
<pre>
val digestedRBCS' : Digestion.DigestedPeptide [] =
  [|{ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 164;
     MissCleavageEnd = 180;
     PepSequence =
      [Gln; Val; Gln; Cys; Ile; Ser; Phe; Ile; Ala; Tyr; Lys; Pro; Pro; Ser;
       Phe; Thr; Gly];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 156;
     MissCleavageEnd = 163;
     PepSequence = [Ile; Ile; Gly; Phe; Asp; Asn; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 148;
     MissCleavageEnd = 155;
     PepSequence = [Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 147;
     MissCleavageEnd = 147;
     PepSequence = [Lys];}; {ProteinID = 0;
                             MissCleavages = 0;
                             MissCleavageStart = 141;
                             MissCleavageEnd = 146;
                             PepSequence = [Glu; Val; Glu; Glu; Cys; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 127;
     MissCleavageEnd = 140;
     PepSequence =
      [Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp; Ser; Ala; Gln; Val; Leu; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 121;
     MissCleavageEnd = 126;
     PepSequence = [Tyr; Trp; Thr; Met; Trp; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 109;
     MissCleavageEnd = 120;
     PepSequence =
      [Glu; His; Gly; Asn; Ser; Pro; Gly; Tyr; Tyr; Asp; Gly; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 93;
     MissCleavageEnd = 108;
     PepSequence =
      [Trp; Ile; Pro; Cys; Val; Glu; Phe; Glu; Leu; Glu; His; Gly; Phe; Val;
       Tyr; Arg];}; {ProteinID = 0;
                     MissCleavages = 0;
                     MissCleavageStart = 91;
                     MissCleavageEnd = 92;
                     PepSequence = [Asn; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 84;
     MissCleavageEnd = 90;
     PepSequence = [Glu; Val; Asp; Tyr; Leu; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 67;
     MissCleavageEnd = 83;
     PepSequence =
      [Phe; Glu; Thr; Leu; Ser; Tyr; Leu; Pro; Asp; Leu; Thr; Asp; Ser; Glu;
       Leu; Ala; Lys];}; {ProteinID = 0;
                          MissCleavages = 0;
                          MissCleavageStart = 66;
                          MissCleavageEnd = 66;
                          PepSequence = [Lys];}; {ProteinID = 0;
                                                  MissCleavages = 0;
                                                  MissCleavageStart = 65;
                                                  MissCleavageEnd = 65;
                                                  PepSequence = [Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 53;
     MissCleavageEnd = 64;
     PepSequence =
      [Val; Asn; Cys; Met; Gln; Val; Trp; Pro; Pro; Ile; Gly; Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 39;
     MissCleavageEnd = 52;
     PepSequence =
      [Ala; Asn; Asn; Asp; Ile; Thr; Ser; Ile; Thr; Ser; Asn; Gly; Gly; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 38;
     MissCleavageEnd = 38;
     PepSequence = [Lys];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 29;
     MissCleavageEnd = 37;
     PepSequence = [Ser; Ser; Ala; Ala; Phe; Pro; Ala; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 0;
     MissCleavageStart = 1;
     MissCleavageEnd = 28;
     PepSequence =
      [Met; Ala; Ser; Ser; Met; Leu; Ser; Ser; Ala; Thr; Met; Val; Ala; Ser;
       Pro; Ala; Gln; Ala; Thr; Met; Val; Ala; Pro; Phe; Asn; Gly; Leu; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 156;
     MissCleavageEnd = 180;
     PepSequence =
      [Ile; Ile; Gly; Phe; Asp; Asn; Thr; Arg; Gln; Val; Gln; Cys; Ile; Ser;
       Phe; Ile; Ala; Tyr; Lys; Pro; Pro; Ser; Phe; Thr; Gly];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 148;
     MissCleavageEnd = 163;
     PepSequence =
      [Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg; Ile; Ile; Gly; Phe; Asp; Asn;
       Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 147;
     MissCleavageEnd = 155;
     PepSequence = [Lys; Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 141;
     MissCleavageEnd = 147;
     PepSequence = [Glu; Val; Glu; Glu; Cys; Lys; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 127;
     MissCleavageEnd = 146;
     PepSequence =
      [Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp; Ser; Ala; Gln; Val; Leu; Lys;
       Glu; Val; Glu; Glu; Cys; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 121;
     MissCleavageEnd = 140;
     PepSequence =
      [Tyr; Trp; Thr; Met; Trp; Lys; Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp;
       Ser; Ala; Gln; Val; Leu; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 109;
     MissCleavageEnd = 126;
     PepSequence =
      [Glu; His; Gly; Asn; Ser; Pro; Gly; Tyr; Tyr; Asp; Gly; Arg; Tyr; Trp;
       Thr; Met; Trp; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 93;
     MissCleavageEnd = 120;
     PepSequence =
      [Trp; Ile; Pro; Cys; Val; Glu; Phe; Glu; Leu; Glu; His; Gly; Phe; Val;
       Tyr; Arg; Glu; His; Gly; Asn; Ser; Pro; Gly; Tyr; Tyr; Asp; Gly; Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 91;
     MissCleavageEnd = 108;
     PepSequence =
      [Asn; Lys; Trp; Ile; Pro; Cys; Val; Glu; Phe; Glu; Leu; Glu; His; Gly;
       Phe; Val; Tyr; Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 84;
     MissCleavageEnd = 92;
     PepSequence = [Glu; Val; Asp; Tyr; Leu; Ile; Arg; Asn; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 67;
     MissCleavageEnd = 90;
     PepSequence =
      [Phe; Glu; Thr; Leu; Ser; Tyr; Leu; Pro; Asp; Leu; Thr; Asp; Ser; Glu;
       Leu; Ala; Lys; Glu; Val; Asp; Tyr; Leu; Ile; Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 66;
     MissCleavageEnd = 83;
     PepSequence =
      [Lys; Phe; Glu; Thr; Leu; Ser; Tyr; Leu; Pro; Asp; Leu; Thr; Asp; Ser;
       Glu; Leu; Ala; Lys];}; {ProteinID = 0;
                               MissCleavages = 1;
                               MissCleavageStart = 65;
                               MissCleavageEnd = 66;
                               PepSequence = [Lys; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 53;
     MissCleavageEnd = 65;
     PepSequence =
      [Val; Asn; Cys; Met; Gln; Val; Trp; Pro; Pro; Ile; Gly; Lys; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 39;
     MissCleavageEnd = 64;
     PepSequence =
      [Ala; Asn; Asn; Asp; Ile; Thr; Ser; Ile; Thr; Ser; Asn; Gly; Gly; Arg;
       Val; Asn; Cys; Met; Gln; Val; Trp; Pro; Pro; Ile; Gly; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 38;
     MissCleavageEnd = 52;
     PepSequence =
      [Lys; Ala; Asn; Asn; Asp; Ile; Thr; Ser; Ile; Thr; Ser; Asn; Gly; Gly;
       Arg];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 29;
     MissCleavageEnd = 38;
     PepSequence = [Ser; Ser; Ala; Ala; Phe; Pro; Ala; Thr; Arg; Lys];};
    {ProteinID = 0;
     MissCleavages = 1;
     MissCleavageStart = 1;
     MissCleavageEnd = 37;
     PepSequence =
      [Met; Ala; Ser; Ser; Met; Leu; Ser; Ser; Ala; Thr; Met; Val; Ala; Ser;
       Pro; Ala; Gln; Ala; Thr; Met; Val; Ala; Pro; Phe; Asn; Gly; Leu; Lys;
       Ser; Ser; Ala; Ala; Phe; Pro; Ala; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 148;
     MissCleavageEnd = 180;
     PepSequence =
      [Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg; Ile; Ile; Gly; Phe; Asp; Asn;
       Thr; Arg; Gln; Val; Gln; Cys; Ile; Ser; Phe; Ile; Ala; Tyr; Lys; Pro;
       Pro; Ser; Phe; Thr; Gly];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 147;
     MissCleavageEnd = 163;
     PepSequence =
      [Lys; Glu; Tyr; Pro; Asn; Ala; Phe; Ile; Arg; Ile; Ile; Gly; Phe; Asp;
       Asn; Thr; Arg];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 141;
     MissCleavageEnd = 155;
     PepSequence =
      [Glu; Val; Glu; Glu; Cys; Lys; Lys; Glu; Tyr; Pro; Asn; Ala; Phe; Ile;
       Arg];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 127;
     MissCleavageEnd = 147;
     PepSequence =
      [Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp; Ser; Ala; Gln; Val; Leu; Lys;
       Glu; Val; Glu; Glu; Cys; Lys; Lys];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 121;
     MissCleavageEnd = 146;
     PepSequence =
      [Tyr; Trp; Thr; Met; Trp; Lys; Leu; Pro; Leu; Phe; Gly; Cys; Thr; Asp;
       Ser; Ala; Gln; Val; Leu; Lys; Glu; Val; Glu; Glu; Cys; Lys];};
    {ProteinID = 0;
     MissCleavages = 2;
     MissCleavageStart = 109;
     MissCleavageEnd = 140;
     PepSequence = [Glu; His; Gly; Asn; Ser; Pro; ...];}; ...|]
</pre>
<button type="button" class="btn" data-toggle="collapse" data-target="#digested2">Hide again</button>  
</div>
<br>
##Nucleotides

![Nucleotides1](img/Nucleotides.svg)

**Figure 1: Selection of covered nucleotide operations** (A) Bilogical principle. (B) Workflow with `BioSeq`. (C) Other covered functionalities.


Let's imagine you have a given gene sequence and want to find out what the according protein might look like.
*)
let myGene = BioSeq.ofNucleotideString "ATGGCTAGATCGATCGATCGGCTAACGTAA"

(*** include-value:myGene ***)

(**
Yikes! Unfortunately we got the 5'-3' coding strand. For proper transcription we should get the complementary strand first:
*)
let myProperGene = Seq.map Nucleotides.complement myGene

(*** include-value:myProperGene ***)

(**
Now let's transcribe and translate it:
*)

let myTranslatedGene = 
    myProperGene
    |> BioSeq.transcribeTemplateStrand
    |> BioSeq.translate 0

(*** include-value:myTranslatedGene ***)

(**
Of course, if your input sequence originates from the coding strand, you can directly transcribe it to mRNA since the 
only difference between the coding strand and the mRNA is the replacement of 'T' by 'U' (Figure 1B)
*)

let myTranslatedGeneFromCodingStrand = 
    myGene
    |> BioSeq.transcribeCodingStrand
    |> BioSeq.translate 0

(*** include-value:myTranslatedGeneFromCodingStrand ***)

(**
Other Nucleotide conversion operations are also covered:
*)

let mySmallGene      = BioSeq.ofNucleotideString  "ATGTTCCGAT"

let smallGeneRev     = BioSeq.reverse mySmallGene 
//Original: ATGTTCCGAT
//Output:   TAGCCTTGTA

let smallGeneComp    = BioSeq.complement mySmallGene
//Original: ATGTTCCGAT
//Output:   TACAAGGCTA

let smallGeneRevComp = BioSeq.reverseComplement mySmallGene
//Original: ATGTTCCGAT
//Reverse:  TAGCCTTGTA
//Output:   ATCGGAACAT

