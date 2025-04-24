namespace BioFSharp.Tests.IO

module NewickTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "Newick" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]