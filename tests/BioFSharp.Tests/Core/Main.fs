namespace BioFSharp.Tests.Core

module All =

    open Expecto

    [<Tests>]
    let main = 
        testList "Core Library" [
            PairwiseAlignmentTests.alignmentTests
            AminoAcidsTests.aminoAcidTests
            DigestionTests.digestionTests
            NucleotideTests.nucleotideTests
            BioSeqTests.bioSeqTests
            BioArrayTests.bioArrayTests
            BioListTests.bioListTests
            ElementsTests.elementsTests
            FormulaTests.formulaTests
            MassTests.massTests
            IBioItem.iBioItemTests
            TaggedSequenceTests.taggedSequencetests
            IsotopicDistributionTests.isotopicDistributionTests
            PhylogeneticTreeTests.phylogeneticTreeTests
        ]
