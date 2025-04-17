namespace BioFSharp.FileFormats

open FSharpAux.IO.SchemaReader.Attribute

type CustomBlastResult = {
    [<FieldAttribute("qseqid")>]      Query_SeqId               : string
    [<FieldAttribute("qgi")>]         Query_GI                  : string
    [<FieldAttribute("qacc")>]        Query_Accesion            : string
    [<FieldAttribute("qaccver")>]     Query_Accesion_Version    : string
    [<FieldAttribute("qlen")>]        Query_Length              : int
    [<FieldAttribute("sseqid")>]      Subject_SeqId             : string
    [<FieldAttribute("sallseqid")>]   Subject_All_SeqIds        : string
    [<FieldAttribute("sgi")>]         Subject_GI                : string
    [<FieldAttribute("sallgi")>]      Subject_All_GIs           : string
    [<FieldAttribute("sacc")>]        Subject_Accession         : string
    [<FieldAttribute("saccver")>]     Subject_Accession_Version : string
    [<FieldAttribute("sallacc")>]     Subject_All_Accession     : string
    [<FieldAttribute("slen")>]        Subject_Length            : int
    [<FieldAttribute("qstart")>]      Query_StartOfAlignment    : string
    [<FieldAttribute("qend")>]        Query_EndOfAlignment      : string
    [<FieldAttribute("sstart")>]      Subject_StartOfAlignment  : string
    [<FieldAttribute("send")>]        Subject_EndOfAlignment    : string
    [<FieldAttribute("qseq")>]        Query_AlignedPartOf       : string
    [<FieldAttribute("sseq" )>]       Subject_AlignedPartOf     : string
    [<FieldAttribute("evalue")>]      Evalue                    : float
    [<FieldAttribute("bitscore")>]    Bitscore                  : float
    [<FieldAttribute("score")>]       RawScore                  : float
    [<FieldAttribute("length")>]      AlignmentLength           : int
    [<FieldAttribute("pident" )>]     Identity                  : float
    [<FieldAttribute("nident")>]      IdentityCount             : int
    [<FieldAttribute("mismatch")>]    MismatchCount             : int
    [<FieldAttribute("positive")>]    PositiveScoringMatchCount : int
    [<FieldAttribute("gapopen")>]     GapOpeningCount           : int
    [<FieldAttribute("gaps")>]        GapCount                  : int
    [<FieldAttribute("ppos")>]        PositiveScoringMatch      : float
    [<FieldAttribute("frames" )>]     Frames                    : string
    [<FieldAttribute("qframe")>]      Query_Frames              : string
    [<FieldAttribute("sframe")>]      Subject_Frames            : string
    [<FieldAttribute("btop" )>]       BTOP                      : string
    [<FieldAttribute("staxids" )>]    Subject_TaxonomyIDs       : string
    [<FieldAttribute("sscinames")>]   Subject_Scientific_Names  : string
    [<FieldAttribute("scomnames")>]   Subject_Common_Names      : string
    [<FieldAttribute("sblastnames")>] Subject_Blast_Names       : string
    [<FieldAttribute("sskingdoms")>]  Subject_Super_Kingdoms    : string
    [<FieldAttribute("stitle")>]      Subject_Title             : string
    [<FieldAttribute("salltitles")>]  Subject_All_Titles        : string
    [<FieldAttribute("sstrand")>]     Subject_Strand            : string
    [<FieldAttribute("qcovs")>]       Query_CoveragePerSubject  : string
    [<FieldAttribute("qcovhsp")>]     Query_CoveragePerHSP      : string
}
