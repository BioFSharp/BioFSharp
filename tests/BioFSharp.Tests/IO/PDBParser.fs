namespace BioFSharp.Tests.IO

module PDBParserTests =
    open Expecto
    open BioFSharp.IO.PDBParser
    open BioFSharp.Tests.ReferenceObjects.PDBParser
    open BioFSharp.FileFormats.PDBParser

    open System.IO
    
    let IOTests =
        testList "PDBParser" [
            
           test "readPDB file function" {
               let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
               
               let lastElement = Seq.last testdata
               Expect.isTrue (lastElement.StartsWith("END"))  
                "End should be the last element in a PDB File"
              
               let firstElement=Seq.head testdata
               Expect.isTrue (firstElement.StartsWith("HEADER")) 
                "First Element should start with HEADER "

               let containsAtom = Seq.exists (fun (element:string) -> 
                element.StartsWith("ATOM")|| 
                element.StartsWith("HETATM")) testdata
               Expect.isTrue containsAtom "PDB Data should contain ATOM or HETATM"
            
               let lengthOfSequence = Seq.length testdata
               Expect.equal lengthOfSequence 2686  "Sequence length should be 
                the same as number of rows in PDB data" 

               let testdata2 = readPBDFile("resources/pdbParser/rubisCOActivase_false.pdb")
              
               let wrongLastElement = Seq.last testdata2
               Expect.isTrue (not (wrongLastElement.StartsWith("END")))  "End should be the last element in a PDB File"

               let exampleFile_Cre = readPBDFile("resources/pdbParser/Cre01g001550t11.pdb")
               
               let lastElement_cre = 
                Seq.last exampleFile_Cre
               
               Expect.isTrue (lastElement_cre.StartsWith("END"))  
                "End should be the last element in every PDB File"

               let containsAtom_cre = 
                   Seq.exists (fun (element:string) -> 
                    element.StartsWith("ATOM")|| 
                    element.StartsWith("HETATM")) exampleFile_Cre
               
               Expect.isTrue containsAtom_cre "every PDB Data needs at least one  
               ATOM or HETATM line"

               let lengthOfSequence_cre = Seq.length exampleFile_Cre
               Expect.equal lengthOfSequence_cre 6556  "Sequence length should be 
                the same as number of rows in PDB data" 

               let testUnique_cre = exampleFile_Cre |> Seq.distinct |> Seq.length

               Expect.equal testUnique_cre lengthOfSequence_cre 
                    "PDB Files should not contain duplicate lines, 
                    so length of sequence should be equal to number of unique 
                    lines"

               let cre_withMultipleChains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")

               let lastElement_creChains = Seq.last cre_withMultipleChains
               Expect.isTrue (lastElement.StartsWith("END"))  
                "End should be the last element in every PDB File"

               let containsAtom_creChains = Seq.exists (fun (element:string) -> 
                element.StartsWith("ATOM")|| 
                element.StartsWith("HETATM")) cre_withMultipleChains
               
               Expect.isTrue containsAtom_creChains 
                   "every PDB Data needs at least one  
                   ATOM or HETATM line"

               let lengthOfSequence_creChains = Seq.length cre_withMultipleChains

               Expect.equal lengthOfSequence_creChains 22292   
                "Sequence length should be 
                the same as number of rows in PDB data" 

               let testUnique_crechains = 
                    cre_withMultipleChains 
                    |> Seq.distinct 
                    |> Seq.length

               Expect.equal testUnique_crechains lengthOfSequence_creChains
                    "PDB Files should not contain duplicate lines, 
                    so length of sequence should be equal to number of unique 
                    lines"

               Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/invalidPDB.pdb") |> ignore) 
                    "Using unknown PDB pathes should lead to an error."


           }

           test "read Metadata with parts of PDB File and real World examples" {
                
                // part tests
                let metadataLines = readMetadata pdbLines 

                Expect.equal metadataLines.Header "RubisCO Activase" 
                    "The HEADER field should be parsed correctly"
                Expect.equal metadataLines.Title 
                    "Testparsing" "The TITLE field should be parsed correctly"
                Expect.equal metadataLines.Compound (Some 
                    ["MOLECULE: RIBULOSE BISPHOSPHATE CARBOXYLASE/OXYGENASE ACTIVASE"]) 
                    "The COMPND field should be parsed correctly"
                Expect.equal metadataLines.Source (Some ["Arabidopsis"]) 
                    "The SOURCE field should be parsed correctly"
                Expect.equal metadataLines.Keywords (Some ["RubisCO Activase"; "Test"]) 
                    "The KEYWDS field should be parsed correctly"
                Expect.equal metadataLines.ExpData (Some ["X-RAY DIFFRACTION"]) 
                    "The EXPDTA field should be parsed correctly"
                Expect.equal metadataLines.Authors (Some ["John Doe"]) 
                    "The AUTHOR field should be parsed correctly"
                Expect.equal metadataLines.Remarks 
                    ["RESOLUTION.    2.90 ANGSTROMS."; "Example remark 2"] 
                    "The REMARK field should be parsed correctly"
                Expect.equal metadataLines.Caveats (Some ["Incorrect residue numbering"]) 
                    "The CAVEAT field should be parsed correctly"
                        
                // real world tests     
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let metadatarealWorld = readMetadata testdata

                Expect.equal metadatarealWorld.Header 
                    "CHAPERONE                               19-AUG-14   4W5W" 
                    "The HEADER field should be parsed correctly for the real
                        PDB file"
                Expect.equal metadatarealWorld.Title 
                    "RUBISCO ACTIVASE FROM ARABIDOPSIS THALIANA" 
                    "The TITLE field should be parsed correctly for the real 
                        PDB file"
                Expect.isSome metadatarealWorld.Compound 
                    "The COMPND field should not be empty in the real PDB file"
                Expect.isSome metadatarealWorld.Source 
                    "The SOURCE field should not be empty in the real PDB file"
                Expect.isSome metadatarealWorld.Keywords 
                    "The KEYWDS field should not be empty in the real PDB file"
                Expect.isSome metadatarealWorld.ExpData 
                    "The EXPDTA field should not be empty in the real PDB file"
                Expect.isSome metadatarealWorld.Authors 
                    "The AUTHOR field should not be empty in the real PDB file"
                Expect.isTrue (metadatarealWorld.Remarks.Length > 0) 
                    "The REMARK field should not be empty in the real PDB file"
                Expect.isNone (metadatarealWorld.Caveats) 
                    "PDB Files without Caveats warnings should have value None" 

                let testdata2 = readPBDFile("resources/pdbParser/rubisCOActivase_false.pdb")
                let metadatarealworld2 = readMetadata testdata2
              
                Expect.isNone metadatarealworld2.Source 
                    "The SOURCE field should be empty when no source is included 
                    in the real PDB file"

                let oxidoreductase = readPBDFile "resources/pdbParser/oxidoreductase.pdb"
                let metadata_oxidoreductase = readMetadata oxidoreductase
                Expect.isSome metadata_oxidoreductase.Compound 
                    "The COMPD field should not be empty in the real PDB file"
                Expect.equal metadata_oxidoreductase.ExpData 
                    (Some ["X-RAY DIFFRACTION"]) "The EXPDTA field should be parsed correctly"

                // Test File with no Metadata
                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readMetadata

                Expect.equal exampleFile_Cre.Header ""
                    "PDB Files without Metadata should have Header an empty Header"
                Expect.equal exampleFile_Cre.Title ""
                    "PDB Files without Metadata should have Title an empty Title"

                Expect.isNone exampleFile_Cre.Compound 
                    "The COMPND field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre.Source 
                    "The SOURCE field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre.Keywords 
                    "The KEYWDS field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre.ExpData 
                    "The EXPDTA field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre.Authors 
                    "The AUTHOR field should be none when no metadata is given"
                Expect.isFalse (exampleFile_Cre.Remarks.Length > 0) 
                    "The REMARK field should be 0 when no metradata is given"
                Expect.isNone (exampleFile_Cre.Caveats) 
                    "PDB Files without Caveats warnings should have value None" 

                // Test File with no Metadata and multiple chains

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readMetadata
               
                Expect.equal exampleFile_Cre_chains.Header ""
                    "PDB Files without Metadata should have Header an empty Header"
                Expect.equal exampleFile_Cre_chains.Title ""
                    "PDB Files without Metadata should have Title an empty Title"

                Expect.isNone exampleFile_Cre_chains.Compound 
                    "The COMPND field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre_chains.Source 
                    "The SOURCE field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre_chains.Keywords 
                    "The KEYWDS field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre_chains.ExpData 
                    "The EXPDTA field should be none when no metadata is given"
                Expect.isNone exampleFile_Cre_chains.Authors 
                    "The AUTHOR field should be none when no metadata is given"
                Expect.isFalse (exampleFile_Cre_chains.Remarks.Length > 0) 
                    "The REMARK field should be 0 when no metradata is given"
                Expect.isNone (exampleFile_Cre_chains.Caveats) 
                    "PDB Files without Caveats warnings should have value None"

                Expect.throws (fun () -> 
                    readPBDFile ("notexisting.pdb")
                    |>readMetadata  |> ignore) 
                    "Using an not existing PDB File should throw an error"
           }

           test "filtering of ATOM and HETATM lines is succesfull"{
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")

                let filterAtomLines (lines: seq<string>) =
                            lines
                            |> Seq.filter (fun line -> 
                                line.StartsWith("ATOM  ") || 
                                line.StartsWith("HETATM")
                            )

                let filteredatoms = filterAtomLines testdata
                Expect.equal (Seq.length filteredatoms) 2169 
                    "Length of atmos should be the same as number of lines 
                        in PDB File"

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> filterAtomLines

                Expect.equal (Seq.length exampleFile_Cre) 6554
                    "Length of atmos should be the same as number of lines 
                        in PDB File" 
                        
                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> filterAtomLines

                Expect.equal (Seq.length exampleFile_Cre_chains) 22289
                    "Length of atmos should be the same as number of lines 
                        in PDB File" 

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> filterAtomLines |> ignore) 
                    "Filtering invalid PDB content should throw an error. 
                    Atom Lines and Link Lines need to be in correct format"
           }

           test "readAtom function works in parts and realWorld examples" {

                // part tests
                let atomLines = readAtom testPDBAtomlines

                Expect.equal (Array.length atomLines) 4 
                    "Length of atmos should be the same as number of lines in PDB File"
                Expect.equal (atomLines.[0].SerialNumber) 2152 
                    "Serialnumber should be the same as in PDB File"
                Expect.equal (atomLines.[0].Coordinates.Y) 0.898 
                    " Y Coordinates should be parsed correctly"
                Expect.isNone (atomLines.[0].SegId) 
                    "ATOM / HETATM lines without an segId, should be parsed with None"
                Expect.equal (atomLines.[1].AtomName)"O" 
                    "Atomname should be the same as in the PDB File"
                Expect.equal (atomLines.[1].Coordinates.Z) 7.355 
                    "Z Coordinates should be parsed correctly"
                Expect.equal (atomLines.[1].Element) "O" 
                    "Element should be the same as in PDB file"
                Expect.equal (atomLines.[2].AltLoc) ' ' 
                    "ATOM / HETATM lines without an alternative location, should be empty"
                Expect.equal (atomLines.[2].Occupancy) 1.00 
                    "Occupancy should be the same as in PDB file"
                Expect.isNone (atomLines.[2].Charge) 
                    "ATOM / HETATM lines without charge, should be parsed with None"
                Expect.equal (atomLines.[3].Coordinates.X) 10.325 
                    "X Coordinates should be parsed correctly"
                Expect.equal (atomLines.[3].TempFactor) 97.83 
                    "Temp. Fctor should be the same as in PDB File"
                Expect.isTrue (atomLines.[3].Hetatm) 
                    "HETATM lines in a PDB file should have the value TRUE"

                // real world tests

                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedatoms = readAtom testdata

                Expect.equal (Array.length parsedatoms) 2169 
                    "Length of atmos should be the same as number of lines in PDB File"
                Expect.equal (parsedatoms.[0].SerialNumber) 1 
                    "the serialnumber of the first atom should be always 1" 
                Expect.isNotEmpty (parsedatoms.[2].AtomName) 
                    "Atomnames should always be present in a PDB file"
                Expect.isFalse (parsedatoms.[0].Hetatm) 
                    "ATOM Lines should be parsed as true"

                let oxidoreductase = readPBDFile "resources/pdbParser/oxidoreductase.pdb"
                let atoms_oxidoreductase = readAtom oxidoreductase

                Expect.equal (Array.length atoms_oxidoreductase) 2607
                    "Length of atmos should be the same as number of lines in PDB File"
                Expect.equal (atoms_oxidoreductase.[0].SerialNumber) 1 
                    "the serialnumber of the first atom should be always 1" 
                Expect.isTrue ((Array.last atoms_oxidoreductase).Hetatm) 
                    "HETATM Lines should be parsed as true"

                let testdata2 = readPBDFile("resources/pdbParser/rubisCOActivase_false.pdb")

                let parsewrong = readAtom testdata2

                Expect.equal (Array.length parsewrong) 2169 
                    "The number of parsed atoms should be correct even with errors"
                Expect.equal (parsewrong.[0].AtomName)"N" 
                    "Atomname should be the same as in the PDB File even with errors"
                Expect.equal (parsewrong.[0].SerialNumber) 1 
                    "the serialnumber of the first atom should be always 1 even with errors" 


                let exampleFile_Cre_read =  readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 

                let filtered_Cre  = 
                    exampleFile_Cre_read
                    |> Seq.filter (fun line -> 
                        line.StartsWith("ATOM  ") || 
                        line.StartsWith("HETATM")
                    )
                    |> Seq.toArray

                let exampleFile_Cre = 
                    exampleFile_Cre_read
                    |> readAtom 

                                           
                Expect.equal (Array.length exampleFile_Cre) filtered_Cre.Length
                    "Length of atmos should be the same as number of lines in 
                    PDB File"
                Expect.equal (exampleFile_Cre.[0].SerialNumber) 1
                    "Serialnumber should be the same as in PDB File"
                Expect.equal (exampleFile_Cre.[0].Coordinates.X) 94.622
                    "X Coordinates should be parsed correctly"
                Expect.equal (exampleFile_Cre.[0].Coordinates.Y) -23.580
                    " Y Coordinates should be parsed correctly"
                Expect.equal (exampleFile_Cre.[0].Coordinates.Z) 26.178 
                    "Z Coordinates should be parsed correctly"
                Expect.isNone (exampleFile_Cre.[0].SegId) 
                    "ATOM / HETATM lines without an segId, should be parsed 
                    with None"
                Expect.equal (exampleFile_Cre.[2].AtomName)"H2" 
                    "Atomname should be the same as in the PDB File"
                Expect.equal (exampleFile_Cre.[1].Element) "H" 
                    "Element should be the same as in PDB file"
                Expect.equal (exampleFile_Cre.[0].AltLoc) ' ' 
                    "ATOM / HETATM lines without an alternative location, 
                    should be empty"
                Expect.equal (exampleFile_Cre.[2].Occupancy) 1.00 
                    "Occupancy should be 1 when no AltLoc is there"
                Expect.isNone (exampleFile_Cre.[0].Charge) 
                    "ATOM / HETATM lines without charge, should be parsed 
                    with None"
                Expect.equal (exampleFile_Cre.[3].TempFactor) 20.45
                    "Temp. Fctor should be the same as in PDB File"
                Expect.isFalse exampleFile_Cre.[124].Hetatm
                    "ATOM lines in a PDB file should have False as HETATM File"

                // cre example with multiple chains

                let exampleFile_Cre_read_chain =  
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")

                let filtered_Cre_chain  = 
                    exampleFile_Cre_read_chain
                    |> Seq.filter (fun line -> 
                        line.StartsWith("ATOM  ") || 
                        line.StartsWith("HETATM")
                    )
                    |> Seq.toArray

                let exampleFile_Cre_chain = 
                    exampleFile_Cre_read_chain
                    |> readAtom 
                                          
                Expect.equal (Array.length exampleFile_Cre_chain) filtered_Cre_chain.Length
                    "Length of atmos should be the same as number of lines in 
                    PDB File"
                Expect.equal (exampleFile_Cre_chain.[0].SerialNumber) 1
                    "Serialnumber should be the same as in PDB File"
                Expect.equal (exampleFile_Cre_chain.[0].Coordinates.X) -14.968
                    "X Coordinates should be parsed correctly"
                Expect.equal (exampleFile_Cre_chain.[0].Coordinates.Y) -49.282
                    " Y Coordinates should be parsed correctly"
                Expect.equal (exampleFile_Cre_chain.[0].Coordinates.Z) -70.902
                    "Z Coordinates should be parsed correctly"
                Expect.isNone (exampleFile_Cre_chain.[0].SegId) 
                    "ATOM / HETATM lines without an segId, should be parsed 
                    with None"
                Expect.equal (exampleFile_Cre_chain.[2].AtomName)"H2" 
                    "Atomname should be the same as in the PDB File"
                Expect.equal (exampleFile_Cre_chain.[1].Element) "H" 
                    "Element should be the same as in PDB file"
                Expect.equal (exampleFile_Cre_chain.[0].AltLoc) ' ' 
                    "ATOM / HETATM lines without an alternative location, 
                    should be empty"
                Expect.equal (exampleFile_Cre_chain.[2].Occupancy) 1.00 
                    "Occupancy should be 1 when no AltLoc is there"
                Expect.isNone (exampleFile_Cre_chain.[0].Charge) 
                    "ATOM / HETATM lines without charge, should be parsed 
                    with None"
                Expect.equal (exampleFile_Cre_chain.[3].TempFactor) 16.12
                    "Temp. Fctor should be the same as in PDB File"
                Expect.isFalse exampleFile_Cre_chain.[124].Hetatm
                    "ATOM lines in a PDB file should have False as HETATM File"

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readAtom |> ignore) 
                    "Using an not existing PDB File should throw an error"             
           }

           test "readSecstructure function works in parts and realWorld examples" {

                // part tests
                let secStructurelines = readSecstructure pdbLines_secStructure

                Expect.equal (Array.length secStructurelines) 4 
                        "The number of parsed secondary structures should be 
                        equal to number of lines in PDB file containing SS Types"
                Expect.equal (secStructurelines.[0].SecstructureType) "Helix" 
                    "HELIX member should be parsed as Helix"
                Expect.equal (secStructurelines.[1].Startresidue) 331 
                    "Startresidue should be equal to resiude in PDB file"
                Expect.equal (secStructurelines.[2].SecstructureType) "Sheet" 
                    "SHEET member should be parsed as Sheet"
                Expect.equal (secStructurelines.[3].Endresidue) 169 
                    "Endresidue should be equal to resiude in PDB file"

                // real world tests
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedrealworld = readSecstructure testdata

                let validresidue residue = residue.Startresidue > 0
                let validresidueEnd residue = residue.Endresidue > 0

                Expect.equal (Array.length parsedrealworld) 17 
                    "The number of parsed secondary structures should be 
                    equal to number of lines in PDB file containing SS Types"
                Expect.equal (parsedrealworld.[0].SecstructureType) "Helix" 
                    "HELIX member should be parsed as Helix"
                Expect.equal ((Array.last parsedrealworld).SecstructureType) "Sheet" 
                    "SHEET member should be parsed as Sheet"
                Expect.all  parsedrealworld validresidue 
                    "Startresidues have to be present and bigger 0 in
                      Helix or Sheet Lines in PDB Files"
                Expect.all  parsedrealworld validresidueEnd 
                    "Endresidues have to be present and bigger 0 in 
                        Helix or Sheet Lines in PDB Files" 

                let oxidoreductase = readPBDFile "resources/pdbParser/oxidoreductase.pdb"
                let oxidoreductase_secStruc = readSecstructure oxidoreductase
                Expect.all  oxidoreductase_secStruc validresidue 
                    "Startresidues have to be present and bigger 0 in 
                    Helix or Sheet Lines in PDB Files"
                Expect.all  oxidoreductase_secStruc validresidueEnd 
                    "Endresidues have to be present and bigger 0 in Helix 
                    or Sheet Lines in PDB Files" 

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readSecstructure

                Expect.isEmpty exampleFile_Cre 
                    "PDB Files without secondary structure informations should 
                    be empty"

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")
                    |> readSecstructure

                Expect.isEmpty exampleFile_Cre 
                    "PDB Files without secondary structure informations should 
                    be empty"

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readSecstructure |> ignore) 
                    "Using an not existing PDB File should throw an error"
           }

           test "readModification function works in parts and realWorld examples" {

                // part tests
                let modificationLines = readModifications pdbLines_modification
                    
                Expect.equal (Array.length modificationLines) 3 
                    "The number of modifications should be equal to the 
                        number of MODRES lines"
                Expect.equal modificationLines.[0].ModifiedResidueNumber 65 
                    "The modified residue number should be parsed correctly"
                Expect.equal modificationLines.[1].ModificationType 
                    "ACETYLATED LYSINE" 
                    "The modification type should be parsed correctly"
                Expect.equal modificationLines.[2].ModifiedResidueNumber 120 
                    "The modified residue number should be parsed correctly"
                Expect.equal modificationLines.[2].ModificationType 
                    "METHYLSELENOCYSTEINE" 
                    "The modification type should be parsed correctly"

                // real world tests
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedTestdata = readModifications testdata

                Expect.isEmpty parsedTestdata 
                    "PDB Files without modification should be empty"

                let testdataWithModres = 
                    readPBDFile "resources/pdbParser/modifiedOxidoreductase.pdb"
                let parsedTestdataWithModres = 
                    readModifications testdataWithModres

                Expect.isGreaterThan (Array.length parsedTestdataWithModres) 0 
                    "PDB Files with a MODRES Entry should have Length greater 0"
                Expect.equal (parsedTestdataWithModres.[0].ModificationType) 
                    "N-TRIMETHYLLYSINE" 
                    "Message should be parsed correctly"

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readModifications 

                Expect.isEmpty exampleFile_Cre "PDB Files without modifications
                    informations should be empty"

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readModifications

                Expect.isEmpty exampleFile_Cre_chains 
                    "PDB Files without modifications informations should be empty"

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readModifications |> ignore) 
                    "Using an not existing PDB File should throw an error"
           }

           test "readResidue function works in parts and realWorld examples" {
           
                // part tests
                let residuesLines = readResidues pdbLines_residue

                Expect.equal residuesLines.Length 5 
                    "The Number of Residues should be equal to number of 
                    different Residue numbers"

                Expect.equal residuesLines.[0].ResidueName "ASN" 
                    "Residuename should be the same as in PDB File "
                Expect.equal residuesLines.[1].ResidueName "MET" 
                    "Residuename should be the same as in PDB File "

                Expect.equal residuesLines.[1].ResidueNumber 69 
                    "Residuenumber should be grouped correctly 
                    and the same as in PDB File"
                Expect.equal residuesLines.[4].ResidueNumber 401 
                    "Residuenumber should be grouped correctly 
                    and the same as in PDB File"

                Expect.isNone residuesLines.[2].InsertionCode 
                    "Residues without an Insertioncode should have the Value none"
                Expect.isSome residuesLines.[4].InsertionCode 
                    "Residues with an Insertioncode should be Some"

                Expect.isNone residuesLines.[0].SecStructureType 
                    "Residues that are not part of Helix or Sheet 
                    should have the value none"
                Expect.equal  residuesLines.[1].SecStructureType (Some"Sheet") 
                    "Residues part of a Sheet should have value Sheet"
                Expect.equal  residuesLines.[3].SecStructureType (Some"Helix") 
                    "Residues part of a Helix should have value Helix"

                Expect.isNone residuesLines.[0].Modification 
                    "Residues without Modifications should have at this 
                    field value None"
                Expect.isSome residuesLines.[1].Modification 
                    "Residues with Modifications should have value Some"
                Expect.equal residuesLines.[1].Modification 
                    (Some"SELENOMETHIONINE") 
                    "Modifications of Residues should be parsed correctly"

                Expect.equal  (Array.length (residuesLines.[3].Atoms)) 2 
                    "Number of atoms should be equal to number of atoms with 
                    equal residuenumber "

                // real world tests

                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedTestdata = readResidues testdata

                Expect.isGreaterThan parsedTestdata.Length 0 
                    "PDB Files should contain at least one Residual Group"
                Expect.exists parsedTestdata 
                    (fun residue -> residue.ResidueName = "MET") 
                    "Evervy PDB File should contain at least one MET residue"
                Expect.exists parsedTestdata 
                    (fun residue -> residue.ResidueNumber = 69) 
                    "PDB File testdata should contain one residue with 69 "
                Expect.all parsedTestdata 
                    (fun residue -> residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                let oxidoreductase = readPBDFile "resources/pdbParser/oxidoreductase.pdb"
                let parsedOxidoreductase = readResidues oxidoreductase

                Expect.exists parsedOxidoreductase
                    ( fun residue -> residue.SecStructureType.IsSome) 
                    "In Real PDB Files some Residues should be part of a sec struc"

                let testdataWithModres = 
                    readPBDFile "resources/pdbParser/modifiedOxidoreductase.pdb"
                let parsedTestdataWithModres = 
                    readResidues testdataWithModres

                Expect.exists parsedTestdataWithModres 
                    ( fun residue -> residue.Modification.IsSome) 
                    "IN Real PDB Files WITH Modifications some Residues 
                    should have a Modification description"

                // cre example
                let creExample = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readResidues 

                let residue_amount_cre = 
                    creExample 
                    |> Array.last 
                    |> fun residue -> residue.ResidueNumber

                Expect.isGreaterThan  creExample.Length 0
                    "PDB Files with Residues should have a length greater than 0"

                Expect.equal creExample.Length residue_amount_cre 
                    "The Number of Residues should be equal to number of 
                    different Residue numbers"

                Expect.equal creExample.[0].ResidueName "MET" 
                    "First Residuename should be MET in PDB File "
                Expect.equal creExample.[1].ResidueName "LEU" 
                    "Residuename should be the same as in PDB File "

                Expect.equal creExample.[0].ResidueNumber 1
                    "complete PDB File without missing data should have 
                    Residuenumber 1 a"

                Expect.equal creExample.[creExample.Length-1].ResidueNumber residue_amount_cre
                    "Residuenumber should be grouped correctly 
                    and the same as in PDB File"

                Expect.isNone creExample.[0].InsertionCode 
                    "Residues without an Insertioncode should have the Value 
                    none"
   
                Expect.isNone creExample.[0].SecStructureType 
                    "Residues that have no secondary structure information should 
                    be none"
              
                Expect.isNone creExample.[0].Modification 
                    "Residues without Modifications should have at this 
                    field value None"
              
                Expect.equal  (Array.length (creExample.[0].Atoms)) 19 
                    "Number of atoms should be equal to number of atoms with 
                    equal residuenumber "

                // cre example with multiple chains 
                
                let creExample_chains = 
                   readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")
                    |> readResidues 

                let residue_amount_creChains = 

                    let residuenumber = 
                            creExample_chains
                            |> Array.map (fun r -> r.ResidueNumber)
                    

                    let folder (acc, current) elem =

                        if elem = 1 then
                            let acc = if current <> [] then (List.rev current)::acc else acc
                            (acc, [elem])
                        else
                            (acc, elem::current)

                    let acc, last = Array.fold folder ([], [])residuenumber

                    let allChunks = 
                        if last <> [] then 
                            List.rev ((List.rev last)::acc) 
                        else 
                            List.rev acc

                    let arrays = allChunks |> List.map List.toArray
                    let summedlength= arrays |> List.sumBy (fun a -> a.Length)

                    let lastitem = arrays |> List.last |> Array.last

                    (lastitem, summedlength)

                Expect.isGreaterThan  creExample_chains.Length 0
                    "PDB Files with Residues should have a length greater than 0"

                Expect.equal creExample_chains.Length (residue_amount_creChains |> snd) 
                    "The Number of Residues should be equal to number of 
                    different Residue numbers"

                Expect.equal creExample_chains.[0].ResidueName "MET" 
                    "First Residuename should be MET in PDB File "
                
                Expect.equal creExample_chains.[1].ResidueName "VAL" 
                    "Residuename should be the same as in PDB File "

                Expect.equal 
                    (creExample_chains 
                    |> Array.head 
                    |> fun x -> x.ResidueNumber)
                    1
                    "complete PDB File without missing data should have 
                    Residuenumber 1 at start"

                Expect.equal creExample_chains.[creExample_chains.Length-1].ResidueNumber (residue_amount_creChains|> fst)
                    "Residuenumber should be grouped correctly 
                    and the same as in PDB File"

                Expect.isNone creExample_chains.[0].InsertionCode 
                    "Residues without an Insertioncode should have the Value 
                    none"
   
                Expect.isNone creExample_chains.[0].SecStructureType 
                    "Residues that have no secondary structure information should 
                    be none"
              
                Expect.isNone creExample_chains.[0].Modification 
                    "Residues without Modifications should have at this 
                    field value None"

                let firstResidue_atoms = 
                    creExample_chains 
                    |> Array.head 
                    |> fun x -> x.Atoms
              
                Expect.equal  firstResidue_atoms.Length 19 
                    "Number of atoms should be equal to number of atoms with 
                    equal residuenumber "

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readResidues |> ignore) 
                    "Using an not existing PDB File should throw an error"

           }

           test "readChain function works in parts and realWorld examples" {
                
                // part tests
                let chainLines = readChain pdbLines_chains
                
                Expect.equal chainLines.Length 2 
                    "Length of Chain List must be equal to number of 
                    different chain Ids in a PDB File"
                Expect.equal (Array.length (chainLines.[0].Residues)) 3 
                    "Length of SubArray must be equal to number of Residues
                    with same chainid"
                Expect.equal chainLines.[0].ChainId 'A' 
                    "First Chain Id need to be A"
                Expect.equal chainLines.[1].ChainId 'B' 
                    "2nd Chain Id need to be B"

                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedMonomerfile = readChain testdata
                let parsedresidue = readResidues testdata

                Expect.equal parsedMonomerfile.Length 1 
                    "Monomer need do be only have one chain id"
                Expect.equal parsedMonomerfile.[0].ChainId 'A' 
                    "A needs to be the ChainId of all Residues"
                Expect.equal (Array.length (parsedMonomerfile.[0].Residues))
                    parsedresidue.Length 
                    "Number of Residues must be in 
                    monomer equal to number of parsed residues"

                let hemoglobin = readPBDFile "resources/pdbParser/hemoglobin.pdb"
                let parsedhemoglobin = readChain hemoglobin

                Expect.equal parsedhemoglobin.Length 4 
                    "Hemoglobin should have as a Polypeptide 4 chains"
                Expect.exists parsedhemoglobin (fun chain -> 
                    chain.ChainId = 'A') 
                    "Polymeres must have at leats one Residue with 
                    Chain Identifier A"
                Expect.exists parsedhemoglobin (fun chain -> 
                    chain.ChainId = 'D') 
                    "Hemoglobin that has 4 chains  must have at leats 
                        one Residue with Chain Identifier D"

                let creExample = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readChain 

                Expect.isGreaterThan creExample.Length 0 
                    "PDB Files with Chains should have a length greater than 0"

                let uniqueChainIds_cre = 
                    creExample 
                    |> Array.map (fun chain -> chain.ChainId)
                    |> Array.distinct 
                  
                Expect.equal creExample.Length uniqueChainIds_cre.Length
                    "The Number of Chains should be equal to number of 
                    different Chain Ids"

                // cre example with multiple chains

                let creExample_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")
                    |> readChain 

                Expect.isGreaterThan creExample_chains.Length 0 
                    "PDB Files with Chains should have a length greater than 0"

                let uniqueChainIds_chains = 
                    creExample_chains 
                    |> Array.map (fun chain -> chain.ChainId)
                    |> Array.distinct 
                  
                Expect.equal creExample_chains.Length uniqueChainIds_chains.Length
                    "The Number of Chains should be equal to number of 
                    different Chain Ids"

                Expect.equal (creExample_chains.[0].ChainId) 'A' 
                    "First Chain Id should be A in PDB File "

                Expect.equal (creExample_chains.[1].ChainId) 'B' 
                    "2nd Chain Id should be B in PDB File "

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readChain |> ignore) 
                    "Using an not existing PDB File should throw an error"
               
           }

           test "readLinkages function works in parts and realWorld examples" {
                
                // part tests
                let linkageLines = readLinkages pdbLines_linkages

                Expect.equal (Array.length linkageLines) 6 
                    "Number of parsed Linkages should be the sum of
                    SSBOND, LINK, CISPEP and CONECT link lines"

                Expect.exists linkageLines (fun l -> l.Linktype = "SS Bond") 
                    "linkageLines contains one SS Bond"
                Expect.exists linkageLines 
                    (fun l -> l.Linktype = "covalent Link") 
                    "linkageLines contains one covalent Link"
                Expect.exists linkageLines (fun l -> l.Linktype = "Cis Peptide") 
                    "linkageLines contains one Cis Peptide Link"
                Expect.exists linkageLines (fun l -> l.Linktype = "Connect") 
                    "linkageLines contains at least one  Connect Link between 
                        non standard ligands or covalent links of non protein complexes"

                let ssbondLinkage = 
                    linkageLines 
                    |> Array.find (fun l -> l.Linktype = "SS Bond")

                Expect.equal (Array.length ssbondLinkage.Atoms1) 3 
                    "Number of Atoms part of Residue described in SS Bond should 
                    be equal to number of Atoms part of the same Residuum 1"
                Expect.equal (Array.length ssbondLinkage.Atoms2) 1 
                    "Number of Atoms part of Residue described in SS Bond should 
                    be equal to number of Atoms part of the same Residuum 2"

                let linkLinkage = 
                    linkageLines 
                    |> Array.find (fun l -> l.Linktype = "covalent Link")
                Expect.exists (linkLinkage.Atoms1) 
                    (fun atom-> atom.SerialNumber = 4505) 
                    "Atom should be part of LINK in the first Linkage"
                Expect.isTrue 
                    (linkLinkage.Atoms2 
                    |> Array.exists (fun a -> a.SerialNumber = 4506)) 
                    "covalent Link should contain Atom 4506 in Atoms2"

                let cispepLinkage = 
                    linkageLines 
                    |> Array.find (fun l -> l.Linktype = "Cis Peptide")

                Expect.exists (cispepLinkage.Atoms1) 
                    (fun atom-> atom.AtomName = "CA") 
                    "Atom with Atomname should be part of Residue 1 in "

                let conectLinkages = 
                    linkageLines 
                    |> Array.filter (fun l -> l.Linktype = "Connect")

                Expect.isGreaterThan conectLinkages.Length 1 
                    "There should be more than one CONNECT parsed"
                Expect.all conectLinkages 
                    (fun linkage -> 
                        linkage.Atoms1.Length = 1 
                        && linkage.Atoms2.Length = 1) 
                        "Connect linkages contain only two atoms connected to 
                        each other"

                Expect.throws (fun () -> 
                    readLinkages invalidPDBContent |> ignore) 
                    "Parsing invalid PDB content should throw an error. 
                    Atom Lines and Link Lines need to be in correct format"  


                // real world tests
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedTestdata = readLinkages testdata

                Expect.isGreaterThan (Array.length parsedTestdata) 0 
                    "Number of Linkages should be greater than 1"
                Expect.all parsedTestdata (fun l -> 
                    l.Linktype = "Cis Peptide" || l.Linktype = "Connect") 
                    "PDB Files that contains only CISPEP and CONNECT should 
                    have no other Linktype"

                let hemoglobin = readPBDFile "resources/pdbParser/hemoglobin.pdb"
                let parsedhemoglobin = readLinkages hemoglobin

                Expect.isGreaterThan (Array.length parsedhemoglobin) 0 
                    "Number of Linkages should be greater than 0"
                Expect.all parsedhemoglobin (fun l -> 
                    l.Linktype <> "Cis Peptide") 
                    "PDB Files that contains NO  CISPEP  
                    should have only other Linktype"

                // example withoutLinkages 

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readLinkages 

                Expect.isEmpty exampleFile_Cre 
                    "PDB Files without Linkages should be empty"

                // cre example with multiple chains but no linkages info

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readLinkages

                Expect.isEmpty exampleFile_Cre_chains 
                    "PDB Files without Linkages should be empty at that point"            
           }

           test "readSites function works in parts and realWorld examples" {

                // part tests
                let siteLines = readSite pdbLines_sites

                Expect.equal (Array.length siteLines) 2 
                    "Number of parsed Sites should be equal to the different 
                    sites present in the protein"
                
                let siteAC1 = 
                    siteLines 
                    |> Array.find (fun site -> site.Sitename = "AC1")

                Expect.equal (Array.length siteAC1.Residues) 5 
                    "Site AC1 should have 5 residues"
                Expect.exists siteAC1.Residues (fun (chainId, residueNum) -> 
                    chainId = 'A' && residueNum = 110) 
                    "Site Residues including Residuenumber and chain id should 
                    be assigned to the correct site"
                Expect.exists siteAC1.Residues (fun (chainId, residueNum) -> 
                    chainId = 'B' && residueNum = 114) 
                    "Site Residues including Residuenumber and chain id should 
                    be assigned to the correct site"

                let numRes =  
                    Seq.item 2 pdbLines_sites 
                    |> fun line -> int (line.Substring(15, 2).Trim())

                Expect.exists siteLines ( fun site -> site.Sitename ="AC2") 
                    "Sitenames should be parsed correctly"
                Expect.equal (Array.length siteLines.[1].Residues) numRes "
                    Number of Residues per SITE should be equal to numRES in 
                    PDBentry"

                // real world tests

                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedTestdata = readSite testdata

                let numberOfSites =
                    testdata
                    |> Seq.filter ( fun line -> line.StartsWith("SITE"))
                    |> Seq.groupBy (fun line -> line.Substring(11, 3).Trim())
                    |> Seq.length

                Expect.isNonEmpty parsedTestdata 
                    "Real World  PDB Files with SITES describing e.g. 
                    active centre or activating co factor need to be parsed 
                    correctly "
                Expect.equal (Array.length parsedTestdata) numberOfSites 
                    "Number of Sites should be equal than SITES to distinct in 
                    PDB Data"

                let testdata_false =  
                    readPBDFile("resources/pdbParser/rubisCOActivase_false.pdb")

                Expect.throws (fun () -> readSite testdata_false|> ignore) 
                    "Mistakes in Site data should lead to an error"  

                    
                // example without Sites 

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readSite 

                Expect.isEmpty exampleFile_Cre 
                    "PDB Files without Sites informations should be empty"

                // cre example with multiple chains but no sites info

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readSite

                Expect.isEmpty exampleFile_Cre_chains 
                    "PDB Files without Sites informations should be empty at that point"

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readSite |> ignore) 
                    "Using an not existing PDB File should throw an error"
           }

           test "readModel function works in parts and realWorld examples" {

                // part tests

                let modelLines = readModels pdbLines_model

                Expect.equal (Array.length modelLines) 2 "Number of Modell blocks should be equal to the last modell id in PDB File"
                Expect.equal modelLines.[0].ModelId 1 "first Modellid should be 1"
                Expect.equal (Array.length (modelLines.[0].Chains)) 2 "Number of chains shoudl be equal to  different chains in the Modell Block" 
                Expect.exists (modelLines.[0].Chains) (fun chain -> chain.ChainId = 'A' ) "parsed Chains must be contain a chain with ID A"

                Expect.isGreaterThan (Array.length modelLines.[0].Linkages) 0 "Linkage Array should be bigger than 0"
                Expect.exists (modelLines.[0].Linkages) (fun l -> l.Linktype = "SS Bond") "The first Modell Block contains at least one SS BOND"

                Expect.equal(Array.length modelLines.[0].Sites) 2 "In the first Modell there are 2 Sites"
                Expect.exists (modelLines.[0].Sites) (fun l -> l.Sitename = "AC1") "The first Modell Block contains one site with name AC1"
                Expect.isGreaterThan (Array.length modelLines.[0].Sites.[1].Residues) 0 "there should be at least one residue"

                // real world tests

                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let parsedTestdata = readModels testdata
                let parsedChains = readChain testdata

                Expect.equal (Array.length parsedTestdata) 1 
                    "PDB Files with only one Modell should have length 1"
                Expect.equal parsedTestdata.[0].ModelId 1 
                    "first Modell ID in a PDB File has to be one"
                Expect.equal (Array.length (parsedTestdata.[0].Chains))
                    (parsedChains.Length) 
                    "PDB Files with only one Modell have to be the 
                    same Chain number as parsed"
                Expect.exists (parsedTestdata.[0].Chains) (fun chain -> 
                    chain.ChainId = 'A') 
                    "parsed Chains must be contain a chain with ID A"

                Expect.exists (parsedTestdata.[0].Linkages) (fun link -> 
                    link.Linktype = "Cis Peptide") 
                    "parsed Chains must be contain a CIS Peptide as found in 
                    PDB File"
                Expect.isGreaterThan (Array.length (parsedTestdata.[0].Linkages)) 
                    0 "There should be at least 1 Linkage"
                Expect.isGreaterThan (Array.length (parsedTestdata.[0].Sites)) 
                    0 "There should be at least 1 Site"
                
                let manyModels_example = 
                    readPBDFile "resources/pdbParser/dnaDuplexComplex.pdb" 

                let parsedManyModels = readModels manyModels_example

                Expect.isGreaterThan parsedManyModels.Length 1 
                    "PDB Files with more than one Modell Block need to be parsed 
                    in different Modells"
                Expect.equal parsedManyModels.[0].ModelId 1 "first Modell ID in
                    a PDB File has to be one"
                Expect.exists parsedManyModels.[0].Chains.[0].Residues 
                    (fun residue -> residue.ResidueName = "DT") 
                    "The  DNA chain should contain a residue with name 'DT'"
                Expect.isTrue
                        (parsedManyModels|> Array.forall (fun model -> 
                            model.Linkages.Length > 0)
                        )
                        "Each model block should contain at least one linkage."

                let manyModels_example2 = 
                    readPBDFile "resources/pdbParser/ribosomL23.pdb"

                let parsedManyModels2 = readModels manyModels_example2

                let numberOfModels = 
                   manyModels_example2 
                    |> Seq.filter (fun line -> line.StartsWith "MODEL")
                    |> Seq.map (fun line -> line.Substring(10,4).Trim() |> int)
                    |> Seq.last

                Expect.equal parsedManyModels2.Length numberOfModels 
                    "Parsed Number of Modell Blocks should be equal to 
                    last Modell ID in PDB File"
                Expect.equal parsedManyModels2.[0].ModelId 1 
                    "first Modell ID in a PDB File has to be one"
                Expect.exists parsedManyModels2.[0].Chains.[0].Residues (fun residue -> residue.ResidueName = "MET") 
                    "The chain should contain a residue with name 'MET'"
                Expect.exists parsedManyModels2.[0].Chains.[0].Residues 
                    (fun residue -> residue.Atoms |> Array.exists (fun atom -> 
                    atom.SerialNumber = 1)) "Each chain should contain at least 
                    one atom with SerialNumber 1"
                Expect.isEmpty parsedManyModels2.[0].Linkages 
                    "When no Linkage is described in a PDB File the List 
                    should be empty"

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                    |> readModels

                Expect.isGreaterThan exampleFile_Cre.Length 0 "Every PDB File 
                    should have at least one Modell Block"

                Expect.equal (Array.length exampleFile_Cre) 1 
                    "Number of Modell blocks should be equal to the last modell 
                    id in PDB File"

                let uniqueModelIds = 
                    exampleFile_Cre 
                    |> Array.map (fun model -> model.ModelId)
                    |> Array.distinct

                Expect.equal (Array.length exampleFile_Cre) uniqueModelIds.Length
                    "Number of Modell blocks should be equal to the last modell 
                    id in PDB File"

                Expect.equal exampleFile_Cre.[0].ModelId 1 "first Modellid and 
                once in single models should be 1"
                Expect.equal (Array.length (exampleFile_Cre.[0].Chains)) 1 
                    "Number of chains shoudl be equal to  different chains in 
                    the Modell Block" 
                Expect.exists (exampleFile_Cre.[0].Chains) (fun chain -> 
                    chain.ChainId = 'A' ) 
                    "parsed Chains must be contain a chain with ID A"

                Expect.equal (Array.length exampleFile_Cre.[0].Linkages) 0 
                    "Linkage Array should zero when no Linkage info exist"
               
                Expect.equal(Array.length exampleFile_Cre.[0].Sites) 0 
                    "When no Site information exist site part should be 0"
                
                Expect.isGreaterThan 
                    (Array.length exampleFile_Cre.[0].Chains.[0].Residues) 0 
                    "there should be at least one residue in the chain"

                // cre example with multiple chains but no model info
                
                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readModels

                Expect.isGreaterThan exampleFile_Cre_chains.Length 0 
                    "Every PDB File should have at least one Modell Block"

                Expect.equal (Array.length exampleFile_Cre_chains) 1 
                    "Number of Modell blocks should be equal to the last modell 
                    id in PDB File"

                let uniqueModelIds_creChains = 
                    exampleFile_Cre_chains 
                    |> Array.map (fun model -> model.ModelId)
                    |> Array.distinct

                Expect.equal 
                    (Array.length exampleFile_Cre_chains) 
                    uniqueModelIds_creChains.Length
                    "Number of Modell blocks should be equal to the last modell 
                    id in PDB File"

                Expect.equal exampleFile_Cre_chains.[0].ModelId 1 "first Modellid and 
                once in single models should be 1"

                Expect.equal (Array.length (exampleFile_Cre_chains.[0].Chains)) 2 
                    "Number of chains shoudld be equal to  different chains in 
                    the Modell Block" 

                let parsedChains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                    |> readChain

                Expect.exists (exampleFile_Cre_chains.[0].Chains) (fun chain -> 
                    chain.ChainId = 'A' ) 
                    "parsed Chains must be contain a chain with ID A"

                Array.iter2 (fun chain1 chain2 -> 
                    Expect.equal chain1.ChainId chain2.ChainId 
                        "Chains in the Model should have the same Chain Id as in the Chain part when only one models exists") 
                    exampleFile_Cre_chains.[0].Chains parsedChains

                Expect.equal (Array.length exampleFile_Cre_chains.[0].Linkages) 0 
                    "Linkage Array should zero when no Linkage info exist"
               
                Expect.equal(Array.length exampleFile_Cre_chains.[0].Sites) 0 
                    "When no Site information exist site part should be 0"

                let allResidues = 
                    exampleFile_Cre_chains 
                    |> Seq.collect ( fun x -> x.Chains)
                    |> Seq.collect (fun x -> x.Residues)

                let parsedResidues = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb")
                    |> readResidues
                
                Expect.isGreaterThan 
                    (Array.length exampleFile_Cre_chains.[0].Chains.[0].Residues) 0 
                    "there should be at least one residue in the chain"

                Seq.iter2 (fun residue1 residue2 -> 
                    Expect.equal residue1.ResidueName residue2.ResidueName 
                        "Residues in the Model should have the same Residue Name as in the Residue part when only one models exists") 
                     allResidues parsedResidues

                Expect.throws (fun () -> 
                    readPBDFile("resources/pdbParser/notexisting.pdb") 
                    |> readModels |> ignore) 
                    "Using an not existing PDB File should throw an error"
                
           }

           test "readStructure function works in parts and realWorld examples" {
                
                // part tests
                File.WriteAllLines("resources/pdbParser/pdbexample.pdb",
                    pdbLines_structure)

                let parsedPDBlines = readStructure "resources/pdbParser/pdbexample.pdb"
               
                Expect.equal parsedPDBlines.Metadata.Header "RubisCO Activase" 
                    "The HEADER field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Title "Testparsing" 
                    "The TITLE field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Compound 
                    (Some ["MOLECULE: RIBULOSE BISPHOSPHATE CARBOXYLASE/OXYGENASE ACTIVASE"]) "The COMPND field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Source (Some ["Arabidopsis"]) "The SOURCE field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Keywords (Some ["RubisCO Activase"; "Test"]) 
                    "The KEYWDS field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.ExpData (Some ["X-RAY DIFFRACTION"]) 
                    "The EXPDTA field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Authors (Some ["John Doe"]) 
                    "The AUTHOR field should be parsed correctly"
                Expect.equal parsedPDBlines.Metadata.Remarks ["RESOLUTION.    2.90 ANGSTROMS."; "Example remark 2"] 
                    "The REMARK field should be parsed correctly"
             
                Expect.equal (Array.length parsedPDBlines.Models) 2 
                    "Number of Modell blocks should be equal to the last modell 
                    id in PDB File"
                Expect.equal parsedPDBlines.Models.[0].ModelId 1 
                    "first Modellid should be 1"
                Expect.equal (Array.length (parsedPDBlines.Models.[0].Chains)) 2 
                    "Number of chains shoudl be equal to  different chains in 
                    the Modell Block" 
                Expect.exists (parsedPDBlines.Models.[0].Chains) (fun chain -> 
                    chain.ChainId = 'A' ) 
                    "parsed Chains must be contain a chain with ID A"
                Expect.isGreaterThanOrEqual (Array.length parsedPDBlines.Models.[0].Chains.[0].Residues) 1 
                    "The chain in Modell 1 should have at least 1 residue"
                Expect.equal parsedPDBlines.Models.[0].Chains.[0].Residues.[0].ResidueName 
                    "MET" "The first residue name should be 'MET'"
    
                Expect.equal (Array.length (parsedPDBlines.Models.[0].Chains.[0].Residues.[0].Atoms)) 2 "The residue should have 2 atoms"
                Expect.equal parsedPDBlines.Models.[0].Chains.[0].Residues.[0].Atoms.[0].SerialNumber 1 "The first atom's serial number should be 1"

                Expect.isGreaterThan (Array.length parsedPDBlines.Models.[0].Linkages) 
                    0 "The PDB File should contain at least one Linkage"
                Expect.exists parsedPDBlines.Models.[0].Linkages (fun linkage -> 
                    linkage.Linktype = "SS Bond") 
                    "Model should contain an SS Bond linkage"
                Expect.isEmpty parsedPDBlines.Models.[1].Linkages 
                    "Modells without Linkage Information should have a empty 
                    Linkage Array"

            // real world tests
                let testdata = readPBDFile("resources/pdbParser/rubisCOActivase.pdb")
                let readTestdataStructure = 
                    readStructure ("resources/pdbParser/rubisCOActivase.pdb")

                let testdata_chains = readChain testdata

                Expect.equal readTestdataStructure.Metadata.Header "CHAPERONE                               19-AUG-14   4W5W" 
                    "The HEADER field should be parsed correctly for the real 
                    PDB file"
                Expect.equal readTestdataStructure.Metadata.Title 
                    "RUBISCO ACTIVASE FROM ARABIDOPSIS THALIANA" 
                    "The TITLE field should be parsed correctly for the real 
                    PDB file"
                Expect.isSome readTestdataStructure.Metadata.Compound 
                    "The COMPND field should not be empty in the real PDB file"
                Expect.isTrue (readTestdataStructure.Metadata.Remarks.Length > 0) 
                    "The REMARK field should not be empty in the real PDB file"

                Expect.equal (Array.length readTestdataStructure.Models) 1 
                    "PDB Files with only one Modell should have length one"
                Expect.equal (readTestdataStructure.Models.[0].ModelId) 1 
                    "first Modell ID in a PDB File has to be one"
                Expect.equal (Array.length (readTestdataStructure.Models.[0].Chains)) (testdata_chains.Length) 
                    "PDB Files with only one Modell have to be the same Chain 
                    number as parsed"
                Expect.exists (readTestdataStructure.Models.[0].Chains) 
                    (fun chain -> chain.ChainId = 'A') 
                    "parsed Chains must be contain a chain with ID A"

                Expect.isGreaterThan (Array.length readTestdataStructure.Models.[0].Chains.[0].Residues) 0 
                    "PDB Files should contain at least one Residual Group"
                Expect.exists readTestdataStructure.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.ResidueName = "MET") 
                    "Evervy PDB File should contain at least one MET residue"
                Expect.exists readTestdataStructure.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.ResidueNumber = 69) 
                    "PDB File testdata should contain one residue with 69 "

                Expect.isGreaterThan 
                    (Array.length readTestdataStructure.Models.[0].Linkages) 0 
                        "PDB Data should contain at least one Linkage"
                Expect.exists  readTestdataStructure.Models.[0].Linkages 
                    (fun l -> l.Linktype = "Cis Peptide") 
                    "This PDB File has one Cis peptide"

                let parseConnects =  
                    readTestdataStructure.Models.[0].Linkages 
                    |> Array.filter ( fun l -> l.Linktype = "Connect")

                Expect.isGreaterThan parseConnects.Length 1 
                    "There should be more than 1 pair of Connected Atoms"


                // example without Modifications, Sites, Linkages and multiple 
                // Models-cre

                let readcreExample = 
                    "resources/pdbParser/Cre01g001550t11.pdb"
                    |> readStructure

                let exampleFile_Cre = 
                    readPBDFile("resources/pdbParser/Cre01g001550t11.pdb") 
                  

                let creExampleChains = readChain exampleFile_Cre

              

                Expect.equal readcreExample.Metadata.Header "" 
                    "The HEADER field should be parsed correctly and be empty with
                    no metadata informations"

                Expect.equal readcreExample.Metadata.Title "" 
                    "The TITLE field should be parsed correctly for the real 
                    PDB file"

                Expect.isNone readcreExample.Metadata.Compound 
                    "The COMPND field should be none when no metadata informations"

                Expect.isTrue (readcreExample.Metadata.Remarks.Length = 0) 
                    "The REMARK field should  be empty when no 
                    metadata info are given"

                Expect.equal (Array.length readcreExample.Models) 1 
                    "PDB Files with only one Modell should have length one"

                Expect.equal (readcreExample.Models.[0].ModelId) 1 
                    "first Modell ID in a PDB File has to be one"

                Expect.equal (Array.length (readcreExample.Models.[0].Chains)) (creExampleChains.Length) 
                    "PDB Files with only one Modell have to be the same Chain 
                    number as parsed"

                Expect.exists (readcreExample.Models.[0].Chains) 
                    (fun chain -> chain.ChainId = 'A') 
                    "parsed Chains must be contain a chain with ID A"

                Expect.isGreaterThan (Array.length 
                    readcreExample.Models.[0].Chains.[0].Residues) 0 
                    "PDB Files should contain at least one Residual Group"

                Expect.exists readcreExample.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.ResidueName = "MET") 
                    "Evervy PDB File should contain at least one MET residue"

                Expect.exists readcreExample.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.ResidueNumber = 1) 
                    "PDB File testdata should contain one residue with 1 "

                Expect.exists readcreExample.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.Atoms |> Array.exists (fun atom -> 
                        atom.SerialNumber = 1)) "Each chain should contain at least 
                        one atom with SerialNumber 1"

                Expect.all readcreExample.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                Expect.all readcreExample.Models.[0].Chains.[0].Residues 
                    (fun residue -> residue.SecStructureType.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                Expect.isEmpty readcreExample.Models.[0].Linkages  
                        "PDB Data should contain at least one Linkage"

                Expect.isEmpty readcreExample.Models.[0].Sites 
                    "PDBFiles without Sites should have an empty Site Array"
                    
                //cre example with multiple chains but no model info 
           
                let readcreExample_chains = 
                    "resources/pdbParser/Cre01g026150t11.pdb"
                    |> readStructure

                let exampleFile_Cre_chains = 
                    readPBDFile("resources/pdbParser/Cre01g026150t11.pdb") 
                  

                let creExampleChains_chains = readChain exampleFile_Cre_chains
             
                Expect.equal readcreExample_chains.Metadata.Header "" 
                    "The HEADER field should be parsed correctly and be empty with
                    no metadata informations"

                Expect.equal readcreExample_chains.Metadata.Title "" 
                    "The TITLE field should be parsed correctly for the real 
                    PDB file"

                Expect.isNone readcreExample_chains.Metadata.Compound 
                    "The COMPND field should be none when no metadata informations"

                Expect.isTrue (readcreExample_chains.Metadata.Remarks.Length = 0) 
                    "The REMARK field should  be empty when no 
                    metadata info are given"

                let models = exampleFile_Cre_chains |> readModels

                Expect.equal 
                    (Array.length readcreExample_chains.Models) 
                    models.Length 
                    "PDB Files with only one Modell should have length one"

                Expect.equal (readcreExample_chains.Models.[0].ModelId) models.[0].ModelId
                    "first Modell ID in a PDB File has to be one or equal"

                let allchains =              
                    readcreExample_chains.Models
                    |> Seq.collect (fun x -> x.Chains)
                    |> Seq.toArray
                    
             
                Expect.equal allchains.Length creExampleChains_chains.Length
                    "PDB Files with only one Modell have to be the same Chain 
                    number as parsed"

                Array.iter2 (fun chain1 chain2 -> 
                    Expect.equal chain1.ChainId chain2.ChainId 
                        "Chains in the Model should have the same Chain Id as in the Chain part when only one models exists") 
                    allchains creExampleChains_chains

                let allResidues_crechain = 
                    readcreExample_chains.Models
                    |> Seq.collect ( fun x -> x.Chains)
                    |> Seq.collect (fun x -> x.Residues)
                    |> Seq.toArray
           
                Expect.isGreaterThan allResidues_crechain.Length 0 
                    "PDB Files should contain at least one Residual Group"

                Expect.exists allResidues_crechain
                    (fun residue -> residue.ResidueName = "MET") 
                    "Evervy PDB File should contain at least one MET residue"

                Seq.iter2 (fun residue1 residue2 -> 
                    Expect.equal residue1 residue2
                        "Residues in the Model should have the same Residue 
                        Name as in the Residue part when only one models exists") 
                     allResidues_crechain (readResidues exampleFile_Cre_chains)

                Expect.exists allResidues_crechain
                    (fun residue -> residue.ResidueNumber = 1) 
                    "real PDB File testdata should contain one residue with 1 "

                Expect.exists allResidues_crechain
                    (fun residue -> 
                        residue.Atoms 
                        |> Array.head 
                        |> fun x -> x.SerialNumber =1
                    ) "Each PDB File should start with SerialNumber 1"

                Expect.all allResidues_crechain 
                    (fun residue -> residue.Modification.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                Expect.all allResidues_crechain
                    (fun residue -> residue.SecStructureType.IsNone) 
                    "PDB Files without modified Residues should have value NONE"

                let allLinkages = 
                    readcreExample_chains.Models
                    |> Seq.collect (fun x -> x.Linkages)
                    |> Seq.toArray

                Expect.isEmpty allLinkages 
                        "PDB Data should contain at least one Linkage"

                let allSites = 
                    readcreExample_chains.Models
                    |> Seq.collect (fun x -> x.Sites)
                    |> Seq.toArray

                Expect.isEmpty allSites
                    "PDBFiles without Sites should have an empty Site Array" 

                Expect.throws (fun () -> 
                    ("resources/pdbParser/notexisting.pdb") 
                    |> readStructure 
                    |> ignore) 
                    "Using an not existing PDB File should throw an error"                                     
           }                              
        ]


