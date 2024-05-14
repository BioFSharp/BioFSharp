namespace BioFSharp.Interactive

open System
open System.Threading.Tasks
open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Formatting
open BioFSharp
open BioFSharp.IO

type FormatterKernelExtension() =

    interface IKernelExtension with
        member _.OnLoadAsync _ =
            Formatters.registerAll()
            Task.CompletedTask
