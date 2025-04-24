namespace BioFSharp.Tests.IO

module StrideTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "Stride" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]