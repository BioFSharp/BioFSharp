namespace BioFSharp.Tests.IO

module GAFTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "GAF" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]