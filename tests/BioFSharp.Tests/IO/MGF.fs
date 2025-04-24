namespace BioFSharp.Tests.IO

module MGFTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "MGF" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]