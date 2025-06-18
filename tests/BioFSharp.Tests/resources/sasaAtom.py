#!/usr/bin/env python3
import os
import freesasa
import pandas as pd
from Bio.PDB import PDBParser

# 1. Arbeitsverzeichnis auf den Ordner setzen, in dem das Skript liegt
script_dir = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_dir)
print("Neues Arbeitsverzeichnis:", os.getcwd())

# 2. Parameter
probe     = 1.4    # Probe‐Radius in Å
npoints   = 100    # Testpunkte pro Atom

# 3. PDB-Datei laden
pdb_file  = "../testdata/exampledata.pdb"
structure = freesasa.Structure(pdb_file)

# 4. Shrake–Rupley Parameter setzen
params = freesasa.Parameters()
params.setProbeRadius(probe)
params.setNPoints(npoints)
params.setAlgorithm(freesasa.ShrakeRupley)

# 5. SASA berechnen
result = freesasa.calc(structure, params)

# --- Serialnummern wie in FreeSASA filtern (nur "echte" Atome, kein H, nur AltLoc " " oder "A") ---
def get_freesasa_serials(pdb_file):
    parser = PDBParser(QUIET=True)
    structure = parser.get_structure("X", pdb_file)
    serials = []
    for model in structure:
        for chain in model:
            for residue in chain:
                # Nur "ATOM"-Residues, keine HETATM (Residue-ID[0] == " ")
                if residue.id[0] != " ":
                    continue
                for atom in residue:
                    # FreeSASA nimmt keine Wasserstoffatome
                    element = atom.element.upper()
                    if element == "H":
                        continue
                    # AltLoc: nur " " (keine alternativen Konformationen) oder "A"
                    if atom.get_altloc() not in (" ", "A"):
                        continue
                    serials.append(atom.get_serial_number())
    return serials


serials = get_freesasa_serials(pdb_file)

# Sicherstellen, dass die Anzahl zu nAtoms passt
if len(serials) != structure.nAtoms():
    raise RuntimeError(
        f"Anzahl gefilterter Atome ({len(serials)}) stimmt nicht mit FreeSASA ({structure.nAtoms()}) überein!"
    )

# 6. Pro-Atom-Flächen holen und DataFrame bauen (jetzt mit Serial)
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

# 7. Ausgabe auf der Konsole
print(df.head(20).to_string(index=False))

# 8. Ergebnisse als CSV speichern
df.to_csv("sasa_per_atom.csv", index=False)
print("\nErgebnisse in 'sasa_per_atom.csv' gespeichert.")


