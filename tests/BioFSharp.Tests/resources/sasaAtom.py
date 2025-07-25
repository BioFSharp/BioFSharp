import os
import freesasa
import pandas as pd
from Bio.PDB import PDBParser

# Set working directory

script_dir = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_dir)
print("new working directory:", os.getcwd())

def compute_sasa_to_csv(pdb_path: str, output_csv_path: str, probe: float = 3.7, npoints: int = 100):
    """
    Args:
        pdb_path (str): path to pdb file.
        output_csv_path (str): output path
        probe (float): radius of probe.
        npoints (int): nr of testpoints
        
    """

    # load structure with Freesasa
    structure = freesasa.Structure(pdb_path)
    
    # set parameter
    params = freesasa.Parameters()
    params.setProbeRadius(probe)
    params.setNPoints(npoints)
    params.setAlgorithm(freesasa.ShrakeRupley)

    # compute SASA 
    result = freesasa.calc(structure, params)

    # serialnr with freesasa
    def get_freesasa_serials(pdb_file):
        parser = PDBParser(QUIET=True)
        structure = parser.get_structure("X", pdb_file)
        serials = []
        for model in structure:
            for chain in model:
                for residue in chain:
                    if residue.id[0] != " ":  # kein HETATM
                        continue
                    for atom in residue:
                        if atom.element.upper() == "H":
                            continue
                        if atom.get_altloc() not in (" ", "A"):
                            continue
                        serials.append(atom.get_serial_number())
        return serials

    serials = get_freesasa_serials(pdb_path)
    assert len(serials) == structure.nAtoms(), "Mismatch in atom count between FreeSASA and Biopython"

    # create frame
    rows = []
    for i in range(structure.nAtoms()):
        rows.append({
            "serial":       serials[i],
            "chain":        structure.chainLabel(i),
            "residue_num":  structure.residueNumber(i),
            "residue":      structure.residueName(i),
            "atom_name":    structure.atomName(i),
            "sasa":         result.atomArea(i)
        })

    df = pd.DataFrame(rows)

    # output 
    df.to_csv(output_csv_path, index=False, encoding="utf-8")
    print(f"SASA-Ergebnisse in '{output_csv_path}' gespeichert.")
    return df


# function call

if __name__ == "__main__":
    pdb_file = "Cre01g001550t11.pdb"
    output_file = "sasa_per_atom_realworld.csv"
    compute_sasa_to_csv(pdb_file, output_file)
    
if __name__ == "__main__":
    pdb_file = "rubisCOActivase.pdb"
    output_file = "sasa_per_atom.csv"
    compute_sasa_to_csv(pdb_file, output_file)

