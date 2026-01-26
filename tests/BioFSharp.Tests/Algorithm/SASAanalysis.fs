namespace BioFSharp.Tests.Algorithm 

module SASATests = 
    
    open BioFSharp.Algorithm.SASA
    open BioFSharp.IO.PDBParser
    open BioFSharp.FileFormats.PDBParser
    open BioFSharp.Tests.ReferenceObjects.SASA

    open Expecto  
    open System.Collections.Generic


    let AlgorithmTests = 
        testList "SASA analysis" [
            
            test "structure prepResidue extraction for SASA calculation" {

                let testdata = getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1
                
                let parsedResidues  =
                    readResidues (readPBDFile "resources/pdbParser/rubisCOActivase.pdb")
                    |> Array.map (fun res ->
                        
                        let filteredAtoms =
                            res.Atoms
                            |> Array.filter (fun a -> not a.Hetatm)
                      
                        { res with Atoms = filteredAtoms }
                    )
                    |> Array.filter (fun r -> r.Atoms.Length > 0)
                    |> Array.map (fun r -> r.ResidueNumber)

                let parsedModelRes = 
                    readStructure "resources/pdbParser/rubisCOActivase.pdb"

                let residuesNumbers = testdata.Values |> Seq.collect (fun res -> res |> Array.map (fun r -> r.ResidueNumber)) |> Seq.toArray

                Expect.equal residuesNumbers parsedResidues "Residues need to 
                be extracted correctly from the PDB file"
                Expect.exists testdata.['A'] (fun residue -> 
                residue.ResidueName = "MET") "Evervy PDB File should contain 
                at least one MET residue"
                Expect.exists testdata.['A'] (fun residue -> 
                    residue.ResidueNumber = 69) 
                    "PDB File testdata should contain one residue with 69 "
                Expect.all testdata.['A'](fun residue -> residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"
                Expect.equal parsedModelRes.Models.[0].Chains.[0].Residues.[0].ResidueName testdata.['A'].[0].ResidueName 
                    "The first residue name should be 'MET' and equal"

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 2 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" -1 |>ignore) 
                    "Extraction of Residues from Parsed PDB File should file when modelid is negative"

                // test PDB File with multiple models

                let htq = getResiduesPerChain "resources/pdbParser/htq.pdb" 2
                let parsedModelhtq = readStructure "resources/pdbParser/htq.pdb" 
                                   
                Expect.isGreaterThan (htq.Count) 1 "Residues with multiple Chains
                need more than one key"

                let residuesWithoutHetatm = 
                    parsedModelhtq.Models.[0].Chains.[0].Residues  
                    |> Array.map (fun res ->
                       
                        let filteredAtoms =
                            res.Atoms
                            |> Array.filter (fun a -> not a.Hetatm)

                        
                        { res with Atoms = filteredAtoms }
                    )
                 
                    |> Array.filter (fun r -> r.Atoms.Length > 0)
                   

                Expect.equal residuesWithoutHetatm.[0].ResidueName htq.['A'].[0].ResidueName "Residues need to be parsed correctly"

                Expect.throws (fun() -> getResiduesPerChain "resources/pdbParser/htq.pdb" -1 |> ignore) 
                    "residue extraction of PDB File should fail at negative model id"

                Expect.throws (fun() -> getResiduesPerChain "resources/pdbParser/htq.pdb" 1254 |> ignore) 
                    "residue extraction of PDB File should fail at unknown model id"
                               
                // Unique residues in file without missing infos 

                let cre_example = 
                    getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1 

                let residuenumber_cre = 
                   cre_example.Values 
                   |> Seq.collect (fun res -> res |> Array.map (fun r -> 
                    r.ResidueNumber)) 
                   |> Seq.toArray

                let allresidues_cre = 
                    cre_example.Values 
                    |> Seq.collect (fun res -> res) 
                    |> Seq.toArray

                let readResidues_cre = 
                    readResidues (readPBDFile "resources/pdbParser/Cre01g001550t11.pdb")
                        |> Array.map (fun res ->
                        
                            let filteredAtoms =
                                res.Atoms
                                |> Array.filter (fun a -> not a.Hetatm)
                      
                            { res with Atoms = filteredAtoms }
                        )
                        |> Array.filter (fun r -> r.Atoms.Length > 0)
                        |> Array.map (fun r -> r.ResidueNumber)

                Expect.equal residuenumber_cre.Length readResidues_cre.Length
                    "The residues identified by residuenumber should be equal 
                    to the residues in the PDB file"

                Expect.exists (allresidues_cre) (fun residue -> 
                residue.ResidueName = "MET") "Evervy PDB File should contain 
                at least one MET residue"
                Expect.exists allresidues_cre (fun residue -> 
                    residue.ResidueNumber = 1) 
                    "complete PDB File testdata should contain one 
                    residue with 1 as residuenumber "
                Expect.all allresidues_cre (fun residue -> 
                    residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                let parsedModelRes_cre = readStructure "resources/pdbParser/Cre01g001550t11.pdb"

                Expect.equal 
                    parsedModelRes_cre.Models.[0].Chains.[0].Residues.[0].ResidueName
                    cre_example.['A'].[0].ResidueName 
                    "The first residue name should be 'MET' and equal"

                Expect.isGreaterThanOrEqual cre_example.Count 1 
                    "Cre01g001550t11.pdb should contain at least one chain / key"

                Expect.equal cre_example.Count 
                    (parsedModelRes_cre.Models.[0].Chains.Length)
                    "The number of chains in the Cre01g001550t11.pdb 
                    file should be equal"

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 500 |>ignore) 
                    "PDB File with unknown modelid should throw an 
                    error" 

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an 
                    error"  

                // test PDB File with multiple models and chains

                let cre_example_chains = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1

                let residuenumber_cre_chains = 
                   (cre_example_chains.Values)
                   |> Seq.collect (fun res -> res |> Array.map (fun r -> 
                    r.ResidueNumber)) 
                   |> Seq.toArray

                let allresidues_cre_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res) 
                    |> Seq.toArray

                let readResidues_cre_chains = 
                    readResidues (readPBDFile "resources/pdbParser/Cre01g026150t11.pdb")
                        |> Array.map (fun res ->
                        
                            let filteredAtoms =
                                res.Atoms
                                |> Array.filter (fun a -> not a.Hetatm)
                      
                            { res with Atoms = filteredAtoms }
                        )
                        |> Array.filter (fun r -> r.Atoms.Length > 0)
                        |> Array.map (fun r -> r.ResidueNumber)

                Expect.equal 
                    residuenumber_cre_chains.Length 
                    readResidues_cre_chains.Length
                    "The residues identified by residuenumber should be equal 
                    to the residues in the PDB file"

                Expect.exists (allresidues_cre_chains) (fun residue -> 
                residue.ResidueName = "MET") "Evervy PDB File should contain 
                at least one MET residue"
                
                Expect.exists allresidues_cre_chains (fun residue -> 
                    residue.ResidueNumber = 1) 
                    "complete PDB File testdata should contain one 
                    residue with 1 as residuenumber "
                
                Expect.all allresidues_cre_chains (fun residue -> 
                    residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                let extractedFirstResidues = 
                    cre_example_chains.Values
                    |> Seq.collect (fun res -> res)
                    |> Seq.head
                   

                let firstRes_cre_chains = 
                    (readStructure "resources/pdbParser/Cre01g026150t11.pdb").Models 
                    |> Seq.collect (fun m -> m.Chains) 
                    |> Seq.collect (fun c -> c.Residues)
                    |> Seq.head

                Expect.equal 
                    extractedFirstResidues
                    firstRes_cre_chains 
                    "The extracted residue should be equal "

                Expect.equal extractedFirstResidues.ResidueName "MET" 
                    "The first residue name should be MET "

                Expect.isGreaterThanOrEqual cre_example_chains.Count 1 
                    "Cre01g001550t11.pdb should contain at least one chain / key"

                let allChains_cre = 
                    cre_example_chains.Keys
                    |> Seq.toArray
                   
                let parsedChains = 
                    (readStructure "resources/pdbParser/Cre01g026150t11.pdb").Models 
                    |> Seq.collect (fun m -> m.Chains) 
                    |> Seq.toArray

                Expect.equal allChains_cre.Length 
                    (parsedChains.Length)
                    "The number of chains in the Cre01g001550t11.pdb 
                    file should be equal when only one model"

                Seq.iteri2 (fun i key chain ->
                    Expect.equal key chain.ChainId
                        $"The ChainID of the {i}th Chain should be equal"
                ) allChains_cre parsedChains

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 500 |>ignore) 
                    "PDB File with unknown modelid should throw an 
                    error" 

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an 
                    error"  

                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/notexistingpdb.pdb" 1 |>ignore) 
                    "Extraction of Residues should fail when pdb file not exists"
            }

            test "single Atoms are extracted correctly per Chain"{
                let atoms = getAtomsPerModel  "resources/pdbParser/rubisCOActivase.pdb"1
                let parsedAtoms = readAtom (readPBDFile "resources/pdbParser/rubisCOActivase.pdb") 

                Expect.isLessThanOrEqual atoms.['A'].Length parsedAtoms.Length 
                    "Number of parsed Atoms should be smaller or equal then number 
                    of Atoms in PDB File"
                Expect.equal (atoms.Count) 1 "PDB File with only one model and 
                    one Chain should have only one atom list"

                Expect.throws (fun () -> getAtomsPerModel "resources/pdbParser/rubisCOActivase.pdb" 2 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present" 

                Expect.throws (fun () -> getAtomsPerModel "resources/pdbParser/rubisCOActivase.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an error"

                Expect.throws (fun() -> 
                    getAtomsPerModel "resources/notexistingpdb.pdb" 1 |>ignore) 
                    "Extraction of Atoms should fail when pdb file not exists"
                
                let atomsHTQ = getAtomsPerModel "resources/pdbParser/htq.pdb" 1
                let parseAtomsHTQ = readAtom (readPBDFile "resources/pdbParser/htq.pdb")

                Expect.isGreaterThan (atomsHTQ.Count) 1 
                    "PDB File with multiple chains should have multiple atom lists"
                
                atomsHTQ
                |> Seq.iter (fun atomList ->
                    Expect.isGreaterThan (atomList.Value.Length) 1 
                        "PDB File with multiple models should have multiple atoms"
                ) 

                Expect.equal atomsHTQ.['A'].[0].AtomName parseAtomsHTQ.[0].AtomName 
                    "Atoms need to be parsed correctly and the first should be equal"

                Expect.throws (fun () -> 
                    getAtomsPerModel  "resources/pdbParser/htq.pdb" 500 |>ignore) 
                    "PDB File with only one model and one Chain should throw an
                    error if modelid is not present"

                Expect.throws (fun () -> 
                    getAtomsPerModel  "resources/pdbParser/htq.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an error"

                // test PDB File with complete atoms information

                let cre_example = getAtomsPerModel "resources/pdbParser/Cre01g001550t11.pdb" 1
                let atom_readinCre = readAtom (readPBDFile "resources/pdbParser/Cre01g001550t11.pdb")
                
                Expect.isGreaterThanOrEqual cre_example.Count 1 
                    "Cre01g001550t11.pdb should contain at least one chain / key"

                Expect.exists cre_example (fun kvp -> 
                    kvp.Value.Length > 0) 
                    "Cre01g001550t11.pdb should contain at least one atom"

                let allAtoms_cre = 
                    cre_example.Values 
                    |> Seq.collect (fun x -> x) 
                    |> Seq.toArray

                Expect.exists allAtoms_cre (fun atom -> 
                    atom.Element = "C") "Cre01g001550t11.pdb should contain at 
                    least one Carbon Atom"
                Expect.exists allAtoms_cre (fun atom -> 
                    atom.Element = "H") "Cre01g001550t11.pdb should contain at 
                    least one water Atom"
                Expect.exists allAtoms_cre (fun atom -> 
                atom.Element = "N") "Cre01g001550t11.pdb should contain at 
                least one Nitogen Atom"

                Expect.exists allAtoms_cre (fun atom -> atom.SerialNumber = 1) 
                    "Cre01g001550t11.pdb should contain at least one Atom with
                    SerialNumber 1"

                Expect.isLessThanOrEqual (cre_example.Values |> Seq.length) 
                    (atom_readinCre |> Seq.length) 
                    "The number of parsed Atoms 
                    should be smaller or equal then number of Atoms in PDB File"

                Expect.throws (fun () -> 
                    getAtomsPerModel "resources/pdbParser/Cre01g001550t11.pdb" 500 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> 
                    getAtomsPerModel "resources/pdbParser/Cre01g001550t11.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an error"

                // test cre file with more than one chain 
   
                let cre_example_chains = 
                    getAtomsPerModel "resources/pdbParser/Cre01g026150t11.pdb" 1

                let atom_readinCre_chains = readAtom (readPBDFile "resources/pdbParser/Cre01g026150t11.pdb")
                
                Expect.isGreaterThanOrEqual cre_example_chains.Count 1 
                    "Cre01g001550t11.pdb should contain at least one chain / key"

                Expect.exists cre_example_chains (fun kvp -> 
                    kvp.Value.Length > 0) 
                    "Cre01g001550t11.pdb should contain at least one atom"

                let allAtoms_cre_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun x -> x) 
                    |> Seq.toArray

                Expect.exists allAtoms_cre_chains (fun atom -> 
                    atom.Element = "C") "Cre01g001550t11.pdb should contain at 
                    least one Carbon Atom"

                Expect.exists allAtoms_cre_chains (fun atom -> 
                    atom.Element = "H") "Cre01g001550t11.pdb should contain at 
                    least one water Atom"

                Expect.exists allAtoms_cre_chains (fun atom -> 
                atom.Element = "N") "Cre01g001550t11.pdb should contain at 
                least one Nitogen Atom"

                Expect.equal 
                    (allAtoms_cre_chains
                    |>Array.head 
                    |> fun atom -> atom.SerialNumber
                    ) 
                    1
                    "Cre01g001550t11.pdb should contain at least one Atom with
                    SerialNumber 1"

                Expect.isLessThanOrEqual (cre_example_chains.Values |> Seq.length) 
                    (atom_readinCre_chains |> Seq.length) 
                    "The number of parsed Atoms 
                    should be smaller or equal then number of Atoms in PDB File"

                Expect.throws (fun () -> 
                    getAtomsPerModel "resources/pdbParser/Cre01g001550t11.pdb" 500 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> 
                    getAtomsPerModel "resources/pdbParser/Cre01g001550t11.pdb" -1 |>ignore) 
                    "PDB File with negative modelid should throw an error"

            }

            test "Van der Waals Radius is determined correctly"{
                let testdata = 
                    getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1
                
                let vdw_raadi = 
                    testdata
                    |> Seq.map ( fun kvp ->
                        kvp.Key,
                        kvp.Value
                        |>Array.Parallel.map(fun residue ->                       
                                (residue.ResidueName,residue.ResidueNumber),residue.Atoms
                                |>Array.Parallel.map(fun atom -> (determine_effective_radius atom residue.ResidueName "Water")
                                )                              
                        )|>dict)|> dict
                        

                Expect.equal (vdw_raadi.['A'].["ASN",65]).[0] (1.64 + 1.4) 
                    "protor radius must be assigned correctly"
                Expect.equal vdw_raadi.['A'].Count testdata.['A'].Length 
                    "In a Atom List all Elements should be assigned to a vdw 
                    radius or protor radius"
                Expect.contains vdw_raadi.['A'].["ASN",65] (1.88 + 1.4) 
                    "Amino acids contain at least one Carbon Atom"
                Expect.contains vdw_raadi.['A'].["CYS",117] (1.77 + 1.4) 
                    "Aminoacids with disulfid bonds / Cystein need at least two S"
                Expect.equal  (determine_effective_radius testdata.['A'].[0].Atoms.[3] "XYZ" "Water") 
                    (0.0 + 1.4) "Atoms with Element O need vdw radius of 0.0 and probe" 

                Expect.throws (fun () -> 
                determine_effective_radius testdata.['A'].[0].Atoms.[3] "XYZ" "ABC" 
                |>ignore
                ) "Probenames that are unknown should lead to a fail"
                           
                // test for File without missing infos 
                let cre_example = getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1

                let all_creResidues = 
                    cre_example.Values 
                    |> Seq.collect (fun res -> res) 
                    |> Seq.toArray

                let vdw_raadi_cre = 
                    (cre_example.Values)
                    |> Seq.collect (fun res -> res)
                    |> Seq.map (fun residue ->
                        let keys = residue.ResidueName, residue.ResidueNumber
                        let residue =
                            residue.Atoms
                            |> Array.map (fun atom -> 
                                determine_effective_radius atom residue.ResidueName "Biotin")

                        keys, residue) |> dict

                Expect.equal vdw_raadi_cre.Count all_creResidues.Length
                    "The number of chains in the Cre01g001550t11.pdb file should be equal"

                Expect.equal (vdw_raadi_cre.["MET",1]).[0] (1.64 + 3.7) 
                    "protor radius FOR Methionin and N  must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["LEU",2]).[0] (1.64 + 3.7) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["HIS",18]).[0] (1.64 + 3.7) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["PRO",311]).[0] (1.64 + 3.7) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["GLN",39]).[13] (1.64 + 3.7) 
                    "N radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"
                
                Expect.equal (vdw_raadi_cre.["MET",1]).[1] (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["ASP",140]).[1] (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["GLU",175]).[1] (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["THR",371]).[3] (0. + 3.7) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["LYS",302]).[3] (0. + 3.7) 
                    "protor radius FOR LYS and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["MET",1]).[4] (1.88 + 3.7) 
                    "CA Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["ARG",4]).[5] (1.88 + 3.7)
                    "CB Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["GLY",187]).[5] (1.61 + 3.7) 
                    "C Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["ILE",299]).[8] (1.88 + 3.7) 
                    "C Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["PHE",305]).[9] (1.61 + 3.7) 
                    "C Atoms need to be assigned correctly for PHE"

                Expect.equal (vdw_raadi_cre.["VAL",30]).[2] (1.88 + 3.7) 
                    "C Atoms need to be assigned correctly for Val"

                Expect.equal (vdw_raadi_cre.["TYR",451]).[10] (1.76 + 3.7) 
                    "oxidied Atoms need to be assigned correctly" 

                Expect.equal (vdw_raadi_cre.["LYS",465]).[22] (1.46 + 3.7) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["ALA",3]).[9] (1.42 + 3.7) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre.["SER",322]).[9] (1.46 + 3.7) 
                    "O Atoms need to be assigned correctly in Ala"

                Expect.equal (vdw_raadi_cre.["ASN",116]).[13] (1.42 + 3.7) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre.["CYS",396]).[9] (1.77 + 3.7) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.isFalse (vdw_raadi_cre.ContainsKey("TRP",8)) 
                    "There should not be a residue with the name TRP in the vdw radius dict"

                Expect.throws (fun () -> 
                    determine_effective_radius all_creResidues.[0].Atoms.[3] "XYZ" "ABC" 
                    |>ignore
                ) "Probenames that are unknown should lead to a fail"


                let vdw_raadi_cre_FLOATProbe = 
                    (cre_example.Values)
                    |> Seq.collect (fun res -> res)
                    |> Seq.map (fun residue ->
                        let keys = residue.ResidueName, residue.ResidueNumber
                        let residue =
                            residue.Atoms
                            |> Array.map (fun atom -> 
                                determine_effective_radius atom residue.ResidueName 4.)

                        keys, residue) |> dict


                Expect.equal vdw_raadi_cre_FLOATProbe.Count all_creResidues.Length
                    "The number of chains in the Cre01g001550t11.pdb file should be equal"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["MET",1]).[0] (1.64 + 4.) 
                    "protor radius FOR Methionin and N  must be assigned 
                    correctly for Cre01g001550t11.pdb"
                
                Expect.equal (vdw_raadi_cre_FLOATProbe.["LEU",2]).[0] (1.64 + 4.) 
                    "protor radius FOR Methionin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["HIS",297]).[0] (1.64 + 4.) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["GLN",39]).[13] (1.64 + 4.) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["PRO",311]).[0] (1.64 + 4.) 
                    "H radius FOR Pro and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["MET",1]).[1] (0. + 4.) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["THR",371]).[3] (0. + 4.) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"


                Expect.equal (vdw_raadi_cre_FLOATProbe.["ASP",140]).[1] (0. + 4.) 
                    "protor radius FOR ASP and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["GLU",175]).[1] (0. + 4.) 
                    "protor radius FOR GLU and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["LYS",302]).[3] (0. + 4.) 
                    "protor radius FOR LYS and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["MET",1]).[4] (1.88 + 4.) 
                    "CA Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["ARG",4]).[5] (1.88 + 4.) 
                    "CB Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["VAL",30]).[2] (1.88 + 4.) 
                    "C Atoms need to be assigned correctly for Val"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["GLY",187]).[5] (1.61 + 4.) 
                    "C Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["ILE",299]).[8] (1.88 + 4.) 
                    "C Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["PHE",305]).[9] (1.61 + 4.) 
                    "C Atoms need to be assigned correctly for PHE"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["LYS",465]).[22] (1.46 + 4.) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["TYR",451]).[10] (1.76 + 4.) 
                    "oxidied Atoms need to be assigned correctly" 
                    
                Expect.equal (vdw_raadi_cre_FLOATProbe.["ALA",3]).[9] (1.42 + 4.) 
                    "O Atoms need to be assigned correctly in Ala"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["SER",322]).[9] (1.46 + 4.) 
                    "O Atoms need to be assigned correctly in Ser"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["ASN",116]).[13] (1.42 + 4.) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe.["CYS",396]).[9] (1.77 + 4.) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.isFalse (vdw_raadi_cre_FLOATProbe.ContainsKey("TRP",8)) 
                    "There should not be a residue with the name TRP in the 
                    vdw radius dict"

                Expect.throws (fun () -> 
                    determine_effective_radius all_creResidues.[0].Atoms.[3] "XYZ" -2
                    |>ignore
                ) "negative probe residues values should lead to a fail"

                // test for cre with multiple chains 
              
                let cre_example_chains = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1

                let all_creResidues_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res) 
                    |> Seq.toArray

                let vdw_raadi_cre_chains = 
                    all_creResidues_chains
                    |> Array.map ( fun res -> 
                        res.Atoms
                        |> Array.map (fun atom -> 
                     (res.ResidueNumber,res.ResidueName),       determine_effective_radius atom res.ResidueName "Biotin") |> dict
                    ) 

                let vdw_raadi_cre_chains = 
                    cre_example_chains 
                    |> Seq.map (fun a -> a.Key,a.Value)
                    |> Seq.collect (fun (key, residues) ->                       
                        residues
                        |> Array.Parallel.map (fun residue ->
                            let keys = key,residue.ResidueName, residue.ResidueNumber
                            let residue =
                                residue.Atoms
                                |> Array.Parallel.map (fun atom -> 
                                    determine_effective_radius atom residue.ResidueName "Biotin")
                            keys, residue)) |> dict
                   

                let parsedResidues_creChain = 
                    readResidues (readPBDFile "resources/pdbParser/Cre01g026150t11.pdb")

                Expect.equal 
                    vdw_raadi_cre_chains.Count
                    parsedResidues_creChain.Length
                    "The number of chains in the pdb file should be equal"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"MET",1]).[0] 
                    (1.64 + 3.7) 
                    "protor radius FOR Methionin and N  must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"LEU",18]).[0] 
                    (1.64 + 3.7) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"HIS",121]).[0] 
                    (1.64 + 3.7) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"PRO",232]).[0] 
                    (1.64 + 3.7) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"GLN",137]).[13] 
                    (1.64 + 3.7) 
                    "N radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"
                
                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"MET",1]).[1] 
                    (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"ASP",100]).[1] 
                    (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"GLU",101]).[1] 
                    (0. + 3.7) 
                    "protor radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"THR",142]).[3] 
                    (0. + 3.7) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"LYS",150]).[3] 
                    (0. + 3.7) 
                    "protor radius FOR LYS and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_chains.['B',"MET",1]).[4] (1.88 + 3.7) 
                    "CA Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"ARG",60]).[5] 
                    (1.88 + 3.7)
                    "CB Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"GLY",74]).[5] 
                    (1.61 + 3.7) 
                    "C Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"ILE",627]).[8] 
                    (1.88 + 3.7) 
                    "C Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"PHE",695]).[9] 
                    (1.61 + 3.7) 
                    "C Atoms need to be assigned correctly for PHE"

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"VAL",299]).[2] 
                    (1.88 + 3.7) 
                    "C Atoms need to be assigned correctly for Val"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"TYR",76]).[10] 
                    (1.76 + 3.7) 
                    "oxidied Atoms need to be assigned correctly" 

                Expect.equal 
                    (vdw_raadi_cre_chains.['B',"ALA",333]).[10] 
                    (1.46 + 3.7) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"ALA",91]).[9] 
                    (1.42 + 3.7) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"SER",8]).[9] 
                    (1.46 + 3.7) 
                    "O Atoms need to be assigned correctly in Ala"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"ASN",42]).[13] 
                    (1.42 + 3.7) 
                    "protor radius FOR ASN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal
                    (vdw_raadi_cre_chains.['A',"CYS",98]).[9] 
                    (1.77 + 3.7) 
                    "protor radius FOR CYS and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_chains.['A',"TRP",127]).[10] 
                    (1.76 + 3.7) 
                    "effective radii must be determined correctly for CD1"
            
                Expect.throws (fun () -> 
                    determine_effective_radius all_creResidues.[0].Atoms.[3] "XYZ" "ABC" 
                    |>ignore
                ) "Probenames that are unknown should lead to a fail"


                let vdw_raadi_cre_FLOATProbe_chains = 
                    cre_example_chains
                    |> Seq.map (fun a -> a.Key,a.Value)
                    |> Seq.collect (fun (key, residues) ->                       
                        residues
                        |> Array.Parallel.map (fun residue ->
                            let keys = key,residue.ResidueName, residue.ResidueNumber
                            let residue =
                                residue.Atoms
                                |> Array.Parallel.map (fun atom -> 
                                    determine_effective_radius atom residue.ResidueName 4.)
                            keys, residue)) |> dict

               
                Expect.equal 
                    vdw_raadi_cre_FLOATProbe_chains.Count
                    parsedResidues_creChain.Length
                    "The number of chains in the pdb file should be equal"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"MET",1]).[0] 
                    (1.64 + 4.) 
                    "protor radius FOR Methionin and N  must be assigned 
                    correctly for Cre01g001550t11.pdb"
                
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"LEU",18]).[0] (1.64 + 4.) 
                    "protor radius FOR Methionin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe_chains.['A',"HIS",121]).[0] (1.64 + 4.) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe_chains.      ['B',"PRO",232]).[0] 
                    (1.64 + 4.) 
                    "H radius FOR Pro and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal (vdw_raadi_cre_FLOATProbe_chains.['A',"GLN",137]).[13] (1.64 + 4.) 
                    "protor radius FOR GLN and N must be assigned 
                    correctly for Cre01g001550t11.pdb"               

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"MET",1]).[1] 
                    (0. + 4.) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"ASP",100]).[1] 
                    (0. + 4.) 
                    "protor radius FOR ASP and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"GLU",101]).[1] 
                    (0. + 4.) 
                    "protor radius FOR GLU and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"THR",142]).[3] 
                    (0. + 4.) 
                    "H radius FOR Methionin and H must be assigned 
                    correctly for Cre01g001550t11.pdb"
                                
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"LYS",150]).[3] 
                    (0. + 4.) 
                    "protor radius FOR LYS and H must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"MET",1]).[4] 
                    (1.88 + 4.) 
                    "CA Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"ARG",60]).[5] 
                    (1.88 + 4.) 
                    "CB Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"GLY",74]).[5] 
                    (1.61 + 4.) 
                    "C Atoms need to be assigned correctly"
                
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"ILE",627]).[8] 
                    (1.88 + 4.) 
                    "C Atoms need to be assigned correctly"
              
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"PHE",695]).[9] 
                    (1.61 + 4.) 
                    "C Atoms need to be assigned correctly for PHE"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"VAL",299]).[2] 
                    (1.88 + 4.) 
                    "C Atoms need to be assigned correctly for Val"
                
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['B',"ALA",333]).[10] 
                    (1.46 + 4.) 
                    "oxidied Atoms need to be assigned correctly"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"TYR",76]).[10] 
                    (1.76 + 4.) 
                    "oxidied Atoms need to be assigned correctly" 
                    
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"ALA",91]).[9] 
                    (1.42 + 4.) 
                    "O Atoms need to be assigned correctly in Ala"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"SER",8]).[9] 
                    (1.46 + 4.) 
                    "O Atoms need to be assigned correctly in Ser"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"ASN",42]).[13] 
                    (1.42 + 4.) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"CYS",98]).[9] 
                    (1.77 + 4.) 
                    "protor radius FOR Leucin and N must be assigned 
                    correctly for Cre01g001550t11.pdb"

                
                Expect.equal 
                    (vdw_raadi_cre_FLOATProbe_chains.['A',"TRP",127]).[10] 
                    (1.76 + 4.) 
                    "effective radii must be determined correctly for CD1"

                Expect.throws (fun () -> 
                    determine_effective_radius all_creResidues.[0].Atoms.[3] "XYZ" -2
                    |>ignore
                ) "negative probe residues values should lead to a fail"

            }

            test "fibonacci spheres are created correctly and uniformy distributed"{
                
                let fibonacci_examples = fibonacciTestPoints 100            
                Expect.equal fibonacci_examples.Length 100 "The number of 
                testpoints to be created needs to be equal to the number you want"
             
                Array.iter (fun p ->
                      let r = sqrt(p.X*p.X + p.Y*p.Y + p.Z*p.Z)
                      Expect.floatClose Accuracy.high r 1.0 "Each point should 
                      lie on the unit sphere") fibonacci_examples
                
           
                Expect.equal (fibonacci_examples.[0].Y) 0.0 "the first Value for 
                y should be 0"
                let z_average = Array.averageBy (fun p -> p.Z) fibonacci_examples 
                Expect.floatClose Accuracy.high z_average 0.0 "The average of the 
                z values should be 0"
                
                
                let dz = 2.0 / float 100
                let expectedZ0    = 1.0 - (0.5) * dz
                let expectedZlast = 1.0 - (float (100-1) + 0.5) * dz

                let z0    = fibonacci_examples.[0].Z
                let zlast = fibonacci_examples.[100-1].Z

                Expect.floatClose Accuracy.high expectedZ0    z0    $"z₀ should 
                    be {expectedZ0}"
                Expect.floatClose Accuracy.high expectedZlast zlast $"zₙ₋₁ should 
                    be{expectedZlast}"   
               
                let testvectorsPython = 
                    [|
                        { X = 0.600000; Y = 0.000000; Z = 0.800000};
                        {X = -0.675810; Y = 0.619097; Z =0.400000};
                        {X= 0.087426; Y = -0.996171; Z = 0.000000};
                        {X=0.557643; Y = 0.727347; Z = -0.400000};
                        {X= -0.590828; Y = -0.104509; Z = -0.800000
}
                    |]

                let fibonacciTest = fibonacciTestPoints 5

                
                Array.iter2 (fun expected actual ->
                    Expect.floatClose Accuracy.low expected.X actual.X "Wrong X component"
                    Expect.floatClose Accuracy.low expected.Y actual.Y "Wrong Y component"
                    Expect.floatClose Accuracy.low expected.Z actual.Z "Z-component is wrong"
                ) testvectorsPython fibonacciTest

             
                let manyPoints = fibonacciTestPoints 10000
                Expect.equal manyPoints.Length 10000 "the same number of points 
                    have to be created then wanted "
                
                let distances = 
                    [| for i in 0 .. fibonacci_examples.Length - 2 do 
                        for j in i+1 .. fibonacci_examples.Length - 1 do
                            yield euclidianDistance fibonacci_examples.[i] fibonacci_examples.[j] |]
    
                let average = Array.average distances
                Expect.isGreaterThan average 0.1 
                    "Average distance between points should not be too small"
                Expect.isLessThan average 2.0 
                    "Average distance between points should not be too large"

                Expect.throws (fun () -> 
                    fibonacciTestPoints -2 |> ignore
                ) "Creating fibonacci points with negative nr of points should lead to a fail"

                Expect.throws (fun () -> 
                    fibonacciTestPoints 0 |> ignore
                ) "Creating fibonacci points with 0 points should lead to a fail"
            }

            test "fibonacci points are scaled correctly onto the atoms"{
                let fibonacci_example = fibonacciTestPoints 100
                let testdata = getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1
                let surfacepoints = 
                    testdata
                    |> Seq.map ( fun kvp ->
                        kvp.Key,
                        kvp.Value
                        |>Array.Parallel.map(fun residue ->                       
                                (residue.ResidueName,residue.ResidueNumber),residue.Atoms
                                |>Array.Parallel.map(fun atom -> (scaleFibonacciTestpoints fibonacci_example atom residue.ResidueName "Water")
                                )                              
                        )|>dict)|> dict

                                   
                Expect.equal surfacepoints.['A'].Count testdata.['A'].Length 
                    "The number of entries containing a surface point should be 
                    equal to the number of atoms"

                surfacepoints.['A'].Values
                |> Seq.collect (fun x -> x)
                |> Seq.iteri (fun i spList ->
                    Expect.equal (Array.length (spList)) 100 
                        "All atoms should contain exact 100 testpoints"
                )

                let dummyatom = testdata.['A'].[0].Atoms.[0]
                let testpointsDummy = scaleFibonacciTestpoints fibonacci_example dummyatom testdata.['A'].[0].ResidueName "Water"
                Expect.equal testpointsDummy.Length 100 "The number of testpoints 
                    should be 100"
                Expect.floatClose Accuracy.high testpointsDummy.[0].X -2.7781552262181566 
                    "The first testpoint x value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy.[0].Y 51.767 
                    "The first testpoint y value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy.[0].Z 13.779599999999999 
                    "The first testpoint z value should be equal to the atom position"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints (fibonacciTestPoints -3) dummyatom "XYZ" "Water" |> ignore
                ) "negative nr of testpoints should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints (fibonacciTestPoints 0) dummyatom "XYZ" "Water" |> ignore
                ) "0 nr of testpoints should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints fibonacci_example dummyatom "XYZ" -2 |> ignore
                ) "negative probe radius should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints fibonacci_example dummyatom "XYZ" "ABC" |> ignore
                ) "unknown probe radius should lead to a fail"
              

                // test for a file without missing infos 
         
                let cre_example = getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1

                let all_creResidues = 
                    cre_example.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.toArray
                              
                let surfacepoints_cre = 
                     cre_example.Values
                     |> Seq.collect (fun res -> res)
                     |> Seq.map (fun residue ->
                        let keys = residue.ResidueName, residue.ResidueNumber
                        let residue =
                            residue.Atoms
                            |> Array.map (fun atom -> 
                                scaleFibonacciTestpoints fibonacci_example atom residue.ResidueName "Biotin")

                        keys, residue) |> dict


                Expect.equal surfacepoints_cre.Count all_creResidues.Length
                    "The number of Residues with computed surface points in 
                    Cre01g001550t11.pdb file should be equal to the number of PDB Files"

                surfacepoints_cre.Values
                |> Seq.collect (fun x -> x)
                |> Seq.iteri (fun i spList ->
                    Expect.equal (Array.length (spList)) 100 
                        "All atoms should contain exact 100 testpoints"
                )

                let cre_testPointAtom =  cre_example.['A'].[0].Atoms.[0]

                let testpointsDummy_cre = scaleFibonacciTestpoints fibonacci_example cre_testPointAtom all_creResidues.[0].ResidueName "Water"

                Expect.equal testpointsDummy.Length 100 "The number of testpoints 
                    should be 100"
                Expect.floatClose Accuracy.high testpointsDummy_cre.[0].X 95.05084
                    "The first testpoint x value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy_cre.[0].Y -23.580
                    "The first testpoint y value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy_cre.[0].Z 29.1876
                    "The first testpoint z value should be equal to the atom position"

                // test for cre file with float probe

                let creTestDummyWithFloatProbe = 
                    scaleFibonacciTestpoints fibonacci_example cre_testPointAtom all_creResidues.[0].ResidueName 4.0

                Expect.equal creTestDummyWithFloatProbe.Length 100 "The number of testpoints 
                    should be 100"
                Expect.floatClose Accuracy.high creTestDummyWithFloatProbe.[0].X 95.41762
                    "The first testpoint x value should be equal to the atom position"
                Expect.floatClose Accuracy.high creTestDummyWithFloatProbe.[0].Y -23.580
                    "The first testpoint y value should be equal to the atom position"
                Expect.floatClose Accuracy.high creTestDummyWithFloatProbe.[0].Z 31.7616
                    "The first testpoint z value should be equal to the atom position"

                // test for cre file with multiple chains 

                let cre_example_chains = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1
        
                let all_creResidues_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.toArray
                              
                let surfacepoints_cre_chains = 
                    cre_example_chains
                    |> Seq.map (fun a -> a.Key,a.Value)
                    |> Seq.collect (fun (key, residues) ->                       
                        residues
                        |> Array.Parallel.map (fun residue ->
                            let keys = key,residue.ResidueName, residue.ResidueNumber
                            let residue =
                                residue.Atoms
                                |> Array.Parallel.map (fun atom -> 
                                    scaleFibonacciTestpoints fibonacci_example atom residue.ResidueName "Biotin")
                            keys, residue)) |> dict

                Expect.equal 
                    surfacepoints_cre_chains.Count 
                    all_creResidues_chains.Length
                    "The number of Residues with computed surface points in 
                    Cre01g001550t11.pdb file should be equal to the number of PDB Files"

                surfacepoints_cre_chains.Values
                |> Seq.collect (fun x -> x)
                |> Seq.iteri (fun i spList ->
                    Expect.equal (Array.length (spList)) 100 
                        "All atoms should contain exact 100 testpoints"
                )

                let cre_testPointAtom_chain =  
                    all_creResidues_chains
                    |> Array.head
                    |> fun res -> res.Atoms
                    |> Array.head
                 
                let testpointsDummy_cre_chain = 
                    scaleFibonacciTestpoints 
                        fibonacci_example cre_testPointAtom_chain all_creResidues.[0].ResidueName "Biotin"

                Expect.equal 
                    testpointsDummy_cre_chain.Length 100 
                    "The number of testpoints should be 100"

                Expect.floatClose Accuracy.high
                    testpointsDummy_cre_chain.[0].X -14.2147002986858412604
                    "The first testpoint x value should be equal to the atom position"
                Expect.floatClose Accuracy.high 
                    testpointsDummy_cre_chain.[0].Y -49.282
                    "The first testpoint y value should be equal to the atom position"
                Expect.floatClose Accuracy.high 
                    testpointsDummy_cre_chain.[0].Z -65.6154
                    "The first testpoint z value should be equal to the atom position"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints (fibonacciTestPoints -3) cre_testPointAtom_chain "XYZ" "Biotin" |> ignore
                ) "negative nr of testpoints should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints (fibonacciTestPoints 0) cre_testPointAtom_chain "XYZ" "Biotin" |> ignore
                ) "0 nr of testpoints should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints fibonacci_example cre_testPointAtom_chain "XYZ" -2 |> ignore
                ) "negative probe radius should lead to a fail"

                Expect.throws (fun () -> 
                    scaleFibonacciTestpoints fibonacci_example cre_testPointAtom_chain "XYZ" "ABC" |> ignore
                ) "unknown probe radius should lead to a fail"

            }

            test "acessible testpoints are identified correctly and euclidian 
                distance is determined correctly" {
                let p = {X = 0.000000; Y= 1.000000; Z= 0.000000}
                let q = {X = 8.000000; Y= -12.000000; Z= 3.000000} 
                let euclid_example1 = euclidianDistance  p q
                Expect.isGreaterThan euclid_example1 0 "Distance should be 
                    greater than 0"
                Expect.floatClose Accuracy.low euclid_example1 15.556 
                    "The distance has to be computed correctly"

                let euclid_example2 = euclidianDistance q p 
                Expect.floatClose Accuracy.low euclid_example2 15.556 
                    "The distance has to be computed correctly and should be 
                    symetrically"
           
                let example_itsself = euclidianDistance p p
                Expect.equal example_itsself 0.0 "The distance between a point 
                    and itself should be 0"

                let testdata = getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1
                let totalnr_points = 100
                let acessiblePointsDictExampleFile  = 
                    testdata
                    |> Seq.map (fun kvp ->
                        let chain    = kvp.Key
                        let residues = kvp.Value

                        let allAtomsOfChain =
                            residues
                            |> Array.collect (fun residue ->
                                residue.Atoms
                                |> Array.map (fun atom -> atom, residue.ResidueName)
                            )

                    
                        let allCounts : float[] = accessibleTestpoints allAtomsOfChain totalnr_points "Water"
                    
                        let atomCountTuples =
                            Array.zip allAtomsOfChain allCounts
                           
                        let perRes =
                            residues
                            |> Array.Parallel.map (fun residue ->
                                let key = residue.ResidueName, residue.ResidueNumber
                   
                                let residueAtoms =
                                    residue.Atoms
                                    |> Array.map (fun atom -> atom, residue.ResidueName)
                                
                                let counts =
                                    atomCountTuples
                                    |> Array.choose (fun ((atom, resName), count) ->
                                        if resName = residue.ResidueName && Array.exists (fun a -> a = atom) residue.Atoms
                                        then Some count
                                        else None
                                    )

                                key, counts
                            )
                            |> dict

                        chain, (perRes :> IDictionary<_,_>)
                    )
                    |> dict
                    

                Expect.equal acessiblePointsDictExampleFile.Count  testdata.Count 
                    "The number of chains for the acessible ids should be the same"
                Expect.equal (acessiblePointsDictExampleFile.Keys |> Seq.toArray) (testdata.Keys |> Seq.toArray) 
                    "The keys of the acessible points should be equal to the 
                    keys of the atoms"
                Expect.equal  (Seq.length acessiblePointsDictExampleFile.['A']) (Seq.length testdata.['A' ]) "The number of Residues should be equal 
                    to the nr of arrays in the same chain showing the points"

                let testpointExample = round ((44.83712/(4.* System.Math.PI * (3.04**2))) * 100.0)

                Expect.floatClose Accuracy.high (acessiblePointsDictExampleFile.['A']).["ASN",65].[0] testpointExample 
                    "The first testpoint should be equal to the value from biopython"


                let residuesHTQ = getResiduesPerChain "resources/pdbParser/htq.pdb" 1
                let acessiblePointsDictHTQ = 
                    residuesHTQ                   
                    |> Seq.map (fun kvp ->
                        let chain    = kvp.Key
                        let residues = kvp.Value
                       
                        let perRes =
                            residues
                            |> Array.Parallel.map (fun residue ->
                          
                                let key = residue.ResidueName, residue.ResidueNumber

                                let atomTuples : (Atom*string)[] =
                                    residue.Atoms
                                    |> Array.map (fun atom -> atom, residue.ResidueName)
 
                                let counts : float[] =
                                    accessibleTestpoints atomTuples totalnr_points "Water"

                                key, counts
                            )
                            |> dict

                        chain, (perRes :> IDictionary<_,_>)
                    )
                    |> dict

                for key in acessiblePointsDictHTQ.Keys do                    
                    let atomLength = residuesHTQ.[key].Length
                    let acessibleLength = Seq.length acessiblePointsDictHTQ.[key]
                    Expect.equal atomLength acessibleLength 
                        $"Number of parsed atoms should be equal to the number 
                        of values showing the acessiblepoint."

                let acessiblePointsnrhtq = 
                    [|
                        for key in acessiblePointsDictHTQ.Keys do
                            for res in acessiblePointsDictHTQ.[key].Keys do                              
                                Array.forall (fun atom -> 
                                    atom <= 100.0 && atom >= 0.0                      
                                    ) acessiblePointsDictHTQ.[key].[res]
                    |]

                Expect.allEqual acessiblePointsnrhtq true 
                    "number of acessible points should be less or equal to nr 
                    of testpoints "

                // test for a file without missing infos 

                let cre_example = getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1
                let totalnr_points = 100
                let acessiblePointsDictExampleFile_cre  = 
                    cre_example
                    |> Seq.map (fun kvp ->
                        let chain    = kvp.Key
                        let residues = kvp.Value

                        let allAtomsOfChain =
                            residues
                            |> Array.collect (fun residue ->
                                residue.Atoms
                                |> Array.map (fun atom -> atom, residue.ResidueName)
                            )

                    
                        let allCounts : float[] = accessibleTestpoints allAtomsOfChain totalnr_points "Biotin"
                    
                        let atomCountTuples =
                            Array.zip allAtomsOfChain allCounts
                           
                        let perRes =
                            residues
                            |> Array.Parallel.map (fun residue ->
                                let key = residue.ResidueName, residue.ResidueNumber
                   
                                let residueAtoms =
                                    residue.Atoms
                                    |> Array.map (fun atom -> atom, residue.ResidueName)
                                
                                let counts =
                                    atomCountTuples
                                    |> Array.choose (fun ((atom, resName), count) ->
                                        if resName = residue.ResidueName && Array.exists (fun a -> a = atom) residue.Atoms
                                        then Some count
                                        else None
                                    )

                                key, counts
                            )
                            |> dict

                        chain, (perRes)
                    )
                    |> dict
                    

                Expect.equal acessiblePointsDictExampleFile_cre.Count  cre_example.Count 
                    "The number of chains for the acessible ids should be the same"
                Expect.equal (acessiblePointsDictExampleFile_cre.Keys |> Seq.toArray) (cre_example.Keys |> Seq.toArray) "The keys of the acessible points should be equal to the keys of the atoms"
                Expect.equal  (Seq.length acessiblePointsDictExampleFile_cre.['A']) (Seq.length cre_example.['A' ]) "The number of Residues should be equal to the nr of arrays in the same chain showing the points"

                let testpointExample_cre = round ((86.0010234937969/(4.* System.Math.PI * (5.34**2))) * 100.0)

                Expect.floatClose Accuracy.high (acessiblePointsDictExampleFile_cre.['A']).["MET",1].[0] testpointExample_cre "The first testpoint should be equal to the value from biopython"

                Expect.floatClose Accuracy.high (acessiblePointsDictExampleFile_cre.['A']).["MET",1].[0] testpointExample_cre "The first testpoint should be equal to the value from biopython"

                let testpointExample_cre2 = round ((7.825430839938659/(4.* System.Math.PI * (5.59**2))) * 100.0)

                Expect.floatClose Accuracy.high (acessiblePointsDictExampleFile_cre.['A']).["MET",1].[4]        testpointExample_cre2 "The first testpoint should be equal to the value from biopython"

                Expect.throws (fun () -> 
                    accessibleTestpoints [||] 100 "Water" |> ignore
                ) "If no atoms are given, the function should throw an error"

                let all_creResidues_atoms = 
                    cre_example.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.collect (fun residue ->
                        residue.Atoms
                        |> Array.map (fun atom -> atom, residue.ResidueName)
                    ) |> Seq.toArray

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms -2 "Water" |> ignore
                ) "If the number of testpoints is negative, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms 0 "Water" |> ignore
                ) "If the number of testpoints is 0, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms 100 "XYZ" |> ignore
                ) "If the probe name is unknown, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms 100 -2 |> ignore
                ) "If the probe radius is negative, the function should throw an error"

                // test for a file with multiple chains

                let cre_example_chains = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1
                
                let acessiblePoints_crechain  = 
                    cre_example_chains
                    |> Seq.collect(fun kvp ->
                        let chain    = kvp.Key
                        let residues = kvp.Value

                        let allAtomsOfChain =
                            residues
                            |> Array.collect (fun residue ->
                                residue.Atoms
                                |> Array.map (fun atom -> atom, residue.ResidueName)
                            )

                    
                        let allCounts : float[] = accessibleTestpoints allAtomsOfChain totalnr_points "Biotin"
                    
                        let atomCountTuples =
                            Array.zip allAtomsOfChain allCounts
                           
                        let perRes =
                            residues
                            |> Array.Parallel.map (fun residue ->
                                let key = chain,residue.ResidueName, residue.ResidueNumber
                   
                                let residueAtoms =
                                    residue.Atoms
                                    |> Array.map (fun atom -> chain,atom, residue.ResidueName)
                                
                                let counts =
                                    atomCountTuples
                                    |> Array.choose (fun ((atom, resName), count) ->
                                        if resName = residue.ResidueName && Array.exists (fun a -> a = atom) residue.Atoms
                                        then Some count
                                        else None
                                    )

                                key, counts
                            )
                        perRes
                        )|> dict

                let chains_acessible = 
                 acessiblePoints_crechain.Keys
                 |> Seq.map (fun (chain,_,_) -> chain)
                 |> Seq.distinct
                 |> Seq.toArray

                let allResidues_chains_cre = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.toArray
                                                  
                Expect.equal chains_acessible.Length  cre_example_chains.Count 
                    "The number of chains for the acessible ids should be the same"
                
                Expect.equal (chains_acessible) (cre_example_chains.Keys |> Seq.toArray) "The keys of the acessible points should be equal to the keys of the atoms"
                
                Expect.equal  
                    (acessiblePoints_crechain.Count) 
                    (allResidues_chains_cre.Length)
                    "The number of Residues should be equal to the nr of arrays in the same chain showing the points"

                let testpointExample_cre_chain = 
                    round ((100.33452740942973/(4.* System.Math.PI * (5.34**2))) * 100.0)

                Expect.floatClose 
                    Accuracy.high (acessiblePoints_crechain.['A',"MET",1]).[0]testpointExample_cre_chain
                    "The first testpoint should be equal to the value from biopython"
          
                let testpointExample_cre2_chain = 
                    round ((11.738146259907989/(4.* System.Math.PI * (5.59**2))) * 100.0)

                Expect.floatClose Accuracy.high 
                   (acessiblePoints_crechain.['A',"MET",1]).[4]
                   testpointExample_cre2_chain
                    "all testpoint should be equal to the value from biopython"
   
                let all_creResidues_atoms_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.collect (fun residue ->
                        residue.Atoms
                        |> Array.map (fun atom -> atom, residue.ResidueName)
                    ) |> Seq.toArray

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms_chains -2 "Water" |> ignore
                ) "If the number of testpoints is negative, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms_chains 0 "Water" |> ignore
                ) "If the number of testpoints is 0, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms_chains 100 "XYZ" |> ignore
                ) "If the probe name is unknown, the function should throw an error"

                Expect.throws (fun () -> 
                    accessibleTestpoints all_creResidues_atoms_chains 100 -2 |> ignore
                ) "If the probe radius is negative, the function should throw an error"
       
            }

            test "SASA per Atom is computed correctly"{

                let testdata = getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1

                let atomSASAtestdata = sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  1 100 "Biotin"

                let nr_chains = atomSASAtestdata.Count
                let nr_residues = atomSASAtestdata.['A'].Count

                Expect.equal nr_chains testdata.Count 
                    "The number of chains should be equal to proiginal number 
                        of Chains"
                Expect.equal nr_residues (testdata.['A']).Length 
                    "The number of residues should be equal to the number of 
                        residues in the PDB file"
                        
                let nr_sasavalues = 
                    atomSASAtestdata.['A'].Values 
                    |> Seq.sumBy (fun x -> x.Length)
                 
                Expect.equal nr_sasavalues (reference_sasaArray.Length) 
                    "The number of SASA values should be the same as in the 
                    Python reference with only one chain"

                Expect.floatClose Accuracy.high 
                    atomSASAtestdata.['A'].[(65,"ASN")].[0] reference_sasaArray.[0] 
                    "The SASA value for the first atom should be equal to the 
                    one from the python reference"

                let exampleAtomsSASA = 
                    atomSASAtestdata.['A'].Values 
                    |> Seq.collect ( fun x -> x) 
                    |> Seq.indexed 
                    |> Seq.toArray
                
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atom with the serialnumber {fsharpSeries+1} 
                        should be equal to the one from the python reference"
                ) exampleAtomsSASA python_SASA_array

                Expect.throws (fun () -> sasaAtom "resources/notexisting.pdb"  2 100 "Biotin" |>ignore)
                    "unknown PDB Files should lead to a error "

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  5 100 "Biotin" |>ignore)
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  -1 100 "XYZ" |>ignore)
                    "SASA Atom with negative model id should fail"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  1 -100 "XYZ" |>ignore)
                    "negative nr of points leads to an error in SASAAtom"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  1 100 -2 |>ignore)
                    "negative probe radius leads to an error in SASAAtom"

                Expect.throws (fun () -> 
                    sasaAtom "resources/pdbParser/rubisCOActivase.pdb"  1 100 "XYZ" 
                    |>ignore)
                    "SASA Atom with unknown Probename should fail"

                // test for a file without missing infos

                let cre_example = 
                    getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1 

                let cre_sasaAtom = 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin"

                Expect.equal cre_sasaAtom.Count cre_example.Count 
                    "The number of chains in the Cre01g001550t11.pdb file 
                    should be equal"

                let cre_exampleResidues = 
                    cre_example.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.toArray 

                let atom_sasaResidues_cre = 
                    cre_sasaAtom.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.equal atom_sasaResidues_cre.Length cre_exampleResidues.Length
                    "The number of residues with stored SASA values for every 
                    atom should be equal to the number of residues in the PDB file"

                let allAtoms_cre = 
                    cre_exampleResidues
                    |> Array.collect (fun residue -> residue.Atoms)

                let sasa_atoms_cre = 
                    atom_sasaResidues_cre
                    |> Array.collect (fun x -> x)

                Expect.equal allAtoms_cre.Length sasa_atoms_cre.Length 
                    "For every best atom in the residue a SASA value should be 
                    computed"

                let creFsharpcomputedSASA = 
                    let atomelements = 
                         allAtoms_cre
                         |> Array.map (fun atom -> atom.Element)

                    Array.zip atomelements sasa_atoms_cre
                    |> Array.filter(fun (el,_) -> el<> "H")
                
                Expect.equal 
                    (creFsharpcomputedSASA.Length) combined_creArray.Length
                    "The number of SASA values should be equal to the number of 
                    SASA values in the python reference for Cre01g001550t11.pdb 
                    with Biotin"

                Expect.floatClose Accuracy.high sasa_atoms_cre.[0] crePythonReference_SASA.[0] 
                    "The SASA value for the first atom should be equal to the 
                    one from the python reference for Cre01g001550t11.pdb 
                    with Biotin"
             
                Array.iteri2 (fun idx (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the idx atom in the list
                        should be equal to the one from the python reference"
                ) creFsharpcomputedSASA combined_creArray

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 8 100 "Biotin" |>ignore)
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "XYZ" |>ignore)
                    "SASA Atom with unknown Probename should fail"

                // test for a file with missing infos with float probe 
                
                let cre_sasaAtom_withfloatprobe = 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4.

                Expect.equal cre_sasaAtom_withfloatprobe.Count cre_example.Count 
                    "The number of chains in the Cre01g001550t11.pdb file 
                    should be equal"

                let atom_sasaResidues_withfloatprobe = 
                    cre_sasaAtom_withfloatprobe.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.equal 
                    atom_sasaResidues_withfloatprobe.Length 
                    cre_exampleResidues.Length
                    "The number of residues with stored SASA values for every 
                    atom should be equal to the number of residues 
                    in the PDB file"

                let sasa_atoms_cre_floatprobe = 
                    atom_sasaResidues_withfloatprobe
                    |> Array.collect (fun x -> x)

                Expect.equal 
                    allAtoms_cre.Length 
                    sasa_atoms_cre_floatprobe.Length 
                    "For every best atom in the residue a SASA value should be 
                    computed"

                let creFsharpcomputedSASA_float = 
                    let atomelements = 
                         allAtoms_cre
                         |> Array.map (fun atom -> atom.Element)

                    Array.zip atomelements sasa_atoms_cre_floatprobe
                    |> Array.filter(fun (el,_) -> el<> "H")
                
                Expect.equal 
                    (creFsharpcomputedSASA_float.Length) 
                    combined_creArray_floatprobe.Length
                    "The number of SASA values should be equal to the number of 
                    SASA values in the python reference for Cre01g001550t11.pdb 
                    with Biotin"

                Expect.floatClose Accuracy.high 
                    sasa_atoms_cre_floatprobe.[0] 
                    crePythonReference_SASA_floatProbe.[0] 
                    "The SASA value for the first atom should be equal to the 
                    one from the python reference for Cre01g001550t11.pdb 
                    with Biotin"
             
                Array.iteri2 (fun idx (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the idx atom in the list
                        should be equal to the one from the python reference"
                ) creFsharpcomputedSASA_float combined_creArray_floatprobe

                Expect.throws (fun () -> 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 2 100 1.4 |>ignore)
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                // test for a cre file with multiple chains

                let cre_example_chains = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1

                let cre_sasaAtom_chains = 
                    sasaAtom "resources/pdbParser/Cre01g026150t11.pdb" 1 100 "Biotin"

                Expect.equal cre_sasaAtom.Count cre_example.Count 
                    "The number of chains in the Cre01g001550t11.pdb file 
                    should be equal"

                let cre_exampleResidues_chains = 
                    cre_example_chains.Values 
                    |> Seq.collect (fun res -> res)
                    |> Seq.toArray 

                let atom_sasaResidues_chainscre = 
                    cre_sasaAtom_chains.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.equal 
                    atom_sasaResidues_chainscre.Length 
                    cre_exampleResidues_chains.Length
                    "The number of residues with stored SASA values for every 
                    atom should be equal to the number of residues in the PDB file"

                let allAtoms_creChains = 
                    cre_exampleResidues_chains
                    |> Array.collect (fun residue -> residue.Atoms)

                let sasa_atoms_creChains = 
                    atom_sasaResidues_chainscre
                    |> Array.collect (fun x -> x)

                Expect.equal 
                    allAtoms_creChains.Length 
                    sasa_atoms_creChains.Length 
                    "For every best atom in the residue a SASA value should be 
                    computed"

                let creFsharpcomputedSASA_chains = 
                    let atomelements = 
                         allAtoms_creChains
                         |> Array.map (fun atom -> atom.Element)

                    Array.zip atomelements sasa_atoms_creChains
                    |> Array.filter(fun (el,_) -> el<> "H")
                
                Expect.equal 
                    (creFsharpcomputedSASA_chains.Length) 
                    combined_cre2Array.Length
                    "The number of SASA values should be equal to the number of 
                    SASA values in the python reference for Cre01g001550t11.pdb 
                    with Biotin"

                Expect.floatClose Accuracy.high 
                    sasa_atoms_creChains.[0] 
                    cre2PythonReference_SASA.[0] 
                    "The SASA value for the first atom should be equal to the 
                    one from the python reference for Cre01g001550t11.pdb 
                    with Biotin"
             
                Array.iteri2 (fun idx (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the idx atom in the list
                        should be equal to the one from the python reference"
                ) creFsharpcomputedSASA_chains combined_cre2Array

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 8 100 "Biotin" |>ignore)
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws (fun () -> 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" -1 100 "Water" |>ignore)
                    "SASA Atom with negative model id should fail"

                Expect.throws (fun () -> 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb"  1 -100 "Water" |>ignore)
                    "negative nr of points leads to an error in SASAAtom"

                Expect.throws (fun () -> 
                    sasaAtom "resources/pdbParser/Cre01g001550t11.pdb"  1 0 "Water" |>ignore)
                    "0 points leads to an error in SASAAtom"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "XYZ" |>ignore)
                    "SASA Atom with unknown Probename should fail"

                Expect.throws (fun () -> sasaAtom "resources/pdbParser/Cre01g001550t11.pdb" 1 100 -2 |>ignore)
                    "SASA Atom with unknown negative probe radius should fail"
                        
            }

            test "Absolute SASA per Residue is computed correctly"{
                let residueSASAtestdata = 
                    sasaResidue ("resources/pdbParser/rubisCOActivase.pdb") 1 100 "Water"

                let parsedResidues_parser = 
                    getResiduesPerChain "resources/pdbParser/rubisCOActivase.pdb" 1

                Expect.all (residueSASAtestdata['A'].Values) (fun x -> x >= 0.0) 
                    "All SASA values need to be null or positive"
                Expect.equal 
                    (residueSASAtestdata['A'].Count) parsedResidues_parser.['A'].Length 
                        "The number of residues should be equal to the number of 
                        residues in the PDB file"          

                Expect.equal 
                    (residueSASAtestdata['A'].Count) python_SASA_arrayResidues.Length 
                    "The number of SASA values should be the same as in the Python reference"

                Expect.floatClose Accuracy.high 
                    residueSASAtestdata['A'].[65,"ASN"] sasaArrays.[0] 
                    "The SASA value for the first residue should be equal to the 
                    one from the python reference"

                let resNumbersOriginal = 
                    (residueSASAtestdata['A'].Keys) 
                    |>Seq.map (fun (x,y) -> x)

                Expect.equal (resNumbersOriginal |> Seq.toArray) resNumber 
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation"

                let exampleAtomsSASA = 
                    residueSASAtestdata['A'].Values 
                    |> Seq.indexed 
                    |> Seq.toArray

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atom with the serialnumber 
                        {fsharpSeries} should be equal to the one from the 
                        python reference"
                ) exampleAtomsSASA python_SASA_arrayResidues

                Expect.throws (fun () -> sasaResidue "notexisting.pdb" 1 100 "Water" |> ignore
                )
                    "Not existing PDB Files lead to an error"

                Expect.throws (fun () -> 
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb"  5 100 "Water" |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb"  -1 100 "Water" |>ignore) 
                    "negative model id leads to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb"  1 0 "Water" |>ignore)
                    "Zero testpoints lead to an error"

                
                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb"  1 -2 "Water" |>ignore)
                    "negative testpoints lead to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb"  1 100 "aa" |>ignore)
                    "unknown probes lead to an error"

                Expect.throws (fun () -> sasaResidue "resources/pdbParser/rubisCOActivase.pdb" 1 100 -2 |>ignore)
                    "negative probes lead to an error"
        
                // Htq example
                let exampleSASAresiduesHTQ = 
                    sasaResidue "resources/pdbParser/htq.pdb" 1 100 "Water"
                
                let parsedChainsHTQ = readModels (readPBDFile "resources/pdbParser/htq.pdb")

                Expect.equal exampleSASAresiduesHTQ.Count (parsedChainsHTQ.[0].Chains.Length) 
                    "The number of chains should be equal to the number of chains 
                    in the PDB file for the corresponding model"

                let collectedResidueshtq = 
                    exampleSASAresiduesHTQ.Values 
                    |> Seq.collect (fun x -> x.Values) 

                Expect.all collectedResidueshtq (fun x -> x >= 0.0) 
                    "All SASA values need to be null or positive"

                Expect.throws (fun () -> 
                    sasaResidue "resources/pdbParser/htq.pdb"  8745 100 "Water" 
                    |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/htq.pdb" -1 100 "Water" 
                    |>ignore) 
                    "negative model id leads to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/htq.pdb" 1 0 "Water" 
                    |>ignore)
                    "Zero testpoints lead to an error"
                
                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/htq.pdb" 1 -2 "Water" 
                    |>ignore)
                    "negative testpoints lead to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/htq.pdb" 1 100 "aa" 
                    |>ignore)
                    "unknown probes lead to an error"

                Expect.throws (fun () -> 
                sasaResidue "resources/pdbParser/htq.pdb" 1 100 -2 |>ignore)
                    "negative probes lead to an error"

                // test for a file without missing infos 
             
                let residueSASAdict_cre = 
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin"

                let allsasaRes_collection = 
                    residueSASAdict_cre.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray
                    
                Expect.all 
                    allsasaRes_collection (fun residuesasa -> residuesasa >= 0.0) 
                    "All SASA values need to be null or positive"

                let residuesdict_cre = 
                    getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1

                let chainIds_sasa_cre = 
                    residueSASAdict_cre.Keys |> Seq.toArray

                let chainIds_pdb_cre = 
                    residuesdict_cre.Keys |> Seq.toArray

                Expect.equal chainIds_sasa_cre chainIds_pdb_cre
                    "The chain ids of the residue SASA dict should be equal to 
                    the chain Ids extracted from the PDB file"

                let sasa_residuesIdentifiers = 
                    residueSASAdict_cre.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray

                let residuesIdentifiers_pdb =
                    residuesdict_cre.Values
                    |> Seq.collect (fun x -> x)
                    |> Seq.map (fun residue -> 
                        residue.ResidueNumber, residue.ResidueName
                     )
                    |> Seq.toArray

                Expect.equal 
                    sasa_residuesIdentifiers 
                    residuesIdentifiers_pdb 
                    "The residue identifiers of the residue SASA dict should be 
                    equal to the residue identifiers extracted from the 
                    PDB file for the cre example"
                                   
                Expect.floatClose Accuracy.high 
                      (allsasaRes_collection.[0]) cre_sasaArrays.[0] 
                    "The SASA value for the first residue should be 
                    nearly equal to the one from the python reference"

                Expect.equal 
                    sasa_residuesIdentifiers cre_residue 
                    "The residue identifiers should be equal 
                    to the ones from the python computation"

                let combined_creArray = 
                    Array.zip sasa_residuesIdentifiers allsasaRes_collection

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atom with the serialnumber 
                        {fsharpSeries} should be equal to the one from the 
                        python reference"
                ) combined_creArray cre_python_SASA_arrayResidues
               
                Expect.throws (fun () -> 
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb"  5 100 "Water" |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb"  -1 100 "Water" |>ignore) 
                    "negative model id leads to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb"  1 0 "Water" |>ignore)
                    "Zero testpoints lead to an error"

                
                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb"  1 -2 "Water" |>ignore)
                    "negative testpoints lead to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb"  1 100 "aa" |>ignore)
                    "unknown probes lead to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb" 1 100 -3 |>ignore)
                    "negative probes lead to an error"
                    
            // test for a file with missing infos with float probe 

                let residueSASAdict_cre_floatprobe = 
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4.

                let allsasaRes_collection_floatprobe = 
                    residueSASAdict_cre_floatprobe.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray
                    
                Expect.all 
                    allsasaRes_collection_floatprobe (fun residuesasa -> 
                        residuesasa >= 0.0) 
                    "All SASA values need to be null or positive, 
                    also with float probes"         

                let chainIds_sasa_cre_floatprobe = 
                    residueSASAdict_cre_floatprobe.Keys |> Seq.toArray

                Expect.equal chainIds_sasa_cre_floatprobe chainIds_pdb_cre
                    "The chain ids of the residue SASA dict should be equal to 
                    the chain Ids extracted from the PDB file, 
                    also with floatprobe"

                let sasa_residuesIdentifiers_floatprobe = 
                    residueSASAdict_cre_floatprobe.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray

                Expect.equal 
                    sasa_residuesIdentifiers_floatprobe 
                    residuesIdentifiers_pdb 
                    "The residue identifiers of the residue SASA dict should be 
                    equal to the residue identifiers extracted from the 
                    PDB file for the cre example, also with float probe"

                                   
                Expect.floatClose Accuracy.high 
                        allsasaRes_collection_floatprobe.[0] 
                        float_cre_sasaArrays.[0] 
                    "The SASA value for the first residue should be 
                    nearly equal to the one from the python reference, 
                    also with floatprobe"

                Expect.equal 
                    sasa_residuesIdentifiers_floatprobe float_cre_residue 
                    "The residue identifiers should be equal 
                    to the ones from the python computation"

                let combined_creArray_float = 
                    Array.zip sasa_residuesIdentifiers_floatprobe allsasaRes_collection_floatprobe

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atom with the serialnumber 
                        {fsharpSeries} should be equal to the one from the 
                        python reference"
                ) combined_creArray_float float_cre_python_SASA_arrayResidues

                // test cre with multiple chains
          
                let creResidue_chains = 
                    sasaResidue 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1 
                        100 
                        "Biotin"

                let residueSASAdict_creChain = 
                    sasaResidue 
                        "resources/pdbParser/Cre01g026150t11.pdb"  
                        1 
                        100 
                        "Biotin"

                let allsasaRes_collection_chain = 
                    residueSASAdict_creChain.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray
                    
                Expect.all 
                    allsasaRes_collection_chain (fun residuesasa -> 
                    residuesasa >= 0.0) 
                    "All SASA values need to be null or positive"

                let residuesdict_cre_chain = 
                    getResiduesPerChain "resources/pdbParser/Cre01g026150t11.pdb" 1

                let chainIds_sasa_cre_chain = 
                    residueSASAdict_creChain.Keys |> Seq.toArray

                let chainIds_pdb_cre_chain = 
                    residuesdict_cre_chain.Keys |> Seq.toArray

                Expect.equal chainIds_sasa_cre_chain chainIds_pdb_cre_chain
                    "The chain ids of the residue SASA dict should be equal to 
                    the chain Ids extracted from the PDB file"

                let sasa_residuesIdentifiers_crechain = 
                    residueSASAdict_creChain.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray

                let residuesIdentifiers_pdb_crechain =
                    residuesdict_cre_chain.Values
                    |> Seq.collect (fun x -> x)
                    |> Seq.map (fun residue -> 
                        residue.ResidueNumber, residue.ResidueName
                     )
                    |> Seq.toArray

                Expect.equal 
                    sasa_residuesIdentifiers_crechain 
                    residuesIdentifiers_pdb_crechain  
                    "The residue identifiers of the residue SASA dict should be 
                    equal to the residue identifiers extracted from the 
                    PDB file for the cre example"
                                   
                Expect.floatClose Accuracy.high 
                      (allsasaRes_collection_chain.[0]) cre2_sasaArrays.[0] 
                    "The SASA value for the first residue should be 
                    nearly equal to the one from the python reference"

                Expect.equal 
                    sasa_residuesIdentifiers_crechain cre2_residue 
                    "The residue identifiers should be equal 
                    to the ones from the python computation"

                let combined_cre2Array = 
                    Array.zip sasa_residuesIdentifiers_crechain allsasaRes_collection_chain

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atom with the serialnumber 
                        {fsharpSeries} should be equal to the one from the 
                        python reference"
                ) combined_cre2Array cre2_python_SASA_arrayResidues
               
                Expect.throws (fun () -> 
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb"   5 100 "Water" |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb"   -1 100 "Water" |>ignore) 
                    "negative model id leads to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb"   1 0 "Water" |>ignore)
                    "Zero testpoints lead to an error"

                
                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb"   1 -2 "Water" |>ignore)
                    "negative testpoints lead to an error"

                Expect.throws(fun () ->
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb"   1 100 "aa" |>ignore)
                    "unknown probes lead to an error"

                Expect.throws(fun () ->
                   sasaResidue "resources/pdbParser/Cre01g026150t11.pdb" 1 100 -3 |>ignore)
                   "negative probes lead to an error"

            }

            test "relative Residue SASA is computed correctly"{

                let rel_testdata = 
                    relativeSASA_aminoacids "resources/pdbParser/rubisCOActivase.pdb" 1 100 "Water" false 

                let residueSASAtestdata = 
                    sasaResidue ("resources/pdbParser/rubisCOActivase.pdb") 1 100 "Water"

                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA chain id should be equal to the 
                            absolute SASA chain id"
                )rel_testdata.Keys residueSASAtestdata.Keys

                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA residue id should be equal to the 
                            absolute SASA residue id"
                 )rel_testdata.['A'].Keys residueSASAtestdata.['A'].Keys

                
                Expect.equal 
                    (rel_testdata.['A'].Count) 
                    python_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                let allrelativeRes_sasa = 
                    rel_testdata.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray
              
                Expect.equal allrelativeRes_sasa.Length
                    python_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                Expect.floatClose Accuracy.high 
                    allrelativeRes_sasa.[0] sasaArrays_rel.[0] 
                    "The SASA value for the first residue should be equal to the 
                    one from the python reference"
                              
                let resNumbersOriginal = 
                    (rel_testdata.['A'].Keys) |> Seq.toArray

                Expect.equal (resNumbersOriginal) residue_rel
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation"

                let combinedTestdata = 
                    Array.zip resNumbersOriginal allrelativeRes_sasa               

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high  fSharpSASA pythonSASA
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) combinedTestdata python_SASA_array_relativeRes


                let rel_testdataWithFixedValue = 
                    relativeSASA_aminoacids "resources/pdbParser/rubisCOActivase.pdb" 1 100 "Water" true 

                let fixedMaxSASA_reference = 
                    rel_testdataWithFixedValue.['A'].Values 
                    |> Seq.indexed 
                    |> Seq.toArray
              
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high (fSharpSASA) (pythonSASA)
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) fixedMaxSASA_reference fixedSASA_rel

                Expect.throws (fun () ->
                    relativeSASA_aminoacids "notexisting.pdb" 1 100 "Biotin" true 
                    |> ignore)
                    "Wrong Filepath leads to an Error in relative Residue 
                    SASA computation"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 5 100  "Biotin" true |>ignore) 
                        "PDB File with wrong modelid should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" -1 100  "Biotin" true |>ignore) 
                        "PDB File with negative modelid should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 1 0  "Biotin" true |>ignore) 
                        "PDB Files with 0 Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 1 -10  "Biotin" true |>ignore) 
                        "PDB Files with negative nr of Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 1 -10  "XYZ" true |>ignore) 
                        "PDB Files with unknown probenames lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 1 -10  -3. true |>ignore) 
                        "PDB Files with negative proberadius lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 1 -10  -3. false|>ignore) 
                        "PDB Files with negative proberadius lead to an error"

                // test for a file without missing infos 

                let relativeSASA_dict_cre = 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin" false

                let residues_cre = getResiduesPerChain "resources/pdbParser/Cre01g001550t11.pdb" 1

                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA chain id should be equal to the 
                            absolute SASA chain id for cre with variable maxSASA"
                )relativeSASA_dict_cre.Keys residues_cre.Keys

                let allrelativeRes_identifiesrs_cre = 
                    relativeSASA_dict_cre.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray 

                let extracted_residues_cre_identifiers = 
                    residues_cre.Values
                    |> Seq.collect (fun x -> x)
                    |> Seq.map (fun residue -> 
                        residue.ResidueNumber, residue.ResidueName
                     )
                    |> Seq.toArray
                    
                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA residue id should be equal to the 
                            absolute SASA residue id for cre"
                 )allrelativeRes_identifiesrs_cre extracted_residues_cre_identifiers

                let cre_allrelativeRes_sasas = 
                    relativeSASA_dict_cre.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.all cre_allrelativeRes_sasas (fun x -> x >= 0.0) 
                    "All relative SASA values need to be null or positive for cre"
                             
                Expect.equal cre_allrelativeRes_sasas.Length
                    crepython_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference, also in cre"

                Expect.floatClose Accuracy.high 
                    cre_allrelativeRes_sasas.[0] cresasaArrays_rel.[0]
                    "The SASA value for the first residue should be equal to the 
                    one from the python reference, also in cre"

                Expect.equal 
                    allrelativeRes_identifiesrs_cre 
                    creresidue_rel 
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation, also in cre"

                let combined_creTestdata = 
                    Array.zip allrelativeRes_identifiesrs_cre cre_allrelativeRes_sasas
             
             
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high fSharpSASA pythonSASA
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) combined_creTestdata crepython_SASA_array_relativeRes

                let crerel_testdataWithFixedValue = 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin" true 

                let crefixedMaxSASA_reference = 
                    crerel_testdataWithFixedValue.['A'].Values 
                    |> Seq.indexed 
                    |> Seq.toArray
               
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high (fSharpSASA) (pythonSASA)
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) crefixedMaxSASA_reference crefixedSASA_rel
                             
   
                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" 5 100  "Biotin" false |>ignore) 
                        "PDB File with unknown modelid should throw 
                        an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" -1 100  "Biotin" true |>ignore) 
                        "PDB File with negative modelid should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" 1 0  "Biotin" true |>ignore) 
                        "PDB Files with 0 Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" 1 -10  "Biotin" true |>ignore) 
                        "PDB Files with negative nr of Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "xyz" true |>ignore) 
                        "PDB File with unknown probe name should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "xyz" false|>ignore) 
                        "PDB File with unknown probe name should throw an error"


                // test for a file with missing infos with float probe

                let relativeSASA_dict_cre_float = 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4. false

                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA chain id should be equal to the 
                            absolute SASA chain id for cre with variable maxSASA 
                            and float probe"
                )relativeSASA_dict_cre_float.Keys residues_cre.Keys

                let allrelativeRes_identifiesrs_cre_float = 
                    relativeSASA_dict_cre_float.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray 
                   
                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA residue id should be equal to 
                            the absolute SASA residue id for cre"
                 )allrelativeRes_identifiesrs_cre_float extracted_residues_cre_identifiers

                let cre_allrelativeRes_sasa_float = 
                    relativeSASA_dict_cre_float.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.all cre_allrelativeRes_sasa_float (fun x -> x >= 0.0) 
                    "All relative SASA values need to be null or positive for cre"
                             
                Expect.equal cre_allrelativeRes_sasa_float.Length
                    float_crepython_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference, also in cre"

                Expect.floatClose Accuracy.high 
                    cre_allrelativeRes_sasa_float.[0] float_cresasaArrays_rel.[0]
                    "The SASA value for the first residue should be equal to the 
                    one from the python reference, also in cre"

                Expect.equal 
                    allrelativeRes_identifiesrs_cre_float
                    float_creresidue_rel 
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation, also in cre"

                let combined_creTestdata_withfloatprobe = 
                    Array.zip 
                        allrelativeRes_identifiesrs_cre_float 
                        cre_allrelativeRes_sasa_float
             
             
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high fSharpSASA pythonSASA
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) combined_creTestdata_withfloatprobe float_crepython_SASA_array_relativeRes

                let crerel_testdataWithFixedValue_float = 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4. true 

                let crefixedMaxSASA_reference_float = 
                    crerel_testdataWithFixedValue_float.['A'].Values 
                    |> Seq.indexed 
                    |> Seq.toArray
               
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high (fSharpSASA) (pythonSASA)
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) crefixedMaxSASA_reference_float float_crefixedSASA_rel

                Expect.throws (fun() -> relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 -4. true |>ignore)
                    "Negative proberadius should lead to an fail"

                Expect.throws (fun() -> relativeSASA_aminoacids "resources/pdbParser/Cre01g001550t11.pdb" 1 100 -4. false |>ignore)
                    "Negative proberadius should lead to an fail"

                // test for testfile with multiple chains

                let relativeSASA_dict_creChain = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1 
                        100
                        "Biotin"
                        false

                let residues_creChain = 
                    getResiduesPerChain 
                        "resources/pdbParser/Cre01g026150t11.pdb" 1

                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA chain id should be equal to the 
                            absolute SASA chain id for cre with variable maxSASA"
                )relativeSASA_dict_creChain.Keys residues_creChain.Keys

                let allrelativeRes_identifiesrs_creChain = 
                    relativeSASA_dict_creChain.Values
                    |> Seq.collect (fun x -> x.Keys)
                    |> Seq.toArray 

                let extracted_residues_creChain_identifiers = 
                    residues_creChain.Values
                    |> Seq.collect (fun x -> x)
                    |> Seq.map (fun residue -> 
                        residue.ResidueNumber, residue.ResidueName
                     )
                    |> Seq.toArray
                    
                Seq.iter2 (fun x y -> 
                        Expect.equal x y 
                            "The relative SASA residue id should be equal to the 
                            absolute SASA residue id for cre"
                 )
                 allrelativeRes_identifiesrs_creChain 
                 extracted_residues_creChain_identifiers

                let creChain_allrelativeRes_sasas = 
                    relativeSASA_dict_creChain.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                Expect.all creChain_allrelativeRes_sasas (fun x -> x >= 0.0) 
                    "All relative SASA values need to be null or positive for cre"
                             
                Expect.equal 
                    creChain_allrelativeRes_sasas.Length
                    cre2python_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference, also in cre"

                Expect.floatClose Accuracy.high 
                    creChain_allrelativeRes_sasas.[0] 
                    cre2sasaArrays_rel.[0]
                    "The SASA value for the first residue should be equal to the 
                    one from the python reference, also in cre"

                Expect.equal 
                    allrelativeRes_identifiesrs_creChain 
                    cre2residue_rel 
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation, also in cre"

                let combined_cre2Testdata = 
                    Array.zip 
                        allrelativeRes_identifiesrs_creChain 
                        creChain_allrelativeRes_sasas
                          
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high fSharpSASA pythonSASA
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) combined_cre2Testdata cre2python_SASA_array_relativeRes

                let creChainrel_testdataWithFixedValue = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1 
                        100
                        "Biotin"
                        true

                let creChainfixedMaxSASA_reference = 
                    creChainrel_testdataWithFixedValue.Values 
                    |> Seq.collect (fun x -> x.Values )
                    |> Seq.indexed 
                    |> Seq.toArray
               
                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.floatClose Accuracy.high (fSharpSASA) (pythonSASA)
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) creChainfixedMaxSASA_reference cre2fixedSASA_rel
                             
   
                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb"  5 100  "Biotin" false |>ignore) 
                        "PDB File with unknown modelid should throw 
                        an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb"  -1 100  "Biotin" true |>ignore) 
                        "PDB File with negative modelid should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb"  1 0  "Biotin" true |>ignore) 
                        "PDB Files with 0 Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb"  1 -10  "Biotin" true |>ignore) 
                        "PDB Files with negative nr of Testpoints lead to an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g026150t11.pdb"  1 100 "xyz" true |>ignore) 
                        "PDB File with unknown probe name should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g026150t11.pdb"  1 100 "xyz" false|>ignore) 
                        "PDB File with unknown probe name should throw an error"
                        
                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g026150t11.pdb"  1 100 -2. true |>ignore) 
                        "PDB File with negative probe should throw an error"

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/pdbParser/Cre01g026150t11.pdb"  1 100 -2. false|>ignore) 
                        "PDB File with negative probe  should throw an error"
            }

            test "differentiation into exposed and buried is sucessfull"{
            
                let rel_testdata = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1 
                        100 
                        "Water" 
                        false

                let differentiatedSASA = 
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        100 
                        "Water" 
                        0.2  
                        false 

                let exposed = 
                    differentiatedSASA
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                    |>dict

                let buried = 
                    differentiatedSASA
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                    |>dict

                let exposedResidues = exposed.['A'] 
                let buriedResidues = buried.['A']

                Expect.equal 
                    (exposedResidues.Count + buriedResidues.Count) (rel_testdata.['A'].Count) 
                    "The number of exposed and buried residues should be equal 
                    to the number of residues in the PDB file"

                Expect.all exposedResidues.Values (fun x -> x >= 0.2 ) 
                    "exposed residues need to be biggert than threshold (e.g. 0.2)"
                Expect.all buriedResidues.Values (fun x -> x >= 0.0 && x <= 0.2) 
                    "buried residues need to have a relative sasa smaller than threshold"

                let referenceExposed = 
                    sasaArrays_rel
                    |> Array.filter (fun x -> x >=0.2)
                    |>Array.length

                let Exposedlength = exposedResidues.Values|>Seq.length
                    
                Expect.isLessThanOrEqual 
                    (Exposedlength - referenceExposed) 1 
                    "Exposed residues should be nearly the same as in reference"

                let referenceBuried = 
                    sasaArrays_rel
                    |> Array.filter (fun x -> x<0.2)
                    |> Array.length

                let buriedLength = buriedResidues.Values|>Seq.length
                
                Expect.isLessThanOrEqual 
                        (buriedLength - referenceBuried) 1 
                        "Buried residues should be nearly the same as in reference"

                let test = exposedResidues.Keys |> Seq.toArray
                let test2 = 
                    python_SASA_array_relativeRes 
                    |> Array.filter (fun (_,value) -> value >=0.2 )
                    |> Array.map ( fun (key,_) -> key)

                Expect.isTrue 
                    (test |> Array.forall (fun x ->  Array.contains x test2))
                    "Exposed residues need to be the same as in reference"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/notexisting.pdb" 
                        1
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                ) "Not existing PDB File lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        3
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                 ) "Not existing Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        -1
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "Negative Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        0 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "0 testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        -2 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                ) "negative testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        100 
                        "xyz" 
                        0.2  
                        false 
                        |> ignore
                ) "unknown probenames lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        100 
                        -2. 
                        0.2  
                        false 
                        |> ignore
                ) "negative proberadius lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1
                        100 
                        "Water" 
                        -0.2  
                        false 
                        |> ignore
                ) "negative cutoff lead to an error"


                // test for a file without missing infos 

                let rel_testdata_cre = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1 
                        100 
                        "Biotin" 
                        false
                    
                let all_residues_cre = 
                    rel_testdata_cre.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                let differentiatedSASA_cre = 
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1 
                        100  
                        "Biotin"
                        0.2 
                        false

                let exposed_cre = 
                    differentiatedSASA_cre
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                    |>dict

                let buried_cre = 
                    differentiatedSASA_cre
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                    |>dict

                let exposedResidues_cre = 
                    exposed_cre.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray
                
                let buriedResidues_cre = 
                    buried_cre.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray

                Expect.equal 
                    (exposedResidues_cre.Length + buriedResidues_cre.Length) 
                    (all_residues_cre.Length) 
                    "The number of exposed and buried residues should be equal 
                    to the number of residues in the PDB file"

                Expect.all exposedResidues_cre (fun x -> x >= 0.2 ) 
                    "exposed residues need to be biggert than threshold (e.g. 0.2)"
                
                Expect.all buriedResidues_cre (fun x -> x >= 0.0 && x <= 0.2) 
                    "buried residues need to have a relative sasa smaller than threshold"

                let referenceExposed_cre = 
                    cresasaArrays_rel
                    |> Array.filter (fun x -> x >=0.2)
                    |> Array.length

                let exposedlength_cre = exposedResidues_cre.Length 
                    
                Expect.isLessThanOrEqual 
                    (exposedlength_cre - referenceExposed_cre) 1 
                    "Exposed residues should be nearly the same as in reference"


                let referenceBuried_cre = 
                    cresasaArrays_rel
                    |> Array.filter (fun x -> x<0.2)
                    |> Array.length

                let buriedLength_cre = buriedResidues_cre |>Seq.length
                
                Expect.isLessThanOrEqual 
                        (buriedLength_cre - referenceBuried_cre) 1 
                        "Buried residues should be nearly the same 
                        as in reference"

                let test_cre = 
                    exposed_cre.Values 
                    |> Seq.collect ( fun x -> x.Keys) 
                    |> Seq.toArray
                
                let test2_cre = 
                    crepython_SASA_array_relativeRes 
                    |> Array.filter (fun (_,value) -> value >=0.2 )
                    |> Array.map ( fun (key,_) -> key)

                Expect.equal test_cre test2_cre 
                    "Exposed residues need to be the same as in reference"
                
                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        3
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                 ) "Not existing Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        -1
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "Negative Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1
                        0 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "0 testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1
                        -2 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                ) "negative testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1
                        100 
                        "xyz" 
                        0.2  
                        false 
                        |> ignore
                ) "unknown probenames lead to an error"
               
                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1
                        100 
                        "Water" 
                        -0.2  
                        false 
                        |> ignore
                ) "negative cutoff lead to an error"

                // test for a file with missing infos with float probe
                
                let rel_testdata_cre_float = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1 
                        100 
                        4. 
                        true
                    
                let all_residues_cre_float = 
                    rel_testdata_cre_float.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                let differentiatedSASA_cre_float = 
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1 
                        100  
                        4. 
                        0.2 
                        true

                let exposed_cre_float = 
                    differentiatedSASA_cre_float
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                    |>dict

                let buried_cre_float = 
                    differentiatedSASA_cre_float
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                    |>dict

                let exposedResidues_cre_float = 
                    exposed_cre_float.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray
                
                let buriedResidues_cre_float = 
                    buried_cre_float.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray

                Expect.equal 
                    (exposedResidues_cre_float.Length + buriedResidues_cre_float.Length) 
                    (all_residues_cre_float.Length) 
                    "The number of exposed and buried residues should be equal 
                    to the number of residues in the PDB file"

                Expect.all exposedResidues_cre_float (fun x -> x >= 0.2 ) 
                    "exposed residues need to be biggert than threshold (e.g. 0.2)"
                
                Expect.all buriedResidues_cre_float (fun x -> x >= 0.0 && x <= 0.2) 
                    "buried residues need to have a relative sasa smaller than threshold"

                let referenceExposed_cre_float = 
                    float_crefixedSASA_rel
                    |> Array.filter (fun (_,x) -> x >=0.2)
                    |> Array.map (fun (_,x) -> x)
                    |> Array.length
                  

                let exposedlength_cre_float = 
                    exposedResidues_cre_float 
                    |> Array.length
                    
                Expect.equal
                     exposedlength_cre_float  referenceExposed_cre_float  
                    "Exposed residues should be nearly the same as in reference"

                let referenceBuried_cre_float = 
                    float_crefixedSASA_rel
                    |> Array.filter (fun (_,x) -> x <0.2)
                    |> Array.map (fun (_,x) -> x)
                    |> Array.length

                let buriedLength_cre_float = buriedResidues_cre_float |>Seq.length
                
                Expect.isLessThanOrEqual 
                        (buriedLength_cre_float - referenceBuried_cre_float) 1 
                        "Buried residues should be nearly the same 
                        as in reference"

                let test_cre_float = 
                    exposed_cre_float.Values 
                    |> Seq.collect ( fun x -> x.Keys) 
                    |> Seq.toArray
                
                let test2_cre_float = 
                    float_crefixedSASA_rel 
                    |> Array.filter (fun (_,value) -> value >=0.2 )
                    |> Array.map ( fun (key,_) -> key)

                Expect.equal test_cre_float test2_cre_float 
                    "Exposed residues need to be the same as in reference"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g001550t11.pdb" 
                        1
                        100 
                        -2. 
                        0.2  
                        false 
                        |> ignore
                    ) "negative proberadius lead to an error"

                    // test for a cre file with multiple chains

                let rel_testdata_creChain = 
                    relativeSASA_aminoacids 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1 
                        100 
                        "Biotin" 
                        false
                    
                let all_residues_creChain = 
                    rel_testdata_creChain.Values
                    |> Seq.collect (fun x -> x.Values)
                    |> Seq.toArray

                let differentiatedSASA_creChain = 
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1 
                        100  
                        "Biotin"
                        0.2 
                        false

                let exposed_creChain = 
                    differentiatedSASA_creChain
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                    |>dict

                let buried_creChain = 
                    differentiatedSASA_creChain
                    |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                    |>dict

                let exposedResidues_creChain = 
                    exposed_creChain.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray
                
                let buriedResidues_creChain = 
                    buried_creChain.Values 
                    |> Seq.collect (fun x -> x.Values) 
                    |> Seq.toArray

                Expect.equal 
                    (exposedResidues_creChain.Length + buriedResidues_creChain.Length) 
                    (all_residues_creChain.Length) 
                    "The number of exposed and buried residues should be equal 
                    to the number of residues in the PDB file"

                Expect.all exposedResidues_creChain (fun x -> x >= 0.2 ) 
                    "exposed residues need to be biggert than threshold (e.g. 0.2)"
                
                Expect.all buriedResidues_creChain (fun x -> x >= 0.0 && x <= 0.2) 
                    "buried residues need to have a relative sasa smaller than threshold"

                let referenceExposed_creChain = 
                    cre2sasaArrays_rel
                    |> Array.filter (fun x -> x >=0.2)
                    |> Array.length

                let exposedlength_creChain = exposedResidues_creChain.Length 
                    
                Expect.equal
                    exposedlength_creChain 
                    referenceExposed_creChain  
                    "Exposed residues should be nearly the same as in reference"


                let referenceBuried_creChain = 
                    cre2sasaArrays_rel
                    |> Array.filter (fun x -> x<0.2)
                    |> Array.length

                let buriedLength_creChain = buriedResidues_creChain |>Seq.length
                
                Expect.equal
                    buriedLength_creChain 
                    referenceBuried_creChain
                        "Buried residues should be nearly the same 
                        as in reference"

                let test_creChain = 
                    exposed_creChain.Values 
                    |> Seq.collect ( fun x -> x.Keys) 
                    |> Seq.toArray
                
                let test2_creChain = 
                    cre2python_SASA_array_relativeRes 
                    |> Array.filter (fun (_,value) -> value >=0.2 )
                    |> Array.map ( fun (key,_) -> key)

                Expect.equal test_creChain test2_creChain 
                    "Exposed residues need to be the same as in reference"
                
                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        3
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                 ) "Not existing Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        -1
                        100 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "Negative Model ID lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1
                        0 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                    ) "0 testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1
                        -2 
                        "Water" 
                        0.2  
                        false 
                        |> ignore
                ) "negative testpoints lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1
                        100 
                        "xyz" 
                        0.2  
                        false 
                        |> ignore
                ) "unknown probenames lead to an error"

                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1
                        100 
                        -2. 
                        0.2  
                        false 
                        |> ignore
                ) "negative proberadius lead to an error"
               
                Expect.throws (fun () ->
                    differentiateAccessibleAA 
                        "resources/pdbParser/Cre01g026150t11.pdb" 
                        1
                        100 
                        "Water" 
                        -0.2  
                        false 
                        |> ignore
                ) "negative cutoff lead to an error"

                                         
            }

            test "chain SASA is computed correct"{
                
                let testdata_chain = 
                    sasaChain "resources/pdbParser/rubisCOActivase.pdb" 1 100 "Water"
                
                let absoluteSASA_rca = 
                    sasaResidue "resources/pdbParser/rubisCOActivase.pdb" 1 100 "Water"

                Seq.iter2 (fun x y -> 
                    Expect.equal x y "The relative SASA chain id should be equal
                    to the absolute SASA chain id"
                ) testdata_chain.Keys absoluteSASA_rca.Keys

                let chainSASA_htq = 
                    sasaChain "resources/pdbParser/htq.pdb" 1 100 "Water"

                Seq.iter (fun x -> 
                      Expect.isGreaterThan x 0.0 
                        "Chain SASA should be greater than 0"
                )chainSASA_htq.Values

                    
                Expect.equal (testdata_chain.Count) sasaArrays_chains.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                Expect.floatClose Accuracy.high 
                    testdata_chain.['A'] sasaArrays_chains.[0] 
                    "The SASA value for the first residue should be equal to 
                    the one from the python reference"
       
                Seq.iter2 (fun fsharpSeries pythonSASA ->
                        Expect.equal pythonSASA fsharpSeries "The Chain identifier should be equal to the one from the python reference"
                ) testdata_chain.Keys chainNumber

                let chainexamples = testdata_chain.Values |> Seq.toArray
               
                Array.iter2 (fun fSharpSASA pythonSASA ->
                    Expect.floatClose Accuracy.high pythonSASA fSharpSASA 
                        $"The SASA value for the atoms should be equal to the 
                        one from the python reference"
                ) chainexamples sasaArrays_chains

                Expect.throws (fun () -> 
                    sasaChain "notexisting.pdb" 1 100 "Water" 
                    |> ignore
                )
                    "Not existing PDB Files lead to an error"

                Expect.throws (fun () -> 
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb"  
                        5
                        100 
                        "Water" 
                        |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb"  
                        -1 100 "Water" 
                        |>ignore
                ) 
                    "negative model id leads to an error"

                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb"  
                        1 
                        0 
                        "Water" 
                        |>ignore
                    )
                    "Zero testpoints lead to an error"

                
                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb"  
                        1 
                        -2 
                        "Water" 
                        |>ignore
                    )
                    "negative testpoints lead to an error"

                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb"  
                        1 
                        100 
                        "aa" 
                        |>ignore)
                    "unknown probes lead to an error"

                Expect.throws (fun () -> 
                    sasaChain 
                        "resources/pdbParser/rubisCOActivase.pdb" 
                        1 
                        100
                        -2 
                        |>ignore)
                    "negative probes lead to an error"


                // test for a file without missing infos 
                
                let chain_cre = 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin"
                let absoluteSASA_cre = 
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "Biotin"

                Seq.iter2 (fun x y -> 
                    Expect.equal x y "The relative SASA chain id should be equal
                    to the absolute SASA chain id"
                ) chain_cre.Keys absoluteSASA_cre.Keys

                
                Seq.iter (fun x -> 
                      Expect.isGreaterThan x 0.0 
                        "Chain SASA should be greater than 0"
                )chain_cre.Values
                                                                         
                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 2 100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if modelid is not present"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" -1 100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if modelid is not present"

                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/Cre01g001550t11.pdb"  
                        1 
                        0 
                        "Water" 
                        |>ignore
                    )
                    "Zero testpoints lead to an error"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 1 -100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if nr of testpoints is not positive"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 1 100 "fun"
                    |>ignore) 
                        "PDB File should throw an error if nr of testpoints is not positive"
                    
                Expect.equal (chain_cre.Count) cre_sasaArrays_chains.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                let allSASA_chains_cre = 
                    chain_cre.Values |> Seq.toArray

                Seq.iter2 (fun fsharpSeries pythonSASA ->
                    Expect.floatClose Accuracy.high  fsharpSeries pythonSASA
                        $"The SASA value should be equal to 
                        the corresponding one from the python reference"
                ) allSASA_chains_cre cre_sasaArrays_chains

                       
                Seq.iter2 (fun fsharpSeries pythonSASA ->
                        Expect.equal pythonSASA fsharpSeries 
                            "The Chain identifier should be equal to the one from 
                            the python reference"
                ) (chain_cre.Keys) cre_chainNumber

                // test for a file with missing infos with float probe

                let float_chain_cre = 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4.
                let float_absoluteSASA_cre = 
                    sasaResidue "resources/pdbParser/Cre01g001550t11.pdb" 1 100 4.

                Seq.iter2 (fun x y -> 
                    Expect.equal x y "The relative SASA chain id should be equal
                    to the absolute SASA chain id"
                ) float_chain_cre.Keys float_absoluteSASA_cre.Keys
                
                Seq.iter (fun x -> 
                      Expect.isGreaterThan x 0.0 
                        "Chain SASA should be greater than 0"
                )float_chain_cre.Values

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g001550t11.pdb" 1 -100 4.
                    |>ignore) 
                        "PDB File should throw an error if nr of testpoints is not positive"
                                
                Expect.equal (float_chain_cre.Count) float_cre_sasaArrays_chains.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                let float_allSASA_chains_cre = 
                    float_chain_cre.Values |> Seq.toArray

                Seq.iter2 (fun fsharpSeries pythonSASA ->
                    Expect.floatClose Accuracy.high  fsharpSeries pythonSASA
                        $"The SASA value should be equal to 
                        the corresponding one from the python reference"
                ) float_allSASA_chains_cre float_cre_sasaArrays_chains
                       
                Seq.iter2 (fun fsharpSeries pythonSASA ->
                        Expect.equal pythonSASA fsharpSeries "The Chain identifier should be equal to the one from the python reference"
                ) (float_chain_cre.Keys) float_cre_chainNumber

                // test for a cre file with multiple chains


                let multiplechain_cre = 
                    sasaChain "resources/pdbParser/Cre01g026150t11.pdb" 1 100 "Biotin"
                
                let absoluteSASA_creMultiple = 
                    sasaResidue "resources/pdbParser/Cre01g026150t11.pdb" 1 100 "Biotin"

                Seq.iter2 (fun x y -> 
                    Expect.equal x y "The relative SASA chain id should be equal
                    to the absolute SASA chain id"
                ) multiplechain_cre.Keys absoluteSASA_creMultiple.Keys
                
                Seq.iter (fun x -> 
                      Expect.isGreaterThan x 0.0 
                        "Chain SASA should be greater than 0"
                )multiplechain_cre.Values
                                                                         
                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g026150t11.pdb" 2 100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if modelid is not present"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g026150t11.pdb" -1 100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if modelid is not present"

                Expect.throws(fun () ->
                    sasaChain 
                        "resources/pdbParser/Cre01g026150t11.pdb"  
                        1 
                        0 
                        "Water" 
                        |>ignore
                    )
                    "Zero testpoints lead to an error"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g026150t11.pdb" 1 -100 "Biotin"
                    |>ignore) 
                        "PDB File should throw an error if nr of testpoints is not positive"

                Expect.throws (fun () -> 
                    sasaChain "resources/pdbParser/Cre01g026150t11.pdb" 1 100 "fun"
                    |>ignore) 
                        "PDB File should throw an error if nr of testpoints is not positive"
                    
                Expect.equal (multiplechain_cre.Count) cre2_sasaArrays_chain.Length 
                    "The number of SASA values should be the same as in the 
                    Python reference"

                let allSASA_chains_creMultiple = 
                    multiplechain_cre.Values |> Seq.toArray

                Seq.iter2 (fun fsharpSeries pythonSASA ->
                    Expect.floatClose Accuracy.high  fsharpSeries pythonSASA
                        $"The SASA value should be equal to 
                        the corresponding one from the python reference"
                ) allSASA_chains_creMultiple cre2_sasaArrays_chain
                       
                Seq.iter2 (fun fsharpSeries pythonSASA ->
                        Expect.equal pythonSASA fsharpSeries 
                            "The Chain identifier should be equal to the one from 
                            the python reference"
                ) (multiplechain_cre.Keys) cre2_chainNumber

            }       
        ]