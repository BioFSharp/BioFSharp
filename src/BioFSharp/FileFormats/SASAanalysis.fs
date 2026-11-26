namespace BioFSharp.FileFormats

module SASA =

    let (|ProbeName|ProbeRadius|) (probe:'T) =
        match box probe with
        | :? string as name -> ProbeName name
        | :? float  as r    -> ProbeRadius r
        | _                 -> invalidArg "probe" "probe should be a string or float "


