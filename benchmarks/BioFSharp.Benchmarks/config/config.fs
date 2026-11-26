module BioFSharp.Benchmarks.Config

open BenchmarkDotNet.Configs


let createConfig (argv: string[]) =
    let folderName =
        argv
        |> Array.toList
        |> function
            | "--filter" :: value :: _ -> value
            | value :: _ when value.StartsWith("--filter=") -> value.Split('=').[1]
            | _ -> "AllBenchmarks"
        |> fun raw ->
            raw.Replace("*", "").Replace("+", "_").Replace(".", "_").Trim()

    ManualConfig
        .Create(DefaultConfig.Instance)
        .WithArtifactsPath($"BenchmarkDotNet.Artifacts/{folderName}")
        