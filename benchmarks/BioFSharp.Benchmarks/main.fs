module Main

open BenchmarkDotNet.Running

open BioFSharp.Benchmarks.IO

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<PDBParserBenchmarks.pdbParserBenchmarks>() |> ignore
    0
    
