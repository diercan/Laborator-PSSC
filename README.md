# Laborator PSSC — Proiectarea Sistemelor Software Complexe

Materialele de laborator pentru cursul de Proiectarea Sistemelor Software Complexe (PSSC). Exemplele urmăresc publicarea notelor unui examen; temele de laborator urmăresc coșul de cumpărături al unui magazin virtual. Ambele domenii sunt modelate în stil *Domain-Driven Design* funcțional, după cartea lui Scott Wlaschin, *Domain Modeling Made Functional*.

## Cerințe preliminare

* **.NET 10 SDK** — versiunea exactă e fixată în [global.json](global.json); verificați cu `dotnet --version`.
* Un IDE: **Visual Studio 2026**, **JetBrains Rider 2025.3+** sau **VS Code + C# Dev Kit**.
* **Docker Desktop** (opțional) — necesar pentru rularea locală a Lucrărilor 5-8 (SQL Server și emulatorul Azure Service Bus); vedeți [compose.yaml](compose.yaml).
* **GitHub Copilot** sau un asistent AI echivalent (ChatGPT, Claude) — pentru Lucrarea 4.

### De ce .NET 10?

Exemplele folosesc C# 14 (membri de extensie `extension(...)`, cuvântul cheie `field`) și nu compilează cu SDK-uri mai vechi. Fișierul [global.json](global.json) fixează SDK-ul folosit de `dotnet` la rădăcina depozitului. Dacă build-ul raportează o versiune de limbaj nesuportată (de exemplu `CS9260`, `CS8652`) sau `NETSDK1045`, instalați .NET 10 de la [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).

## Lucrări

| # | Lucrare | Concepte | Cod |
|---|---|---|---|
| 1 | Mediul de lucru și aplicații consolă | `record`, `switch expressions`, `with` | — |
| 2 | Sistem de tipuri DDD | obiecte-valoare, stări închise, `Result` | [Lucrarea-02/Exemple/Examples.slnx](Lucrarea-02/Exemple/Examples.slnx) |
| 3 | Workflow DDD | operații pure, compunere, evenimente și erori, teste | [Lucrarea-03/Exemple/Examples.slnx](Lucrarea-03/Exemple/Examples.slnx) |
| 4 | Proiectare asistată de AI | Event Storming, GitHub Copilot | referință: [Lucrarea 3](Lucrarea-03/README.md) |
| 5 | Persistență cu EF Core | repository-uri, I/O la marginile workflow-ului | [Lucrarea-05/Exemple/Examples.slnx](Lucrarea-05/Exemple/Examples.slnx) · [SQL/create-db.sql](Lucrarea-05/SQL/create-db.sql) |
| 6 | API Web | ASP.NET Core, API minimal, OpenAPI | [Lucrarea-06/Exemple/Examples.slnx](Lucrarea-06/Exemple/Examples.slnx) |
| 7 | Comunicare sincronă | `HttpClient` tipizat, reziliență, contracte partajate | [Lucrarea-07/Exemple/Examples.slnx](Lucrarea-07/Exemple/Examples.slnx) |
| 8 | Comunicare asincronă | evenimente, Azure Service Bus, worker | [Lucrarea-08/Exemple/Examples.slnx](Lucrarea-08/Exemple/Examples.slnx) |
| — | [Proiect](Proiect/README.md) | proiectul de semestru | — |

## Structura unui laborator

Fiecare `Lucrarea-NN/README.md` descrie tema și cerințele, iar `Lucrarea-NN/Exemple/` conține o soluție exemplu (`Examples.slnx`) care ilustrează conceptele. Proiectele `Examples.Functional`, `Examples.Domain`, `Examples.Domain.Tests`, `Examples.Data` și `SQL/create-db.sql` sunt identice în Lucrările 3-8 (verificat automat de `tools/Check-LabConsistency.ps1`); o corecție într-unul dintre ele se propagă cu `pwsh tools/Sync-Labs.ps1 -Source 08 -Target "07","06","05","03"`. Dacă lucrați dintr-o copie a unui singur laborator, copiați și fișierele din rădăcina depozitului (`global.json`, `.editorconfig`, `NuGet.config`) — fără ele, `dotnet build` nu găsește versiunile pachetelor NuGet.

Fiecare laborator cu cod are în `Lucrarea-NN/docs/adr/` un *Architecture Decision Record* (ADR) cu decizia pe care o adaugă față de laboratorul anterior, alternativele respinse și consecințele lor. Lanțul se citește ca istoricul arhitecturii exemplului: [ADR-0001](Lucrarea-02/docs/adr/0001-sistem-de-tipuri-inchis-cu-erori-ca-valori.md) sistemul de tipuri → [ADR-0002](Lucrarea-03/docs/adr/0002-operatii-pure-compuse-in-workflow.md) operații și workflow → [ADR-0003](Lucrarea-05/docs/adr/0003-persistenta-sql-first-la-marginile-workflow-ului.md) persistență → [ADR-0004](Lucrarea-06/docs/adr/0004-api-minimal-cu-rezultate-tipizate.md) API Web → [ADR-0005](Lucrarea-07/docs/adr/0005-contracte-partajate-si-client-http-rezilient.md) comunicare sincronă / [ADR-0006](Lucrarea-08/docs/adr/0006-evenimente-de-integrare-prin-service-bus-si-cloudevents.md) comunicare asincronă.

## Referințe

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/), Pragmatic Bookshelf, 2018
