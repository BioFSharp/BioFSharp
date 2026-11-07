namespace BioFSharp.Tests.Core

module StructureTests =

    open BioFSharp
    open BioFSharp.FileFormats
    open BioFSharp.IO
    open BioFSharp.Tests.ReferenceObjects.PDB
    open Expecto

    let parse (lines: string []) =
        lines
        |> Array.choose PDB.tryParseCoordinateLine
        |> Structure.ofCoordinates

    let roundtrip (lines: string []) =
        lines
        |> parse
        |> Structure.toCoordinates
        |> Array.map PDB.Coordinate.toString
   
    let structureTests =
        testList "Structure" [
            testCase "HasA hierarchy" (fun () ->
                let actual = parse HasA
                let residues = actual.Models.[0].Chains.[0].Residues
                Expect.equal actual.Models.Length 1 "HasA should have one model"
                Expect.equal (actual.Models.[0].Chains |> Array.map (fun c -> c.Identifier)) [|"A"|] "HasA should have one chain A"
                Expect.equal residues.Length 223 "HasA should have 223 residues"
                Expect.equal (residues |> Array.filter (fun r -> r.IsHetero) |> Array.length) 68 "HasA should have 68 hetero residues"
                Expect.equal (residues |> Array.sumBy (fun r -> r.Atoms.Length)) 1291 "HasA should have 1291 atoms"
            )

            testCase "HasA first residue" (fun () ->
                let actual = (parse HasA).Models.[0].Chains.[0].Residues.[0]
                Expect.equal actual.Name "MET" "First residue should be MET"
                Expect.equal actual.SequenceNumber 1 "First residue should have sequence number 1"
                Expect.equal actual.Atoms.Length 8 "MET 1 should have 8 atoms"
                Expect.isFalse actual.IsHetero "MET 1 should not be hetero"
            )

            testCase "Without MODEL records" (fun () ->
                let actual = parse chainSequence
                Expect.equal (actual.Models |> Array.map (fun m -> m.Number)) [|1|] "Coordinates without MODEL should be model 1"
            )

            testCase "Chains" (fun () ->
                let actual =
                    (parse chainSequence).Models.[0].Chains
                    |> Array.map (fun c -> c.Identifier, c.Residues |> Array.map (fun r -> r.Name, r.SequenceNumber, r.IsHetero))
                let expected =
                    [|
                        "A", [| "ALA", 1, false; "GLY", 2, false; "HOH", 101, true |]
                        "B", [| "SER", 1, false |]
                    |]
                Expect.equal actual expected "Atoms should be grouped by chain, also when a chain continues after another chain"
            )

            testCase "Models" (fun () ->
                let actual =
                    (parse modelSequence).Models
                    |> Array.map (fun m -> m.Number, m.Chains.[0].Residues.[0].Atoms.[0].X)
                Expect.equal actual [| 1, 11.104; 2, 12.104 |] "Each MODEL should be its own model"
            )

            testCase "Insertion codes" (fun () ->
                let actual =
                    (parse insertionCodeSequence).Models.[0].Chains.[0].Residues
                    |> Array.map (fun r -> r.SequenceNumber, r.InsertionCode, r.Name)
                Expect.equal actual [| 52, "", "ALA"; 52, "A", "GLY"; 53, "", "SER" |] "Residues with insertion codes should be separate residues"
            )

            testCase "Empty input" (fun () ->
                let actual = Structure.ofCoordinates []
                Expect.isEmpty actual.Models "Empty input should give no models"
            )
            
            testCase "HasA roundtrip" (fun () ->
                Expect.sequenceEqual (roundtrip HasA) HasA "HasA structure roundtrip unsuccessful"
            )

            testCase "Glucagon roundtrip" (fun () ->
                Expect.sequenceEqual (roundtrip glucagon) glucagon "Glucagon structure roundtrip unsuccessful"
            )

            testCase "Chains roundtrip" (fun () ->
                Expect.sequenceEqual (roundtrip chainSequence) chainSequence "Chain structure roundtrip unsuccessful"
            )

            testCase "Models roundtrip" (fun () ->
                Expect.sequenceEqual (roundtrip modelSequenceWithTerminators) modelSequenceWithTerminators "Model structure roundtrip unsuccessful"
            )
            
            testCase "Hetero order roundtrip" (fun () ->
                Expect.sequenceEqual (roundtrip heteroOrderSequence) heteroOrderSequence "Hetero residues should be written in their original order"
            )

            testCase "Empty structure" (fun () ->
                Expect.isEmpty (Structure.toCoordinates { Models = [||] }) "Empty structure should give no coordinates"
            )
        ]
        