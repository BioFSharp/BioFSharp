module Main

open BenchmarkDotNet.Running

open BioFSharp.Benchmarks.IO
open BioFSharp.Benchmarks.Config

open System.Reflection

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<PDBParserBenchmarks.pdbParserBenchmarks>() |> ignore
    0
    
