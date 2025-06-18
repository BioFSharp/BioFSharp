namespace BioFSharp.Tests.ReferenceObjects

module SASA = 

    open Deedle

    // reference python freesasa : Atoms 
    
    let python_results_atom = Frame.ReadCsv(path = "resources/sasa_per_atom.csv", 
        separators = ",", hasHeaders = true)

    let python_resultsSASA: Series<int, float> = python_results_atom.GetColumn<float>("sasa")
    let python_resultsAtoms: Series<int, int> = python_results_atom.GetColumn<int>("serial")

    let serialArrays = python_resultsAtoms.Values |> Seq.toArray

    let reference_sasaArray: float[] =
        python_resultsSASA.Values    
        |> Seq.toArray 

    let python_SASA_array = Array.zip serialArrays reference_sasaArray

    // reference python freesasa: Residues

    let python_results_res = Frame.ReadCsv(path = "resources/sasa_per_residue.csv", 
        separators = ",", hasHeaders = true)

    let python_resultsSASA_res: Series<int, float> = python_results_res.GetColumn<float>("Abs_SASA")
    let python_resultsResidues: Series<int, int> = python_results_res.GetColumn<int>("ResNum") 
    let python_resultsResnames: Series<int, string> = python_results_res.GetColumn<string>("ResName")

                
    let sasaArrays = python_resultsSASA_res.Values |> Seq.toArray
    let resNumber = python_resultsResidues.Values |> Seq.toArray
    let resNames = python_resultsResnames.Values |> Seq.toArray

    let residue = Array.map2 (fun x y -> x,y) resNumber resNames

    let python_SASA_arrayResidues = Array.zip (residue) sasaArrays

    // reference python freesasa: relative 

    let python_results_relativeres = Frame.ReadCsv(path = "resources/relsasa_per_residue.csv", 
        separators = ",", hasHeaders = true)

    let python_resultsSASA_relative: Series<int, float> = 
        python_results_relativeres.GetColumn<float>("Rel_SASA")
    let python_resultsResidues_relative: Series<int, int> = 
        python_results_relativeres.GetColumn<int>("ResNum") 
    let python_resultsResnames_relative: Series<int, string> = 
        python_results_relativeres.GetColumn<string>("ResName")

    let sasaArrays_rel = python_resultsSASA_relative.Values |> Seq.toArray
    let resNumber_rel = python_resultsResidues_relative.Values |> Seq.toArray
    let resNames_rel = python_resultsResnames_relative.Values |> Seq.toArray

    let residue_rel = Array.map2 (fun x y -> x,y) resNumber_rel resNames_rel

    let python_SASA_array_relativeRes = Array.zip (residue_rel) sasaArrays_rel

    /// reference python freesasa: chains

    let python_results_chain = Frame.ReadCsv(path = "resources/sasa_per_chain.csv", 
        separators = ",", hasHeaders = true)

    let python_resultsSASA_chains: Series<int, float> = python_results_chain.GetColumn<float>("Abs_SASA")
    let python_resultsChains: Series<int, char> = python_results_chain.GetColumn<char>("Chain")

    let sasaArrays_chains = python_resultsSASA_chains.Values |> Seq.toArray
    let chainNumber = python_resultsChains.Values 

