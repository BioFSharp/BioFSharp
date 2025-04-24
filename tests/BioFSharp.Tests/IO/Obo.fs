namespace BioFSharp.Tests.IO

module OboTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "Obo" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]