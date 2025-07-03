namespace BioFSharp.Benchmarks.Algorithm

module SASABenchmarks =
    open BenchmarkDotNet.Attributes
    open BioFSharp.Algorithm.SASA


    let testdata = "resources/rubisCOActivase.pdb"
    let htq = "resources/htq.pdb"
    let residues_testdata = getResiduesPerChain testdata 1 
    let residues_htq =  getResiduesPerChain htq 1

    [<MemoryDiagnoser>] 
    type ExtractResidues() =
             
        [<Benchmark>]
        member this.extractTestdata_rubisco() =
            let lines = getResiduesPerChain testdata 1
            lines

        [<Benchmark>]
        member this.extractTestdata_htq() =
            let lines = getResiduesPerChain htq 1
            lines

    type extractAtoms() =

        
        [<Benchmark>]
        member this.extractAtoms_rubisco() =
            let lines = getAtomsPerModel testdata 1
            lines
        [<Benchmark>]
        member this.extractAtoms_htq() =
            let lines = getAtomsPerModel htq 1
            lines


    type vdw_lookup() =
             
        [<Benchmark>]
        member this.VDW_rubisco() =
            let vdw_raadi = 
                    residues_testdata
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (determine_effective_radius atom residue.ResidueName "Water")
                                )                              
                        ))|> dict

                   
            vdw_raadi

        [<Benchmark>]
        member this.vdw_htq() =
            let vdw_raadi = 
                    residues_htq
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (determine_effective_radius atom residue.ResidueName "Water")
                                )                              
                        ))|> dict
            vdw_raadi

    type testpoints() =

            [<Benchmark>]
            member this.testpoints_rubisco() =
                let lines = fibonacciTestPoints 10
                lines

            [<Benchmark>]
            member this.testpoints_htq() =
                let lines = fibonacciTestPoints 10000
                lines

    type scale_testpoints() =

        let testPoints = fibonacciTestPoints 100
          
        [<Benchmark>]
        member this.scaleTespoints_rubisco() =
            let testpoints =
                    residues_testdata
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (scaleFibonacciTestpoints testPoints atom residue.ResidueName "Water")
                                )                              
                        ))|> dict
            testpoints

        [<Benchmark>]
        member this.scaleTestpoints_htq() =
            let testpoints =  
                    residues_htq
                    |> Seq.map ( fun kvp ->
                            kvp.Key,
                            kvp.Value
                            |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (scaleFibonacciTestpoints testPoints atom residue.ResidueName "Water")
                                )                              
                        ))|> dict

            testpoints

  
    type acessiblePoints() =
         
        [<Benchmark>]
        member this.acessiblePoints_rubisco() =
            let acessiblePointsDict_small= 
                        residues_testdata
                        |> Seq.map (fun kvp ->
                           
                            let residues = kvp.Value
                            
                            let allAtomsOfChain =
                                residues
                                |> Array.collect (fun residue ->
                                    residue.Atoms
                                    |> Array.map (fun atom -> atom, residue.ResidueName)
                                )

                            accessibleTestpoints allAtomsOfChain 100 "Water"
                        )
                        |> Seq.toArray

            acessiblePointsDict_small
                
              
        [<Benchmark>]
        member this.acessiblePoints_htq() =
            let acessiblePointsDictHTQ = 
                    residues_htq               
                    |> Seq.map (fun kvp ->
                       
                        let residues = kvp.Value

                        let allAtomsOfChain =
                            residues
                            |> Array.collect (fun residue ->
                                residue.Atoms
                                |> Array.map (fun atom -> atom, residue.ResidueName)
                            )

                        accessibleTestpoints allAtomsOfChain 100 "Water"
                    )
                    |>Seq.toArray

            acessiblePointsDictHTQ
            
   
    
    type sasaAtoms() =

        
        [<Benchmark>]
        member this.SASAatom_rubisco() =
            sasaAtom testdata 1 100 "Water"


        [<Benchmark>]
        member this.SASAatom_htq() =
            sasaAtom htq 1 100 "Water"


    

    type sasaResidues() =

        
        [<Benchmark>]
        member this.SASAresidue_rubisco() =
            sasaResidue testdata 1 100  "Water"
            
        [<Benchmark>]
        member this.SASAresidue_htq() =
            sasaResidue htq 1 100 "Water"


    

    type relSASA() =
        
        [<Benchmark>]
        member this.relSASAresidue_rubisco() =
            relativeSASA_aminoacids testdata 1 100 "Water" true   
            
        [<Benchmark>]
        member this.relSASAresidue_htq() =
            relativeSASA_aminoacids htq 1 100 "Water" true


    

    type differentiate() =

        [<Benchmark>]
        member this.differentiateExposedandBuried_rubisco() =
            let diffTest= differentiateAccessibleAA testdata 1 100 "Water" 0.2 false 

            let allAcessibles = 
                diffTest 
                |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                |>dict

            let allNonAcessibles = 
                diffTest 
                |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                |>dict

            allAcessibles,allNonAcessibles
            
       

        [<Benchmark>]
        member this.differentiateExposedandBuried_htq() =

            let diffTest = differentiateAccessibleAA htq 1 100 "Water" 0.2 false

            let allAcessibles = 
                diffTest    
                |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)
                |>dict

            let allNonAcessibles = 
                diffTest 
                |> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)
                |>dict

            allAcessibles,allNonAcessibles
            

    

    type chainSASA() =

        [<Benchmark>]
        member this.computeSASAchain_rubisco() =
            sasaChain testdata 1 100 "Water"    
            
        [<Benchmark>]
        member this.computeSASAchain_htq() =
            sasaResidue htq 1 100 "Water"

