namespace BioFSharp.Tests.IO

module SOFTTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "SOFT" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]