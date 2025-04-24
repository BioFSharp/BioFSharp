namespace BioFSharp.Tests.IO

module CustomBlastResultTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "CustomBlastResult" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]