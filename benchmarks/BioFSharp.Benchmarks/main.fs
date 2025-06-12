module Main

open BenchmarkDotNet.Running
open System.Reflection
open BioFSharp.Benchmarks.Config



[<EntryPoint>]
let main argv =
    let config = createConfig argv
    BenchmarkSwitcher
        .FromAssembly(Assembly.GetExecutingAssembly())
        .Run(argv, config)
        |> ignore

    0 // return an integer exit code

