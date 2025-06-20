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
        member this.extractTestdata_small() =
            let lines = getResiduesPerChain testdata 1
            lines

        [<Benchmark>]
        member this.extractTestdata_big() =
            let lines = getResiduesPerChain htq 1
            lines

    type extractAtoms() =

        
        [<Benchmark>]
        member this.extractAtoms_small() =
            let lines = getAtomsPerModel testdata 1
            lines
        [<Benchmark>]
        member this.extractAtoms_big() =
            let lines = getAtomsPerModel htq 1
            lines


    type vdw_lookup() =
             
        [<Benchmark>]
        member this.VDW_small() =
            let vdw_raadi = 
                    residues_testdata
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (determine_effective_radius atom residue.ResidueName)
                                )                              
                        ))|> dict

                   
            vdw_raadi

        [<Benchmark>]
        member this.vdw_big() =
            let vdw_raadi = 
                    residues_htq
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (determine_effective_radius atom residue.ResidueName)
                                )                              
                        ))|> dict
            vdw_raadi

    type testpoints() =

            [<Benchmark>]
            member this.testpoints_small() =
                let lines = fibonacciTestPoints 10
                lines

            [<Benchmark>]
            member this.testpoints_big() =
                let lines = fibonacciTestPoints 10000
                lines

    type scale_testpoints() =

        let testPoints = fibonacciTestPoints 100
          
        [<Benchmark>]
        member this.scaleTespoints_small() =
            let testpoints =
                    residues_testdata
                    |> Seq.map ( fun kvp ->
                         kvp.Key,
                         kvp.Value
                         |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (scaleFibonacciTestpoints testPoints atom residue.ResidueName)
                                )                              
                        ))|> dict
            testpoints

        [<Benchmark>]
        member this.scaleTestpoints_big() =
            let testpoints =  
                    residues_htq
                    |> Seq.map ( fun kvp ->
                            kvp.Key,
                            kvp.Value
                            |>Seq.map(fun residue ->                       
                                residue.ResidueName,residue.Atoms
                                |>Seq.map(fun atom -> (scaleFibonacciTestpoints testPoints atom residue.ResidueName)
                                )                              
                        ))|> dict

            testpoints

  
    type acessiblePoints() =
         
        [<Benchmark>]
        member this.acessiblePoints_small() =
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

                            accessibleTestpoints allAtomsOfChain 100
                        )
                        |> Seq.toArray

            acessiblePointsDict_small
                
              
        [<Benchmark>]
        member this.acessiblePoints_big() =
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

                        accessibleTestpoints allAtomsOfChain 100
                    )
                    |>Seq.toArray

            acessiblePointsDictHTQ
            
   
    
    type sasaAtoms() =

        
        [<Benchmark>]
        member this.SASAatom_small() =
            sasaAtom testdata 1 100


        [<Benchmark>]
        member this.SASAatom_big() =
            sasaAtom htq 1 100


    

    type sasaResidues() =

        
        [<Benchmark>]
        member this.SASAresidue_small() =
            sasaResidue testdata 1 100    
            
        [<Benchmark>]
        member this.SASAresidue_big() =
            sasaResidue htq 1 100


    

    type relSASA() =
        
        [<Benchmark>]
        member this.relSASAresidue_small() =
            relativeSASA_aminoacids testdata 1 100    
            
        [<Benchmark>]
        member this.relSASAresidue_big() =
            relativeSASA_aminoacids htq 1 100


    

    type differentiate() =

        [<Benchmark>]
        member this.differentiateExposedandBuried_small() =
            let diffTest= differentiateAccessibleAA testdata 1 100 20

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
        member this.differentiateExposedandBuried_big() =

            let diffTest = differentiateAccessibleAA htq 1 100 0.2

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
        member this.computeSASAchain_small() =
            sasaChain testdata 1 100    
            
        [<Benchmark>]
        member this.computeSASAchain_big() =
            sasaResidue htq 1 100

