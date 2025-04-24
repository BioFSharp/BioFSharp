namespace BioFSharp.Tests.IO

module DSSPTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "DSSP" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]