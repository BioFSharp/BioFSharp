namespace BioFSharp.Tests.ReferenceObjects

module PDBParser =   
    open System.IO
    
    
    let pdbLines = seq {
                        "HEADER    RubisCO Activase"
                        "TITLE     Testparsing"
                        "COMPND    MOLECULE: RIBULOSE BISPHOSPHATE CARBOXYLASE/OXYGENASE ACTIVASE"
                        "SOURCE    Arabidopsis"
                        "KEYWDS    RubisCO Activase, Test"
                        "EXPDTA    X-RAY DIFFRACTION"
                        "AUTHOR    John Doe"
                        "REMARK    RESOLUTION.    2.90 ANGSTROMS."
                        "REMARK    Example remark 2"
                        "CAVEAT    Incorrect residue numbering"
                    }

    let testPDBAtomlines = seq {
                            "ATOM   2152  C   ALA A 362      31.316   0.898   7.414  1.00146.74           C  "  
                            "ATOM   2153  O   ALA A 362      31.777   2.039   7.355  1.00 98.10           O  "
                            "ATOM   2154  CB  ALA A 362      32.212  -0.438   5.488  1.00131.30           C  "
                            "TER    2155      ALA A 362                                                      " 
                            "HETATM 2156  S   SO4 A 401      10.325  29.405  12.422  1.00 97.83           S  "  
                        }



    let pdbLines_secStructure = seq {
                        "HEADER    CHAPERONE                               19-AUG-14   4W5W   "
                        "HELIX    9 AA9 LYS A  310  VAL A  316  1                                   7    "
                        "HELIX   10 AB1 THR A  331  THR A  357  1                                  27  "
                        "SHEET    1 AA1 2 MET A  69  MET A  70  0                               "
                        "SHEET    2 AA2 5 LEU A 167  ILE A 169  1  O  PHE A 168   N  ILE A 129 "}

    let pdbLines_modification = seq {
                    "HEADER    CHAPERONE                               19-AUG-14   4W5W   "
                    "MODRES 1ABC SER A 65  SEP    SERINE PHOSPHATE"
                    "MODRES 1ABC LYS A 70  ALY    ACETYLATED LYSINE"
                    "MODRES 1ABC ASN B 120 MSE    METHYLSELENOCYSTEINE"
                }

    let pdbLines_residue = seq {
                    "MODRES 3XYZ MET A  69  MSE   SELENOMETHIONINE"
                    "HELIX   10 AB1 THR A  331  THR A  357  1                                  27 "
                    "SHEET    1 AA1 2 MET A  69  MET A  70  0                                     "
                    "ATOM      1  N   ASN A  65      -3.207  51.767  10.770  1.00101.75           N  "
                    "ATOM     39  SD  MET A  69       9.570  48.802  17.088  1.00105.52           S  "
                    "ATOM     40  CE  MET A  69      10.217  48.523  15.433  1.00102.41           C  "
                    "ATOM     41  N   MET A  70       6.769  45.021  20.326  1.00 91.81           N  "
                    "ATOM   2113  OG1 THR A 357      28.880  11.203  -2.620  1.00 75.24           O  "
                    "ATOM   2114  CG2 THR A 357      26.961  11.154  -1.152  1.00 69.82           C  "
                    "HETATM 2156  S   SO4 A 401A     10.325  29.405  12.422  1.00 97.83           S  "
                    }

    let pdbLines_chains = seq {
                        "SHEET    1 AA1 2 MET A  69  MET A  70  0                                     "
                        "HELIX   10 AB1 THR B  331  THR A  357  1                                  27 "
                        "ATOM      1  N   ASN A  65      -3.207  51.767  10.770  1.00101.75           N  "
                        "ATOM     39  SD  MET A  69       9.570  48.802  17.088  1.00105.52           S  "
                        "ATOM     40  CE  MET A  69      10.217  48.523  15.433  1.00102.41           C  "
                        "ATOM     41  N   MET A  70       6.769  45.021  20.326  1.00 91.81           N  "
                        "ATOM   2113  OG1 THR B 357      28.880  11.203  -2.620  1.00 75.24           O  "
                        "ATOM   2114  CG2 THR B 357      26.961  11.154  -1.152  1.00 69.82           C  "
                        "HETATM 2156  S   SO4 B 401A     10.325  29.405  12.422  1.00 97.83           S  "
                        }

    let pdbLines_linkages = seq {
                    "ATOM   4502  CA  CYS A  25      11.111  22.222  33.333  1.00 10.00           C  "
                    "ATOM   4503  CB  CYS A  25      12.111  23.222  34.333  1.00 10.00           C  "
                    "ATOM   4504  SG  CYS A  25      13.111  24.222  35.333  1.00 10.00           S  "
                    "ATOM   4505  CA  CYS B  80      14.111  25.222  36.333  1.00 10.00           C  "
                    "ATOM   4506  CB  TYR B  85      15.111  26.222  37.333  1.00 10.00           C  "
                    "ATOM   4507  CA  SER A  15      16.111  27.222  38.333  1.00 10.00           C  "
                    "ATOM   4508  CB  VAL B  18      17.111  28.222  39.333  1.00 10.00           C  "
                    "SSBOND   1 CYS A   25    CYS B   80                                                  "
                    "LINK         NE2 CYS B  80                FE   TYR B  85     1555   1555  2.12   "
                    "CISPEP   1 SER A   15    VAL B   18                                                  "
                    "CONECT 4503 4502 4504 4505                                                           "
                }

    let invalidPDBContent = seq {
                    "ATOM   4502  CA  CYS A  25      11.111  22.222  33.333  1.00 10.00           C  "
                    "SSBOND   1 CYS A   25    CYS B   80                                                  "
                    "ATOM   4509  CA  GLY A"  // missing informations
                    "LINK   GLY"  // wrong format
                }

    let pdbLines_sites = seq {
                    "SITE     1 AC1  5 GLY A 110  GLN A 111  GLY A 112  LYS A 113 "
                    "SITE     2 AC1  5 SER B 114 "
                    "SITE     1 AC2  3 THR A 250  ARG A 251  ASN B 317 "      
                    "ATOM    355  NZ  LYS A 109      12.172  20.859   9.773  1.00117.05           N  "
                    "ATOM    360  N   GLN A 111      12.790  31.412   9.742  1.00 74.67           N  "
                    "ATOM   1231  CB  THR A 250      10.198  42.193   2.989  1.00 68.40           C  "
                }

    let pdbLines_model = seq {
                    "SITE     1 AC1        A   1 "
                    "SITE     2 AC2  5 SER A  25 "      
                    "MODEL        1                                                           "
                    "ATOM      1  N   MET A   1      20.154  34.198  27.627  1.00 54.69           N  "
                    "ATOM      2  CA  MET A   1      21.560  34.601  27.379  1.00 54.10           C  "
                    "HETATM    3  C   MET B   2      21.933  36.041  27.761  1.00 53.59           C  "
                    "ATOM   4502  CA  CYS A  25      11.111  22.222  33.333  1.00 10.00           C  "
                    "ATOM   4503  CB  CYS A  25      12.111  23.222  34.333  1.00 10.00           C  "
                    "ATOM   4504  SG  CYS A  25      13.111  24.222  35.333  1.00 10.00           S  "
                    "ATOM   4505  CA  CYS B  80      14.111  25.222  36.333  1.00 10.00           C  "
                    "ATOM   4506  CB  TYR B  85      15.111  26.222  37.333  1.00 10.00           C  "
                    "ATOM   4507  CA  SER A  15      16.111  27.222  38.333  1.00 10.00           C  "
                    "ATOM   4508  CB  VAL B  18      17.111  28.222  39.333  1.00 10.00           C  "
                    "SSBOND   1 CYS A   25    CYS B   80                                                  "
                    "TER                                                                    "
                    "MODEL        2                                                           "
                    "ATOM      1  N   GLY A   1      10.123  20.456  30.789  1.00 60.00           N  "
                    "ATOM      2  CA  GLY A   1      11.456  21.789  31.123  1.00 61.00           C  "
                    "ATOM      3  C   GLY A   1      12.789  22.123  32.456  1.00 62.00           C  "
                    "TER                                                                    "
                    "ENDMDL                                                                 "
                }

    let pdbLines_structure = seq {
                    "HEADER    RubisCO Activase"
                    "TITLE     Testparsing"
                    "COMPND    MOLECULE: RIBULOSE BISPHOSPHATE CARBOXYLASE/OXYGENASE ACTIVASE"
                    "SOURCE    Arabidopsis"
                    "KEYWDS    RubisCO Activase, Test"
                    "EXPDTA    X-RAY DIFFRACTION"
                    "AUTHOR    John Doe"
                    "REMARK    RESOLUTION.    2.90 ANGSTROMS."
                    "REMARK    Example remark 2"
                    "MODEL        1                                                           "
                    "ATOM      1  N   MET A   1      20.154  34.198  27.627  1.00 54.69           N  "
                    "ATOM      2  CA  MET A   1      21.560  34.601  27.379  1.00 54.10           C  "
                    "HETATM    3  C   MET B   2      21.933  36.041  27.761  1.00 53.59           C  "
                    "ATOM   4502  CA  CYS A  25      11.111  22.222  33.333  1.00 10.00           C  "
                    "ATOM   4503  CB  CYS A  25      12.111  23.222  34.333  1.00 10.00           C  "
                    "ATOM   4504  SG  CYS A  25      13.111  24.222  35.333  1.00 10.00           S  "
                    "ATOM   4505  CA  CYS B  80      14.111  25.222  36.333  1.00 10.00           C  "
                    "ATOM   4506  CB  TYR B  85      15.111  26.222  37.333  1.00 10.00           C  "
                    "ATOM   4507  CA  SER A  15      16.111  27.222  38.333  1.00 10.00           C  "
                    "ATOM   4508  CB  VAL B  18      17.111  28.222  39.333  1.00 10.00           C  "
                    "SSBOND   1 CYS A   25    CYS B   80                                                  "
                    "TER                                                                    "
                    "MODEL        2                                                           "
                    "ATOM      1  N   GLY C   1      10.123  20.456  30.789  1.00 60.00           N  "
                    "ATOM      2  CA  GLY C   1      11.456  21.789  31.123  1.00 61.00           C  "
                    "ATOM      3  C   GLY D   1      12.789  22.123  32.456  1.00 62.00           C  "
                    "TER                                                                    "
                    "ENDMDL                                                                 "
                }
                