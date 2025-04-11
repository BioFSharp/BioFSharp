namespace BioFSharp.FileFormats

///Wrapper and its helpers for Clustal Omega multiple alignment tools
module ClustalOCLI =

    open FSharpAux

    ///Contains modifier parameter type for Clustal Omega wrapper
    module Parameters = 
    
        ///Input file format
        type FileFormat = 
            ///FastA file format
            | FastA
            ///Clustal file format
            | Clustal
            ///MSF file format
            | MSF
            ///Phylip file format
            | Phylip
            ///Selex file format
            | Selex
            ///Stockholm file format
            | Stockholm
            ///Vienna file format
            | Vienna
    
        let internal stringOfFileFormatOut (f:FileFormat) =
            match f with
            | FastA -> "--outfmt=fa "
            | Clustal -> "--outfmt=clu "
            | MSF -> "--outfmt=msf "
            | Phylip -> "--outfmt=phy "
            | Selex -> "--outfmt=selex "
            | Stockholm -> "--outfmt=st "
            | Vienna -> "--outfmt=vie "

        let internal stringOfFileFormatIn (f:FileFormat) =
            match f with
            | FastA -> "--infmt=fa "
            | Clustal -> "--infmt=clu "
            | MSF -> "--infmt=msf "
            | Phylip -> "--infmt=phy "
            | Selex -> "--infmt=selex "
            | Stockholm -> "--infmt=st "
            | Vienna -> "--infmt=vie "
        ///Types of sequences
        type SeqType = 
            | Protein
            | DNA
            | RNA

        let internal stringOfSeqType (s:SeqType) = 
            match s with
            | Protein -> "--seqtype=Protein "
            | RNA -> "--seqtype=RNA "
            | DNA -> "--seqtype=DNA "

        ///Optional modifiers for input
        type InputCustom =
            ///Forced sequence input file format (default: auto)
            | Format of FileFormat
            ///Dealign input sequences
            | Dealign
            ///Disable check if profile, force profile (default no)
            | IsProfile
            ///Force a sequence type (default: auto)
            | SeqType of SeqType    
    
        let internal stringOfInputCustom (i:InputCustom) = 
            match i with
            | Format f -> stringOfFileFormatIn f
            | Dealign -> "--dealign "
            | IsProfile -> "--is-profile "
            | SeqType s -> stringOfSeqType s

        ///Optional modifiers to specify clustering
        type ClusteringCustom =
            ///Pairwise distance matrix input file (skips distance computation)
            | DistanceMatrixInput of string
            ///Pairwise distance matrix output file
            | DistanceMatrixOutput of string
            ///Guide tree input file (skips distance computation and guide tree clustering step)
            | GuideTreeInput of string
            ///Guide tree output file
            | GuideTreeOutput of string
            ///Use full distance matrix for guide-tree calculation (slow; mBed is default)
            | Full
            ///Use full distance matrix for guide-tree calculation during iteration (mBed is default)
            | FullIter
            /// Soft maximum of sequences in sub-clusters
            | ClusterSize of int
            ///	Clustering output file
            | ClusteringOut of string
            /// Use Kimura distance correction for aligned sequences (default no)
            | UseKimura
            /// convert distances into percent identities (default no)
            | PercentID
    
        let internal stringOfClusteringCustom (c:ClusteringCustom) = 
            match c with
            | DistanceMatrixInput c -> sprintf "--distmat-in=%s " c
            | DistanceMatrixOutput c -> sprintf "--distmat-out=%s " c
            | GuideTreeInput c -> sprintf "--guidetree-in=%s " c
            | GuideTreeOutput c -> sprintf "--guidetree-out=%s " c
            | Full -> "--full "
            | FullIter -> "--full-iter "
            | ClusterSize i -> sprintf "--cluster-size=%i " i
            | ClusteringOut c -> sprintf "--clustering-out=%s " c
            | UseKimura -> "--use-kimura "
            | PercentID -> "--percent-id "

        ///Optional modifiers for input
        type OutputCustom =
            ///	MSA output file format (default: fasta)
            | Format of FileFormat
            ///	in Clustal format print residue numbers (default no)
            | ResidueNumber 
            ///	number of residues before line-wrap in output
            | Wrap of int
            /// Aligned sequences are ordered according to guide tree instead of input order
            | OutputOrderAsTree

        let internal stringOfOutputCustom (o:OutputCustom) = 
            match o with
            | Format f -> stringOfFileFormatOut f
            | ResidueNumber -> "--residuenumber "
            | Wrap i -> sprintf "--wrap=%i " i
            | OutputOrderAsTree -> "--output-order=tree-order "
        
        ///Specify maximum number of iterations for given step
        type IterationCustom =
            /// Number of (combined guide tree/HMM) iterations
            | Iterations of int
            /// Maximum guide tree iterations
            | MaxGuideTreeIterations of int
            ///  Maximum number of HMM iterations
            | MaxHMMIterations of int
    
        let internal stringOfIterationCustom (i:IterationCustom) =
            match i with
            | Iterations i -> sprintf "--iter=%i " i
            | MaxGuideTreeIterations i -> sprintf "--max-guidetree-iterations=%i " i
            | MaxHMMIterations i -> sprintf "--max-hmm-iterations=%i " i

    
        /// Will exit early, if exceeded
        type LimitsCustom =
            /// Maximum allowed number of sequences
            | MaxSeqNumber of int
            /// Maximum allowed sequence length
            | MaxSeqLength of int
    
        let internal stringOfLimits (l:LimitsCustom) =
            match l with
            | MaxSeqNumber i -> sprintf "--maxnumseq=%i " i
            | MaxSeqLength i -> sprintf "--maxseqlen=%i " i

        ///Optional, miscallaneous modifiers 
        type MiscallaneousCustom =
            /// Set options automatically (might overwrite some of your options)
            | Auto
            /// Number of processors to use
            | Threads of int
            /// Log all non-essential output to this file
            | Log of string
            /// Print help and exit
            //| Help
            /// Verbose output (ranging from 0 [nonverbose,standard] to 3 [very verbose,everything above 3 is set to 3])
            | VerboseLevel of int
            /// Print version information and exit
            | Version
            /// Print long version information and exit
            | LongVersion
            /// Force file overwriting
            | Force
    
        let internal stringOfMiscallaneous (m:MiscallaneousCustom) =
            match m with
            | Auto -> "--auto "
            | Threads i -> sprintf "--threads=%i " i
            | Log s -> sprintf "--log=%s " s
            //| Help -> "--help "
            | VerboseLevel i -> 
                let sb = System.Text.StringBuilder()
                let n = if i < 0 then 0; elif i > 3 then 3; else i
                let s = 
                    for i = 0 to (n-1) do 
                        sb.Append("-v ") |> ignore
                    sb.ToString()
                sb.Clear() |> ignore
                s
            | Version -> "--version "
            | LongVersion -> "--long-version "
            | Force -> "--force "

        ///Collection of parameters for specifying clustalo alignment
        type ClustalParams = 
            /// Specify input parameters
            | Input of seq<InputCustom>
            /// Specify output parameters
            | Output of seq<OutputCustom>
            /// Specify clustering parameters
            | Clustering of seq<ClusteringCustom>
            /// Specify iteration parameters
            | Iteration of seq<IterationCustom>
            /// Specify limits parameters
            | Limits of seq<LimitsCustom>
            /// Specify miscallaneous parameters
            | Miscallaneous of seq<MiscallaneousCustom>



        ///Create argument string for clustal parameter
        let stringOfClustalParams (c:ClustalParams) = 
            let iterCustom f s =
                Seq.map f s
                |> String.concat ""
            match c with
            | Input s -> iterCustom stringOfInputCustom s
            | Output s -> iterCustom stringOfOutputCustom s
            | Clustering s -> iterCustom stringOfClusteringCustom s
            | Iteration s -> iterCustom stringOfIterationCustom s
            | Limits s -> iterCustom stringOfLimits s
            | Miscallaneous s -> iterCustom stringOfMiscallaneous s

    open Parameters                

    ///Specify the type of input and assign file path
    type Input = 
        ///Use this option to make a multiple alignment from a set of sequences. A sequence file must contain more than one sequence (at least two sequences).
        | SequenceFile of string 
        ///Use this option to align two alignments (profiles) together.
        | TwoProfiles of string * string 
        /// Use this option to add new sequences to an existing alignment.
        | SequenceFileAndProfile of string * string
        /// Use this option to make a new multiple alignment of sequences from the input file and use the HMM as a guide (EPA).
        | SequenceFileAndHMM of string * string
    
    let internal stringOfInputType (i:Input) = 
        match i with
        | SequenceFile s -> sprintf "-i %s " s
        | TwoProfiles (s1,s2) -> sprintf "--p1 %s --p2 %s " s1 s2
        | SequenceFileAndProfile (s1,s2) -> sprintf "-i %s --p1 %s " s1 s2
        | SequenceFileAndHMM (s1,s2) -> sprintf "-i %s --hmm-in %s " s1 s2