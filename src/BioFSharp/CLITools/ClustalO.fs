namespace BioFSharp.CLITools

open FSharpAux
open BioFSharp.IO
open BioFSharp
open BioFSharp.FileFormats
open BioFSharp.CLIArgs.ClustalO
open System
open System.IO
open System.Diagnostics

/// A wrapper to perform Clustal Omega alignment tasks    
type ClustalO (?rootPath: string) =

    let tsToFasta (ts:TaggedSequence<string,char>) = Fasta.FastaItem.create ts.Tag ts.Sequence 
   
    let rootPath = 
        match rootPath with
        | Some r -> 
            if File.Exists r then r
            else failwith "clustalo file could not be found for given rootPath"
        | None -> 
            let defaultPath = __SOURCE_DIRECTORY__ |> String.replace "src\BioFSharp.IO" @"lib\clustal-omega\clustalo.exe"
            printfn "try %s" defaultPath
            if File.Exists defaultPath then defaultPath
            else failwith "Default clustalo file could not be found, define rootPath argument."
    let runProcess rootPath arg name =           
        let beginTime = DateTime.UtcNow
        printfn "Starting %s..." name
        let p = 
            new ProcessStartInfo
                (FileName = rootPath, UseShellExecute = false, Arguments = arg, 
                RedirectStandardError = false, CreateNoWindow = true, 
                RedirectStandardOutput = false, RedirectStandardInput = true) 
            |> Process.Start
        p.WaitForExit()
        printfn "%s done." name
        printfn "Elapsed time: %A" (DateTime.UtcNow.Subtract(beginTime))
    //#endif
    ///Runs clustalo tool with given input file paths and parameters and creates output file for given path
    member this.AlignFromFile((inputPath:Input),(outputPath:string),(parameters:seq<ClustalParams>),(?name:string)) = 
        let out = sprintf "-o %s " outputPath
        let arg = 
            Seq.map stringOfClustalParams parameters
            |> String.concat ""
            |> fun x -> (stringOfInputType inputPath) + out + x
        let name = defaultArg name arg
        runProcess rootPath arg name

    ///Runs clustalo tool with given sequences and parameters and returns an alignment
    member this.AlignSequences((input:seq<TaggedSequence<string,char>>),(parameters:seq<ClustalParams>),(?name:string)) = 
        let format = 
            let inputFormat = [InputCustom.Format FileFormat.FastA]
            let outPutFormat = [OutputCustom.Format FileFormat.Clustal]
            [ClustalParams.Input inputFormat; ClustalParams.Output outPutFormat]
        let temp = System.IO.Path.GetTempPath()
           
        let inPath = temp + Guid.NewGuid().ToString() + ".fasta"
        let outPath = temp + Guid.NewGuid().ToString() + ".aln"
            
        let inArg = sprintf "-i %s " inPath
        let outArg = sprintf "-o %s " outPath       

        try 
            Fasta.write id inPath (input |> Seq.map tsToFasta)
            let arg =
                let p = 
                    parameters
                    |> Seq.map (fun cParam -> 
                            match cParam with
                            | Output oParam  -> ClustalParams.Output (Seq.filter (fun x -> match x with | OutputCustom.Format _ -> false | _ -> true) oParam)
                            | Input iParam -> ClustalParams.Input (Seq.filter (fun x -> match x with | InputCustom.Format _ -> false | _ -> true) iParam)
                            | _ -> cParam)
                Seq.map stringOfClustalParams (Seq.append p format)
                |> String.concat ""
                |> fun x -> inArg + outArg + x
            let name = defaultArg name arg
            runProcess rootPath arg name
            Clustal.read outPath
        finally       
            System.IO.File.Delete inPath
            System.IO.File.Delete outPath


