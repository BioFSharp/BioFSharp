namespace BioFSharp.CLITools

open System
open System.Diagnostics

type CLITool(rootPath:string) =
    member this.createArgString (argConverter: 'TParam -> string list) (parameters: 'TParam list) =
        parameters 
        |> Seq.map argConverter
        |> Seq.concat
        |> String.concat " "
            
    member this.printArgs (processName: string) (argConverter: 'TParam -> string list) (parameters: 'TParam list) =
        printfn $"Starting process {processName} in path {rootPath}"
        printfn "Args:"
        parameters
        |> List.map argConverter
        |> List.iter (fun op -> printfn "\t%s" (String.concat " " op))

    member this.runProcess(
        name: string,
        exec: string,
        argConverter: 'TParam -> string list,
        parameters: 'TParam list,
        ?prependArgString: string
    ) =
        let prependArgString = defaultArg prependArgString ""
        let argstring = [prependArgString; this.createArgString argConverter parameters] |> String.concat " " 
        let beginTime = DateTime.UtcNow
        printfn "Starting %s..." name
        let p =                        
            new ProcessStartInfo(
                FileName = rootPath + exec, 
                UseShellExecute = false, 
                Arguments = argstring, 
                RedirectStandardError = false, 
                CreateNoWindow = true, 
                RedirectStandardOutput = false, 
                RedirectStandardInput = true
            ) 
            |> Process.Start
        p.WaitForExit()
        printfn "%s done." name
        printfn "Elapsed time: %A" (DateTime.UtcNow.Subtract(beginTime))
        p