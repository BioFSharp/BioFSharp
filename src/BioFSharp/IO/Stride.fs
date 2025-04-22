namespace BioFSharp.IO

open System
open System.IO
open BioFSharp
open BioFSharp.BioItemConverters
open BioFSharp.FileFormats.Stride

module Stride =
    
    type StrideLine with
        
        static member ofParseResults residueindex ordinalresidueindex residuename chainid aminoacid secondarystructure accessiblesurface phi psi =
            
            let handleResult (columnName: string) (result:Result<'A,'B*exn>)=
                match result with 
                | Ok value -> value
                | Error (value,e) -> failwith $"""parser error for {columnName} column value: "{value}";{System.Environment.NewLine} {e.Message}"""
    
            StrideLine.create 
                (residueindex        |> handleResult "residueindex")
                (ordinalresidueindex |> handleResult "ordinalresidueindex")
                (residuename         |> handleResult "residuename")
                (chainid             |> handleResult "chainid")
                (aminoacid           |> handleResult "aminoacid")
                (secondarystructure  |> handleResult "secondarystructure")
                (accessiblesurface   |> handleResult "accessiblesurface")
                (phi                 |> handleResult "phi")
                (psi                 |> handleResult "psi")
    
        static member ofString (line: string) = 
    
            let tryParseColumnBy f (c: string) = (try f c |> Ok with e -> Error (c,e))
    
            let residueindex                    = line.[11..14]     |> tryParseColumnBy (fun c -> c.Trim() |> int )
            let ordinalresidueindex             = line.[16..19]     |> tryParseColumnBy (fun c -> c.Trim() |> int )
            let residuename                     = line.[5..7]       |> tryParseColumnBy (fun c -> c.Trim())
            let chainid                         = line.[10..10]     |> tryParseColumnBy (fun c -> c.Trim())
            let aminoacid                       = line.[5..7]       |> tryParseColumnBy (fun c -> c.Trim().ToCharArray() |> AminoAcids.threeLettersToOption |> Option.get)
            let secondarystructure              = line.[24..38]     |> tryParseColumnBy (SecondaryStructure.ofString SecondaryStructureFormat.Stride)
            let accessiblesurface               = line.[64..68]     |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let phi                             = line.[42..48]     |> tryParseColumnBy (fun c -> c.Trim() |> float)
            let psi                             = line.[52..58]     |> tryParseColumnBy (fun c -> c.Trim() |> float)
    
            StrideLine.ofParseResults residueindex ordinalresidueindex residuename chainid aminoacid secondarystructure accessiblesurface phi psi

    let readLines (source: seq<string>) =
        let en = source.GetEnumerator()
        let rec loop (lineIndex:int) (acc: StrideLine list) =
            if en.MoveNext() then
                match en.Current with
                | asignment when en.Current.StartsWith("ASG") -> 
                    let line =
                        try
                            asignment |> StrideLine.ofString
                        with e as exn ->
                            failwith $"parser failed at line {lineIndex}: {en.Current}{System.Environment.NewLine}{e.Message}"
                    loop (lineIndex + 1) (line::acc)
                | _ -> 
                    loop (lineIndex + 1) acc

            else acc |> List.rev
        loop 0 []
    
    let read (path:string) =
        path
        |> File.ReadAllLines
        |> readLines
