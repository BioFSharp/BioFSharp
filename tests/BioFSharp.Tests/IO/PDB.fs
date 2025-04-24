namespace BioFSharp.Tests.IO

module PDBTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "PDB" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]