namespace BioFSharp.Tests.IO

module GenBankTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "GenBank" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]