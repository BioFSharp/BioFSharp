namespace BioFSharp.FileFormats

/// Mgf <http://www.matrixscience.com/help/data_file_help.html>`_ is a simple
/// human-readable format for MS/MS data. It allows storing MS/MS peak lists and
/// exprimental parameters.
module MGF = 

    open System
    open FSharpAux

    /// Returns the posible charge/charges in a list. Returns None if the information can't be optained
    let tryParseCharge str =
        let subParse (digit,sign) =
            if sign = "-" then 
                - int digit
            else
                int digit

        match str with
            | Regex.Active.RegexGroups @"(?<digit>\d*)(?<sign>[+-])" l -> Some (l |> List.map (fun g -> subParse (g.["digit"].Value,g.["sign"].Value)))
            | _ -> None

    /// Returns the retention time and precursor intensity from 'mgf-title' string. Returns None if title does not contain the information
    let tryParseTitle title =
        match title with
            | Regex.Active.RegexValues @"[+-]?(\d+\.?\d*|\.\d+)([eE][+-]?\d+)?" [ ret; intens; ] -> Some( float ret, float intens )
            | _ -> None

    /// Represents 
    type MGFEntry = {   
        Parameters : Map<string,string>
        Mass       : float []
        Intensity  : float []
    } with
        static member create parameters mass intensity =
            {Parameters = parameters; Mass = mass; Intensity = intensity;}

        /// Returns the precursor mass. Returns None if the information can't be optained
        static member tryGetPrecursorMass (mgf:MGFEntry) =
            if mgf.Parameters.ContainsKey("PEPMASS") then
                Some (String.tryParseFloatDefault nan mgf.Parameters.["PEPMASS"])
            else
                None
    
        /// Returns the precursor mz. Returns None if the information can't be optained
        static member tryGetPrecursorMZ (mgf:MGFEntry) =
            if mgf.Parameters.ContainsKey("PRECURSORMZ") then
                Some (String.tryParseFloatDefault nan mgf.Parameters.["PRECURSORMZ"])
            else
                None        
        /// Returns the precursor mass. Returns None if the information can't be optained
        static member tryGetPrecursorCharges (mgf:MGFEntry) =
            if mgf.Parameters.ContainsKey("CHARGE") then
                tryParseCharge mgf.Parameters.["CHARGE"]
            else
                None        

        /// Returns the title string of a 'mgf-entry'. Returns None if the information can't be optained
        static member tryGetTitle (mgf:MGFEntry) =
            if mgf.Parameters.ContainsKey("TITLE") then
                Some (mgf.Parameters.["TITLE"])
            else
                None

        /// Converts a MgfEntry to string.
        /// Use Seq.write to write to file. 
        static member toLines (mgf:MGFEntry) =
            seq { 
                yield "BEGIN IONS"
                for p in mgf.Parameters do
                    yield sprintf "%s=%s" p.Key p.Value
                for m,i in  Seq.zip mgf.Mass mgf.Intensity do
                    yield sprintf "%f %f" m i
                yield "END IONS"
            } 

        /// Converts a MgfEntry to string.
        /// Use Seq.write to write to file. 
        static member toString (mgf:MGFEntry) =
            mgf
            |> MGFEntry.toLines
            |> String.concat System.Environment.NewLine