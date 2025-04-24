namespace BioFSharp.Tests.IO

module GFF3Tests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "GFF3" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]