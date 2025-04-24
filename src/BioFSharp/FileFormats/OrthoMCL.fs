namespace BioFSharp.FileFormats

open FSharpAux    
open FSharpAux.IO
open FSharpAux.IO.SchemaReader
open FSharpAux.IO.SchemaReader.Attribute

type internal EvalueConverter() = 
    inherit ConverterAttribute()
    override this.convertToObj = 
        Converter.Collection(fun (strs : seq<string>) -> 
            (
                String.tryParseFloatDefault nan (String.concat "e" strs)
                )
            |> box)

type OrthoMCL = {
    [<FieldAttribute(0)>]
    Query_SeqId   : string
    [<FieldAttribute(2)>]
    Subject_SeqId : string
    [<FieldAttribute(1)>]
    Orthomcl_group : string
    [<FieldAttribute([|3;4|])>]
    [<EvalueConverter()>]
    Evalue        : float
    [<FieldAttribute(5)>]
    Identity      : float
    [<FieldAttribute(6)>]
    Similarity    : float    
    }
