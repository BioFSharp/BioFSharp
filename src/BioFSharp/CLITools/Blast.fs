namespace BioFSharp.CLITools

open BioFSharp.CLIArgs.Blast

///A Wrapper to perform different BLAST tasks
type Blast (rootPath:string) =

    inherit CLITool(rootPath)

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