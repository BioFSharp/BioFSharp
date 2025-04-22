namespace BioFSharp.IO

open System.IO
open BioFSharp
open BioFSharp.FileFormats.DSSP

module DSSP =

    type DSSPLine with

        static member ofParseResults residueindex residuename insertioncode chainid aminoacid secondarystructure accessiblesurface nh_o_1_relidx nh_o_1_energy o_nh_1_relidx o_nh_1_energy nh_o_2_relidx nh_o_2_energy o_nh_2_relidx o_nh_2_energy tco kappa alpha phi psi x_ca y_ca z_ca chain =
            
            let handleResult (columnName: string) (result:Result<'A,'B*exn>)=
                match result with 
                | Ok value -> value
                | Error (value,e) -> failwith $"""parser error for {columnName} column value: "{value}";{System.Environment.NewLine} {e.Message}"""
    
            DSSPLine.create 
                (residueindex        |> handleResult "residueindex")
                (residuename         |> handleResult "residuename")
                (insertioncode       |> handleResult "insertioncode")
                (chainid             |> handleResult "chainid")
                (aminoacid           |> handleResult "aminoacid")
                (secondarystructure  |> handleResult "secondarystructure")
                (accessiblesurface   |> handleResult "accessiblesurface")
                (nh_o_1_relidx       |> handleResult "nh_o_1_relidx")
                (nh_o_1_energy       |> handleResult "nh_o_1_energy")
                (o_nh_1_relidx       |> handleResult "o_nh_1_relidx")
                (o_nh_1_energy       |> handleResult "o_nh_1_energy")
                (nh_o_2_relidx       |> handleResult "nh_o_2_relidx")
                (nh_o_2_energy       |> handleResult "nh_o_2_energy")
                (o_nh_2_relidx       |> handleResult "o_nh_2_relidx")
                (o_nh_2_energy       |> handleResult "o_nh_2_energy")
                (tco                 |> handleResult "tco")
                (kappa               |> handleResult "kappa")
                (alpha               |> handleResult "alpha")
                (phi                 |> handleResult "phi")
                (psi                 |> handleResult "psi")
                (x_ca                |> handleResult "x_ca")
                (y_ca                |> handleResult "y_ca")
                (z_ca                |> handleResult "z_ca")
                (chain               |> handleResult "chain")
    
    
        static member ofString (line: string) = 
    
            let tryParseColumnBy f (c: string) = (try f c |> Ok with e -> Error (c,e))
            let tryParseTupleColumnBy (f: 'c -> 'a *'b) (c: string) = (try f c |> fun (a,b) -> Ok a, Ok b with e -> Error (c,e), Error (c,e))
    
            let residueindex                    = line.[0..4]        |> tryParseColumnBy (fun c -> c.Trim() |> int )
            let residuename                     = line.[5..9]        |> tryParseColumnBy (fun c -> c.Trim())
            let insertioncode                   = line.[10..10]      |> tryParseColumnBy (fun c -> c.Trim())
            let chainid                         = line.[11..11]      |> tryParseColumnBy (fun c -> c.Trim())
            let aminoacid                       = line.[13..13]      |> tryParseColumnBy (fun c -> c.Trim())
            let secondarystructure              = line.[16..33]      |> tryParseColumnBy (SecondaryStructure.ofString SecondaryStructureFormat.DSSP)
            let accessiblesurface               = line.[34..37]      |> tryParseColumnBy (fun c -> c.Trim() |> int)
            let nh_o_1_relidx, nh_o_1_energy    = line.[38..49]      |> tryParseTupleColumnBy (fun c -> c.Trim().Split(',') |> fun [|idx;energy|] -> int (idx.Trim()), float (energy.Trim()))
            let o_nh_1_relidx, o_nh_1_energy    = line.[50..60]      |> tryParseTupleColumnBy (fun c -> c.Trim().Split(',') |> fun [|idx;energy|] -> int (idx.Trim()), float (energy.Trim()))
            let nh_o_2_relidx, nh_o_2_energy    = line.[61..71]      |> tryParseTupleColumnBy (fun c -> c.Trim().Split(',') |> fun [|idx;energy|] -> int (idx.Trim()), float (energy.Trim()))
            let o_nh_2_relidx, o_nh_2_energy    = line.[72..82]      |> tryParseTupleColumnBy (fun c -> c.Trim().Split(',') |> fun [|idx;energy|] -> int (idx.Trim()), float (energy.Trim()))
            let tco                             = line.[83..90]      |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let kappa                           = line.[91..96]      |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let alpha                           = line.[97..102]     |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let phi                             = line.[103..108]    |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let psi                             = line.[109..114]    |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let x_ca                            = line.[115..121]    |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let y_ca                            = line.[122..128]    |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let z_ca                            = line.[129..135]    |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let chain                           = line.[136..]       |> tryParseColumnBy (fun c -> c.Trim())
    
            DSSPLine.ofParseResults residueindex residuename insertioncode chainid aminoacid secondarystructure accessiblesurface nh_o_1_relidx nh_o_1_energy o_nh_1_relidx o_nh_1_energy nh_o_2_relidx nh_o_2_energy o_nh_2_relidx o_nh_2_energy tco kappa alpha phi psi x_ca y_ca z_ca chain

    let readLines (source: seq<string>) =
        let en = source.GetEnumerator()
        let rec loop (yieldLine:bool) (lineIndex:int) (acc: DSSPLine list) =
            if en.MoveNext() then
                match en.Current with
                | tableStartLine when en.Current.StartsWith("  #  RESIDUE") -> 
                    loop true (lineIndex + 1) acc
                | dsspLine when yieldLine -> 
                    let line =
                        try
                            dsspLine |> DSSPLine.ofString
                        with e as exn ->
                            failwith $"parser failed at line {lineIndex}: {en.Current}{System.Environment.NewLine}{e.Message}"
                    loop yieldLine (lineIndex + 1) (line::acc)
                | _ ->  
                    loop false (lineIndex + 1) acc
            else acc |> List.rev
        loop false 0 []
    
    let read (path:string) =
        path
        |> File.ReadAllLines
        |> readLines