namespace BioFSharp.Tests.ReferenceObjects

module SASA = 

    open Deedle

    // reference python freesasa : Atoms 
    
    let python_results_atom = 
        Frame.ReadCsv(path = "resources/pdbParser/sasa_per_atom.csv", 
        separators = ";", hasHeaders = true)

    let python_resultsSASA: Series<int, float> = python_results_atom.GetColumn<float>("sasa")
    let python_resultsAtoms: Series<int, int> = python_results_atom.GetColumn<int>("serial")

    let serialArrays = python_resultsAtoms.Values |> Seq.toArray

    let reference_sasaArray: float[] =
        python_resultsSASA.Values    
        |> Seq.toArray 

    let python_SASA_array = Array.zip serialArrays reference_sasaArray

        // for cre
       
    let crePythonReference_atom = 
        Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_per_atom_probeBiotin.csv", 
        separators = ";", hasHeaders = true)

    let crePythonReference_SASA: Series<int, float> = crePythonReference_atom.GetColumn<float>("sasa")
    let crePythonReference_Atoms: Series<int, string> = crePythonReference_atom.GetColumn<string>("atom_name")

    let serial_ValuesCre = crePythonReference_Atoms.Values |> Seq.toArray

    let atomsasa_cre: float[] =
        crePythonReference_SASA.Values    
        |> Seq.toArray 

    let combined_creArray = Array.zip serial_ValuesCre atomsasa_cre

        // for cre with float

    let crePythonReference_atom_floatProbe = 
        Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_per_atom_probe4f.csv", 
        separators = ";", hasHeaders = true)

    let crePythonReference_SASA_floatProbe: Series<int, float> = crePythonReference_atom_floatProbe.GetColumn<float>("sasa")
    let crePythonReference_Atoms_floatProbe: Series<int, string> = crePythonReference_atom_floatProbe.GetColumn<string>("atom_name")

    let serial_ValuesCre_floatprobe = 
        crePythonReference_Atoms_floatProbe.Values |> Seq.toArray

    let atomsasa_cre_floatprobe: float[] =
        crePythonReference_SASA_floatProbe.Values    
        |> Seq.toArray 

    let combined_creArray_floatprobe = 
        Array.zip serial_ValuesCre_floatprobe atomsasa_cre_floatprobe


        /// another cre_example with multiple chains 

    let cre2PythonReference_atom = 
        Frame.ReadCsv(path = "resources/pdbParser/Cre01g026150t11_output.csv", 
        separators = ";", hasHeaders = true)

    let cre2PythonReference_SASA: Series<int, float> = 
        cre2PythonReference_atom.GetColumn<float>("sasa")
    let cre2PythonReference_Atoms: Series<int, string> = 
        cre2PythonReference_atom.GetColumn<string>("atom_name")

    let serial_Values_Cre2 = cre2PythonReference_Atoms.Values |> Seq.toArray

    let atomsasa_cre2  = cre2PythonReference_SASA.Values |> Seq.toArray 

    let combined_cre2Array = Array.zip serial_Values_Cre2 atomsasa_cre2

    // reference python freesasa: absolut Residues

    let python_results_res = Frame.ReadCsv(path = "resources/pdbParser/sasa_per_residue.csv", 
        separators = ";", hasHeaders = true)

    let python_resultsSASA_res: Series<int, float> = python_results_res.GetColumn<float>("Abs_SASA")
    let python_resultsResidues: Series<int, int> = python_results_res.GetColumn<int>("ResNum") 
    let python_resultsResnames: Series<int, string> = python_results_res.GetColumn<string>("ResName")

                
    let sasaArrays = python_resultsSASA_res.Values |> Seq.toArray
    let resNumber = python_resultsResidues.Values |> Seq.toArray
    let resNames = python_resultsResnames.Values |> Seq.toArray

    let residue = Array.map2 (fun x y -> x,y) resNumber resNames

    let python_SASA_arrayResidues = Array.zip (residue) sasaArrays

       // reference for pdbfile without missing infos and stored probe

    let cre_python_results_res = Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_per_absResidue_probeBiotin.csv", 
        separators = ";", hasHeaders = true)

    let cre_python_resultsSASA_res: Series<int, float> = 
        cre_python_results_res.GetColumn<float>("Abs_SASA")

    let cre_python_resultsResidues: Series<int, int> = 
        cre_python_results_res.GetColumn<int>("ResNum") 

    let cre_python_resultsResnames: Series<int, string> =
        cre_python_results_res.GetColumn<string>("ResName")

                
    let cre_sasaArrays = cre_python_resultsSASA_res.Values |> Seq.toArray
    let cre_resNumber = cre_python_resultsResidues.Values |> Seq.toArray
    let cre_resNames = cre_python_resultsResnames.Values |> Seq.toArray

    let cre_residue = Array.map2 (fun x y -> x,y) cre_resNumber cre_resNames

    let cre_python_SASA_arrayResidues = Array.zip (cre_residue) cre_sasaArrays

    // reference for pdbfile without missing infos and float probe

    let float_cre_python_results_res = Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_per_absResidue_probe4f.csv", 
        separators = ";", hasHeaders = true)

    let float_cre_python_resultsSASA_res: Series<int, float> = 
        float_cre_python_results_res.GetColumn<float>("Abs_SASA")

    let float_cre_python_resultsResidues: Series<int, int> = 
        float_cre_python_results_res.GetColumn<int>("ResNum") 

    let float_cre_python_resultsResnames: Series<int, string> =
        float_cre_python_results_res.GetColumn<string>("ResName")

                
    let float_cre_sasaArrays = 
        float_cre_python_resultsSASA_res.Values 
        |> Seq.toArray

    let float_cre_resNumber = 
        float_cre_python_resultsResidues.Values 
        |> Seq.toArray

    let float_cre_resNames = 
        float_cre_python_resultsResnames.Values
        |> Seq.toArray

    let float_cre_residue = Array.map2 (fun x y -> x,y) float_cre_resNumber float_cre_resNames

    let float_cre_python_SASA_arrayResidues = 
        Array.zip float_cre_residue float_cre_sasaArrays

        // another cre example 
   
    let pythonCre2_example = 
        Frame.ReadCsv(path = "resources/pdbParser/Cre01g026150t11_outputAbsResidue.csv", separators = ";", hasHeaders = true)

    let python_resultsSASA_cre2: Series<int, float> = 
        pythonCre2_example.GetColumn<float>("Abs_SASA")

    let python_resultsResidues_cre2: Series<int, int> = 
        pythonCre2_example.GetColumn<int>("ResNum") 

    let python_resultsResnames_cre2: Series<int, string> =
        pythonCre2_example.GetColumn<string>("ResName")
             

    let cre2_resNumber = 
        python_resultsResidues_cre2.Values 
        |> Seq.toArray

    let cre2_resNames = 
        python_resultsResnames_cre2.Values
        |> Seq.toArray

    let cre2_residue = Array.map2 (fun x y -> x,y) cre2_resNumber cre2_resNames

    let cre2_sasaArrays = 
        python_resultsSASA_cre2.Values 
        |> Seq.toArray

    let cre2_python_SASA_arrayResidues = 
        Array.zip cre2_residue cre2_sasaArrays


    // reference python freesasa: relative 

    let python_results_relativeres = 
        Frame.ReadCsv(path = "resources/pdbParser/relsasa_per_residue.csv", 
        separators = ";", hasHeaders = true)

    let python_resultsSASA_relative: Series<int, float> = 
        python_results_relativeres.GetColumn<float>("Rel_SASA_CustomValues")
    let python_resultsSASA_relativeFix: Series<int, float> = 
        python_results_relativeres.GetColumn<float>("Rel_SASA_fixValues")
    let python_resultsResidues_relative: Series<int, int> = 
        python_results_relativeres.GetColumn<int>("ResNum") 
    let python_resultsResnames_relative: Series<int, string> = 
        python_results_relativeres.GetColumn<string>("ResName")

    let sasaArrays_rel = python_resultsSASA_relative.Values |> Seq.toArray
    let resNumber_rel = python_resultsResidues_relative.Values |> Seq.toArray
    let resNames_rel = python_resultsResnames_relative.Values |> Seq.toArray

    let residue_rel = Array.map2 (fun x y -> x,y) resNumber_rel resNames_rel

    let python_SASA_array_relativeRes = Array.zip (residue_rel) sasaArrays_rel

    let fixedSASA_rel =  
        let fix = python_resultsSASA_relativeFix.Values |> Seq.toArray

        Array.zip (residue_rel) fix

        // for cre relative
  
    let crepython_results_relativeres = 
        Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_relSASA_probeBiotin.csv", 
        separators = ";", hasHeaders = true)

    let crepython_resultsSASA_relative: Series<int, float> = 
        crepython_results_relativeres.GetColumn<float>("Rel_SASA_CustomValues")

    let crepython_resultsSASA_relativeFix: Series<int, float> = 
        crepython_results_relativeres.GetColumn<float>("Rel_SASA_fixValues")

    let crepython_resultsResidues_relative: Series<int, int> = 
        crepython_results_relativeres.GetColumn<int>("ResNum") 

    let crepython_resultsResnames_relative: Series<int, string> = 
        crepython_results_relativeres.GetColumn<string>("ResName")

    let cresasaArrays_rel = 
        crepython_resultsSASA_relative.Values 
        |> Seq.toArray

    let creresNumber_rel = 
        crepython_resultsResidues_relative.Values 
        |> Seq.toArray

    let creresNames_rel = 
        crepython_resultsResnames_relative.Values 
        |> Seq.toArray

    let creresidue_rel = Array.map2 (fun x y -> x,y) creresNumber_rel creresNames_rel

    let crepython_SASA_array_relativeRes = Array.zip (creresidue_rel) cresasaArrays_rel

    let crefixedSASA_rel =  
        let fix = crepython_resultsSASA_relativeFix.Values |> Seq.toArray

        Array.zip (creresidue_rel) fix

        // for cre relative with float probe 
      
    let float_crepython_results_relativeres = 
        Frame.ReadCsv(path = "resources/pdbParser/cre_sasa_relSASA_probe4f.csv", 
        separators = ";", hasHeaders = true)

    let float_crepython_resultsSASA_relative: Series<int, float> = 
        float_crepython_results_relativeres.GetColumn<float>("Rel_SASA_CustomValues")

    let float_crepython_resultsSASA_relativeFix: Series<int, float> = 
        float_crepython_results_relativeres.GetColumn<float>("Rel_SASA_fixValues")

    let float_crepython_resultsResidues_relative: Series<int, int> = 
        float_crepython_results_relativeres.GetColumn<int>("ResNum") 

    let float_crepython_resultsResnames_relative: Series<int, string> = 
        float_crepython_results_relativeres.GetColumn<string>("ResName")

    let float_cresasaArrays_rel = 
        float_crepython_resultsSASA_relative.Values 
        |> Seq.toArray
    
    let float_creresNumber_rel = 
        float_crepython_resultsResidues_relative.Values 
        |> Seq.toArray

    let float_creresNames_rel = 
        float_crepython_resultsResnames_relative.Values 
        |> Seq.toArray

    let float_creresidue_rel = 
        Array.map2 (fun x y -> x,y) float_creresNumber_rel float_creresNames_rel

    let float_crepython_SASA_array_relativeRes = 
        Array.zip (float_creresidue_rel) float_cresasaArrays_rel

    let float_crefixedSASA_rel =  
        let fix = float_crepython_resultsSASA_relativeFix.Values |> Seq.toArray

        Array.zip (float_creresidue_rel) fix

        // Another cre example 

    let cre2python_results_relativeres = 
        Frame.ReadCsv(path = "resources/pdbParser/Cre01g026150t11_output_relativeResidue.csv", 
        separators = ";", hasHeaders = true)

    let cre2python_resultsSASA_relative: Series<int, float> = 
        cre2python_results_relativeres.GetColumn<float>("Rel_SASA_CustomValues")

    let cre2python_resultsSASA_relativeFix: Series<int, float> = 
        cre2python_results_relativeres.GetColumn<float>("Rel_SASA_fixValues")

    let cre2python_resultsResidues_relative: Series<int, int> = 
        cre2python_results_relativeres.GetColumn<int>("ResNum") 

    let cre2python_resultsResnames_relative: Series<int, string> = 
        cre2python_results_relativeres.GetColumn<string>("ResName")

    let cre2sasaArrays_rel = 
        cre2python_resultsSASA_relative.Values 
        |> Seq.toArray
    
    let cre2resNumber_rel = 
        cre2python_resultsResidues_relative.Values 
        |> Seq.toArray

    let cre2resNames_rel = 
        cre2python_resultsResnames_relative.Values 
        |> Seq.toArray

    let cre2residue_rel = 
        Array.map2 (fun x y -> x,y) cre2resNumber_rel cre2resNames_rel

    let cre2python_SASA_array_relativeRes = 
        Array.zip (cre2residue_rel) cre2sasaArrays_rel

    let cre2fixedSASA_rel =  
        let fix = cre2python_resultsSASA_relativeFix.Values |> Seq.toArray

        Array.zip (cre2residue_rel) fix


    /// reference python freesasa: chains

    let python_results_chain = Frame.ReadCsv(path = "resources/pdbParser/sasa_per_chain.csv", 
        separators = ";", hasHeaders = true)

    let python_resultsSASA_chains: Series<int, float> = python_results_chain.GetColumn<float>("Abs_SASA")
    let python_resultsChains: Series<int, char> = python_results_chain.GetColumn<char>("Chain")

    let sasaArrays_chains = python_resultsSASA_chains.Values |> Seq.toArray
    let chainNumber = python_resultsChains.Values 

        // reference for cre chain computation
        
    let python_results_chain_cre = 
        Frame.ReadCsv(path = "resources/pdbParser/sasa_per_chain_biotin.csv", 
        separators = ";", hasHeaders = true)

    let cre_python_resultsSASA_chains: Series<int, float> = python_results_chain_cre.GetColumn<float>("Abs_SASA")
    let cre_python_resultsChains: Series<int, char> = python_results_chain_cre.GetColumn<char>("Chain")

    let cre_sasaArrays_chains = cre_python_resultsSASA_chains.Values |> Seq.toArray
    let cre_chainNumber = cre_python_resultsChains.Values 

      // reference for cre chain computation with float probe

    let float_python_results_chain_cre = 
        Frame.ReadCsv(path = "resources/pdbParser/sasa_per_chain_4f.csv", 
        separators = ";", hasHeaders = true)

    let float_cre_python_resultsSASA_chains: Series<int, float> = float_python_results_chain_cre.GetColumn<float>("Abs_SASA")
    
    let float_cre_python_resultsChains: Series<int, char> = float_python_results_chain_cre.GetColumn<char>("Chain")

    let float_cre_sasaArrays_chains = 
        float_cre_python_resultsSASA_chains.Values |> Seq.toArray

    let float_cre_chainNumber = float_cre_python_resultsChains.Values

    // another cre example for chains
           
    let python_results_chain_cre2 = 
        Frame.ReadCsv(path = "resources/pdbParser/Cre01g026150t11_chainSASA.csv", 
        separators = ";", hasHeaders = true)

    let cre2_python_resultsSASA_chain: Series<int, float> = python_results_chain_cre2.GetColumn<float>("Abs_SASA")
    let cre2_python_resultsChain: Series<int, char> = python_results_chain_cre2.GetColumn<char>("Chain")

    let cre2_sasaArrays_chain = 
        cre2_python_resultsSASA_chain.Values |> Seq.toArray
    let cre2_chainNumber = 
        cre2_python_resultsChain.Values 