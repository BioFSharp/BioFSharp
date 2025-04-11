module FileFormatTests

open Expecto

[<Tests>]
let main = 
    testList "FileFormats and IO" [
        // Fasta
        FastaTests.fastaTests
    ]
