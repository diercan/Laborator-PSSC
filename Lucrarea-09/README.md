# Lucrarea 6: Comunicare între contexte folosind evenimente

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: trimiterea de evenimente dupa terminarea procesării către contextul facturării și cel al livrării

**Sarcina 1**

Analizați și rulați soluția din directorul exemple. Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

În contextul workflow-ului pentru plasarea unei comenzi realizați următoarele:
* generați un eveniment care să indice faptul că o comandă a fost preluată
* procesați evenimentul pentru a genera factura (se va apela procesul de generare a facturii)
* procesați evenimentul pentru a iniția livrarea (se va apale a procesul de livrare)
* trebuie implementat la alegere fie workflow-ul pentru a genera factura fie cel pentru a iniția livrarea


