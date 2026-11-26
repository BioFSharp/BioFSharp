module Main

open BenchmarkDotNet.Running

open BioFSharp.Benchmarks.Config

open System.Reflection

[<EntryPoint>]
let main argv =
    let config = createConfig argv
    BenchmarkSwitcher
        .FromAssembly(Assembly.GetExecutingAssembly())
        .Run(argv, config)
        |> ignore

    0 // return an integer exit code
    
