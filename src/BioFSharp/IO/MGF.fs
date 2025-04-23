namespace BioFSharp.IO

open System
open BioFSharp.FileFormats.MGF

open FSharpAux
open FSharpAux.IO
open System.IO


module MGF =

    /// Reads an mgf file into a collection of MgfEntries
    let read path =

        let parseLineInside paramsMap ml (line:string) =
            let spLine = line.Split('=')
            // spectrum-specific parameters
            if spLine.Length > 1 then
                let tmp = Map.add spLine.[0] spLine.[1] paramsMap
                (true,ml,tmp)
            else
                // peak list
                if spLine.Length = 1 then
                    let spLine' = line.Split(' ')
                    let m = String.tryParseFloatDefault nan spLine'.[0]
                    let i = String.tryParseFloatDefault nan spLine'.[1]
                    (true,(m,i)::ml,paramsMap)
                else 
                    failwithf "Format Exception: peak list" 


        let mgfFrom ml (p:Map<string,string>) =
            let m,i = ml  |> List.rev |> List.unzip
            MGFEntry.create p (m|> List.toArray) (i |> List.toArray)    
    
    
        let tmp =
            Seq.fromFile path      
            // filter comments (#;!/)
            |> Seq.filter (fun line ->not ( line.StartsWith("#") &&
                                            line.StartsWith("!") &&
                                            line.StartsWith("/") ))
            // filter empty lines
            |> Seq.filter (fun line -> line <> String.Empty)
            |> Seq.fold (fun state line ->                                         
                                let line = line.Trim()
                                //printfn "->%s" line
                                match state with
                                | (true,ml,p),l -> if line <> "END IONS" then
                                                      (parseLineInside p ml (line:string)),l
                                                   else
                                                      let tmp = mgfFrom ml p
                                                      (false,[],Map.empty),(tmp::l)
                                | (false,_,_),l   -> if line <> "BEGIN IONS" then failwithf "Format Exception: BEGIN IONS"
                                                     (true,[],Map.empty),l
                                                                                                
                                    ) ((false,[],Map.empty),[])
        snd tmp

    let write (filePath: string) (mgf: #seq<MGFEntry>) =
        seq {
            for entry in mgf do yield! MGFEntry.toLines entry
        }
        |> fun f -> File.WriteAllLines(filePath, f)