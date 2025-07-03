namespace BioFSharp.Algorithm


open BioFSharp.FileFormats.PDBParser
open BioFSharp.IO.PDBParser
open BioFSharp.FileFormats.SASA

open System.Collections
open System
open System.Collections.Generic
open System.Threading.Tasks
open System.IO 


module commonvdWraadi = 
       // Raadi taken from boondii or calculated using Van der waals volumen of literature

    let vdw_raadi =
        Map[
            "H",1.2;
            "N", 1.55;
            "C",1.7;
            "O", 1.52;
            "P", 1.8;
            "S",1.8;
            "Biotin",3.7;
            "Water", 1.4;
            "Methane", 2.0;
            "Ethanol", 2.5;
            "Glycerol", 3.0;
            "Glucose", 4.0;
            "Adenosine", 4.2;
            "ATP",6.0
            "NADH", 6.5

        ]

    let allProtOR  =
        [
            "ALA", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "OXT", 1.46
            ]
            ; "ARG", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD",  1.88; "NE",  1.64; 
                "CZ",  1.61; "NH1", 1.64; "NH2", 1.64; "OXT", 1.46
            ]
            ; "ASN", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61; "OD1", 1.42; "ND2", 1.64; 
                "OXT", 1.46
            ]
            ; "ASP", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61; "OD1", 1.42; "OD2", 1.46; 
                "OXT", 1.46
            ]
            ; "CYS", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "SG",  1.77; "OXT", 1.46
            ]
            ; "GLN", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD",  1.61; "OE1", 1.42; 
                "NE2", 1.64; "OXT", 1.46
            ]
            ; "GLU", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD",  1.61; "OE1", 1.42; 
                "OE2", 1.46; "OXT", 1.46
            ]
            ; "GLY", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "OXT", 1.46
            ]
            ; "HIS", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61; "ND1", 1.64; "CD2", 1.76; 
                "CE1", 1.76; "NE2", 1.64; "OXT", 1.46
            ]
            ; "ILE", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG1", 1.88; "CG2", 1.88; "CD1", 1.88; 
                "OXT", 1.46
            ]
            ; "LEU", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD1", 1.88; "CD2", 1.88; 
                "OXT", 1.46
            ]
            ; "LYS", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD",  1.88; "CE",  1.88; 
                "NZ",  1.64; "OXT", 1.46
            ]
            ; "MET", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "SD",  1.77; "CE",  1.88; 
                "OXT", 1.46
            ]
            ; "PHE", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61; "CD1", 1.76; "CD2", 1.76; 
                "CE1", 1.76; "CE2", 1.76; "CZ",  1.76; "OXT", 1.46
            ]
            ; "PRO", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.88; "CD",  1.88; "OXT", 1.46
            ]
            ; "SER", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "OG",  1.46; "OXT", 1.46
            ]
            ; "THR", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; "CB",  
                1.88; "OG1", 1.46; "CG2", 1.88; "OXT", 1.46
            ]
            ; "TRP", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61; "CD1", 1.76; "CD2", 1.61; 
                "NE1", 1.64; "CE2", 1.61; "CE3", 1.76; "CZ2", 1.76; 
                "CZ3", 1.76; "CH2", 1.76; "OXT", 1.46
            ]
            ; "TYR", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG",  1.61;"CD1", 1.76; "CD2", 1.76; 
                "CE1", 1.76; "CE2", 1.76;"CZ",  1.61; "OH",  1.46; 
                "OXT", 1.46
            ]
            ; "VAL", Map.ofList [
                "N",   1.64; "CA", 1.88; "C",   1.61; "O",   1.42; 
                "CB",  1.88; "CG1", 1.88; "CG2", 1.88; "OXT", 1.46
            ]
        ]
        |> Map.ofList


module SASA =

    open commonvdWraadi

    // module to filter out multiple altlocs and prepare the structure data for 
    //SASA calculation
       
    // extract Residues from the PDB File

    let filterBestAltLoc residuearray = 
        residuearray
        |> Array.map (fun res ->
                    let proteinAtoms =
                        res.Atoms
                        |> Array.filter (fun a -> not a.Hetatm)
                    // dictionary to store for evety atom name the atom with
                    //the best occupancy   
                    let dict = System.Collections.Generic.Dictionary<string, 
                        float * ResizeArray<Atom>>()

                    // look for every residue if it contains atoms with 
                    // multiple atlloc and take the one with the best occupancy 
                    // or AltLoc
                    for a in proteinAtoms do
                        match dict.TryGetValue a.AtomName with
                        | false, _ ->
                            let bestAtomsStore = ResizeArray<Atom>()
                            bestAtomsStore.Add a
                            dict.Add(a.AtomName, (a.Occupancy, bestAtomsStore))
                        | true, (bestOcc, bestAtomsStore) ->
                            if a.Occupancy > bestOcc then
                                let bestAtomsStore2 = ResizeArray<Atom>()
                                bestAtomsStore2.Add a
                                dict.[a.AtomName] <- (a.Occupancy, bestAtomsStore2)
                            elif a.Occupancy = bestOcc && (a.AltLoc = ' ' || 
                                a.AltLoc = 'A') then
                                    bestAtomsStore.Add a
                            else
                                ()

                    // create a new residue with only the best atoms
                    let bestAtoms =
                        dict
                        |> Seq.collect (fun kvp -> snd kvp.Value :> seq<Atom>)
                        |> Seq.toArray

                    { res with Atoms = bestAtoms }
                ) 
                |> Array.filter (fun res -> res.Atoms.Length > 0)

    // isolate residues from the chains in the model and filter out multiple altlocs

    let getResiduesPerChain (filepath:string) (modelid:int) =
        let model = 
            readStructure filepath
            |> fun pdb ->
                pdb.Models
                |> Array.tryFind (fun m -> m.ModelId = modelid)
                |> Option.defaultWith (fun () ->
                    failwithf "PDB File contains no model with modelid = %d." modelid)

        model.Chains
        |>Array.Parallel.map(fun chain -> 
            let filteredResidues = filterBestAltLoc chain.Residues
            chain.ChainId,filteredResidues)
        |>dict

    // collect all atoms from the residues in the model and store it per Chain

    let getAtomsPerModel (filepath: string) ((modelid:int) )  = 
        let collectedAtoms residuearray = 
            Array.Parallel.collect (fun residue -> residue.Atoms)  residuearray
    
        getResiduesPerChain filepath modelid
        |> Seq.map (fun (residue) -> residue.Key,collectedAtoms residue.Value)   
        |> dict  

  
    // match VDW Raddi with atoms in the list :

    
    let determine_effective_radius (atom: Atom) (residueName: string) (probe)  =
           
        // match case function to determine the effective radius of an atom 
        // (first protor, then vdw)
        let baseRadius =
            match Map.tryFind residueName allProtOR
                |> Option.bind (fun resMap -> 
                    Map.tryFind atom.AtomName resMap) with
            | Some r -> r
            | None ->
                let el = atom.Element 
                match Map.tryFind el vdw_raadi with
                | Some br -> br
                // when the element is not in the vdw_raadi map, return 0.0 
                //will be ignored in the SASA calculation
                | None    -> 0.

        let proberadius =
            match probe with
                | ProbeName name ->
                    match Map.tryFind name vdw_raadi with
                    | Some r -> r
                    | None   -> failwith "Unknown probename"
                | ProbeRadius f ->
                    f

        baseRadius + proberadius

    // Helper function to create testpoints over a sphere using 
    // Fibonacci sphere

    let fibonacciTestPoints (nr_point:int): Vector3D array =
        // create nr of test points 
        [| for i in 0 .. (nr_point-1) do
            let z = 1.0 - (float i + 0.5) *( 2.0/float nr_point)
            let radius = sqrt (1.0 - z * z)
            //define golden spiral (approx 2.39RAD or 137.5 degree
            let theta = (System.Math.PI * (3.0 - sqrt 5.0))* float i
            let x = cos theta * radius
            let y = sin theta * radius
            {X=x; Y=y; Z=z}        
        |]

    // Scale atoms on their points by scaling and transformation 
    let scaleFibonacciTestpoints (testPoints:Vector3D array) (atom:Atom) 
        (residuename:string) (probe)  =

        let vdwRadius_eff = determine_effective_radius atom residuename probe

        testPoints 
        |> Array.map (fun tp ->
                { X = atom.Coordinates.X + tp.X * vdwRadius_eff;
                  Y = atom.Coordinates.Y + tp.Y * vdwRadius_eff;
                  Z = atom.Coordinates.Z + tp.Z * vdwRadius_eff; 
                }
        )
 
    // compute the euclidian distance between a Atom and the testpoints 

    let euclidianDistance (testPoint:Vector3D) (atomCentre:Vector3D) =
        let xDistance = (testPoint.X - atomCentre.X)**2
        let yDistance = (testPoint.Y - atomCentre.Y)**2
        let zDistance = (testPoint.Z - atomCentre.Z)**2
        sqrt (xDistance + yDistance + zDistance)

    // create a hash grid for the atoms in the model
    let buildSpatialHash (atomCenters: Vector3D[]) (cellSize: float) =
        // dictionary with cell coordinates as key and atom indices as values
        let hash = Dictionary<int * int * int, ResizeArray<int>>()

        /// compute the cell coordinates for a given atom
        let cellCoordsOf (atompos: Vector3D) =
            let xi = int (floor (atompos.X / cellSize))
            let yi = int (floor (atompos.Y / cellSize))
            let zi = int (floor (atompos.Z / cellSize))
            xi, yi, zi

        // atoms in the model are placed in the grid
        for i in 0 .. atomCenters.Length - 1 do
            let key = cellCoordsOf atomCenters.[i]
            match hash.TryGetValue key with
            | true, list -> list.Add i
            | _           -> hash.[key] <- ResizeArray([| i |])

        hash, cellCoordsOf

    // find for every atom actual all possible other overlapping atoms in 
    // neighbouring cells
    let getOverlappingAtoms 
        (spatialHash: Dictionary<int * int * int, ResizeArray<int>>) 
        (cellCoordsOf: Vector3D -> int * int * int)
        (atomCenters: Vector3D[])(effectiveRadii: float[]) (atomIndex: int) =
            //define Index and centre of actual atom
            let actAtomIndex = atomCenters.[atomIndex]
            let actAtomRadius = effectiveRadii.[atomIndex]
            // define the actual grid cell pf the atom by 
            let xi, yi, zi = cellCoordsOf actAtomIndex
            let offset = 1  // create offset for the neighbouring cells of + -1

            seq {
                // check the neighbouring cells and ans store cells with 
                // potential overlapping atoms
                for dx in -offset .. offset do
                    for dy in -offset .. offset do
                        for dz in -offset .. offset do
                            let neighborKey = (xi + dx, yi + dy, zi + dz)
                            if spatialHash.ContainsKey neighborKey then
                                for otherAtom in spatialHash.[neighborKey] do
                                    // check if the atom is not the same as the
                                    //actual atom AND DEFINE the distance between 
                                    //the atoms --> store potential overlapping atoms
                                    if otherAtom <> atomIndex then
                                        let centerOtherAtom = 
                                            atomCenters.[otherAtom]
                                        let radiusOtherAtom = 
                                            effectiveRadii.[otherAtom]
                                        if euclidianDistance actAtomIndex centerOtherAtom 
                                            < (actAtomRadius + radiusOtherAtom) then
                                                yield otherAtom
            }

    /// Computes the number of visible test points for each atom
    let accessibleTestpoints (allAtoms: (Atom * string)[]) (nr_testpoints: int) (probe) =
        // generate a unit sphere containing the specified number of 
        // testpoints
        let unitSpherePoints = fibonacciTestPoints nr_testpoints

        // extract atom centers and effective VDW radii for all atoms
        let atomCenters = 
            allAtoms 
            |> Array.map fst 
            |> Array.map (fun a -> a.Coordinates)
        let effectiveRadii = 
            allAtoms 
            |> Array.map (fun (a, r) -> determine_effective_radius a r probe)

        // build a spatial hash grid with cell size equal to twice the 
        // maximum radius
        let maxRadius    = effectiveRadii |> Array.max
        let cellSize     = 2.0 * maxRadius
        let spatialHash, cellCoordsOf = buildSpatialHash atomCenters cellSize

        // create an array to store the number of accessible points per atom
        let accessedTestpoints = Array.zeroCreate<float> allAtoms.Length

        // determine the maximum number of threads for parallel processing
        let maxThreads = max 1 (int (float Environment.ProcessorCount * 0.8))
        let opts = ParallelOptions(MaxDegreeOfParallelism = maxThreads)

        // scale and evaluate accessible test points for each atom in parallel
        Parallel.For(0, allAtoms.Length, opts, fun i _ ->
            let (atom, resName) = allAtoms.[i]

            // compute effective radius and scale unit sphere points to the 
            // atom's surface
            let surfacePts = scaleFibonacciTestpoints unitSpherePoints atom resName probe
                
            // initialize visibility array: all points start as visible
            let visible = BitArray(nr_testpoints, true)

            // identify overlapping atoms to reduce collision checks
            let neighbors = 
                getOverlappingAtoms spatialHash cellCoordsOf atomCenters effectiveRadii i

            // hide points buried by neighboring atoms
            for other in neighbors do
                let centerO = atomCenters.[other]
                let rO = effectiveRadii.[other]
                for idx in 0 .. nr_testpoints - 1 do
                    if visible.[idx] && euclidianDistance surfacePts.[idx] 
                        centerO < rO then
                            visible.[idx] <- false

            // count visible points and store result
            let count =
                visible
                |> Seq.cast<bool>
                |> Seq.filter id
                |> Seq.length
            accessedTestpoints.[i] <- float count
        ) |> ignore

        accessedTestpoints

    /// Computes the Solvent Accessible Surface Area (SASA) for each atom in the model

    let sasaAtom (filepath: string) (modelid: int) (totalnr_points: int) (probe) =
        // flatten the residue dictionary into an array of (chainId, Residue) tuples
        let chainResidueList : (char * Residue)[] =
            getResiduesPerChain filepath modelid
            |> Seq.collect (fun kvp ->
                let chainId = kvp.Key
                kvp.Value |> Array.map (fun res -> (chainId, res)))
            |> Seq.toArray

        // create a flat array of (Atom, ResidueName) for all atoms
        let allAtomsWithRes : (Atom * string)[] =
            chainResidueList
            |> Array.collect (fun (_chain, res) -> 
                res.Atoms |> Array.map (fun atom -> atom, res.ResidueName))

        // compute the number of accessible points per atom
        let accessCounts : float[] = accessibleTestpoints allAtomsWithRes totalnr_points probe

        // compute effective VDW radius for each atom 
        // ( Protor or boodii VDW radius + atom)
        let effectiveRadii : float[] =
            allAtomsWithRes
            |> Array.map (fun (atom, resName) -> determine_effective_radius atom resName probe)

        // prepare helper arrays for grouping by residue
        let atomsPerRes : int[] = chainResidueList |> Array.map (fun (_ch, r) -> 
            r.Atoms.Length)

        // starts describes the starting index of each residue in the flat array
        let starts : int[] = Array.scan (+) 0 atomsPerRes

        // compute SASA per residue and per atom: (count/total) * 4πr²
        let sasaPerResidue : float[][] =
            [| for i in 0 .. atomsPerRes.Length - 1 do
                    let len = atomsPerRes.[i]
                    let s   = starts.[i]
                    let nr_acessiblePointsPerResidue = accessCounts.[s .. s + len - 1]
                    let raadiPerResidue  = effectiveRadii.[s .. s + len - 1]
                    yield
                        Array.map2 (fun r nr_acessiblepoints ->
                            let ratio = nr_acessiblepoints / float totalnr_points
                            ratio * 4.0 * System.Math.PI * (r ** 2.0)
                    ) raadiPerResidue nr_acessiblePointsPerResidue
            |]

        // assemble results into a nested dictionary: chain -> (resNum, resName) 
        // -> SASA array
        let chainAtomSASAdict = System.Collections.Generic.Dictionary<char, 
            System.Collections.Generic.Dictionary<int * string, float[]>>()
        for i in 0 .. chainResidueList.Length - 1 do
            let chainId, res = chainResidueList.[i]
            let sasaArr = sasaPerResidue.[i]
            let inner =
                match chainAtomSASAdict.TryGetValue chainId with
                | true, d -> d
                | false, _ ->
                    let d = System.Collections.Generic.Dictionary<int * string, 
                        float[]>()
                    chainAtomSASAdict.Add(chainId, d)
                    d
            inner.Add((res.ResidueNumber, res.ResidueName), sasaArr)

        chainAtomSASAdict

   
    // determine the SASA per residue 

    let sasaResidue (filepath:string) (modelid:int) (nrPoints: int) (probe) =
        sasaAtom filepath modelid nrPoints probe
        |> Seq.map (fun kvp -> 
            kvp.Key, 
            kvp.Value|>Seq.map(fun res -> 
                res.Key,
                res.Value|>Seq.sum)|> dict
        )
        |> dict

       
    // dictionary with maxSASAvalues of proteinogen amino acids 

    let maxSASA (modelid:  int) (nrPoints: int) (probe) =
            
        let rootFolder = Path.Combine(__SOURCE_DIRECTORY__, "../Resources/rsa_tripeptide")

        if not (Directory.Exists rootFolder) then
            failwithf "no reference found: %s" rootFolder
        Directory.EnumerateFiles(rootFolder, "*.pdb", SearchOption.AllDirectories)
        |> Seq.collect (fun filePath ->
           
            let outer: IDictionary<char, IDictionary<(int * string), float>> =
                sasaResidue filePath modelid nrPoints probe

        
            outer.Values
            |> Seq.choose (fun inner ->
                inner
                |> Seq.tryPick (fun kvp ->
                    let (i, name) = kvp.Key
                    if i = 2 then Some (name, kvp.Value)
                    else None)
            )
        )
        |> Map.ofSeq

    let freesasa_Max = 
        Map [
         "ALA",108.76;
         "ARG",238.17;
         "ASN",145.01;
         "ASP",142.76;
         "CYS",132.2;
         "GLN",178.83;
         "GLU",174.18;
         "GLY",81.09;
         "HIS",182.97;
         "ILE",175.73;
         "LEU",179.56;
         "LYS",204.98;
         "MET",193.1;
         "PHE",199.88;
         "PRO",137.21;
         "SER",118.34;
         "THR",140.6;
         "TRP",249.19;
         "TYR",214.19;
         "VAL",151.97   
        ]

           
    // compute the relative SASA for each residue in the model

    let relativeSASA_aminoacids (filepath:string) (modelid:int) (nrPoints: int) (probe) (fixedMaxSASA:bool) = 
        let sasaresidues = sasaResidue filepath modelid nrPoints (probe)
            
        let maxSASA =  
            if fixedMaxSASA = false then 
               maxSASA modelid nrPoints probe
            else 
               freesasa_Max

        sasaresidues
        |> Seq.map (fun kvp -> 
            kvp.Key, 
            kvp.Value |> Seq.filter (fun pair -> 
                Map.containsKey (snd pair.Key) maxSASA) 
                |> Seq.map (fun res  -> 
                    let maxSASAvalue = maxSASA.[snd res.Key]
                    let relativeSASA = (res.Value/ maxSASAvalue) 
                    res.Key,relativeSASA)|> dict
                )
                |> dict

    

    // Split residue List into acessibe and non acessible residues

    
    let differentiateAccessibleAA (filepath: string)(modelId: int) 
        (nrPoints: int) (probe) (threshold: float) (fixedMaxSASA:bool) =

        // get acess to the relative SASA values for the residues in the model 
     
        let chainDict = relativeSASA_aminoacids filepath modelId nrPoints probe fixedMaxSASA

        // create a dictionary with the chainId as Key and as Value another dictionary
    
        let result = Dictionary<_, _>()

        
        for KeyValue(chainId, innerDict) in chainDict do
        
            let exposed    = Dictionary<_, _>()
            let buried = Dictionary<_, _>()

            for KeyValue(resId, value) in innerDict do
                if value > threshold then
                    exposed.Add(resId, value)
                else
                    buried.Add(resId, value)

            result.Add(chainId, {| Exposed = exposed; Buried = buried |})

        result

        // compute the total SASA for each chain in the model

    let sasaChain (filepath:string) (modelid:int) (nrPoints: int) (probe) =
        sasaResidue filepath modelid nrPoints probe
        |> Seq.map (fun kvp -> 
            kvp.Key, 
            kvp.Value.Values |> Seq.sum
        )
        |> dict