(**
---
title: SASA analyis
category: Algorithms
categoryindex: 2
index: 4
---
*)

(*** hide ***)

(*** condition: prepare ***)

#r "../src/BioFSharp/bin/Release/netstandard2.0/BioFSharp.dll"

open BioFSharp.IO.PDBParser
open BioFSharp.Algorithm.SASA

(**
# Solvent Acessible Surface Area analysis

<i> Summary </i>: This documentation part provides an example and detailed description,
how to use SASA functions in BioFSharp. 

<h2>Abstract:</h2>
Solvent accessible surface Area analysis (SASA) is a standard process to define 
exposed and buried areas.<br>

For that reason we create a accessibility analysis algorithm, which is comparable 
with the Free SASA library in C and Python (<i>Mitternacht,2016</i>). Our Algorithm
is based on the Shrake rupley algorithm to determine which amino acids are accessible
(<i>Shrake&Rupley,1973;Cock et all, 2009</i>). As input, we need a PDB File and 
the implemented <a href = "03_07_pdb.html"> PDB Parser </a>. 
Accessible amino acids are those that are exposed to the solvent and can be modified 
by the labelling reagent. Non accessible amino acids are those that are buried
to the solvent and therefore cannot be modified by the labelling reagent. Using 
the relative SASA for residues we can categorize the amino acids of any protein 
into exposed and buried.

<figure>
    <img src="img/overviewSASA.png" alt="drawing" title="SASA analysis" width="100%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.1. Overview of the SASA analysis</b>: <br> 
            The diagram illustrates the method to differentiate between accessible 
            amino acids and non-accessible in proteins. Some icons are taken from
            BioIcon (<i>Duerr et all, 2024 </i>). 
        </font>
    </figcaption>
</figure>

The implemented SASA algorithm computes the relative SASA of 1HTQ in 
approximately 21 seconds and is with that usable for further AI-driven analysis
of protein structures as well as Proximity analysis.

*)

(**
<br>

# 1. Theoretical Background

<p>
One method that is used often to analyse protein -protein interactions on a large 
scale is MS - based proximity labelling. The labelling of nearby  proteins is influenced 
by the spatial proximity of proteins to the target (bait)protein, but also by other 
factors as the protein abundance in the probe and the reactivity. To analyse the 
effect of the reactivity of the protein on the labeling efficiency the first step, 
is the performance of the SASA analysis on the level of residues in the protein, 
to get knowledge of the aminoacids that are acessible and that, that are 
non-acessible.
</p>
*)

(**
<br>

### 1.1. Principle of the SASA analysis

SASA analysis is a method to determine which amino acids are exposed to the solvent 
and which are buried computing the solvent acessible surface area of a biomolecule. 
We compute the SASA using the Shrake - Rupley algorithm. The idea behind the Algorithm 
is that every atom in the biomolecule is represented as solved sphere. Now the acessible 
surface of the Atom is determined by calculating the van-de-Waals raadi of the atoms. 
The surface of each of the spheres is approximated by creating testpoints, that are 
uniformely distributed over each sphere representing one atom. 
For every Atom the number of testpoints overlapped by another Atom is determined.
A test point is considered overlapped if the distance to the center of the other 
atom is smaller than the effective radius (VDW Radii atom + VDW Raddii probe) of 
the other atom (Fig.2)(<i>Shrake&Rupley,1973</i>).

<figure>
    <img src="img/shrake_Rupley.png" alt="drawing" title="data structure" width="80%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.2. Principle of the Shake-Rupley Algorithm</b>: <br> 
            This figur represents a chematic overview of the principle to compute 
            the solvent acessible Surface area of a Atom on the example of two 
            Atoms. Testpoints that are overlapped by the other Atom are marked red, 
            exposed are marked green.
        </font>
    </figcaption>
</figure>

*)

(**

<br>

### 1.2. The Van der Waals Radius

<p>
To consider if an atom is exposed or buried, we need the van der Waals radius of 
atoms. The van-der Waals radius (vdW) is the experimentally identified  distance 
of the centre of the atom to the outer edge of the electron cloud and is important 
for the calculation of the SASA, because it is used to determine the effective 
radius of the atoms. The effective radius is the sum of the van der Waals radii 
of the atoms and the probe radius. The probe radius is a measure of the size of 
the solvent molecule,  that is used to calculate the SASA. Often the Van der Waals 
Raadi of Bondii are used. In the following Table the most used Boondii Raadii for 
protein atoms are shown (<i>Bondi,1964</i>). 
 
</p>

<table border="1" cellspacing="0" cellpadding="5">
  <caption><strong> Table 1. Van der Waals Radii (Bondi, 1964)</strong>: lierally described van der waals raadi of Bondi </caption>
  <thead>
    <tr>
      <th>Element</th>
      <th>Symbol</th>
      <th>vdW Radius (Å)</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Hydrogen</td>
      <td>H</td>
      <td>1.20</td>
    </tr>
    <tr>
      <td>Carbon</td>
      <td>C</td>
      <td>1.70</td>
    </tr>
    <tr>
      <td>Nitrogen</td>
      <td>N</td>
      <td>1.55</td>
    </tr>
    <tr>
      <td>Oxygen</td>
      <td>O</td>
      <td>1.52</td>
    </tr>
    <tr>
      <td>Sulfur</td>
      <td>S</td>
      <td>1.80</td>
    </tr>
    <tr>
      <td>Phosphorus</td>
      <td>P</td>
      <td>1.80</td>
    </tr>
    <tr>
      <td>Water</td>
      <td>H<sub>2</sub>O</td>
      <td>1.4</td>
    </tr>
    <tr>
        <td>Biotin</td>
        <td></td>
        <td>3.7</td>
  </tbody>
</table>

<p>
ProtOr raadi are a variant of the van der Waals radii, which are determined specially 
for proteins. Van der Waals radii lead in proteins to different packing densities, which can lead 
to inaccuracies in the calculation of the solvent accessible surface area. The 
ProtOr radii are determined by the packing density of proteins and are used 
to calculate the SASA more accurately because they consider the specific packing 
density of proteins through different protein environments. For that reason the protor describes the complete atomgroup,e.g. the methyl group (<i>Tsai et al,1999</i>). An example is shown in Fig.3 (<i>Tsai et al,1999</i>).

</p>

<figure>
    <img src="img/alanin_protor.png" alt="drawing" title="vdw_example" width="80%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.3. Protor radii example alanin</b>: <br> 
           Shown is how to determine single protor radii on the example of alanin. 
           The different atom groups are shown in different colors and the Protor 
           radii are shown as spheres around the atoms. 
        </font>
    </figcaption>
</figure>



<table border="1" cellspacing="0" cellpadding="5">
    <caption> <strong> Table 2. ProtOR Raddi (Tsai et al., 1999)</strong>: 
    literally Described ProtOR raadi and to which atomname they are assigned to  
    </caption>
  <thead>
    <tr>
      <th>Atomic Group</th>
      <th>ProtOr Radius (Å)</th>
      <th>Atom Names</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>C3H0</td>
      <td>1.61</td>
      <td>C (backbone C), CG (ASN/ASP), CD (GLN/GLU)</td>
    </tr>
    <tr>
      <td>C3H1</td>
      <td>1.76</td>
      <td>Aromatic CH: CG, CD1, CD2, CE1, CE2, CZ (PHE/TYR/TRP)</td>
    </tr>
    <tr>
      <td>C4H1</td>
      <td>1.88</td>
      <td>Tetrahedral CH: CA (α-C), CG1 (ILE), CD1 (LEU)</td>
    </tr>
    <tr>
      <td>C4H2</td>
      <td>1.88</td>
      <td>Methylene: CB (most), CG2 (ILE/VAL), CH2 (TRP)</td>
    </tr>
    <tr>
      <td>C4H3</td>
      <td>1.88</td>
      <td>Methyl: CB (ALA), CD1/CD2 (LEU), CE (MET), CG2 (THR/VAL)</td>
    </tr>
    <tr>
      <td>N3H1</td>
      <td>1.64</td>
      <td>Backbone N, ND1/NE
    </tr>
    </table>

*)

(**

<br>

# 2. Extract Residues from Proteins:

<p>
The first step of all SASA functions is, that they extract Residues from a 
parsed PDB File. The PDB file is parsed using the <b> readStructure </b> function 
of the PDB Parser module. The function <b> getResiduesPerChain </b>
has a short helper function inside, that has as input the residuearray and removes 
all HETATM entries as well as multiple Alternative Locations for the same Atom 
in one residue. To decide, which Location for one atom is kept we use two criteria’s:  
 </p>

<ol>
<li> the atom with the best occupancy </li> 
<li> if two atoms have the same occupancy, we take the one with the AltLoc 'A' or ' ' 
(empty) </li>
</ol>
(Fig.4)

<figure>
    <img src="img/altLoc.png" alt="drawing" title="isolate single altloc" width="100%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.4. Filter best Alternative Location</b>: <br> 
            Shown is a schematic illustration of the steps to keep per chain and 
            residue only one atom with the same atomname, when different alternative 
            Locations exist.
        </font>
    </figcaption>
</figure>

The function call looks as follows, whereby the use of the residue reader is 
exemplary for the residuearray:

*)

let residuearray = readPBDFile ("data/rubisCOActivase.pdb") |> readResidues 
filterBestAltLoc residuearray

(**
<details>
<summary> If you just want to clear already extracted residues from Alternative 
Locations, you  can use this function. 
Here on the example of the residuearray shown above. </summary>
*)

(***hide***)
(filterBestAltLoc residuearray) 
(***include-it:***)


(**
</details>
*)

(**
<details>
<summary> Click here to see how the proof looks like to see that some data are 
removed </summary>
*)

(filterBestAltLoc residuearray) <> residuearray
(***include-it:***)

(**
</details>
*)

(**
<p>
The main function is the <b> getResiduePerChain </b> function, which 
has as input the path as well as the modelid of the model to isolate. The output 
is a <char,residuearray> Dictionary. The char represents the chainID and the 
residuearray the filtered residues present in this chain (Fig.5).
</p>

<figure>
    <img src="img/extractResidues.png" alt="drawing" title="create Dictionary 
    of extracted residues" width="100%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.5. Dictionary of extracted residues per chain id:</b>: <br> 
            Shown is a schematic illustration of the steps to create a dictionary 
            with the chainid as key and the residues as value. The residues are 
            filtered using the filterBestAltLoc function.
        </font>
    </figcaption>
</figure>

*)

getResiduesPerChain ("data/rubisCOActivase.pdb") 1

(**
<details>
<summary> Click here to look how the list looks like on the Example RubisCO Activase, 
which contains only one model and for every Atom exactly one alternative Location. 
The first results represent the dictionary where all chains are stored and the 
snd Array represents all Residues present in Chain A of the RubisCO Activase.
 </summary>
*)

(***hide:***)
let dic = getResiduesPerChain ("data/rubisCOActivase.pdb") 1
(***include-value:dic***)

(***hide***)
dic.['A']
(***include-it:***)

(**
</details>
*)

(**

# 3. Compute Solvent acessible surface area: 

<p>
The function explained in chapter 2, is called in all SASA Algorithm functions 
and used as basis. As already explained in Chapter 1.1 every SASA algorithm is 
based on four main steps:
</p>

<ol>
<li> The lookup of the van der Waals radius of the atoms and the computation of 
the effective radius </li> 
<li> the creation of sphere points  </li>
<li> the computation of the distance between sphere points and the atom centre 
of the protein. </li>
<li> categorize test points based on distance and effective radius in buried and 
exposed </li>
</ol>

<p>
<b>To 1</b>:
The effective van der Waals Radius is the sum of the choosen probe radius 
and the  vdw radius / ProtOR radius of the corresponding Atom. Raadi that are 
used for the computation can be looked up and are explained in Table 1 and 2 of 
Chapter 1.3 (VDW Radius). <br>
<b>To 2</b>: Test points are computed  on the atoms using 
the mathematical Shrake Rupley algorithm (<i>Shrake&Rupley,1973</i>). Scaling the 
test points on the atoms surface is done by sum of the coordinates of that atom 
and the product of the test points  coordinate and the effective van der waals radii 
of the atom.<br>
<b>To 3</b>: The distance between two 3D points is computed using the Euclidian distance,
that is the root of all squared single distances <br>
<b>To 4</b>: A test point is considered buried / non  accessible if the distance to the 
centre of the other atom is smaller than the effective radius of the other atom 
(Fig.2) (<i>Shrake&Rupley,1973</i>). For the complete atom we compare all test 
points against other atoms and determine the sum of exposed test points.

</p>

<p>
SASA is one method to determine which amino acids are exposed to the solvent 
and which are buried. The SASA is the area of a biomolecule that is accessible 
for the solvent.
</p>

*)

(**

<br>

### 3.1. Compute SASA for every Atom:

<p>
If you especially want to know which atom is mostly accessible you need to call 
the <b> sasaAtom </b> function. The function has four variables as input: The 
file path of the PDB File to analyse, the modelid of the model of interest,
the number of Test points per Atom and the name of the probe or the value if the
probe is not common. The output is a dictionary with the chain id as key and another 
dictionary as value. The inner dictionary has the residue number and name as key 
and the SASA array as value (Fig.6).
</p>

<figure>
    <img src="img/sasaAtom.png" alt="drawing" title="sasa atom" width="100%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.6. Result of sasaAtom</b>: <br> 
            Shown is the structure of the dictionary representing the result 
            of the compute sasa Atom function. 
        </font>
    </figcaption>
</figure>

<p>
The sasa Per Atom which will be shown in the output dictionary is the ratio of 
the number of accessible points per atom and the total number of test points pro atom
(numberOfTestpoints). The ratio is then transformed in area with the mathematical
area formula for spheres (4pi*(eff_radius) <sup>2</sup>).
</p>

*)

let filepath = ("data/rubisCOActivase.pdb")
let modelid = 1
let numberOfTestpoints = 100
let probe = "Water"

sasaAtom filepath modelid numberOfTestpoints probe

(**

<details>
<summary> On this example of RubisCO Activase, you could see how the output of 
the computed sasa per Atoms look like, when the probe is water. The first output 
is a dictionary with the chain id and the integrated snd dictionary and the snd 
output shows how the SASA per Atom would look like on the example of the residue
65,ASN in chain A of RubisCO Activase </summary>.
*)

(***hide***)
let atomSASA = sasaAtom filepath modelid numberOfTestpoints probe
(***include-value:atomSASA***)

atomSASA.['A'].[(65, "ASN")]
(***include-it:***)

(**
</details>
*)

(**

### 3.2. Compute the SASA of residues

For Residues, in case of proteins the amino acids, two different SASA Values can 
be computed: The absolute SASA and the relative SASA.

#### 3.2.1. Absolute Residue SASA:

<p>
The absolute SASA for Residues is the Sum of all atom SASA for the corresponding 
Residues. For that reason, we have for the <b> sasaResidue </b> function, the same 
inputs as for the sasaAtom function. The output is a dictionary with the chain id as key and another dictionary 
as value. The inner dictionary is a dictionary with the Key that is a tuple of 
Residue number and Residue name, and a Value that is the absolute SASA of the residue (Fig.7).
</p>

<figure>
    <img src="img/sasaResidue.png" alt="drawing" title="residue sasa structure" width="100%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.7. Result of sasaAtom</b>: <br> 
            Shown is the structure of the dictionary representing the result of 
            the compute sasa Atom function.b) shows an example of such a residue 
            Sasa  dictionary computed for PDB id - 4w5w. 
        </font>
    </figcaption>
</figure>

<p>
If you just want to compare the absolute solvent Area for every residue, you can 
use the following function call:
</p>
*)

sasaResidue filepath modelid numberOfTestpoints probe

(**
<details>
<summary> Click here to see an exemplary output for sasaResidues, applied to 
RubisCO Activase. The first output is a dictionary with the chain id and the 
integrated secondd dictionary and the Second output shows how the SASA would look 
like in chain A of RubisCO Activase. </summary>
*)

(***hide***)
let residueSASA = sasaResidue filepath modelid numberOfTestpoints probe
(***include-value:residueSASA***)

residueSASA.['A']
(***include-it:***)

(**
</details>
*)

(**
<br>

#### 3.2.2. Relative Residue SASA: 

<p>
The relative SASA (rSASA) is often used because this is a Value to predict if a 
residue is accessible or if it is buried. rSASA is the ratio of the absolute SASA 
and the maximal SASA of the corresponding residue. 
The Max SASA is the absolute SASA of the amino acid in fully extended Conformation 
in Ala - X - Ala or Gly - X - Gly Tripeptides. In our algorithm, we compute the
maxSASA for the twenty Amino acids using the exact same parameters as in the 
<b> sasaResidue </b> function, when we set the variable fixedMaxSASA as false 
The maxSASA Values for one hundred test points are for example:
</p>

*)

(***hide***)
maxSASA 1 100
(***include-it:***)

(**

<p>
In Comparison with the Free SASA relative SASAs we see small differences. The reason 
for that is, that Free SASA uses fix max SASA values, that are computed with 
water as probe. If you want to have the exact same results to compare, just set the last variable 
fixedMaxSASA as True. 
To get the relative residue SASA for a model in the PDB File
as decimal, you can call the following function.
</p>
*)

relativeSASA_aminoacids filepath modelid numberOfTestpoints probe false

(**

<details>

<summary> Click here to see an exemplary output for relativeSASA_aminoacids, 
applied to RubisCO Activase. The first output is a dictionary with the chain id 
and the integrated second dictionary and the Second output shows how the relative 
SASA would be using computed maxSASA.</summary>

*)

(***hide***)
let rSASA = relativeSASA_aminoacids filepath modelid numberOfTestpoints probe false 
(***include-value:rSASA***)

rSASA.['A']
(***include-it:***)

(**
</details>
*)

(**

### 3.3. Compute the SASA of chains:

<p>
Sometimes you are interested in the SASA of a whole chain, e.g. when you want to 
analyse protein-protein interaction. The function <b>sasaChain</b> computes the 
sum of all residues SASA in the Chain. The result is again a dictionary with the 
chain id as key and the SASA of the chain as value. To call the function you need the same 
inputs as for sasaResidue.
</p>

*)

sasaChain filepath modelid numberOfTestpoints probe

(**
<details>
<summary> Click here to look how the dictionary output would look like on the example
of 1HTQ, which has multiple chains.</summary>
*)

(***hide***)
sasaChain ("data/htq.pdb") 1 100 probe
(***include-it:***)

(**
</details>
*)

(**

<br>

# 4. Application example of relative SASA usage:

<p>
One example to use the relative SASA is the classification of residues in a protein
into exposed and buried. This is done by the choice of a threshold and the computation
of the relative SASA. is the relative SASA lower as the threshold they are buried, 
and when they are higher as the threshold the residue is exposed. The threshold 
itself is mostly between 0.2 and 0.3 in literature. We create a function 
<b>differentiateAccessibleAA</b> you can call that has as input again 
filepath,modelid and number of points and additional the chosen threshold. 
The output is a dictionary, containing as key the chain id and as Value two diverse types 
of dictionaries that show which are exposed and which are buried.

</p>

*)

let threshold = 0.2

differentiateAccessibleAA filepath modelid numberOfTestpoints  probe threshold  false

(**
<details>
<summary> Click here to see an example how we use the relative SASA to 
categorize amino Acids of RubisCO Activase in buried and exposed.</summary>

*)

(***hide***)
differentiateAccessibleAA filepath modelid numberOfTestpoints probe threshold false
(***include-it:***)

(**
</details>
*)


(**

<br>

#  Bibliography 

<ul style="list-style-type: none;">
    <li>Bondi, A. "Van Der Waals Volumes and Radii". The Journal of Physical 
    Chemistry 68, no. 3 (March 1964): 441-51. https://doi.org/10.1021/j100785a001.</li>
    <li>Cock, Peter J. A., Tiago Antao, Jeffrey T. Chang, Brad A. Chapman, Cymon J.
    Cox, Andrew Dalke, Iddo Friedberg, u.a. &quotBiopython: Freely Available 
    Python Tools for Computational Molecular Biology and Bioinformatics&quot. 
    Bioinformatics 25, Nr. 11 (1. Juni 2009): 1422 &#45 23. 
    https://doi.org/10.1093/bioinformatics/btp163.</li>
    <li> Duerr, Simon, Kanaren,Kathryn, Derek Croote, Emmett Leddin, and Jeroen 
    Van Goey. "duerrsimon/bioicons: April24". Zenodo, 25. April 2024. 
    https://doi.org/10.5281/ZENODO.11068293.</li>
    <li>Mitternacht, Simon. &quotFreeSASA: An Open Source C Library for Solvent 
    Accessible Surface Area Calculations&quot. F1000Research 5 (18 February 2016): 
    189. https://doi.org/10.12688/f1000research.7931.1.</li>
    <li>Shrake, A., and J.A. Rupley. &quotEnvironment and Exposure to Solvent 
    of Protein Atoms. Lysozyme and Insulin&quot. Journal of Molecular Biology 79,
    Nr. 2 (September 1973): 351 &#45 71. https://doi.org/10.1016/0022-2836(73)90011-9. </li>
    <li>Tsai, Jerry, Robin Taylor, Cyrus Chothia, and Mark Gerstein. ‘The Packing Density in Proteins: Standard Radii and Volumes 1 1Edited by J. M. Thornton’. Journal of Molecular Biology 290, no. 1 (July 1999): 253–66. https://doi.org/10.1006/jmbi.1999.2829.</li>
</ul>
*)