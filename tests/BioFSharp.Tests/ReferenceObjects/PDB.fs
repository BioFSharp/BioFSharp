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

    let chainSequence =
        [|
            """ATOM      1  N   ALA A   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """ATOM      2  N   GLY A   2      11.104   6.134  -6.504  1.00  0.00           N  """
            """TER       3      GLY A   2""".PadRight(80, ' ')
            """ATOM      4  N   SER B   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """TER       5      SER B   1""".PadRight(80, ' ')
            """HETATM    6  O   HOH A 101      11.104   6.134  -6.504  1.00  0.00           O  """
        |]

    let insertionCodeSequence =
        [|
            """ATOM      1  N   ALA A  52      11.104   6.134  -6.504  1.00  0.00           N  """
            """ATOM      2  N   GLY A  52A     11.104   6.134  -6.504  1.00  0.00           N  """
            """ATOM      3  N   SER A  53      11.104   6.134  -6.504  1.00  0.00           N  """
        |]

    let modelSequenceWithTerminators =
        [|
            """MODEL        1""".PadRight(80, ' ')
            """ATOM      1  N   ALA A   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """TER       2      ALA A   1""".PadRight(80, ' ')
            """ENDMDL""".PadRight(80, ' ')
            """MODEL        2""".PadRight(80, ' ')
            """ATOM      1  N   ALA A   1      12.104   6.134  -6.504  1.00  0.00           N  """
            """TER       2      ALA A   1""".PadRight(80, ' ')
            """ENDMDL""".PadRight(80, ' ')
        |]
        
    let heteroOrderSequence =
        [|
            """ATOM      1  N   ALA A   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """TER       2      ALA A   1""".PadRight(80, ' ')
            """ATOM      3  N   GLY B   1      11.104   6.134  -6.504  1.00  0.00           N  """
            """TER       4      GLY B   1""".PadRight(80, ' ')
            """HETATM    5 FE   HEM A 201      11.104   6.134  -6.504  1.00  0.00          FE  """
            """HETATM    6 FE   HEM B 201      11.104   6.134  -6.504  1.00  0.00          FE  """
            """HETATM    7  O   HOH A 301      11.104   6.134  -6.504  1.00  0.00           O  """
            """HETATM    8  O   HOH B 301      11.104   6.134  -6.504  1.00  0.00           O  """
        |]

    let terNucleotide = """TER     487       DA B  24""".PadRight(80, ' ')

    let glucagon = TestingUtils.readEmbeddedDocument "Glucagon.txt"
    
    let HasA = TestingUtils.readEmbeddedDocument "HasA.txt"
    
    let contactSequence =
        [|
            """ATOM      1  N   ALA A   1       0.000   0.000   0.000  1.00  0.00           N  """
            """ATOM      2  CA  ALA A   1       1.000   0.000   0.000  1.00  0.00           C  """
            """ATOM      3  N   GLY A   2       4.500   0.000   0.000  1.00  0.00           N  """
            """ATOM      4  CA  GLY A   2       5.500   0.000   0.000  1.00  0.00           C  """
            """ATOM      5  N   SER A   3      20.000   0.000   0.000  1.00  0.00           N  """
            """HETATM    6  O   HOH A 101       5.500   3.000   0.000  1.00  0.00           O  """
        |]
        