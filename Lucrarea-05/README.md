# Lucrarea 5: Interacțiunea cu baza de date

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: citirea/scrierea din/în baza de date cu EF Core 10 (abordare *SQL-first*): `DbContext`, `DbSet`, maparea în `OnModelCreating`; repository-uri ca adaptoare pentru porturile domeniului (`IStudentsRepository`, `IGradesRepository`); I/O la marginile workflow-ului (citește din bază → logică pură → scrie în bază).

**Sarcina 1**

Creați baza de date cu [SQL/create-db.sql](SQL/create-db.sql), fie pe un SQL Server local, fie în Docker (`docker compose up -d sqlserver sqlserver-init` — vedeți [compose.yaml](../compose.yaml)). Analizați și rulați soluția din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`). Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

În contextul workflow-ului pentru plasarea unei comenzi realizați următoarele:
* creați o nouă bază de date SQL care va conține următoarele tabele: 
    - Products (Id, Code, Name, Price, QuantityType), 
    - Customers(Id, Code, Name), 
    - Order(Id, Date, DeliveryAddress, CustomerId), 
    - OrderItem(Id, OrderId, ProductId, Quantity)
* implementați un `DbContext` EF Core cu câte un `DbSet` pentru fiecare tabelă și configurați maparea în `OnModelCreating`
* înainte de a executa workflow-ul încărcați starea din baza de date
* implementați funcțiile de verificare a existenței produsului și stocului astfel încât să folosească informații din baza de date
* după executare workflow-ului salvați rezultatul în baza de date

**Decizii de arhitectură**

Alegerile de proiectare ale acestui laborator – abordarea SQL-first, porturile și repository-urile, upsert-ul după numărul matricol, modelul de citire separat – sunt documentate în [ADR-0003](docs/adr/0003-persistenta-sql-first-la-marginile-workflow-ului.md), care continuă [ADR-0002](../Lucrarea-03/docs/adr/0002-operatii-pure-compuse-in-workflow.md) din Lucrarea 3.

**Referințe**

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/ref=sr_1_1?dchild=1&keywords=Domain+Modeling+Made+Functional&qid=1632338254&sr=8-1), Pragmatic Bookshelf, 2018 — cap. 12 (persistență la marginile sistemului)

[2] Microsoft Documentation, [EF Core](https://learn.microsoft.com/ef/core/)

[3] Microsoft Documentation, [Rulați SQL Server în Docker](https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker)
