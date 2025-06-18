#!/usr/bin/env python3 
"""
Berechnet den SASA (Solvent Accessible Surface Area) pro Kette
für eine PDB-Struktur mit FreeSASA und schreibt die Ergebnisse in eine CSV-Datei.
"""

import os
import pandas as pd
import freesasa
from collections import defaultdict

def set_working_directory():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    os.chdir(script_dir)
    print("Neues Arbeitsverzeichnis:", os.getcwd())
    return script_dir

def calculate_sasa_per_chain_freesasa(pdb_file: str,
                                      probe_radius: float = 1.4,
                                      n_points: int = 100) -> list:
    """
    Berechnet die absolute SASA pro Kette mit FreeSASA.
    Gibt eine Liste von Tupeln (chain_id, abs_sasa) zurück.
    """
    structure = freesasa.Structure(pdb_file)
    params = freesasa.Parameters()
    params.setProbeRadius(probe_radius)
    params.setNPoints(n_points)
    params.setAlgorithm(freesasa.ShrakeRupley)
    result = freesasa.calc(structure, params)
    # Pro Residuum Flächen holen
    residue_areas = result.residueAreas()
    chain_sasa = defaultdict(float)
    for chain_id, resdict in residue_areas.items():
        for resnum, area in resdict.items():
            chain_sasa[chain_id] += area.total
    return list(chain_sasa.items())

def main():
    script_dir = set_working_directory()
    pdb_file = os.path.abspath(os.path.join(script_dir, "../testdata/exampledata.pdb"))

    print("Berechne absolute SASA pro Kette mit FreeSASA …")
    chain_sasa_list = calculate_sasa_per_chain_freesasa(
        pdb_file,
        probe_radius=1.4,
        n_points=100,
    )

    df = pd.DataFrame(chain_sasa_list, columns=["Chain", "Abs_SASA"])

    output_file = os.path.abspath("sasa_per_chain.csv")
    df.to_csv(output_file, index=False)
    print(f"Ergebnisse in '{output_file}' gespeichert.")

if __name__ == "__main__":
    main()

