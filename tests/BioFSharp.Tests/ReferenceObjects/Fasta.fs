namespace BioFSharp.Tests.ReferenceObjects

module Fasta =

    open BioFSharp
    open BioFSharp.AminoAcids
    open BioFSharp.FileFormats.Fasta

    let testString_one_entry = """>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
"""                                 .ReplaceLineEndings()

    let testString_two_entries = """>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
>gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]
LCLYTHIGRNIYYGSYLYSETWNTGIMLLLITMATAFMGYVLPWGQMSFWGATVITNLFSAIPYIGTNLVEWIWGGFSVD
KATLNRFFAFHFILPFTMVALAGVHLTFLHETGSNNPLGLTSDSDKIPFHPYYTIKDFLGLLILILLLLLLALLSPDMLG
DPDNHMPADPLNTPLHIKPEWYFLFAYAILRSVPNKLGGVLALFLSIVILGLMPFLHTSKHRSMMLRPLSQALFWTLTMD
LLTLTWIGSQPVEYPYTIIGQMASILYFSIILAFLPIAGXIENY
"""                                 .ReplaceLineEndings()

    let expectedFastaItem =
        FastaItem.create
            "gi|5524211|gb|AAD44166.1| cytochrome b [Elephas maximus maximus]"
            (
                [
                    Leu;Cys;Leu;Tyr;Thr;His;Ile;Gly;Arg;Asn;Ile;Tyr;Tyr;Gly;Ser;Tyr;Leu;Tyr;Ser;Glu
                    Thr;Trp;Asn;Thr;Gly;Ile;Met;Leu;Leu;Leu;Ile;Thr;Met;Ala;Thr;Ala;Phe;Met;Gly;Tyr
                    Val;Leu;Pro;Trp;Gly;Gln;Met;Ser;Phe;Trp;Gly;Ala;Thr;Val;Ile;Thr;Asn;Leu;Phe;Ser
                    Ala;Ile;Pro;Tyr;Ile;Gly;Thr;Asn;Leu;Val;Glu;Trp;Ile;Trp;Gly;Gly;Phe;Ser;Val;Asp
                    Lys;Ala;Thr;Leu;Asn;Arg;Phe;Phe;Ala;Phe;His;Phe;Ile;Leu;Pro;Phe;Thr;Met;Val;Ala
                    Leu;Ala;Gly;Val;His;Leu;Thr;Phe;Leu;His;Glu;Thr;Gly;Ser;Asn;Asn;Pro;Leu;Gly;Leu
                    Thr;Ser;Asp;Ser;Asp;Lys;Ile;Pro;Phe;His;Pro;Tyr;Tyr;Thr;Ile;Lys;Asp;Phe;Leu;Gly
                    Leu;Leu;Ile;Leu;Ile;Leu;Leu;Leu;Leu;Leu;Leu;Ala;Leu;Leu;Ser;Pro;Asp;Met;Leu;Gly
                    Asp;Pro;Asp;Asn;His;Met;Pro;Ala;Asp;Pro;Leu;Asn;Thr;Pro;Leu;His;Ile;Lys;Pro;Glu
                    Trp;Tyr;Phe;Leu;Phe;Ala;Tyr;Ala;Ile;Leu;Arg;Ser;Val;Pro;Asn;Lys;Leu;Gly;Gly;Val
                    Leu;Ala;Leu;Phe;Leu;Ser;Ile;Val;Ile;Leu;Gly;Leu;Met;Pro;Phe;Leu;His;Thr;Ser;Lys
                    His;Arg;Ser;Met;Met;Leu;Arg;Pro;Leu;Ser;Gln;Ala;Leu;Phe;Trp;Thr;Leu;Thr;Met;Asp
                    Leu;Leu;Thr;Leu;Thr;Trp;Ile;Gly;Ser;Gln;Pro;Val;Glu;Tyr;Pro;Tyr;Thr;Ile;Ile;Gly
                    Gln;Met;Ala;Ser;Ile;Leu;Tyr;Phe;Ser;Ile;Ile;Leu;Ala;Phe;Leu;Pro;Ile;Ala;Gly;Xaa
                    Ile;Glu;Asn;Tyr
                ]
            )