namespace BioFSharp.Tests.IO

module All =

    open Expecto

    [<Tests>]
    let main = 
        testList "FileFormat IO" [
            FastaTests.IOTests
            FastqTests.IOTests
            AgilentRawTests.IOTests
            BlastQueriesTests.IOTests
            CustomBlastResultTests.IOTests
            ClustalTests.IOTests
            DSSPTests.IOTests
            StrideTests.IOTests
            GAFTests.IOTests
            PDBTests.IOTests
        ]
