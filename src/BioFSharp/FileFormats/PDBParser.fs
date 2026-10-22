namespace BioFSharp.FileFormats

module PDBParser =

    // Type for storing all informations about Metadata -- Structure
    // Revdata is not included, but it can if you need more informations, 
    // than add type Revdata as well as Journals and DEBREF

    type Metadata = {
        Header: string;
        Title: string;
        Compound: string list option;
        Source: string list option;
        Keywords: string list option;
        ExpData: string list option;
        Authors: string list option;
        Remarks: string list;
        Caveats:string list option   
    }

    // create type for 3d coordinates x y z representing Location

    type Vector3D = {
        X: float;
        Y: float;
        Z: float
    }

    // Type for For Single Atom Location, also used for Record types with 
    // similar dtructure  in PDB data like HETATM 

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

    // Parsing of existing secondary structures types in the biomolecule

    type SSType = {
        SecstructureType: string;
        Startresidue: int;
        Endresidue:int
    }

    // defines if a residue is modified and in which form

    type Modification = {
        ModifiedResidueNumber: int;
        ModificationType: string;
    }

    //  definition of a single monomer within the polymer 
    // (Amino acid or molecule in chain)

    type Residue = {
        ResidueName: string;
        ResidueNumber: int;
        InsertionCode: char option;
        SecStructureType: string option;
        Modification: string option;
        Atoms: Atom array; 
    }

    // describe one chain in the biomolecule

    type Chain = {
        ChainId: char;
        Residues: Residue array
    }

    // type to describe how single residues / atoms are linked together

    type Linkages = {
        Linktype: string
        Atoms1: Atom array
        Atoms2: Atom array
    }

     // SiteType record to store the site name and associated residues
     // Residues Stores Chain ID and Residue Number
    type SiteType = {
        Sitename: string
        Residues: (char * int) array 
    }

    // Record type to store the whole model of the biomolecule
    type Model = {
         ModelId: int
         Chains: Chain array
         Linkages: Linkages array 
         Sites: SiteType array 
    }

    // Type to store the whole PDB file information 
    // (Metadata and biomolecule models)
  
    type Structure = {
        Metadata: Metadata;
        Models: Model array
     }



    


