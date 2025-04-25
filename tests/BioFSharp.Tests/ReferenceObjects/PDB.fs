namespace BioFSharp.Tests.ReferenceObjects

module PDB =

    open BioFSharp.FileFormats.PDB

    let atomSingleton = """ATOM   1058  N   ARG A 141      -6.466  12.036 -10.348  7.00 19.11           N  """

    let atomSequence = TestingUtils.readEmbeddedDocument "ATOMSequence.txt"

    let hetatmSingleton = """HETATM 1109  CAD HEM A   1       7.618   5.696 -20.432  6.00 21.38           C  """

    let hetatmSequence = TestingUtils.readEmbeddedDocument "HETATMSequence.txt"

    let terSingleton = """TER    1070      ARG A 141""".PadRight(80, ' ')

    let glucagon = TestingUtils.readEmbeddedDocument "Glucagon.txt"
    
    let HasA = TestingUtils.readEmbeddedDocument "HasA.txt"