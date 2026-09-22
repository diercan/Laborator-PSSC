# Proiect PSSC

Implementarea unui sistem software pentru preluarea de comenzi, facturare și expediere. Fiecare echipă (3 studenți) va alege câte un workflow din cele trei contexte pe care le va defini și implementa. Exemple de workflow-uri: preluare comandă, anulare comandă, modificare comandă, returnare comandă.

Fiecare echipă va crea un proiect privat pe GitHub unde va pune codul sursă. La final se va urmări existența unui istoric din care să rezulte colaborarea între membrii echipei.

## Etape

1. **Descoperirea domeniului** — folosiți Event Storming (vedeți [Lucrarea 4, Partea 1](../Lucrarea-04/README.md)) pentru a identifica evenimentele, comenzile și agregările contextului ales. Pentru definirea workflow-urilor se va folosi notația prezentată în cursul al doilea. Workflow-urile vor trebui prezentate în **săptămâna a 7-a**, la orarul de laborator.
2. **Implementare** — în .NET 10, după modelul funcțional prezentat la curs și exersat la laborator: sistemul de tipuri ca în [Lucrarea 2](../Lucrarea-02/README.md), operațiile și compunerea lor ca în [Lucrarea 3](../Lucrarea-03/README.md), persistența ca în [Lucrarea 5](../Lucrarea-05/README.md), API-ul ca în [Lucrarea 6](../Lucrarea-06/README.md). Cele trei workflow-uri (comandă, facturare, expediere) trebuie să comunice între ele prin canale de comunicare asincrone, ca în [Lucrarea 8](../Lucrarea-08/README.md).

## Evaluare

Se va puncta: calitatea sistemului de tipuri, modul în care au fost implementate operațiile specifice domeniului, modul în care au fost reprezentate rezultatele (`Result`, evenimente, erori), modul în care au fost compuse operațiile, interacțiunea cu baza de date, comunicarea asincronă între cele trei workflow-uri, gradul de finalizare și istoricul de colaborare din Git.
