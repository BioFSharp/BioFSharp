namespace BioFSharp.FileFormats
///Contains functions for reading and writing GFF3 files
module GFF3 =

    open FSharpAux
    open FSharpAux.IO
    open System.Collections.Generic
    
    ///represents fields of one GFF3 entry line
    type GFFEntry = {
        ///name of sequence where the feature is located
        Seqid       : string
        ///program, organization or database where the sequence is derived from
        Source      : string
        ///feature, type or method; has to be a term from SO or SO accession number
        Feature     : string
        ///positive 1-based integer start coordinate, relative to the landmark given in column 1
        StartPos    : int
        ///positive 1-based integer end coordinate, relative to the landmark given in column 1
        EndPos      : int
        ///the score of the feature; semantics are ill-defined
        Score       : float
        ///the strand of the feature
        Strand      : char
        ///for CDS features: indicates where the feature begins with reference to the reading frame
        Phase       : int
        ///a semicolon-separated list of tag-value pairs, providing additional information about each feature
        Attributes  : Map<string,(string list)>
        ///additional supplement information about the feature (optional)
        Supplement  : string [] 
                   }
    
    ///represents all kinds of lines which can be present in a GFF3 file
    type GFFLine<'SequenceItem>  =
    | GFFEntryLine    of GFFEntry
    | Comment         of string
    | Directive       of string
    | Fasta           of seq<Fasta.FastaItem<'SequenceItem>>
