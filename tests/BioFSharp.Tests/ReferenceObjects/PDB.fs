namespace BioFSharp.Tests.ReferenceObjects

module PDB =

    open BioFSharp.FileFormats.PDB

    let atomSingleton = """ATOM   1058  N   ARG A 141      -6.466  12.036 -10.348  7.00 19.11           N  """

    let atomSequence = TestingUtils.readEmbeddedDocument "ATOMSequence.txt"

    let hetatmSingleton = """HETATM 1109  CAD HEM A   1       7.618   5.696 -20.432  6.00 21.38           C  """

    let hetatmSequence = TestingUtils.readEmbeddedDocument "HETATMSequence.txt"

    let terSingleton = """TER    1070      ARG A 141""".PadRight(80, ' ')
    
    let terWithInsertionCode = """TER    1070      ARG A 141A""".PadRight(80, ' ')
     
    let modelSingleton = """MODEL        1""".PadRight(80, ' ')

    let endModelSingleton = """ENDMDL""".PadRight(80, ' ')

    let modelSequence =
        [|
            """MODEL        1""".PadRight(80, ' ')
            """ATOM      1  N   ALA A   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """ENDMDL""".PadRight(80, ' ')
            """MODEL        2""".PadRight(80, ' ')
            """ATOM      1  N   ALA A   1      12.104   6.134  -6.504  1.00  0.00           N  """
            """ENDMDL""".PadRight(80, ' ')
        |]
        
    let terNucleotide = """TER     487       DA B  24""".PadRight(80, ' ')

    let glucagon = TestingUtils.readEmbeddedDocument "Glucagon.txt"
    
    let HasA = TestingUtils.readEmbeddedDocument "HasA.txt"