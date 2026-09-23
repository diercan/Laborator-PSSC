# Lucrarea 6: Folosirea unui model DDD pentru a implementa un API Web

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: expunerea workflow-ului printr-un API Web ASP.NET Core (API minimal, endpoint-uri ca funcții); document OpenAPI generat automat (`/openapi/v1.json`) cu interfață interactivă Swagger UI (`/swagger`); maparea rezultatului workflow-ului (`Result`) la coduri HTTP (200 pe succes, 400 `ValidationProblemDetails` pe eșec de validare).

**Sarcina 1**

Necesită baza de date din [Lucrarea 5](../Lucrarea-05/README.md). Analizați și rulați soluția din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`). Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

În contextul workflow-ului pentru plasarea unei comenzi realizați următoarele:
* implementare endpoint pentru a vizualiza coșul de cumpărături
* implementați endpoint adăugare produs în coșul de cumpărături
* implementare endpoint marcare coș de cumpărături plătit

**Rulare**

```
dotnet run --project Exemple/Examples.Api
```

Deschideți `https://localhost:7195/swagger`.

**Decizii de arhitectură**

Alegerile de proiectare ale acestui laborator – API minimal cu rezultate tipizate, maparea `Result` la 200/400, documentul OpenAPI generat nativ, validarea doar în domeniu – sunt documentate în [ADR-0004](docs/adr/0004-api-minimal-cu-rezultate-tipizate.md), care continuă [ADR-0003](../Lucrarea-05/docs/adr/0003-persistenta-sql-first-la-marginile-workflow-ului.md) din Lucrarea 5.

**Referințe**

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/ref=sr_1_1?dchild=1&keywords=Domain+Modeling+Made+Functional&qid=1632338254&sr=8-1), Pragmatic Bookshelf, 2018 — cap. 11 (DTO-uri la graniță)

[2] Microsoft Documentation, [Minimal APIs în ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
