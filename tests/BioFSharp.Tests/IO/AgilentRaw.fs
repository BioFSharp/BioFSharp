namespace BioFSharp.Tests.IO

module AgilentRawTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    let IOTests =
        testList "AgilentRaw" [
            ptest "read" {
                ()
            }
            ptest "write" {
                ()
            }
        ]