module BasicTasks

open BlackFox.Fake
open Fake.IO
open Fake.DotNet
open Fake.IO.Globbing.Operators

open ProjectInfo

let setPrereleaseTag = BuildTask.create "SetPrereleaseTag" [] {
    printfn "Please enter pre-release package suffix"
    let suffix = System.Console.ReadLine()
    prereleaseSuffix <- suffix
    prereleaseTag <- (sprintf "%s-%s" release.NugetVersion suffix)
    isPrerelease <- true
}

let clean = BuildTask.create "Clean" [] {
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    ++ "pkg"
    |> Shell.cleanDirs 
}

/// builds the solution file (dotnet build solution.sln)
let buildSolution =
    BuildTask.create "BuildSolution" [ clean ] { 
        solutionFile 
        |> DotNet.build (fun p ->
            let msBuildParams =
                {p.MSBuildParams with 
                    Properties = ([
                        "warnon", "3390"
                    ])
                    DisableInternalBinLog = true
                }
            {
                p with 
                    MSBuildParams = msBuildParams
                    
            }
            |> DotNet.Options.withCustomParams (Some "-tl")
        )
    }