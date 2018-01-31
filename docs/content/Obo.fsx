(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"
#r "../../bin/BioFSharp.dll"
#r "../../bin/BioFSharp.IO.dll"
#r "../../bin/FSharp.Care.dll"
#r "../../bin/FSharp.Care.IO.dll"
(** Work in progress *)

open FSharp.Care.IO
open BioFSharp.IO

let test = 
    Seq.fromFile "Psi-MS.txt"
    |> Obo.parseOboTerms
    |> Seq.toArray

test.[test.Length-1]
