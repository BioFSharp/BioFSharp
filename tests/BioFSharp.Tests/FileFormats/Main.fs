namespace BioFSharp.Tests.FileFormats

module All =

    open Expecto

    [<Tests>]
    let main = 
        testList "FileFormat models" [
            FastaTests.fileFormatTests
            FastqTests.fileFormatTests
        ]
