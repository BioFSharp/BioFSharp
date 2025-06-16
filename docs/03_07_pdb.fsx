(**
---
title: PDB
category: BioParsers
categoryindex: 3
index: 6
---
*)

(*** hide ***)

(*** condition: prepare ***)


#r "../src/BioFSharp/bin/Release/netstandard2.0/BioFSharp.dll"

open BioFSharp.IO.PDBParser
open BioFSharp.FileFormats.PDBParser

(**
# PDBParser

<i> Summary </i>: This part of the documentation provides an detailed overview 
of how to parse PDB files using the BioFSharp. Parts of the Codes are optimized,
for the improvement of memory and speed, with the help of ChatGPT (OpenAI,2024).
The implemented PDB parser functions are completely tested using Expecto for 
Unittesing and Benchmarkdotnet for the Performance testing.

<h2>Abstract</h2>

Protein Data Bank (PDB) file format is the standard format for representing 
three-dimensional structures of biological macromolecules, such as proteins and
nucleic acids, along with additional metadata information. The PDB format is 
commonly stored in text files with the extension `.pdb. and serve as a foundation 
for computational biology and structural bioinformatics applications. Accurate 
extraction and processing of pdb data are essential for tasks such as structural
analysis and AI-driven protein folding predictions in bioinformatics.<br>

For that reason we implement a parser for the PDB file format in BioFSharp,
using the approach of Haskell by M.j.Gajda (hPDB) as template 
(<i>Gaja, 2010</i>; <i>Gaja, 2013</i>) 
that read the PDB file format and structure the data in the types metadata, atom,
residue, chain, model and complete structure (Fig.1). <br>

<figure>
    <img src="img/pdb_reader.png" alt="drawing" title="PDB parser function" 
    width="60%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.1. Overview of a PDB parser's function</b>: <br> 
            The diagram illustrates the function of a PDB file writer and a 
            PDB file reader. Some icons are taken from BioIcon 
            (<i>Duerr et all, 2024 </i>).
        </font>
    </figcaption>
</figure>

The implemented PDB parser reads
and structure the data of 1HTQ in approximately 18 seconds and is with that 
usable for further AI-driven analysis of protein structures.

*)

(**

<br>

# 1.Theoretical Background

### 1.1.Protein structure modelling 

<p>
A protein is a functional and structural important part in Living cells. 
In general it is a Bio macromolecule from amino acid residues covalently linked 
together by peptide bonds. The structure of proteins is organized in four 
hierarchical levels

<ul>
   <li> <b> primary structure </b>: Sequence of covalently linked amino acid 
   residues </li>
   <li> <b>secoundary structure </b>: Local conformation of a polypeptide chain,
   typically &#945; helices or &#946; strands </li>
   <li ><b> tertiary structure </b>: 3D structure of a protein, describing the 
   average relative position of all atoms present, including bond lengths and 
   bond angles </li>
    <li> <b> Quaternary structure </b>: Spatial structure of a protein complex
    consisting of more than one polypeptide chain </li>
</ul>

The structure of a protein is especially important for the functionality of a 
protein because it defines possible protein-protein interactions and play additional 
a role in protein folding. For that reasons the protein structure information’s, 
often identified experimentally e.g. by X ray microscopy, are stored in the PDB 
file format, (<i>Campbell et all,2016</i>) (Fig 2).
</p>

<figure>
    <img src="img/figure_protstruct.png" alt="drawing" title="protein structure 
    modell" width="60%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.2. protein structure modell</b>: <br> 
            Shown is the protein structure modell. Structure is hierarchically 
            ordered in Primary, Secoundary, Tertiary and Quarternary. 
            Informations about Structures experimentally identified are stored 
            in the central protein data bank. Icons are taken from Bioicons and 
            the PDB (<i>Duerr et all, 2024; Berman et all, 2003</i>).
        </font>
    </figcaption>
</figure>

### 1.2. The PDB-file-Format 

<p>
The PDB file format is a text file, which is used for representing the 3D 
Structure of Proteins in form of atomic coordinates and additional information’s 
of this proteins. In General the PDB file consists of series of text records and 
each contains different types of information about the structure. A text record 
describes one line in the file. 
This records are arranged in a specific defined order that
describes the structure of the protein. Files that are written in the format 
end with .pdb and get in the PDB often a unique identifier (<i>Berman et all.,
2000; RCSB PDB,2024</i>).
The general structure of a PDB file is shown in Fig.3 and is described exactly 
in the following abstract on the example 
<a href="https://doi.org/10.2210/pdb4w5w/pdb"> <i>RuBisCO Activase of A.thaliana 
</i> </a>, that is used in the implementation part as well to show how the parser 
works (<i>wwPDB,2012</i>). 
</p>

<figure>
    <img src="img/file_overview.png" alt="drawing" title="structure PDB data" width="60%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.3. the Protein Data Bank file format</b>: <br>
            Shown is a general overview of the structure of the PDB file format
            and the function of the three main parts. Parts of this figure are
            taken from the Biocon Repository (<i>Duerr et all., 2024)</i></a>.
        </font>
    </figcaption>
</figure>

<p>
On the top of the .PDB File is a more introductory part where record types are 
used that describe more general information’s about the protein and the 
researchers that identified the protein. 
Some Record types present in the title section are:
<ul>
    <li>HEADER: Brief description of the structure </li>
    <li>TITLE: Name of experiment or molecule represented in the entry, 
    here: Rubisco Activase from A.thaliana </li>
    <li>SOURCE: Biological or chemical source of each molecule in the entry,
    described by common name and scientific name </li>
</ul>
<i>(wwPDB,2021)</i>
</p>

<figure>
    <img src="img/introductionary_part.png" alt="drawing" 
    title="introductionary" width="60%"/> 
    <figcaption>
        <font size="2">
        <b>Fig.4. the introductionary Part in a PDB File</b>: <br> 
        First Part in a PDB File on the example 
        <a href="https://doi.org/10.2210/pdb4w5w/pdb"> RuBisCO Activase of 
        <i>A.thaliana</i> </a> .
        </font>
    </figcaption>
</figure>

<p>
The next big part of the PDB format contains structural information about the 
strucure in form of atomic coordinates or other forms: 

<ul>
<li> <b>Secondary structure section</b>: </li>
<ul>
    <li>HELIX: describe position of &#945-helices in the molecule as well as 
    the residues around the helices </li>
    <li>SHEET: describe position of &#946-sheets in the molecule as well as 
    residues around sheets</li>
</ul>
<li> <b>other sections:</b> </li>
<ul>
    <li>CISPEP: specify peptides as well as prolines found in cis conformation 
    </li>
    <li>SITE: Miscellaneous feature, which describes residues comprising 
    catalytic, co-factor, anti-codon and other regulatory ligands  
    </li>
    <li>LINK: Describes the covalent linkage e.g. through a peptide bond, 
    between two residues </li> 
</ul>
<li><b>Coordinate Section</b>: </li>
<ul>
    <li> ATOM: represents atomic coordinates for standard amino acids and 
    nucleotides as well as their temperature and occupancy. element symbol 
    is always present and charge optional. Each single atom is described in 
    one single line. </li>
    <li>TER: Keyword representing the end of ATOM or HETATM list --> chain end  </li>
    <li>HETATM: same as ATOM, but for non - polymer or other chemical 
    coordinates present in HET </li>
    <li>MODEL: specifies the model serial number, when multiple models of a
    structure are presented in one file, most case when structures are determined 
    by NMR</li>
</ul>
<li><b>connectivity and bookkeeping section</b>: </li>
<ul>
    <li>CONECT: describes which atoms are linked covalently and in which form </li>
    <li>MASTER: bookkeeping record or summary of file</li>
    <li> END: represents the File end of a PDB File</li>
</ul>
</ul>
(Fig.5), <i>(wwPDB,2021)</i>
</p>

<figure>
    <img src="img/structural_part.png" alt="drawing" title="structural 
    description of entry" width="80%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.5. Structural part and connectivity in a PDB file</b>:<br> 
            Shown is the structural part on the example 
            <a href="https://doi.org/10.2210/pdb4w5w/pdb"> 
            <i> RuBisCO Activase of A.thaliana</i> </a> . While a) represents 
            the part where the primary structure is described, b) represents 
            the part where the coordinates are given and secoundary data as well. 
            c) represents at the beginn also some coordinative data and then 
            some connectivity data as well as the END Keyword.
        </font>
    </figcaption>
</figure>

### 1.3.PDB-file reader 

<p>
A PDB Parser is needed to parse information’s stored in the PDB file and process 
them further (Fig 1.). Functions of a PDB Reader are: 
<ul>
    <li>the Read in of a PDB file</li> 
    <li>the parsing of data read by line: Identification of Record types and extract data </li>
    <li>data structuring: store data in structured format  and allow access to data </li>
    <li> data processing and analysis: validate data, visualize, and analyse </li>
    <li> error handling: handling of errors or missing data </li>
</ul>
In case of protein modelling a  PDB File Reader is important for the extraction of atom coordinates, which are important for 3D visualization of protein as well as analysis and comparison of protein structure. 
*)

(**

<br>

# 2. Read PDB files with BioFSharp

### 2.1. The PDB Parser file format:

The first step in the implentation as well the understanding of the PDB parser 
format is the defination of the data types. This data types summarize the in 
chapter 1.3 described record types. We dont need for every record type a different 
data type, because this record types are linked together e.g. HETATM and ATOM, 
are only slightly different in the data they contain and can be handled in a 
single data type. The type Metadata contains several general information, and 
is seperately defined but connected to the type Structure . 
Structure contains several other informations stored in the hierarchy lower 
record types, which are atom,residue,chain and model. Residue and model contain 
also some additional informations shown in Fig.6. One example is the atom type 
storing the atomar information in the PDB File. 

     type Atom = {   
        SerialNumber: int;
        AtomName: string;
        AltLoc: char;
        Coordinates: Vector3D;
        Occupancy: float;
        TempFactor: float;
        SegId: string option;
        Element: string;
        Charge: string option;
        Hetatm: bool; 
    }

The used data types can be found 
<a href="https://github.com/vLeidel/BioFSharp/blob/vleidel--PDBParser%26SASAanalysis/src/BioFSharp/FileFormats/PDBParser.fs">here </a>.

<figure>
    <img src="img/data_structure_diagram.png" alt="drawing" title="data structure" width="80%"/> 
    <figcaption>
        <font size="2">
            <b>Fig.6. Data structure to describe PDB Record types</b>: <br> 
            This figure represents the hierarchy of collections contained in a 
            PDB file. dashed Arrows represent the transitive relationsship while 
            normal arrows represent a direct link. Blue boxes represent the main 
            types relevant in a PDB File, while green boxes represent types 
            that carry important informations for the main types. 
            (modified after Fig.1 in,<i>Gajda,December 2013</i>). 
        </font>
    </figcaption>
</figure>

*)

(**
### 2.2. Read the PDB file:

Now we can beginn with the general read in of the PDB file. First we read in 
the PDB file as a sequence of lines, That is done similar to the txt file 
(readPDBFile).
  
*)

readPBDFile ("data/rubisCOActivase.pdb") 

(**
<details>
<summary>
Click here to see how it looks like to read a PDB file in BioFSharp, without analysis yet.
</summary>
*)

(*** hide ***)
readPBDFile "data/rubisCOActivase.pdb" |> Seq.toList
(***include-it:***)

(**
</details>

<p>
The readPDBFile function is the basis for all following functions, that convert
the linewise read in record type information’s into the in chapter 2.1 defined 
data model types. To understand the converting functions, it is important to keep 
in mind, that every item in the sequence is a line in the PDB file, that 
represents different type of information. If you need to parse more information,
than already done e.g. DBRef showing informations about the primary sequence, you can simply add this fields or complete types and functions extracting the informations to the 
<a href="https://github.com/vLeidel/BioFSharp/tree/vleidel--PDBParser%26SASAanalysis/src/BioFSharp">BioFSharp.IO.PDBParser module´</a>functions.
</p>
*)

(**
<br>

### 2.3.Read Metadata:

<p>
Now we can begin to convert the read in sequence of lines into the different 
datatypes shown in Fig.6. The first data type we describe here is the Metadata 
type. Metadata contains general information about the PDB file, such as the Header, 
the Author or information about the source of the protein and the experiment, 
where the coordinates are for example identified. The function readMetadata 
extracts information from the sequence of lines and returns a Metadata type. 
The Metadata type contains the following fields, representing the 
exactly equally named record types in the PDB file:
</p>

<ul>
<li>Header: Unique PDB identifier </li>
<li>Title: Title for represented analysis</li>
<li>Compound: Macromolecular content of entry </li>
<li>Source:biological or chemical source of each biological molecule in exp. </li>
<li>Keywords: set of relevant categorizing terms</li>
<li>ExpData: describes experiment in file </li>
<li>Authors: auhors of ODB file</li>
<li>Remarks: additional experimental details, annotations, comments, and information</li>
<li>Caveats: Errors and unresolved issues in file </li>
</ul>
(<i>wwPDB,2021</i>)

    
<p>
The function readMetadata itself needs a sequence of lines as input, 
which you get through the in 2.2 described readPDBFile function and returns 
the Metadata type with the informations describing the Metadata.
</p>
*)

readPBDFile ("data/rubisCOActivase.pdb") 
|> readMetadata

(**
<details>
<summary>
Click here to see an example of how the console output for extracted metadata 
from the PDB file, that is read in, looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb") 
|> readMetadata
(***include-it:***)

(**
</details>
*)

(**
<br>

### 2.4.Read Atomic information:

<p>
In addition to the metadata, we also want to extract structural information about 
the biomolecule structure, given in the PDB file for further analysis. The parsing 
/ Reading of the structural information is done using various levels described 
in this and the next chapters. The levels are designed using the “bottom-up” approach,
where we first read the atomic information, describing the details and generalize
then gradually (Fig.7).
</p>

<figure>
    <img src="img/structure_level.png" alt="drawing" title="structure level" width="60%"/> 
        <figcaption>
            <font size="2">
                <b>Fig.7. various levels of structure in the PDB Parser</b>: <br> 
                Shown is a schematic diagram that visualizes the various levels 
                to describe the protein structure. The last (top) level to describe 
                a protein is structure, that summarizes all levels before. Parts 
                of the diagram are taken from BioIcons  (<i>Duerr,2012</i>)
. 
            </font>
    </figcaption>
</figure>

<p>
The most precise level to describe the 3D structure of a protein is the atomic level.
For that reason you have first to read the atomic information from the PDB file, 
which are stored in the ATOM and HETATM record types. The function readAtom uses
again the sequence of lines as input and returns a list with Atom type items. In 
this case every Atom is described by the following fields:
</p>

<ul>
<li>SerialNumber: unique serial number of atom in entry </li>
<li>AtomName: name of atom in entry e.g. CA </li>
<li>AltLoc: alternative location indicator, if more than one conformation of 
the protein is described </li>
<li>Coordinates: 3D Vector containing X,Y,Z,coordinates of atom in Angstrom </li>
<li>Occupancy: probability of atom being present in this conformation in crystal 
structure </li>
<li>TempFactor: temperature factor of atom in Angstrom </li>
<li>SegId: optional segment identifier </li>
<li>Element: element symbol of atom, e.g. C </li>
<li>Charge: optional charge of atom,  </li>
<li>Hetatm: boolean, true if atom is a HETATM, false if ATOM </li>
</ul>
(<i>wwPDB,2021</i>)

<p>
The principle of the readAtom function is similar to the readMetadata function, 
where we read in the sequence of lines, filter for ATOM and HETATM items and then 
convert the information's OF this items to the corresponding Atom type fields (Fig.8).
</p>

<figure>
<img src="img/atomparser.png" alt="drawing" title="data structure" width="100%"/> 
<figcaption>
<font size="2">
<b>Fig.8. Function of a Atom Parser</b>: <br> 
This figure represents the general function of an atom parser. Lines carrying atom 
information start with ATOM or HETATM. That line are filtered and then every field 
is translated and put to the corresponding type. Table representing the single 
types is taken from <i> UCSF Computer Graphics Laboratory </i>.
</font>
</figcaption>
</figure>

<p>
If you just need all Atom's without information's about the Residue, Chain or Model,
they belong to, you can use the readAtom function directly and get a list of all 
Atoms in the PDB file.
</p>
*)

readPBDFile ("data/rubisCOActivase.pdb")
|> readAtom

(**
<details>
<summary>
Click here to see an example of how the console output for extracted Atoms
from the PDB file, that is read in, looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|> readAtom
(***include-it:***)

(**
</details>
*)

(**
<br>

### 2.5.Read Residues:

<p>
As you can see in Fig.7, the next level to describe the structure of a protein 
is the residue level. A residue is in the case of proteins the Amino acid, that 
consists of several Atoms, which are covalently linked together. The readResidues 
function groups the Atoms you extracted in the previous step by the 
correspondence to the Residue they belong to and returns a list of items of the 
Residue type. One Residue could be exactly identified by the name, the nr and if 
it contains the insertion code The Residue type describing one residue is described
by the following fields:
</p>

<ul>
<li>ResidueName: Name of residue, e.g. ALA </li>
<li>ResidueNumber: unique number Number of residue in chain </li>
<li>InsertionCode: optional insertion code for residue, if one residue is 
described more than once </li>
<li>SecStructureType: type of secondary structure, the residue belongs to, e.g. 
HELIX </li>
<li>Modification: describes modifications of residue, e.g. Phosphorylation, when 
modified </li> 
<li>Atoms: List of Atoms, that belong to the residue </li>
</ul>

<p>
If you just need all Residues without information's about the Chain or Model,
they belong to, you can use the readResidue function directly and get a list of all 
Residues in the PDB file. The input of the readResidues function is again the sequence 
of lines, read in in readPDBFile.
</p>

*)

readPBDFile ("data/rubisCOActivase.pdb")
|> readResidues

(**
<details>
<summary>
Click here to see an example of how the console output for grouped Atoms by Residues 
correspondance from the PDB file, that is read in, looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|> readResidues
(***include-it:***)

(**
</details>
*)

(**
<br>

### 2.6.Read Chains:

<p>
When you have a multimer e.g. haemoglobin as protein to analyse, you should analyse
every chain separately. For that reason, the readChain function handles with the 
next higher level to describe the structure of a protein, the chain level (Fig.7.).
Chains consist of several residues, in the case of proteins of amino acids, which 
are linked together by peptide bonds. If a protein contains only one chain this 
is also the primary structure, otherwise it is only a part of the primary structure.
The readChain function groups the Residues you extracted in the previous step by 
the chainID, which is the unique identifier of the chain in the PDB file and the 
only additional field besides the Residues list field.<br>
If you dont need to seperate by the correspondance to one model, if the PDB file 
contains more than one model, you can use the readChain function directly and get 
a list of all Chains in the PDB file. 

</p>
*)

readPBDFile ("data/rubisCOActivase.pdb")
|> readChain

(**
<details>
<summary>
Click here to see an example of how the console output for grouped Residues by 
Chain correspondance from the PDB file, that is read in, looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|> readChain
(***include-it:***)

(**
</details>
*)

(**
<br>

### 2.7.Read Models:

<p>
Often PDB files describe more than one model of a protein. A model is a specific 
conformation (spatial arrangement) of the protein as determined by experimental 
methods (e.g. X-ray crystallography, NMR). For example a protein conformation could 
be described in the native and in the unfolded state. The Model data type, which 
is the result of the readModel function, contains beside of the Chains part of 
the Model in addtion the  Linkage field, describing in which form the Atoms of 
the Model are linked together and the Site field, which describes which Sites 
exist, e.g. Active centre. Linkages and Sites are additional Readers, you could
also use seperate. All three read functions, use again the sequence of lines 
(readPDBFile) as inputand return a Linkage,Site or Model type. The functions looks
like follwing
</p>

*)

readPBDFile ("data/rubisCOActivase.pdb")
|> readLinkages

readPBDFile ("data/rubisCOActivase.pdb")
|> readSite

readPBDFile ("data/rubisCOActivase.pdb")
|> readModels

(**
<details>
<summary>
Click here to see an example of how the console output for linkages description 
looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|>readLinkages
(***include-it:***)

(**
</details>
*)

(**
<details>
<summary>
Click here to see an example of how the console output for Sites description in
the PDB File looks like.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|>readSite
(***include-it:***)

(**
</details>
*)

(**
<details>
<summary>
Click here to see an example of how the console output for grouped Chains by 
models correspondance from the PDB file, that is read in, looks like as well as
the linkages and the sites.</summary>
*)

(***hide ***)
readPBDFile ("data/rubisCOActivase.pdb")
|>readModels
(***include-it:***)

(**
</details>
*)

(**
<br>

### 2.8.Read the complete structure:

The last level to describe the structures in the PDB file is a summary of the entire 
information's (All Models) in the PDB file and the Metadata. The two field of the 
structure type are Models and Metadata, which are lists of the Model type and 
the Metadata type. As input you just need the filepath of the PDB file you want 
to analyse and the function returns a Structure type, containing all information's 
you read in in the previous steps.
*)

readStructure ("data/rubisCOActivase.pdb")

(**
<details>
<summary>
Click here to see an example of how the console output for the structure type of the PDB File 4W5W-Rubisco activase from <i>Arabidopsis thaliana</i> looks like. </summary>
*)

(***hide ***)
readStructure ("data/rubisCOActivase.pdb")
(***include-it:***)

(**
</details>
*)

(**
<br>

# 3.Application of the PDB Parser - Example

<p>
One example of the application of the PDB Parser is the first step in protein structure
modeling, the pre modeling step. In this step you extract per model the coordinates 
of all Atoms and one information for the color code e.g.,correspondance to which 
Residuename it belongs and create a 3D scatterplot. The function to extract looks like  following
</p>
*)

let extractData (pdbstructure: Structure) =
    let title = pdbstructure.Metadata.Title
    let extractedData =
        pdbstructure.Models
        |> Array.map (fun model ->
            model.Chains
            |> Array.collect (fun chain ->
                chain.Residues
                |> Array.collect (fun residue ->
                    residue.Atoms
                    |> Array.map (fun atom -> (residue.ResidueName, atom)) 
                )
            )
        )
    (title, extractedData)

(**
<details>
<summary> click here to see how the extracted data look like on the example RubisCO 
Activase and extracted residues correspondance to atoms </summary>
*)

(***hide ***)
extractData (readStructure ("data/rubisCOActivase.pdb"))
(***include-it:***)

(**
</details>
*)

(**
<p>
This data are then used to create a 3D scatterplot, where the color of the Atom 
points represent the Residuename and which you can use to validate that Atoms are 
read in correctly.
</p>

<iframe src="img/rubisCOpreModel.html" width="1000" height="800"></iframe>

*)

(**

<br>

#  Bibliography 

<ul style="list-style-type: none;">
    <li> Berman, H. M. „About RCSB PDB: A Living Digital Data Resource That 
    Enables Scientific Breakthroughs Across The Biological SciencesThe Protein 
    Data Bank“, 1. Januar 2000. https://doi.org/10.1093/nar/28.1.235.</li>
    <li> Berman, H. M. „The Protein Data Bank“. Nucleic Acids Research 28, 
    Nr.1 (1. Januar 2000): 235–42. https://doi.org/10.1093/nar/28.1.235.</li>
    <li> Berman, Helen, Kim Henrick, and Haruki Nakamura. „Announcing the Worldwide 
    Protein Data Bank“. Nature Structural & Molecular Biology 10, Nr. 12 (Dezember 2003): 
    980–980. https://doi.org/10.1038/nsb1203-980.</li>
    <li> Campbell, Neil A., Jane B. Reece, Lisa A. Urry, Michael L. Cain, Steven 
    A. Wasserman, Peter V. Minorsky, Robert Jackson, Steven A. Wasserman, and 
    Robert Jackson. Campbell Biologie. published from Jürgen J. Heinisch and 
    Achim Paululat. 10., Aktualisierte Auflage. Always learning. 
    Hallbergmoos/Germany: Pearson, 2016.</li>
    <li> Duerr, Simon, Kanaren,Kathryn, Derek Croote, Emmett Leddin, and Jeroen 
    Van Goey. „duerrsimon/bioicons: April24“. Zenodo, 25. April 2024. 
    https://doi.org/10.5281/ZENODO.11068293.</li>
    <li> Gajda, Michał Jan. „BioHaskell/hPDB“. github, 2013. 
    https://github.com/BioHaskell/hPDB.</li>
    <li> Gajda, Michał Jan.„hPDB – Haskell Library for Processing Atomic 
    Biomolecular Structures in Protein Data Bank Format“. BMC Research Notes 6, 
    Nr. 1 (Dezember 2013): 483. https://doi.org/10.1186/1756-0500-6-483.</li>
    <li> OpenAI. (2025). ChatGPT (Version GPT-01, March 2025) [Large language model].
    https://openai.com/chatgpt/overview/. </li>
    <li> RCSB PDB. „PDB-101: Learn: Guide to Understanding PDB Data: 
    PDB Overview“. Guide to Understanding PDB Data. acessed 25. September 2024. 
    https://pdb101.rcsb.org/learn/guide-to-understanding-pdb-data/introduction.
    </li>
    <li>Schneider, Kevin, Benedikt Venn, and Timo M&uumlhlhaus. &quotPlotly.NET: A Fully Featured Charting Library for .NET Programming Languages&quot. F1000Research 11 (23. Septembre 2022): 1094. https://doi.org/10.12688/f1000research.123971.1.</li>
    <li>UCSF Computer Graphics Laboratory. &quotIntroduction to Protein Data 
    Bank Format&quot, Oktober 2022. 
    https://www.cgl.ucsf.edu/chimera/docs/UsersGuide/tutorials/pdbintro.html.</li>
    <li> wwPDB. „Protein Data Bank Contents Guide“: Guide. Atomic Coordinate Entry
    Format - Version 3.3, 21. November 2012. 
    https://www.wwpdb.org/documentation/file-format-content/format33/v3.3.html.
    </li>

</ul>

*)