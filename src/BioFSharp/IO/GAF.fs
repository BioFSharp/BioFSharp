namespace BioFSharp.IO

open System.IO
open FSharpAux.IO
open BioFSharp.FileFormats.GAF

module GAF =
        
    let readLines (lines: seq<string>) =
        let strEnumerator = lines.GetEnumerator()

        let version = 
            strEnumerator.MoveNext() |> ignore
            strEnumerator.Current

        let isVersion2 = 
            version.StartsWith("!gaf-version: 2")

        let rec parseSingle (accE:GAFEntry list) (accH:string list)=
            if strEnumerator.MoveNext() then 
                let currentString = strEnumerator.Current
                if currentString.StartsWith("!") then 
                    parseSingle accE (currentString::accH) 
                else 
                    parseSingle ((GAFEntry.create currentString isVersion2)::accE) accH

            else 
                GAF.create 
                    (accH |> List.rev |> Seq.cast)
                    (accE |> List.rev |> Seq.cast)

        parseSingle [] [version]

    let read (filepath: string) =
        filepath
        |> FileIO.readFile
        |> readLines

    let write filepath (gaf: GAF) : unit =
        System.IO.File.WriteAllLines(filepath, GAF.toLines gaf)