namespace BioFSharp.Benchmarks.IO

module PDBParserBenchmarks =
    open BenchmarkDotNet.Attributes
    open BioFSharp.IO.PDBParser


    let testdata = "resources/rubisCOActivase.pdb"
    let htq = "resources/htq.pdb"

    [<MemoryDiagnoser>]
    type readPDBFileBenchmarks() =
               
        [<Benchmark>]
        member this.ReadPDBFileNormal() =
            let lines = readPBDFile testdata
            lines |> Seq.length

        [<Benchmark>]
        member this.ReadPDBFileExtreme() =
            let lines = readPBDFile htq
            lines |> Seq.length

    type readMetadataBenchmarks() =

         [<Benchmark>]
            member this.readMetadataNormal() =
                let lines = readMetadata (readPBDFile testdata)
                lines 

        [<Benchmark>]
        member this.readMetadataExtreme() =
            let lines = readMetadata (readPBDFile htq)
            lines 

    type readAtomBenchmarks() =

        [<Benchmark>]
        member this.readAtomNormal() =
            let lines = readAtom (readPBDFile testdata)
            lines 

        [<Benchmark>]
        member this.readAtomExtreme() =
            let lines = readAtom (readPBDFile htq)
            lines 

    type readResidueBenchmarks() =
       
        [<Benchmark>]
        member this.readResidueNormal() =
            let lines = readResidues (readPBDFile testdata)
            lines 

        [<Benchmark>]
        member this.readResidueExtreme() =
            let lines = readResidues (readPBDFile htq)
            lines 

    type readChainBenchmarks() =

        [<Benchmark>]
        member this.readChainNormal() =
            let lines = readChain (readPBDFile testdata)
            lines 

        [<Benchmark>]
        member this.readChainExtreme() =
            let lines = readChain (readPBDFile htq)
            lines 

    type readLinkageBenchmarks() =

        [<Benchmark>]
        member this.readLinkageNormal() =
            let lines = readLinkages (readPBDFile testdata)
            lines 

        [<Benchmark>]
        member this.readLinkageExtreme() =
            let lines = readLinkages (readPBDFile htq)
            lines 

    type readSiteBenchmarks() =
    
        [<Benchmark>]
        member this.readSiteNormal() =
            let lines = readSite (readPBDFile testdata)
            lines 

        [<Benchmark>]
        member this.readSiteExtreme() =
            let lines = readSite (readPBDFile htq)
            lines 

    type readModelsBenchmarks() =

        [<Benchmark>]
        member this.readModelNormal() =
            let lines = readModels (readPBDFile testdata)
            lines 
      
        [<Benchmark>]
        member this.readModel_extreme() =
            let lines = readModels (readPBDFile htq)
            lines

    type readStructureBenchmarks() =

        [<Benchmark>]
        member this.readStructureNormal() =
            let lines = readStructure  testdata
            lines 

        [<Benchmark>]
        member this.readStructureHigh() =
            let lines = readStructure  htq
            lines 


  

