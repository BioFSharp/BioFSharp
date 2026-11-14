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
   
    let atomAt (x, y, z) =
        let template = (parse [| atomSingleton |]).Models[0].Chains[0].Residues[0].Atoms[0]
        { template with X = x; Y = y; Z = z }

    let neighborAtoms =
        [|
            atomAt (-0.5, 0., 0.)
            atomAt (0.5, 0., 0.)
            atomAt (-3.9, -3.9, -3.9)
            atomAt (10., 10., 10.)
            atomAt (-1.0, 0., 0.)
        |]
       
    let allAtoms = fun (_: PDB.Atom) -> true

    let alphaCarbons = fun (a: PDB.Atom) -> PDB.AtomName.toString a.Name = " CA "
    
    
    let rotate (ax: float, ay: float, az: float) (tx: float, ty: float, tz: float) (atom: PDB.Atom) =
        let x1, y1, z1 = atom.X, cos ax * atom.Y - sin ax * atom.Z, sin ax * atom.Y + cos ax * atom.Z
        let x2, y2, z2 = cos ay * x1 + sin ay * z1, y1, -(sin ay) * x1 + cos ay * z1
        let x3, y3, z3 = cos az * x2 - sin az * y2, sin az * x2 + cos az * y2, z2
        { atom with X = x3 + tx; Y = y3 + ty; Z = z3 + tz }
    
    let structureTests =
        testList "Structure" [
            testCase "HasA hierarchy" (fun () ->
                let actual = parse HasA
                let residues = actual.Models[0].Chains[0].Residues
                Expect.equal actual.Models.Length 1 "HasA should have one model"
                Expect.equal (actual.Models[0].Chains |> Array.map (fun c -> c.Identifier)) [|"A"|] "HasA should have one chain A"
                Expect.equal residues.Length 223 "HasA should have 223 residues"
                Expect.equal (residues |> Array.filter (fun r -> r.IsHetero) |> Array.length) 68 "HasA should have 68 hetero residues"
                Expect.equal (residues |> Array.sumBy (fun r -> r.Atoms.Length)) 1291 "HasA should have 1291 atoms"
            )

            testCase "HasA first residue" (fun () ->
                let actual = (parse HasA).Models[0].Chains[0].Residues[0]
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
                    (parse chainSequence).Models[0].Chains
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
                    |> Array.map (fun m -> m.Number, m.Chains[0].Residues[0].Atoms[0].X)
                Expect.equal actual [| 1, 11.104; 2, 12.104 |] "Each MODEL should be its own model"
            )

            testCase "Insertion codes" (fun () ->
                let actual =
                    (parse insertionCodeSequence).Models[0].Chains[0].Residues
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
            
            testCase "Distance" (fun () ->
                Expect.floatClose Accuracy.high (Structure.distance (atomAt (0., 0., 0.)) (atomAt (3., 4., 0.))) 5.0 "Distance of (0,0,0) and (3,4,0) should be 5"
            )

            testCase "Brute force neighbor search" (fun () ->
                let actual = Structure.NeighborSearch.bruteForce 1.5 neighborAtoms
                Expect.equal actual [| 0, 1; 0, 4; 1, 4 |] "Pairs within 1.5 should be found, including a pair at exactly 1.5"
            )

            testCase "Grid neighbor search with negative coordinates" (fun () ->
                let actual = Structure.NeighborSearch.grid 1.5 neighborAtoms
                Expect.equal actual [| 0, 1; 0, 4; 1, 4 |] "Grid should find pairs across cell boundaries at negative coordinates"
            )

            testCase "Grid equals brute force on HasA" (fun () ->
                let atoms = Structure.atoms (parse HasA).Models[0]
                for cutoff in [ 4.0; 8.0 ] do
                    Expect.equal (Structure.NeighborSearch.grid cutoff atoms) (Structure.NeighborSearch.bruteForce cutoff atoms) $"Grid and brute force should find the same pairs at {cutoff}"
                Expect.equal (Structure.NeighborSearch.grid 4.0 atoms).Length 7294 "HasA should have 7294 atom pairs within 4"
            )

            testCase "Grid neighbor search without atoms" (fun () ->
                Expect.isEmpty (Structure.NeighborSearch.grid 4.0 [||]) "No atoms should give no pairs"
            )

            testCase "Grid neighbor search with invalid cutoff" (fun () ->
                Expect.throwsT<System.ArgumentException> (fun () -> Structure.NeighborSearch.grid 0.0 neighborAtoms |> ignore) "A cutoff of 0 should fail"
            )
            
            testCase "Residue contacts" (fun () ->
                let model = (parse contactSequence).Models[0]
                Expect.equal (Structure.residueContacts allAtoms 4.0 model) [| 0, 1; 1, 3 |] "Residues with atoms within 4 should be in contact"
            )

            testCase "Residue contacts of alpha carbons" (fun () ->
                let model = (parse contactSequence).Models[0]
                Expect.isEmpty (Structure.residueContacts alphaCarbons 4.0 model) "Alpha carbons 4.5 apart should not be in contact at 4"
                Expect.equal (Structure.residueContacts alphaCarbons 5.0 model) [| 0, 1 |] "Alpha carbons 4.5 apart should be in contact at 5"
            )

            testCase "Contact map" (fun () ->
                let actual = Structure.contactMap allAtoms 4.0 (parse contactSequence).Models[0]
                let expected =
                    array2D [
                        [ false; true;  false; false ]
                        [ true;  false; false; true  ]
                        [ false; false; false; false ]
                        [ false; true;  false; false ]
                    ]
                Expect.equal actual expected "Contact map should be symmetric with one row per residue"
            )

            testCase "HasA residue contacts" (fun () ->
                let model = (parse HasA).Models[0]
                Expect.equal (Structure.residues model).Length 223 "HasA should have 223 residues"
                Expect.equal (Structure.residueContacts allAtoms 4.0 model).Length 693 "HasA should have 693 residue contacts within 4"
                Expect.equal (Structure.residueContacts alphaCarbons 8.0 model).Length 647 "HasA should have 647 alpha carbon contacts within 8"
            )
            
            testCase "Bond angle" (fun () ->
                let center = atomAt (0., 0., 0.)
                Expect.floatClose Accuracy.high (Structure.angle (atomAt (1., 0., 0.)) center (atomAt (0., 1., 0.))) 90.0 "Perpendicular bonds should give 90 degrees"
                Expect.floatClose Accuracy.high (Structure.angle (atomAt (1., 0., 0.)) center (atomAt (-1., 0., 0.))) 180.0 "Opposite bonds should give 180 degrees"
                Expect.floatClose Accuracy.high (Structure.angle (atomAt (1., 0., 0.)) center (atomAt (0.5, sqrt 0.75, 0.))) 60.0 "Bonds of an equilateral triangle should give 60 degrees"
            )

            testCase "Dihedral angle" (fun () ->
                let a, b, c = atomAt (1., 0., 0.), atomAt (0., 0., 0.), atomAt (0., 0., 1.)
                Expect.floatClose Accuracy.high (Structure.dihedral a b c (atomAt (1., 0., 1.))) 0.0 "Atoms on the same side should give 0 degrees (cis)"
                Expect.floatClose Accuracy.high (Structure.dihedral a b c (atomAt (-1., 0., 1.))) 180.0 "Atoms on opposite sides should give 180 degrees (trans)"
                Expect.floatClose Accuracy.high (Structure.dihedral a b c (atomAt (0., 1., 1.))) 90.0 "A clockwise quarter turn should give +90 degrees"
                Expect.floatClose Accuracy.high (Structure.dihedral a b c (atomAt (0., -1., 1.))) -90.0 "A counterclockwise quarter turn should give -90 degrees"
            )
            
            testCase "Backbone torsions of HasA" (fun () ->
                let actual = Structure.backboneTorsions (parse HasA).Models.[0].Chains.[0]
                Expect.equal actual.Length 155 "All 155 amino acids, but no heme, sulfate or water, should have backbone torsions"
                Expect.isNone actual.[0].Phi "The first residue should have no phi"
                Expect.isNone actual.[actual.Length - 1].Psi "The last residue should have no psi"
                Expect.isNone actual.[actual.Length - 1].Omega "The last residue should have no omega"
                Expect.equal (actual |> Array.filter (fun t -> t.Phi.IsSome) |> Array.length) 152 "Phi should be missing at the chain start and after both chain breaks"
            )

            testCase "Backbone torsions at chain breaks of HasA" (fun () ->
                let torsions = Structure.backboneTorsions (parse HasA).Models.[0].Chains.[0]
                let at number = torsions |> Array.find (fun t -> t.Residue.SequenceNumber = number)
                Expect.isNone (at 28).Psi "GLY 28 is followed by a gap and should have no psi"
                Expect.isNone (at 45).Phi "GLY 45 follows a gap and should have no phi"
                Expect.isNone (at 126).Psi "ASP 126 is followed by a gap and should have no psi"
                Expect.isNone (at 138).Phi "ASP 138 follows a gap and should have no phi"
                Expect.isSome (at 28).Phi "GLY 28 should still have phi"
                Expect.isSome (at 45).Psi "GLY 45 should still have psi"
            )

            testCase "Backbone torsions with a moved residue" (fun () ->
                let chain = (parse HasA).Models.[0].Chains.[0]
                let moved =
                    { chain with
                        Residues =
                            chain.Residues
                            |> Array.mapi (fun i residue ->
                                if i >= 3 then { residue with Atoms = residue.Atoms |> Array.map (fun a -> { a with X = a.X + 10.0 }) }
                                else residue
                            )
                    }
                let actual = Structure.backboneTorsions moved
                Expect.isNone actual.[2].Psi "Residue 3 should have no psi when residue 4 is moved away"
                Expect.isNone actual.[2].Omega "Residue 3 should have no omega when residue 4 is moved away"
                Expect.isNone actual.[3].Phi "Residue 4 should have no phi when it is moved away"
                Expect.isSome actual.[3].Psi "Residue 4 should still have psi, since its neighbor moved with it"
            )

            testCase "Right-handed helices in HasA" (fun () ->
                let pairs =
                    Structure.backboneTorsions (parse HasA).Models.[0].Chains.[0]
                    |> Array.choose (fun t -> match t.Phi, t.Psi with | Some phi, Some psi -> Some (phi, psi) | _ -> None)
                let rightHanded = pairs |> Array.filter (fun (phi, psi) -> phi > -100. && phi < -30. && psi > -80. && psi < -10.) |> Array.length
                let leftHanded = pairs |> Array.filter (fun (phi, psi) -> phi > 30. && phi < 100. && psi > 10. && psi < 80.) |> Array.length
                Expect.isGreaterThan rightHanded leftHanded "Most helical residues should be right-handed (negative phi and psi)"
            )
            
            testCase "Backbone torsion values of HasA" (fun () ->
                let torsions = Structure.backboneTorsions (parse HasA).Models.[0].Chains.[0]
                let accuracy = { absolute = 1e-6; relative = 0.0 }
                let expected =
                    [
                        2,   ( -80.76470295, 150.0232299,  179.4284815)
                        50,  ( -98.20191063, 123.3601233, -179.5331651)
                        100, ( -54.93909091, 136.0193708,  176.8513965)
                        150, (-123.25981571, 102.6597476,  179.2311984)
                    ]
                for (number, (phi, psi, omega)) in expected do
                    let actual = torsions |> Array.find (fun t -> t.Residue.SequenceNumber = number)
                    Expect.floatClose accuracy (Option.get actual.Phi) phi $"Phi of residue {number} was not equal"
                    Expect.floatClose accuracy (Option.get actual.Psi) psi $"Psi of residue {number} was not equal"
                    Expect.floatClose accuracy (Option.get actual.Omega) omega $"Omega of residue {number} was not equal"
            )
            
        
            testCase "RMSD" (fun () ->
                let a = [| atomAt (0., 0., 0.); atomAt (1., 0., 0.) |]
                let b = [| atomAt (1., 0., 0.); atomAt (2., 0., 0.) |]
                Expect.floatClose Accuracy.high (Structure.rmsd a a) 0.0 "Identical atoms should have an RMSD of 0"
                Expect.floatClose Accuracy.high (Structure.rmsd a b) 1.0 "Atoms shifted by 1 should have an RMSD of 1"
                Expect.throwsT<System.ArgumentException> (fun () -> Structure.rmsd a [| atomAt (0., 0., 0.) |] |> ignore) "Different numbers of atoms should fail"
            )

            testCase "Superposition of a triangle" (fun () ->
                let reference = [| atomAt (0., 0., 0.); atomAt (1., 0., 0.); atomAt (0., 1., 0.) |]
                let mobile = [| atomAt (5., 5., 5.); atomAt (5., 6., 5.); atomAt (4., 5., 5.) |]
                let actual = Structure.superpose reference mobile
                Expect.floatClose Accuracy.high (Structure.rmsd reference actual) 0.0 "The moved and rotated triangle should fit exactly"
            )

            testCase "Superposition of rotated HasA alpha carbons" (fun () ->
                let reference = Structure.atoms (parse HasA).Models.[0] |> Array.filter alphaCarbons
                for angles in [ (0.7, -1.2, 2.5); (System.Math.PI, 0., 0.); (0., System.Math.PI, 0.); (0., 0., System.Math.PI) ] do
                    let mobile = reference |> Array.map (rotate angles (10., -20., 5.))
                    let actual = Structure.superpose reference mobile
                    Expect.floatClose { absolute = 1e-9; relative = 0.0 } (Structure.rmsd reference actual) 0.0 $"A copy rotated by {angles} should fit exactly"
            )

            testCase "Superposition does not mirror" (fun () ->
                let reference = Structure.atoms (parse HasA).Models.[0] |> Array.filter alphaCarbons
                let mirrored = reference |> Array.map (fun a -> { a with X = -a.X })
                let actual = Structure.superpose reference mirrored
                Expect.isGreaterThan (Structure.rmsd reference actual) 1.0 "A mirror image cannot be superposed by a rotation"
            )
        ]
        