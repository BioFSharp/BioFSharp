#!/usr/bin/env python3
"""
Berechnet den SASA (Solvent Accessible Surface Area) pro Residue
für eine PDB-Struktur mit FreeSASA und schreibt die Ergebnisse in eine CSV-Datei.
"""

import os
import pandas as pd
import freesasa

# Arbeitsverzeichnis auf den Ordner setzen, in dem das Skript liegt
script_dir = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_dir)
print("Neues Arbeitsverzeichnis:", os.getcwd())

def calculate_sasa_per_residue_freesasa(pdb_file: str,
                                        probe_radius: float = 1.4,
                                        n_points: int = 100) -> list:
    """
    Berechnet die absolute SASA pro Residue mit FreeSASA.
    Gibt eine Liste von Tupeln (chain_id, resname, resnum, abs_sasa) zurück.
    """
    structure = freesasa.Structure(pdb_file)
    params = freesasa.Parameters()
    params.setProbeRadius(probe_radius)
    params.setNPoints(n_points)
    params.setAlgorithm(freesasa.ShrakeRupley)
    result = freesasa.calc(structure, params)
    residue_areas = result.residueAreas()

    results = []
    for chain_id, resdict in residue_areas.items():
        for resnum, area in resdict.items():
            resname = area.residueType
            abs_sasa = area.total
            results.append((chain_id, resname, resnum, abs_sasa))
    return results

def main():
    pdb_file = os.path.abspath(os.path.join(script_dir, "../testdata/exampledata.pdb"))

    print("Berechne absolute SASA pro Residue mit FreeSASA …")
    residue_sasa_list = calculate_sasa_per_residue_freesasa(
        pdb_file,
        probe_radius=1.4,
        n_points=100,
    )

    # In ein DataFrame umwandeln
    df = pd.DataFrame(residue_sasa_list, columns=[
        "Chain", "ResName", "ResNum", "Abs_SASA"
    ])

    # CSV-Datei schreiben
    output_file = os.path.abspath("sasa_per_residue.csv")
    df.to_csv(output_file, index=False)
    print(f"Ergebnisse in '{output_file}' gespeichert.")

if __name__ == "__main__":
    main()

