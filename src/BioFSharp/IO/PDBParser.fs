namespace BioFSharp.IO

open BioFSharp.FileFormats.PDBParser

open System
open System.IO
open System.Collections.Generic

module PDBParser =

    // Read all elements linewise
    let readPBDFile (filePath: string): seq<string> = 
            File.ReadLines (filePath)

    // Parser for Metadata information --> store in a type metadata with 
    // general informations about the entry
    let readMetadata (lines: seq<string>) : Metadata =

        // create dictionary to store lines per keyword (HEADER,TITLE,...) 
        let metadataDict = Dictionary<string, ResizeArray<string>>()

        // fill dictionary with content by iterating over all lines once

        for line in lines do
            // keyword of dictionary is the prefix which is allways in the first six characters 
            if line.Length >= 6 then
                let prefix = line.Substring(0, 6)
                // trim the content of this lines which starts in metadata fields at column 10
                let content =
                    if line.Length > 10 then
                        line.Substring(10).Trim()
                    else
                        ""

                // assign content to the corresponding keywords you wanted
                match prefix with
                | "HEADER" ->
                    // HEADER is always only one line - assign content to Header (string)
                    metadataDict.["HEADER"] <- ResizeArray([ content ])

                | "TITLE " ->
                    // TITLE is also always one line (string) store content herer
                    metadataDict.["TITLE "] <- ResizeArray([ content ])

                //  "COMPND", "SOURCE", "EXPDTA", "AUTHOR", "REMARK", "CAVEAT" can be part in more than one line
                // Sdd Content to the corresponding entry in the dictionary 
                | "COMPND"
                | "SOURCE"
                | "EXPDTA"
                | "AUTHOR"
                | "REMARK"
                | "CAVEAT" ->
                    // If prefix is part of dictionary add content, if not create entry
                    if not (metadataDict.ContainsKey(prefix)) then
                        metadataDict.[prefix] <- ResizeArray()
                    metadataDict.[prefix].Add(content)

                // KEYWDS are seperated by ,
                | "KEYWDS" ->
                    // add Keywds that can contain 1 ..n words to the dictionary 
                    if not (metadataDict.ContainsKey("KEYWDS")) then
                        metadataDict.["KEYWDS"] <- ResizeArray()
                    let keywords =
                        content.Split([|','|])
                        // remove empty spaces from splitted content
                        |> Array.map (fun k -> k.Trim())
                    // add all keywords 
                    metadataDict.["KEYWDS"].AddRange(keywords)

                | _ -> 
                    // ignore other prefixes 
                    ()

       
        // helper functions to get content of dictionary

        // get content of the entrys that contain exacty one part e.g. Header 
        let getSingleItem (key: string) =
            if metadataDict.ContainsKey(key) && metadataDict.[key].Count > 0 then
                metadataDict.[key].[0]
            else
                ""

        // get a list for the fields that contain more than one line e.g.REMARK 
        let getList (key: string) =
            if metadataDict.ContainsKey(key) then
                metadataDict.[key] |> Seq.toList
            else
                []

        // get optional lists or none if not part pf PDB file 
        let getListOption (key: string) =
            let items = getList key
            match items with
            | [] -> None
            | _  -> Some items


        {           
            Header   = getSingleItem "HEADER"
            Title    = getSingleItem "TITLE "           
            Compound = getListOption "COMPND"
            Source   = getListOption "SOURCE"
            ExpData  = getListOption "EXPDTA"
            Authors  = getListOption "AUTHOR"       
            Keywords = getListOption "KEYWDS"            
            Remarks  = getList "REMARK"           
            Caveats  = getListOption "CAVEAT"
        }



     // Helper function to parse Integers and Floats from a string slice and 
     // extract the informations e.g. Serialnumber
    let parseIntSpan (line: string) (startIdx: int) (length: int) (defaultVal: int) =
        // extract a part of the string and remove leading and trailing whitespaces
        let slice = line.Substring(startIdx, length).Trim()
        let mutable i = 0
        // try to parse the string to an integer; if not possible return the 
        //default value 0
        if Int32.TryParse(slice, &i) then i else defaultVal

    let parseFloatSpan (line: string) (startIdx: int) (length: int) (defaultVal: float) =
        let slice = line.Substring(startIdx, length).Trim()
        let mutable v = 0.0
        if Double.TryParse(slice, &v) then v else defaultVal

    // Function to parse the Atom informations from a PDB file    
    let readAtom (lines : seq<string>) : Atom array =

        // extract all lines that start with ATOM or HETATM
        let atomLines =
            lines
            |> Seq.filter (fun line ->
                (line.StartsWith("ATOM  ") || line.StartsWith("HETATM"))
            )
            |> Seq.toArray

        // iterate over all lines in parallel and parse the informations 
        // describing the fields of type Atom
        atomLines
        |> Array.Parallel.map (fun line ->
            try
                let serialNumber = parseIntSpan line 6 5 0
                // extract atomname at position 12 with length 4 and convert 
                // to string
                let atomName     = line.Substring(12, 4).Trim().ToString()

                let altLoc       = line.[16]
                let x          = parseFloatSpan line 30 8 0.0
                let y          = parseFloatSpan line 38 8 0.0
                let z          = parseFloatSpan line 46 8 0.0
                let occupancy  = parseFloatSpan line 54 6 0.0
                let tempFactor = parseFloatSpan line 60 6 0.0

                // extract the  optional segment identifier
                let segId =
                    if line.Length >= 76 then
                        let v = line.Substring(72, 4).Trim()
                        if v = "" then None else Some (v.ToString())
                    else
                        None

                // extract the optional element
                let element =
                    if line.Length >= 78 then
                        line.Substring(76, 2).Trim().ToString()
                    else
                        ""

                // extract the optional charge
                let charge =
                    if line.Length >= 80 then
                        let c = line.Substring(78, 2).Trim()
                        if c = "" then None else Some(c.ToString())
                    else
                        None

                let hetatm = line.StartsWith("HETATM")

                // return the parsed informations as a record of type Atom
                {
                    SerialNumber = serialNumber
                    AtomName     = atomName
                    AltLoc       = altLoc
                    Coordinates  = { X = x; Y = y; Z = z }
                    Occupancy    = occupancy
                    TempFactor   = tempFactor
                    SegId        = segId
                    Element      = element
                    Charge       = charge
                    Hetatm       = hetatm
                }
            with _ ->
                // if an error occurs during parsing, return a default value
                {
                    SerialNumber = 0
                    AtomName     = ""
                    AltLoc       = ' '
                    Coordinates  = { X = 0.0; Y = 0.0; Z = 0.0 }
                    Occupancy    = 0.0
                    TempFactor   = 0.0
                    SegId        = None
                    Element      = ""
                    Charge       = None
                    Hetatm       = false
                }
        )



    // Function to parse the Secondary Structure information from a PDB file
    let readSecstructure (lines: seq<string>): SSType array =
        lines
        |> Seq.choose (fun line ->
            // create a sequence that stores all lines that start with HELIX or SHEET 
            // as well as the txpe of the line directly
            if line.StartsWith("HELIX") then
                Some (line, "Helix")
            elif line.StartsWith("SHEET") then
                Some (line, "Sheet")
            else
                None
        )
        // define start and end of the corresponding secundary structures paralell
        |> Seq.toArray
        |> Array.Parallel.map (fun (line, secType) ->
            // trim the startresidue and endresidue directly from the HELIX or SHEET line
            let startResSpan = 
                if secType = "Helix" then 
                    line.Substring(21,4) 
                else 
                    line.Substring(22,4)

            // convert the trimmed span  to an integer 
            let startResTrimmed = startResSpan.Trim()
            let mutable startresidue = 0
            if not (Int32.TryParse(startResTrimmed, &startresidue)) then startresidue <- 0

            let endResSpan = line.Substring(33,4).Trim()
            let mutable endresidue = 0
            if not (Int32.TryParse(endResSpan, &endresidue)) then endresidue <- 0

            {
                SecstructureType = secType
                Startresidue = startresidue
                Endresidue = endresidue
            }
        )

    // Function to parse which modifications are present on which residue 
    // in the PDB file
    let readModifications (lines: seq<string>): Modification array =
        // create a sequence that stores all lines that store modifications 
        // in the proteins (MODRES
        let modLines =
            lines
            |> Seq.filter (fun line -> line.StartsWith("MODRES"))
            |> Seq.toArray

        // define the number of the modified residue and the type of 
        // modification in parallel
        modLines
        |> Array.Parallel.map (fun line ->
            // trim ModifiedResidueNumber that is stored in Column 18..21
            let numberSpan = line.Substring(18, 4).Trim()
            let mutable n = 0
            if not (Int32.TryParse(numberSpan, &n)) then
                n <- 0

            // trim modificationtype starting at Column 29 and 
            // if its shorter than empty
            let modType = 
                if line.Length > 29 then 
                    line.Substring(29).Trim() 
                else 
                    " "

            {
                ModifiedResidueNumber = n
                ModificationType = 
                    if modType = "" then "" 
                    else modType.ToString()
            }
        )

    // Residueparser that groups the Atoms by the residuenumber and the 
    //residuename as well as the optional insertioncode
    let readResidues (lines: seq<string>) : Residue array =
    
        // read in all secondary structures and modifications present in the biomolecule
        let secStrucs = readSecstructure lines
        let mods = readModifications lines


        // grouping lines / atoms by creating a dictionary with the key (ResidueName, ResidueNumber, InsertionCode)
        let dict = Dictionary<(string * int * char option), ResizeArray<string>>()

        for line in lines do
            if line.StartsWith("ATOM") || line.StartsWith("HETATM") then
            
                // extract residuename, residuenumber and 
                let residueName = line.Substring(17, 3).Trim().ToString()

                // extract residuenumber from the line and convert it to an integer
                let rNumSpan = line.Substring(22, 4).Trim()
                let mutable rNum = 0
                if not (Int32.TryParse(rNumSpan, &rNum)) then
                    rNum <- 0

                // extract the optional insertion code
                let insertionCode =
                    if line.Length > 26 && line.[26] <> ' ' then 
                        Some line.[26] 
                    else 
                        None

                // define extracted infos as key --> grouping
                let key = (residueName, rNum, insertionCode)

                // create dictionary key if not present with a empty resizeArray
                if not (dict.ContainsKey(key)) then
                    dict.Add(key, ResizeArray<_>())

                // add line to the corresponding key
                dict.[key].Add(line)

        // convert the dictionary to an array of tuples containing the key (groups) and and the  content (lines (atoms))
        let groupedResidues =
            dict
            |> Seq.map (fun kvp ->
                let key = kvp.Key // (resName, rNum, iCode)
                let linesForResidue = kvp.Value :> seq<string>  // convert ResizeArray to seq
                (key, linesForResidue)
            )
            |> Seq.toArray

        // parse modifications, secondary structures and atoms for each residue group
        
        groupedResidues
        |> Array.Parallel.map (fun ((resName, rNum, iCode), linesForResidue) ->
           
            let atoms = readAtom linesForResidue

            // only secundary structures that are present in the residue group
            let secType =
                secStrucs
                |> Array.tryFind (fun ss -> rNum >= ss.Startresidue && rNum <= ss.Endresidue)
                |> Option.map (fun ss -> ss.SecstructureType)

            // only modifications that are present in the residue group
            let modificationType =
                if Array.isEmpty mods then None
                else
                    mods
                    |> Array.tryFind (fun m -> m.ModifiedResidueNumber = rNum)
                    |> Option.map (fun m -> m.ModificationType)

            // create residue object
            {
                ResidueName      = resName
                ResidueNumber    = rNum
                InsertionCode    = iCode
                SecStructureType = secType
                Modification     = modificationType
                Atoms            = atoms
            }
        )



    // grouping all residues by the correspondance to an unique chain 
    let readChain (lines: seq<string>) : Chain array =
    
        // grouping ATOM and HETATM lines by chainId as key in a dictionary
        let dict = Dictionary<char, ResizeArray<string>>()
    
        for line in lines do
            if line.StartsWith("ATOM") || line.StartsWith("HETATM") then
                // extract chainId from Column 21 being exactly one character
                let chainId = line.[21]
                // add chainId as key to dictionary if not already present
                if not (dict.ContainsKey(chainId)) then
                    dict.Add(chainId, ResizeArray())
                // add line to the corresponding chainId
                dict.[chainId].Add(line)
    
        // convert dictionary to array of tuples (key, value) for parallel processing
        dict
        |> Seq.toArray
        |> Array.Parallel.map (fun kvp ->
            let chainId = kvp.Key
            let chainLines = kvp.Value :> seq<string>
            let residues = readResidues chainLines
            { ChainId = chainId; Residues = residues }
        )


    // Function to parse informations how the atoms are connected to each other
    let readLinkages (lines: seq<string>) : Linkages array =
        // Prepare Lookup Tables for atoms and residues  present in the 
        // corresponding Chains

        // parse chains from the pdb file
        let parsedChains = readChain lines

        // create a map of residues with the key (ChainId, ResidueNumber) to
        // get corresponding modelblockess to the atoms in the residues
        let residueMap =
            parsedChains
            |> Seq.collect (fun chain ->
                chain.Residues
                |> Seq.map (fun residue -> 
                    (chain.ChainId, residue.ResidueNumber), residue)
            )
            |> Map.ofSeq

        // create lookup dictionary storing the atoms by their serial number 
        // --> get easy acess in Connnect parsing
        let atomMap =
            let dict = Dictionary<int, Atom>()
            parsedChains
            |> Seq.collect (fun chain -> 
                chain.Residues 
                |> Seq.collect (fun r -> 
                    r.Atoms)
            )
            |> Seq.iter (fun atom ->
                dict.[atom.SerialNumber] <- atom
            )
            dict

        // helper functions 
        // try to get an atom from the dictionary by the serial number
        let tryGetAtom (dict: Dictionary<int, Atom>) (serial: int) : Atom option =
            // mutable atom to get the standard value of atom (empty atom
            let mutable atom = Unchecked.defaultof<Atom>
            // look for atom with corresponding serial number and store it in 
            // the mutable value atom
            if dict.TryGetValue(serial, &atom) then 
                Some atom 
            else None

        // helper function to get the residue in the map with the key 
        // (ChainId, ResidueNumber), 
        //if not found return an empty list and a warning
        let findAtoms chainId residueNum =
            match Map.tryFind (chainId, residueNum) residueMap with
            | Some residue -> residue.Atoms
            | None ->
                printfn "Warning: Residue not found for Chain %c and 
                        ResidueNumber %d" chainId residueNum
                [||]

        // create the new Linkage record of the corresponding Linktype 
        let createLinkage linkType atoms1 atoms2 =
            { 
            Linktype = linkType; 
            Atoms1 = atoms1; 
            Atoms2 = atoms2 }

        // create a dictionary to group all Linkages by the Linktype
        let linkageDict = Dictionary<string, Linkages list>()

        // helperfunction to add a linkage to the dictionary to the 
        // corresponding key
        let addLinkage linkage =
            if linkageDict.ContainsKey(linkage.Linktype) then
                linkageDict.[linkage.Linktype] <- linkage :: linkageDict.[linkage.Linktype]
            else
                linkageDict.[linkage.Linktype] <- [ linkage ]

        // iterate over the lines and parse the Linkages of the pdbfile 
        lines
        |> Seq.iter (fun line ->
            if line.Length >= 6 then
                // extract the first 6 characters to determine the record type
                let record =
                    if line.Length >= 6 then
                        line.Substring(0, 6).Trim()
                    else
                        ""

                match record with
                | "SSBOND" ->
                    // parse chain IDs and residue numbers for SSBOND
                    let chain1 = if line.Length > 15 then line.[15] else ' '
                    let residue1 =
                        if line.Length >= 21 then
                            line.Substring(17, 4).Trim() |> int
                        else
                            0
                    let chain2 = if line.Length > 29 then line.[29] else ' '
                    let residue2 =
                        if line.Length >= 35 then
                            line.Substring(31, 4).Trim() |> int
                        else
                            0

                    let atoms1 = findAtoms chain1 residue1
                    let atoms2 = findAtoms chain2 residue2
                    addLinkage (createLinkage "SS Bond" atoms1 atoms2)

                | "LINK" ->
                    // parse chain IDs and residue numbers for LINK  
                    let chain1 = if line.Length > 21 then line.[21] else ' '
                    let residue1 =
                        if line.Length >= 26 then
                            line.Substring(22, 4).Trim() |> int
                        else
                            0
                    let chain2 = if line.Length > 51 then line.[51] else ' '
                    let residue2 =
                        if line.Length >= 56 then
                            line.Substring(52, 4).Trim() |> int
                        else
                            0

                    let atoms1 = findAtoms chain1 residue1
                    let atoms2 = findAtoms chain2 residue2
                    let linkage = createLinkage "covalent Link" atoms1 atoms2
                    addLinkage linkage

                | "CISPEP" ->
                    // parse chain IDs and residue numbers for CISPEP
                    let chain1 = if line.Length > 15 then line.[15] else ' '
                    let residue1 =
                        if line.Length >= 21 then
                            line.Substring(17, 4).Trim() |> int
                        else
                            0
                    let chain2 = if line.Length > 29 then line.[29] else ' '
                    let residue2 =
                        if line.Length >= 33 then
                            line.Substring(31, 4).Trim() |> int
                        else
                            0

                    let atoms1 = findAtoms chain1 residue1
                    let atoms2 = findAtoms chain2 residue2
                    addLinkage (createLinkage "Cis Peptide" atoms1 atoms2)

                | "CONECT" ->
                    // parse atom connections for CONECT
                    let atomNumber1 =
                        if line.Length >= 11 then
                            match line.Substring(6, 5).Trim() |> System.Int32.TryParse with
                            | (true, num) -> num
                            | _ -> 0
                        else
                            0

                    let connectedAtomsPositions = [11; 16; 21; 26]
                    for pos in connectedAtomsPositions do
                        // boundary check to proof if position has at least 
                        // 5 characters
                        if line.Length >= pos + 5 then
                            let atomNumStr = line.Substring(pos, 5).Trim()
                            // convert atomNr into int if not empty
                            if atomNumStr <> "" then
                                match System.Int32.TryParse(atomNumStr) with
                                | (true, atomNumber2) ->
                                    // try to get the atoms from the dictionary 
                                    // and add them to the linkage
                                    match tryGetAtom atomMap atomNumber1, 
                                        tryGetAtom atomMap atomNumber2 with
                                    | Some a1, Some a2 ->
                                        addLinkage (createLinkage "Connect" [| a1 |] [| a2 |])
                                    | _ -> ()
                                | _ -> ()
                | _ -> ()
        
        )

        // grouped Linkages are converted to a list and returned
        linkageDict.Values
        |> Seq.concat
        |> Seq.toArray

    // Parse Site informations part of the special case, representing residues 
    // comprising functional sides e.g. catalytic, cofactor  
    let readSite (lines: seq<string>): SiteType array =
        // Parses a residue from a given position in the SITE record and 
        // extract chain ID and residue number.
        
        let parseResidue (line: string) (pos: int) : (char * int) option =
             // Validates that the extracted values are correctly formatted 
             // and long enough .     
            if line.Length < pos + 6 || 
                String.IsNullOrWhiteSpace(line.Substring(pos, 6)) then
                    None  // If residue field is missing, return None

            else
                let chainId = line.[pos]
                let residueStr = line.Substring(pos + 1, 4).Trim()

                if not (System.Char.IsLetterOrDigit(chainId)) then
                    failwithf "Error: Chain ID '%c' at position %d is not a 
                        valid letter or digit in line: %s" chainId pos line

                // parse residue number
                match System.Int32.TryParse(residueStr) with
                | (true, value) -> Some (chainId, value) // Return valid tuple
                | _ -> failwithf "Error: Residue number '%s' at position %d is 
                    not a valid integer in line: %s" residueStr pos line

        // Create a dictionary to store site names and their residues
        let siteDict = System.Collections.Generic.Dictionary<string, 
            System.Collections.Generic.List<(char * int)>>()

        // Process each line and extract SITE record information
        lines
        |> Seq.iter (fun line ->
            if line.StartsWith("SITE") then
                let siteName = line.Substring(11, 3).Trim()

                let residuePositions = [| 22; 33; 44; 55 |]  

                let residues =
                    residuePositions
                    |> Array.choose (parseResidue line)
                    |> Array.toList

                if List.isEmpty residues then
                    failwithf "Error: SITE entry '%s' has no valid residues 
                        in line: %s" siteName line

                match siteDict.TryGetValue(siteName) with
                | true, existingResidues -> existingResidues.AddRange(residues)
                | false, _ -> 
                    siteDict.[siteName] <- 
                        System.Collections.Generic.List<(char * int)>(residues)

        )

        // Convert the dictionary to a list of SiteType records
        [| for KeyValue(site, residues) in siteDict -> 
            { Sitename = site; Residues = Array.ofSeq residues } |]

    // Helper: Split the PDB file lines (as an array) into model blocks 
    //(each block is an array of lines) This function assumes that each model
    // starts with a "MODEL " record and ends with "ENDMDL".
    let collectModelBlocks (lines: string[]) : string[][] =
        // Use a ResizeArray to collect model blocks dynamically
        let modelBlocks = ResizeArray<string[]>()
        let currentBlock = ResizeArray<string>()

        for line in lines do
            if line.Length >= 6 then
                // extract the first 6 characters to determine the record type
                let recordType =
                    if line.Length >= 6 then
                        line.Substring(0, 6).Trim()
                    else
                        ""
                
                match recordType with
                | "MODEL" ->
                    // If there's already a block in progress, add it before 
                    //starting a new one
                    if currentBlock.Count > 0 then
                        modelBlocks.Add(currentBlock.ToArray())
                        currentBlock.Clear()
                    currentBlock.Add(line)

                | "ENDMDL" ->
                    currentBlock.Add(line)
                    // Model block is completed – add it and clear currentBlock
                    modelBlocks.Add(currentBlock.ToArray())
                    currentBlock.Clear()

                | "ATOM" | "HETATM" ->
                    currentBlock.Add(line)

                | _ ->
                    () // Ignore other record types

            else
                ()

        // In case some lines remain without a closing ENDMDL record, 
        // add them as a block
        if currentBlock.Count > 0 then
            modelBlocks.Add(currentBlock.ToArray())
            currentBlock.Clear()

        modelBlocks.ToArray()

    // parse different models from the PDB file
    let readModels (lines: seq<string>) : Model array =
        // Convert all input lines to an array for fast, indexed access.
        let pdbLineArray = Seq.toArray lines

        // Collect model blocks as an array of string arrays.
        let modelBlocks = collectModelBlocks pdbLineArray

        // Parse global residues as an array.
        let allResidues : Residue array = readResidues pdbLineArray

        // Build a dictionary for fast lookup of global residues.
        let allResiduesDict =
            let dict = Dictionary<(string * int), Residue>()
            allResidues |> Array.iter (fun r ->
                dict.[(r.ResidueName, r.ResidueNumber)] <- r )
            dict

        // Parse global linkages and sites and convert them into arrays.
        let allLinkages = readLinkages pdbLineArray 
        let allSites    = readSite pdbLineArray 

        // Process each model block in parallel.
        modelBlocks
        |> Array.Parallel.mapi (fun idx modelLines ->
            // Determine the model id: if a "MODEL" record exists, extract its 
            // ID; otherwise, use idx+1.
            let modelId =
                match modelLines |> Array.tryFind (fun l -> 
                    l.StartsWith "MODEL") with
                | Some modelLine ->
                    // substring from index 5 to end, then trim and parse
                    let numStr = 
                        if modelLine.Length >= 6 then
                            modelLine.Substring(6).Trim()
                        else
                            ""
                    match Int32.TryParse(numStr) with
                    | (true, num) -> num
                    | _ -> idx + 1
                | None -> idx + 1

            // Parse chains from the current model block.
            // Assume parseChain accepts an array of lines and returns a list;
            // we then convert the result to an array.
            let chains : Chain array = readChain modelLines 

            // Update each chain's residues using the global dictionary.
            let chainsUpdated =
                chains
                |> Array.map (fun chain ->
                    // Assume chain.Residues is already an array of Residue.
                    let residuesArray = chain.Residues
                    // Process residues imperatively.
                    for i in 0 .. residuesArray.Length - 1 do
                        let residue = residuesArray.[i]
                        // Use the precomputed key if available; otherwise, 
                        // construct the tuple.
                        let key = (residue.ResidueName, residue.ResidueNumber)
                        let mutable globalResidue = Unchecked.defaultof<Residue>
                        if allResiduesDict.TryGetValue(key, &globalResidue) then
                            // First check: if the Atoms arrays are 
                            // reference-equal, simply update.
                            if obj.ReferenceEquals(globalResidue.Atoms, 
                                residue.Atoms) then
                                residuesArray.[i] <- globalResidue
                            // Else, if they differ, update only the Atoms field.
                            elif globalResidue.Atoms <> residue.Atoms then
                                residuesArray.[i] <- 
                                { globalResidue with Atoms = residue.Atoms }
                            else
                                residuesArray.[i] <- globalResidue
                        else
                            residuesArray.[i] <- residue
                    { chain with Residues = residuesArray }
                )

            // Build a set of atom serial numbers from all chains for filtering.
            let atomSerialNumbersInChains =
                chainsUpdated
                |> Array.collect (fun chain ->
                    chain.Residues |> Array.collect (fun r -> r.Atoms))
                |> Array.map (fun atom -> atom.SerialNumber)
                |> Set.ofArray

            // Filter global linkages: retain only those where both Atom lists 
            //contain at least one atom present in the current model.
            let linkagesFiltered =
                allLinkages
                |> Array.filter (fun link ->
                    (link.Atoms1 |> Array.exists (fun atom -> 
                        atomSerialNumbersInChains.Contains atom.SerialNumber))
                    && (link.Atoms2 |> Array.exists (fun atom -> 
                        atomSerialNumbersInChains.Contains atom.SerialNumber))
                )

            // Filter global sites by checking if at least one residue 
            // (by chain id and residue number) exists in the current model.
            let sitesFiltered =
                allSites
                |> Array.filter (fun site ->
                    site.Residues
                    |> Array.exists (fun (chainId, residueNum) ->
                        chainsUpdated |> Array.exists (fun chain ->
                            chain.ChainId = chainId &&
                            (chain.Residues |> Array.exists (fun residue -> 
                                residue.ResidueNumber = residueNum))
                        )
                    )
                )

            { ModelId   = modelId
              Chains    = chainsUpdated
              Linkages  = linkagesFiltered
              Sites     = sitesFiltered }
        )



    // Function to parse and summarize the whole information in a PDB file
    let readStructure (filepath:string) :Structure =

            //  type sumarizing all relevant informations present in the PDB File


            let pdbfile = readPBDFile filepath

            let metadata = readMetadata pdbfile

            let model = readModels pdbfile

            {
                Metadata = metadata;
                Models = model
            }