namespace BioFSharp.Tests.Algorithm

module All =

    open Expecto

    [<Tests>]
    let main = 
        testList "Algorithms" [
            SASATests.AlgorithmTests
        ]

