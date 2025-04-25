namespace BioFSharp.Tests.IO

module PDBTests =

    open BioFSharp.Tests.ReferenceObjects.PDB
    open BioFSharp.FileFormats
    open BioFSharp.IO

    open Expecto

    open BioFSharp

    let IOTests =
        testList "PDB" [
            testCase "ATOM singleton parsing" (fun _ ->
                let actual = PDB.tryParseCoordinateLine atomSingleton
                let expected = 
                    Some (PDB.Coordinate.Atom { 
                        SerialNumber = 1058
                        Name = { 
                            ChemicalSymbol = " N"
                            RemotenessIndicator = " "
                            BranchDesignator = " " 
                        }
                        AlternateLocationIndicator = ""
                        ResidueName = "ARG"
                        ChainIdentifier = "A"
                        ResidueSequenceNumber = 141
                        ResidueInsertionCode = ""
                        X = -6.466
                        Y = 12.036
                        Z = -10.348
                        Occupancy = 7.0
                        TemperatureFactor = 19.11
                        SegmentIdentifier = ""
                        ElementSymbol = "N"
                        Charge = "" 
                    })
                Expect.equal actual expected "PDB.tryParseCoordinateLine did not return the correct ATOM record"
            )

            testCase "ATOM singleton roundtrip" (fun () ->
                let actual =
                    [atomSingleton]
                    |> Seq.choose PDB.tryParseCoordinateLine
                    |> Seq.item 0
                    |> PDB.Coordinate.toString

                Expect.equal actual atomSingleton "atom singleton roundtrip unsuccessful"
            )

            testCase "ATOM sequence roundtrip" (fun () ->
                let actual =
                    atomSequence
                    |> Array.choose PDB.tryParseCoordinateLine
                    |> Array.map PDB.Coordinate.toString

                Expect.sequenceEqual actual atomSequence "atom sequence roundtrip unsuccessful"
            )

            testCase "HETATM singleton parsing" (fun _ ->
                let actual = PDB.tryParseCoordinateLine hetatmSingleton
                let expected = 
                    Some (PDB.Coordinate.HetAtom { 
                        SerialNumber = 1109
                        Name = { 
                            ChemicalSymbol = " C"
                            RemotenessIndicator = "A"
                            BranchDesignator = "D" 
                        }
                        AlternateLocationIndicator = ""
                        ResidueName = "HEM"
                        ChainIdentifier = "A"
                        ResidueSequenceNumber = 1
                        ResidueInsertionCode = ""
                        X = 7.618
                        Y = 5.696
                        Z = -20.432
                        Occupancy = 6.0
                        TemperatureFactor = 21.38
                        SegmentIdentifier = ""
                        ElementSymbol = "C"
                        Charge = "" 
                    })
                Expect.equal actual expected "PDB.tryParseCoordinateLine did not return the correct HETATM record"
            )

            testCase "HETATM singleton roundtrip" (fun () ->
                let actual =
                    [hetatmSingleton]
                    |> Seq.choose PDB.tryParseCoordinateLine
                    |> Seq.item 0
                    |> PDB.Coordinate.toString

                Expect.equal actual hetatmSingleton "atom singleton roundtrip unsuccessful"
            )

            testCase "HETATM sequence roundtrip" (fun () ->
                let actual =
                    hetatmSequence
                    |> Array.choose PDB.tryParseCoordinateLine
                    |> Array.map PDB.Coordinate.toString

                Expect.sequenceEqual actual hetatmSequence "atom sequence roundtrip unsuccessful"
            )

            testCase "Glucagon roundtrip" (fun _ ->
                let actual = 
                    glucagon
                    |> Array.choose PDB.tryParseCoordinateLine
                    |> Array.map PDB.Coordinate.toString

                Expect.equal actual glucagon "glucagon chain roundtrip unsuccessful"
            )

            testCase "HasA roundtrip" (fun _ ->
                let actual = 
                    HasA
                    |> Array.choose PDB.tryParseCoordinateLine
                    |> Array.map PDB.Coordinate.toString

                Expect.equal actual HasA "glucagon chain roundtrip unsuccessful"
            )
        ]

