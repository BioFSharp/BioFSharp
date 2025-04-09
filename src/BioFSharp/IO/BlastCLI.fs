namespace BioFSharp.IO

open BioFSharp.FileFormats
open BioFSharp.FileFormats.BlastCLI
open DynamicObj
open System
open System.Diagnostics
open FSharpAux
open FSharpAux.IO
open FSharpAux.IO.SchemaReader
open FSharpAux.IO.SchemaReader.Attribute

module BlastCLI =

    let ncbiPath = "../../lib/ncbi-blast/bin"

    ///A Wrapper to perform different BLAST tasks
    type BlastCLIWrapper (rootPath:string) =

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

        member this.createProcess(
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
                new ProcessStartInfo
                  (FileName = rootPath + exec, UseShellExecute = false, Arguments = argstring, 
                   RedirectStandardError = false, CreateNoWindow = true, 
                   RedirectStandardOutput = false, RedirectStandardInput = true) 
                |> Process.Start
            p.WaitForExit()
            printfn "%s done." name
            printfn "Elapsed time: %A" (DateTime.UtcNow.Subtract(beginTime))

        ///Creates a BLAST database from given source/s
        member this.makeblastdb (parameters:MakeBlastDbParams list) =
            this.printArgs "makeblastdb" MakeBlastDbParams.toCLIArgs parameters
            this.createProcess(
                name = "makeblastdb",
                exec = "/makeblastdb.exe",
                argConverter = MakeBlastDbParams.toCLIArgs,
                parameters = parameters
            )
        
        ///Compares a nucleotide query to a nucleotide database
        member this.blastN (parameters: BlastN.BlastNParams list)=
            this.printArgs "blastn" BlastN.BlastNParams.toCLIArgs parameters
            this.createProcess(
                name = "blastn",
                exec = "/blastn.exe",
                argConverter = BlastN.BlastNParams.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task blastn"
        )

        member this.megablast (parameters: BlastN.MegablastParameters list)=
            this.printArgs "makeblastdb" BlastN.MegablastParameters.toCLIArgs parameters
            this.createProcess(
                name = "megablast",
                exec = "/blastn.exe",
                argConverter = BlastN.MegablastParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task megablast"
            )

        member this.``dc-megablast`` (parameters: BlastN.DCMegablastParameters list)=
            this.printArgs "DC-Megablast" BlastN.DCMegablastParameters.toCLIArgs parameters
            this.createProcess(
                name = "dc-megablast",
                exec = "/blastn.exe",
                argConverter = BlastN.DCMegablastParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task dc-megablast"
            )

        member this.``blastn-short`` (parameters: BlastN.BlastNShortParameters list)=
            this.printArgs "blastn-short" BlastN.BlastNShortParameters.toCLIArgs parameters
            this.createProcess(
                name = "blastn-short",
                exec = "/blastn.exe",
                argConverter = BlastN.BlastNShortParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task blastn-short"
            )

        member this.blastp (parameters: BlastP.BlastPParameters list)=
            this.printArgs "blastp" BlastP.BlastPParameters.toCLIArgs parameters
            this.createProcess(
                name = "blastp",
                exec = "/blastp.exe",
                argConverter = BlastP.BlastPParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task blastp"
            )

        member this.``blastp-short`` (parameters: BlastP.BlastPShortParameters list)=
            this.printArgs "blastp-short" BlastP.BlastPShortParameters.toCLIArgs parameters
            this.createProcess(
                name = "blastp",
                exec = "/blastp.exe",
                argConverter = BlastP.BlastPShortParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task blastp-short"
            )

        member this.``blastp-fast`` (parameters: BlastP.BlastPFastParameters list)=
            this.printArgs "blastp-fast" BlastP.BlastPFastParameters.toCLIArgs parameters
            this.createProcess(
                name = "blastp-fast",
                exec = "/blastp.exe",
                argConverter = BlastP.BlastPFastParameters.toCLIArgs,
                parameters = parameters,
                prependArgString = "-task blastp-fast"
            )

    let readCustomBlastResult (cAttributes:seq<OutputCustom>) separator filePath =
        let lineHasHeader = cAttributes |> Seq.map stringOfOutputCustom |> String.concat (separator.ToString())
        let csvReader = SchemaReader.Csv.CsvReader<CBlastResult>(SchemaMode=SchemaReader.Csv.Fill)
        csvReader.ReadFile(filePath,separator,lineHasHeader)


