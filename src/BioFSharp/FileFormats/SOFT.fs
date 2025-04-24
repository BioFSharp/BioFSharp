namespace BioFSharp.FileFormats

[<AutoOpen>]
module SOFT =

    type DataTable = {
        Headers: (string*string) []
        Rows: string []
    }

    type SampleRecord = {
        Accession               : string;
        Title                   : string;
        Type                    : string;
        PlatformId              : string;
        SupplementaryFile       : (int * string) list;
        Table                   : (string) list;
        SourceName              : (int * string) list;
        Organism                : (int * string) list;
        Characteristics         : (int * string) list;
        BiomaterialProvider     : (int * string) list;
        TreatmentProtocol       : (int * string) list;
        GrowthProtocol          : (int * string) list;
        Molecule                : (int * string) list;
        ExtractProtocol         : (int * string) list;
        Label                   : (int * string) list;
        LabelProtocol           : (int * string) list;
        HybProtocol             : (string) list;
        ScanProtocol            : (string) list;
        DataProcessing          : (string) list;
        Description             : (string) list;
        GeoAccession            : (string) list;
        Anchor                  : (string) list;
        TagCount                : (string) list;
        TagLength               : (string) list;
        Relation                : (string) list;
        DataTable               : DataTable
    }

    type PlatformRecord = {
        Accession               : string;
        Title                   : string;
        Distribution            : string;
        Technology              : string;
        Organism                : string list;
        Manufacturer            : string list;
        ManufactureProtocol     : string list;
        CatalogNumber           : string list;
        WebLink                 : string list;
        Support                 : string list;
        Coating                 : string list;
        Description             : string list;
        Contributor             : string list;
        PubmedId                : string list;
        GeoAccession            : string list;
        AdditionalAttributes    : Map<string,string>
        DataTable               : DataTable
    }
  
    type SeriesRecord = {
        Accession           : string;
        Title               : string;
        Summary             : (string) list;
        OverallDesign       : string list;
        PubmedId            : (string) list;
        WebLink             : (string) list;
        Contributor         : (string) list;
        Variable            : (int * string) list;
        VariableDescription : (int * string) list;
        VariableSampleList  : (int * (string list)) list;
        Repeats             : (int * string) list;
        RepeatsSampleList   : (int * (string list)) list;
        SampleId            : (string) list;
        GeoAccession        : (string) list;
        Type                : (string) list;
        SubmissionDate      : (string) list;
    }

    module Series =

        open FSharpAux.IO

        type GSE = {
            SeriesMetadata      : SeriesRecord
            SampleMetadata      : Map<string,SampleRecord>
            PlatformMetadata    : Map<string,PlatformRecord>
        } with  
            static member create (seriesRecords: Map<string,SeriesRecord>, sampleRecords: Map<string,SampleRecord>, platformRecords: Map<string,PlatformRecord>) : GSE =
                {
                    SeriesMetadata =
                        seriesRecords
                        |> Map.toList
                        |> List.exactlyOne
                        |> snd

                    SampleMetadata      = sampleRecords
                    PlatformMetadata    = platformRecords
                }

        ///returns platform metadata associated with the input series GSE representation
        let getAssociatedPlatforms (gse:GSE) =
            gse.PlatformMetadata
            |> Map.toList
            |> List.map snd

        ///returns platform accessions associated with the input series GSE representation
        let getAssociatedPlatformAccessions (gse:GSE) =
            gse.PlatformMetadata
            |> Map.toList
            |> List.map fst

        ///returns sample metadata associated with the input series GSE representation
        let getAssociatedSamples (gse:GSE) =
            gse.SampleMetadata
            |> Map.toList
            |> List.map snd

        ///returns sample accessions associated with the input series GSE representation
        let getAssociatedSampleAccessions (gse:GSE) =
            gse.SampleMetadata
            |> Map.toList
            |> List.map fst


    module Platform =

        open FSharpAux.IO

        type GPL = {
            PlatformMetadata    : PlatformRecord
            SeriesMetadata      : Map<string,SeriesRecord>
            SampleMetadata      : Map<string,SampleRecord>
        } with
            static member create (seriesRecords: Map<string,SeriesRecord>, sampleRecords: Map<string,SampleRecord>, platformRecords: Map<string,PlatformRecord>) : GPL =
                {
                    PlatformMetadata    = 
                        platformRecords
                        |> Map.toList
                        |> List.exactlyOne
                        |> snd

                    SeriesMetadata      = seriesRecords
                    SampleMetadata      = sampleRecords

                }

        ///returns series metadata associated with the input platform GPL representation
        let getAssociatedSeries (gpl:GPL) =
            gpl.SeriesMetadata
            |> Map.toList
            |> List.map snd

        ///returns series accessions associated with the input platform GPL representation
        let getAssociatedSeriesAccessions (gpl:GPL) =
            gpl.SeriesMetadata
            |> Map.toList
            |> List.map snd
            |> List.map (fun record -> record.Accession)

        ///returns sample metadata associated with the input platform GPL representation
        let getAssociatedSamples (gpl:GPL) =
            gpl.SampleMetadata
            |> Map.toList
            |> List.map snd

        ///returns sample accessions associated with the input platform GPL representation
        let getAssociatedSampleAccessions (gpl:GPL) =
            gpl.SampleMetadata
            |> Map.toList
            |> List.map fst
