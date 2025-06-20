namespace BioFSharp.Tests.Algorithm 

module SASATests = 
    
    open BioFSharp.Algorithm.SASA
    open BioFSharp.IO.PDBParser
    open BioFSharp.FileFormats.PDBParser
    open BioFSharp.Tests.ReferenceObjects.SASA

    open Expecto
    open Deedle
   
    open System.Collections.Generic


    let AlgorithmTests = 
        testList "SASA analysis" [
            
            test "Structure prepResidue extraction for SASA calculation" {

                let testdata = getResiduesPerChain "resources/rubisCOActivase.pdb" 1
                let htq = getResiduesPerChain "resources/htq.pdb" 2
                let parsedModelhtq = readStructure "resources/htq.pdb" 

                let parsedResidues  =
                    readResidues (readPBDFile "resources/rubisCOActivase.pdb")
                    |> Array.map (fun res ->
                        
                        let filteredAtoms =
                            res.Atoms
                            |> Array.filter (fun a -> not a.Hetatm)
                      
                        { res with Atoms = filteredAtoms }
                    )
                    |> Array.filter (fun r -> r.Atoms.Length > 0)
                    |> Array.map (fun r -> r.ResidueNumber)


                let parsedModelRes = readStructure "resources/rubisCOActivase.pdb"

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
                
                Expect.throws (fun () -> 
                    getResiduesPerChain "resources/rubisCOActivase.pdb" 2 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"
            }

            test "single Atoms are extracted correctly per Chain"{
                let atoms = getAtomsPerModel  "resources/rubisCOActivase.pdb"1
                let parsedAtoms = readAtom (readPBDFile "resources/rubisCOActivase.pdb") 

                Expect.isLessThanOrEqual atoms.['A'].Length parsedAtoms.Length 
                    "Number of parsed Atoms should be smaller or equal then number 
                    of Atoms in PDB File"
                Expect.equal (atoms.Count) 1 "PDB File with only one model and 
                    one Chain should have only one atom list"
                
                let atomsHTQ = getAtomsPerModel "resources/htq.pdb" 1
                let parseAtomsHTQ = readAtom (readPBDFile "resources/htq.pdb")

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
                    getResiduesPerChain  "resources/htq.pdb" 500 |>ignore) 
                    "PDB File with only one model and one Chain should throw an
                    error if modelid is not present"

            }

            test "Van der Waals Radius is determined correctly"{
                let testdata = getResiduesPerChain "resources/rubisCOActivase.pdb" 1
                let vdw_raadi = 
                    testdata
                    |> Seq.map ( fun kvp ->
                        kvp.Key,
                        kvp.Value
                        |>Array.Parallel.map(fun residue ->                       
                                (residue.ResidueName,residue.ResidueNumber),residue.Atoms
                                |>Array.Parallel.map(fun atom -> (determine_effective_radius atom residue.ResidueName)
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
                Expect.equal  (determine_effective_radius testdata.['A'].[0].Atoms.[3] "XYZ") 
                    (1.52 + 1.4) "Atoms with Element O need vdw radius of 1.52" 
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
                    Expect.floatClose Accuracy.low expected.Z actual.Z "Z-componenr is wrong"
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
            }

            test "fibonacci points are sclaed correctly onto the atoms"{
                let fibonacci_example = fibonacciTestPoints 100
                let testdata = getResiduesPerChain "resources/rubisCOActivase.pdb" 1
                let surfacepoints = 
                    testdata
                    |> Seq.map ( fun kvp ->
                        kvp.Key,
                        kvp.Value
                        |>Array.Parallel.map(fun residue ->                       
                                (residue.ResidueName,residue.ResidueNumber),residue.Atoms
                                |>Array.Parallel.map(fun atom -> (scaleFibonacciTestpoints fibonacci_example atom residue.ResidueName)
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
                let testpointsDummy = scaleFibonacciTestpoints fibonacci_example dummyatom testdata.['A'].[0].ResidueName
                Expect.equal testpointsDummy.Length 100 "The number of testpoints 
                    should be 100"
                Expect.floatClose Accuracy.high testpointsDummy.[0].X -2.7781552262181566 
                    "The first testpoint x value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy.[0].Y 51.767 
                    "The first testpoint y value should be equal to the atom position"
                Expect.floatClose Accuracy.high testpointsDummy.[0].Z 13.779599999999999 
                    "The first testpoint z value should be equal to the atom position"
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
                    "The distance has to be computed correctly and should be symetrically"
           
                let example_itsself = euclidianDistance p p
                Expect.equal example_itsself 0.0 "The distance between a point 
                    and itself should be 0"

                let testdata = getResiduesPerChain "resources/rubisCOActivase.pdb" 1
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

                    
                        let allCounts : float[] = accessibleTestpoints allAtomsOfChain totalnr_points
                    
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
                    

                Expect.equal acessiblePointsDictExampleFile.Count  testdata.Count "The number of chains for the acessible ids should be the same"
                Expect.equal (acessiblePointsDictExampleFile.Keys |> Seq.toArray) (testdata.Keys |> Seq.toArray) "The keys of the acessible points should be equal to the keys of the atoms"
                Expect.equal  (Seq.length acessiblePointsDictExampleFile.['A']) (Seq.length testdata.['A' ]) "The number of Residues should be equal to the nr of arrays in the same chain showing the points"

                let testpointExample = round ((44.83712/(4.* System.Math.PI * (3.04**2))) * 100.0)

                Expect.floatClose Accuracy.high (acessiblePointsDictExampleFile.['A']).["ASN",65].[0] testpointExample "The first testpoint should be equal to the value from biopython"


                let residuesHTQ = getResiduesPerChain "resources/htq.pdb" 1
                let acessiblePointsDictHTQ = 
                    residuesHTQ                   
                    |> Seq.map (fun kvp ->
                        let chain    = kvp.Key
                        let residues = kvp.Value

                        // für jede Residue ein Dictionary (ResidueName,ResidueNumber) -> float[] 
                        let perRes =
                            residues
                            |> Array.Parallel.map (fun residue ->
                                // Schlüssel für diese Residue
                                let key = residue.ResidueName, residue.ResidueNumber

                                // Baue das Tuple-Array (Atom * ResidueName)
                                let atomTuples : (Atom*string)[] =
                                    residue.Atoms
                                    |> Array.map (fun atom -> atom, residue.ResidueName)

  
                                let counts : float[] =
                                    accessibleTestpoints atomTuples totalnr_points

                                key, counts
                            )
                            |> dict

                        chain, (perRes :> IDictionary<_,_>)
                    )
                    |> dict

                for key in acessiblePointsDictHTQ.Keys do                    
                    let atomLength = residuesHTQ.[key].Length
                    let acessibleLength = Seq.length acessiblePointsDictHTQ.[key]
                    Expect.equal atomLength acessibleLength $"Number of parsed atoms should be equal to the number of values showing the acessiblepoint."

                let acessiblePointsnr = 
                    [|
                        for key in acessiblePointsDictHTQ.Keys do
                            for res in acessiblePointsDictHTQ.[key].Keys do                              
                                Array.forall (fun atom -> 
                                    atom <= 100.0 && atom >= 0.0                      
                                    ) acessiblePointsDictHTQ.[key].[res]
                    |]

                Expect.allEqual acessiblePointsnr true "number of acessible points should be less or equal to nr of testpoints "
          
            }

            test "SASA per Atom is computed correctly"{

                let testdata = getResiduesPerChain "resources/rubisCOActivase.pdb" 1

                let atomSASAtestdata = sasaAtom "resources/rubisCOActivase.pdb"  1 100

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

                Expect.throws (fun () -> sasaAtom "resources/rubisCOActivase.pdb"  5 100 |>ignore)
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"
                        
            }

            test "Absolute SASA per Residue is computed correctly"{
                let residueSASAtestdata = 
                    sasaResidue ("resources/rubisCOActivase.pdb") 1 100
                let parsedResidues_parser = 
                    getResiduesPerChain "resources/rubisCOActivase.pdb" 1

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

                Expect.throws (fun () -> 
                    sasaResidue "resources/rubisCOActivase"  5 100 |>ignore) 
                    "PDB File with only one model and one Chain should throw an 
                    error if modelid is not present"

                let exampleSASAresiduesHTQ = sasaResidue "resources/htq.pdb" 1 100
                let parsedChainsHTQ = readModels (readPBDFile "resources/htq.pdb")

                Expect.equal exampleSASAresiduesHTQ.Count (parsedChainsHTQ.[0].Chains.Length) 
                    "The number of chains should be equal to the number of chains 
                    in the PDB file for the corresponding model"

                let collectedResidues = 
                    exampleSASAresiduesHTQ.Values 
                    |> Seq.collect (fun x -> x.Values) 

                Expect.all collectedResidues (fun x -> x >= 0.0) 
                    "All SASA values need to be null or positive"
            }

            test "relative Residue SASA is computed correctly"{

                let rel_testdata = 
                    relativeSASA_aminoacids "resources/rubisCOActivase.pdb" 1 100 

                let residueSASAtestdata = 
                    sasaResidue ("resources/rubisCOActivase.pdb") 1 100

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

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/rubisCOActivase.pdb" 5 100 
                    |>ignore) 
                        "PDB File with only one model and one Chain should throw 
                        an error if modelid is not present"


                Expect.equal (rel_testdata.['A'].Count) python_SASA_array_relativeRes.Length 
                    "The number of SASA values should be the same as in the Python reference"

                let difference = rel_testdata.['A'].[65,"ASN"] - sasaArrays_rel.[0]

                Expect.isTrue (difference <= 0.1) 
                    "The SASA value for the first residue should be equal to 
                        the one from the python reference"

                let resNumbersOriginal = 
                    (rel_testdata.['A'].Keys) |>Seq.map (fun (x,y) -> x)

                Expect.equal (resNumbersOriginal |> Seq.toArray) resNumber_rel 
                    "The serialnumbers of the residues should be equal to the 
                    ones from the python computation"

                let exampleAtomsSASA = 
                    rel_testdata.['A'].Values 
                    |> Seq.indexed 
                    |> Seq.toArray

                Array.iter2 (fun (fsharpSeries,fSharpSASA) (_,pythonSASA) ->
                    Expect.isTrue ((fSharpSASA - pythonSASA) <= 0.1) 
                        $"The SASA value for the atom with the serialnumber {fsharpSeries} 
                        should be equal to the one from the python reference"
                ) exampleAtomsSASA python_SASA_array_relativeRes
            
            }

            test "differentiation into exposed and buried is sucessfull"{
            
                let rel_testdata = 
                    relativeSASA_aminoacids "resources/rubisCOActivase.pdb" 1 100

                let differentiatedSASA = 
                    differentiateAccessibleAA "resources/rubisCOActivase.pdb" 1 100 0.2

                let exposed = differentiatedSASA|> Seq.map (fun kvp -> kvp.Key,kvp.Value.Exposed)|>dict
                let buried = differentiatedSASA|> Seq.map (fun kvp -> kvp.Key,kvp.Value.Buried)|>dict

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

                Expect.equal (exposedResidues.Values|>Seq.length) 
                    referenceExposed 
                    "Exposed residues should be the same as in reference"


                let referenceBuried = 
                    sasaArrays_rel
                    |> Array.filter (fun x -> x<0.2)
                    |> Array.length

                Expect.equal (buriedResidues.Values|>Seq.length) 
                    referenceBuried
                    "Buried residues should be the same as in reference"

                let test = exposedResidues.Keys |> Seq.toArray
                let test2 = 
                    python_SASA_array_relativeRes 
                    |> Array.filter (fun (_,value) -> value >=0.2 )
                    |> Array.map ( fun (key,_) -> key)

                Expect.equal test test2 
                    "Exposed residues need to be the same as in reference"

            }

            test "chain SASA is computed correct"{
                
                let testdata_chain = sasaChain "resources/rubisCOActivase.pdb" 1 100 
                let absoluteSASA_rca = sasaResidue "resources/rubisCOActivase.pdb" 1 100

                Seq.iter2 (fun x y -> 
                    Expect.equal x y "The relative SASA chain id should be equal
                    to the absolute SASA chain id"
                ) testdata_chain.Keys absoluteSASA_rca.Keys

                let chainSASA_htq = sasaChain "resources/htq.pdb" 1 100

                Seq.iter (fun x -> 
                      Expect.isGreaterThan x 0.0 
                        "Chain SASA should be greater than 0"
                )chainSASA_htq.Values

                Expect.throws (fun () -> 
                    relativeSASA_aminoacids "resources/htq.pdb" 100 100 |>ignore) 
                        "PDB File with only one model and one Chain should throw 
                        an error if modelid is not present"

           
            

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
            }
       
        ]

