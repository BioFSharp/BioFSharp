namespace BioFSharp.Tests.IO

module OrthoMCLTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "OrthoMCL" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]