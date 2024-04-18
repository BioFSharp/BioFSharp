module DocumentationTasks

open Helpers
open ProjectInfo
open BasicTasks

open BlackFox.Fake

let buildDocs =
    BuildTask.create "BuildDocs" [ buildSolution ] {
        printfn "building docs with stable version %s" stableVersionTag

        runDotNet
            (sprintf
                "fsdocs build --clean --properties Configuration=Release --parameters fsdocs-package-version %s"
                stableVersionTag)
            "./"
    }

let buildDocsPrerelease =
    BuildTask.create "BuildDocsPrerelease" [ setPrereleaseTag; buildSolution ] {
        printfn "building docs with prerelease version %s" prereleaseTag

        runDotNet
            (sprintf
                "fsdocs build --clean --properties Configuration=Release --parameters fsdocs-package-version %s"
                prereleaseTag)
            "./"
    }

let watchDocs =
    BuildTask.create "WatchDocs" [ buildSolution ] {
        printfn "watching docs with stable version %s" stableVersionTag

        runDotNet
            (sprintf
                "fsdocs watch --clean --properties Configuration=Release --parameters fsdocs-package-version %s"
                stableVersionTag)
            "./"
    }

let watchDocsPrerelease =
    BuildTask.create "WatchDocsPrerelease" [ setPrereleaseTag; buildSolution ] {
        printfn "watching docs with prerelease version %s" prereleaseTag

        runDotNet
            (sprintf
                "fsdocs watch --clean --properties Configuration=Release --parameters fsdocs-package-version %s"
                prereleaseTag)
            "./"
    }