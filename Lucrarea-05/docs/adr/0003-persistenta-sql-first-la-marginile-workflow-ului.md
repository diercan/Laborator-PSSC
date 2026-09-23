# ADR-0003: Persistența este SQL-first cu EF Core 10, expusă domeniului prin porturi, iar I/O-ul stă exclusiv la marginile workflow-ului

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 5 – Interacțiunea cu baza de date ([README](../../README.md))
* **Extinde**: [ADR-0002](../../../Lucrarea-03/docs/adr/0002-operatii-pure-compuse-in-workflow.md) (operații și workflow)
* **Extinsă de**: [ADR-0004](../../../Lucrarea-06/docs/adr/0004-api-minimal-cu-rezultate-tipizate.md) (API Web)

## Context și problemă

Lucrarea 5 adaugă baza de date: studenții cunoscuți se citesc din tabela `Student`, iar catalogul publicat se scrie în tabela `Grade`. Materialul de laborator cere studenților să creeze ei înșiși tabelele cu un script SQL, deci exemplul trebuie să pornească de la o schemă existentă, nu să o genereze.

Versiunea anterioară a exemplului avea următoarele probleme:

* EF Core mapa notele ca `decimal(18, 0)`, deși scriptul declara `decimal(18, 2)`: **nota finală 7,88 se salva ca 8** – bug real, nedetectat pentru că nu exista nicio verificare a preciziei.
* Starea `CalculatedStudentGrade` din domeniu purta `GradeId` (cheia EF) și `IsUpdated` (un indicator „murdar”) – detalii de persistență în modelul de business.
* La salvare, repository-ul încărca **toți** studenții (urmăriți de `DbContext`) și **toate** notele, apoi ataşa entitățile ca `Modified` pe toate coloanele. Două linii cu același matricol în aceeași cerere aruncau excepție la `SaveChanges`.
* Fără cheie străină, fără index unic pe matricol (deși codul presupunea unicitatea cu `Single()`); scriptul nu era idempotent (`CREATE DATABASE` necondiționat) și nu avea date inițiale, deci exemplul **nu putea reuși** la prima rulare.
* Connection string hard-codat în aplicația consolă, `DbContext` creat manual și niciodată eliberat, fără `CancellationToken`.
* Citirea catalogului trecea prin tipurile domeniului, deși coloanele pot fi `NULL`, ceea ce domeniul nu reprezintă.

## Factori de decizie

* **Studenții rulează deja un script SQL**; migrațiile EF ar fi un subiect nou, în afara temei.
* **Domeniul nu știe de EF Core** (Wlaschin, cap. 12): persistența este un detaliu la marginea sistemului, accesat prin porturi definite de domeniu.
* **I/O doar înainte și după nucleul pur**: „citește din bază → logică pură → scrie în bază”; domeniul testat în Lucrarea 3 trebuie să rămână neschimbat.
* **Aceeași bază de date pentru Lucrările 5–8**, cu un script identic verificat automat.
* **Rulare locală fără instalare**: SQL Server în Docker, prin `compose.yaml`.
* **Fără secrete în depozit**.

## Opțiuni considerate

1. **SQL-first**: scriptul `create-db.sql` este sursa adevărului, oglindit manual în `OnModelCreating`.
2. **Code-first cu migrații EF**: modelul C# generează schema, `dotnet ef migrations` versionează schimbările.
3. **Micro-ORM (Dapper)** cu SQL scris manual în repository-uri.

Subdecizii: cheie EF și indicator „murdar” în domeniu **sau** upsert după cheia naturală (matricol); citire prin domeniu **sau** model de citire separat.

## Decizie

Opțiunea 1, cu upsert după matricol și model de citire separat. Regulile concrete:

* `SQL/create-db.sql` este **idempotent** (`IF DB_ID(...) IS NULL`, `IF OBJECT_ID(...) IS NULL`) și declară: `Student(StudentId, RegistrationNumber VARCHAR(7) UNIQUE, Name NVARCHAR(50))`; `Grade(GradeId, StudentId FK → Student, Exam/Activity/Final DECIMAL(4, 2) NULL CHECK (> 0 AND <= 10), UNIQUE(StudentId))`. Un `MERGE` inserează cei patru studenți folosiți și de consola din Lucrarea 3 (`LM12345`, `LM54321`, `LM67890`, `LM98765`). Același fișier este în Lucrările 5–8 (setul „SQL” din `tools/lab-consistency.json`).
* `GradesContext.OnModelCreating` **oglindește** scriptul: numele tabelelor, `HasMaxLength(7).IsUnicode(false)` pentru matricol, `HasMaxLength(50)` pentru nume, indexurile unice `UQ_Student_RegistrationNumber` și `UQ_Grade_StudentId`, cheia străină `FK_Grade_Student` ca relație 1-la-1, `HasPrecision(4, 2)` pe cele trei note. Constrângerile `CHECK` rămân doar în SQL: domeniul le garantează deja prin `Grade`.
* **Fără migrații**: schema se schimbă în script, apoi în `OnModelCreating`.
* Porturile stau în domeniu (`IStudentsRepository.GetExistingAsync(registrationNumbers, ct)` → `IReadOnlySet<StudentRegistrationNumber>`; `IGradesRepository.SaveAsync(Exam.Published, ct)`), adaptoarele în `Examples.Data`.
* `StudentsRepository`: interogare `AsNoTracking` cu `Contains` (EF Core 10 o traduce în `IN (...)`). Rândurile propriei baze de date sunt **de încredere**: un matricol invalid în tabelă este corupție, nu eroare de utilizator → `GetValueOrThrow` cu `InvalidDataException`.
* `GradesRepository.SaveAsync`: **o singură interogare urmărită** cu `Include(Grade)` pe matricolele din examen; dacă un student validat anterior nu mai există, `InvalidOperationException` (eroare de infrastructură, nu de validare); rând `Grade` existent → actualizare, altfel inserare (**upsert după matricol**); `Option<Grade>` → `decimal?` pentru nota finală; un singur `SaveChangesAsync`.
* Domeniul **nu mai conține** `GradeId` sau `IsUpdated`.
* **Model de citire separat**: `IGradesQuery.GetAllAsync` întoarce `StudentGradeRow(RegistrationNumber, Name, Exam?, Activity?, Final?)` proiectat direct din EF, fără a trece prin domeniu (coloanele pot fi `NULL`).
* Workflow-ul rămâne cel din Lucrarea 3: `ExecuteAsync` = `GetExistingAsync` (impur) → `Publish(...)` (pur) → `TapAsync(SaveAsync)` → `Map(ToEvent)` (impur). **Zero modificări în `Examples.Domain` față de Lucrarea 3.**
* Gazda consolă folosește `Host.CreateApplicationBuilder`: `AddGradesData(connectionString)` înregistrează `DbContext`-ul și cele trei servicii de date ca *scoped*; `AddScoped<PublishExamWorkflow>`; `AddSingleton(TimeProvider.System)`; un `AsyncServiceScope` per rulare. Connection string-ul vine din `ConnectionStrings:DefaultConnection` (`appsettings.json` cu autentificare integrată, fără parolă, sau `dotnet user-secrets` – `UserSecretsId = PSSC.<proiect>` din `Directory.Build.props`), cu mesaj clar dacă lipsește.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| Script idempotent, `DECIMAL(4, 2)`, `UQ_*`, `CK_*`, `FK_*`, seed `MERGE` | [create-db.sql](../../SQL/create-db.sql) |
| `OnModelCreating` oglindește scriptul (`HasPrecision(4, 2)`, indexuri, relație 1-la-1) | [GradesContext.cs](../../Exemple/Examples.Data/GradesContext.cs) |
| `AsNoTracking` + `Contains`; `InvalidDataException` pentru rânduri corupte | [StudentsRepository.cs](../../Exemple/Examples.Data/Repositories/StudentsRepository.cs) |
| O interogare urmărită, upsert după matricol, `Option` → `decimal?` | [GradesRepository.cs](../../Exemple/Examples.Data/Repositories/GradesRepository.cs) |
| Model de citire care ocolește domeniul | [IGradesQuery.cs](../../Exemple/Examples.Data/Queries/IGradesQuery.cs), [GradesQuery.cs](../../Exemple/Examples.Data/Queries/GradesQuery.cs), [StudentGradeRow.cs](../../Exemple/Examples.Data/Queries/StudentGradeRow.cs) |
| Înregistrare *scoped* a `DbContext`, repository-urilor și interogării | [ServiceCollectionExtensions.cs](../../Exemple/Examples.Data/ServiceCollectionExtensions.cs) |
| Entități EF separate de stările domeniului | [GradeEntity.cs](../../Exemple/Examples.Data/Entities/GradeEntity.cs), [StudentEntity.cs](../../Exemple/Examples.Data/Entities/StudentEntity.cs) |
| Porturi definite de domeniu | [IStudentsRepository.cs](../../Exemple/Examples.Domain/Repositories/IStudentsRepository.cs), [IGradesRepository.cs](../../Exemple/Examples.Domain/Repositories/IGradesRepository.cs) |
| Secvența impur → pur → impur | [PublishExamWorkflow.cs](../../Exemple/Examples.Domain/Workflows/PublishExamWorkflow.cs) |
| Gazdă generică, connection string din configurație | [Program.cs](../../Exemple/Examples.ConsoleApp/Program.cs), [GradesInput.cs](../../Exemple/Examples.ConsoleApp/GradesInput.cs), [appsettings.json](../../Exemple/Examples.ConsoleApp/appsettings.json) |
| `UserSecretsId` comun tuturor laboratoarelor | [Directory.Build.props](../../Exemple/Directory.Build.props) |
| SQL Server 2025 + inițializare în Docker | [compose.yaml](../../../compose.yaml) |

## Consecințe

### Pozitive

* Bug-ul de precizie dispare: `HasPrecision(4, 2)` și `DECIMAL(4, 2)` păstrează două decimale (verificat: 7,88 rămâne 7,88).
* O singură interogare la salvare, indiferent de numărul de studenți; actualizările se fac pe loc, fără ștergere și reinserare.
* Scriptul se poate rula oricâte ori; exemplul reușește din prima datorită datelor inițiale.
* Domeniul testat în Lucrarea 3 este **neschimbat** – dovada concretă că I/O-ul stă la margini.
* Cheia naturală (matricolul) este singura identitate pe care domeniul o cunoaște; EF rămâne invizibil deasupra `Examples.Data`.

### Negative / compromisuri

* Schema există în **două locuri** (script și `OnModelCreating`) care trebuie ținute în sincron manual; fără migrații nu există o unealtă care să detecteze deriva.
* Fără istoric al schemei: o schimbare pe o bază existentă cere un script `ALTER` scris de mână.
* Fereastra dintre `GetExistingAsync` și `SaveAsync` **nu este tranzacțională**: un student șters între timp produce o excepție de infrastructură (în API: 500). Acceptat pentru laborator.
* Baza de date permite `NULL` acolo unde domeniul nu îl reprezintă; de aceea citirea catalogului **trebuie** să folosească modelul de citire, nu `ValidatedStudentGrade`.
* Repository-urile nu au teste automate (ar cere o bază reală sau Testcontainers); testele rămân pe nucleul pur.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – SQL-first (aleasă)

* Bine, pentru că se potrivește cu ce cer sarcinile de laborator (studenții scriu scriptul).
* Bine, pentru că schema este vizibilă într-un singur fișier ușor de citit și de rulat în Docker.
* Rău, pentru că maparea EF se scrie de mână și poate devia de la script.

### Opțiunea 2 – code-first cu migrații

* Bine, pentru că schema și modelul nu pot devia, iar schimbările sunt versionate.
* Rău, pentru că introduce un subiect nou (`dotnet ef`, `Migrations/`), schimbă modul în care studenții creează baza și ascunde SQL-ul pe care laboratorul vrea să-l arate.

### Opțiunea 3 – Dapper

* Bine, pentru că SQL-ul este explicit și performanța previzibilă.
* Rău, pentru că obiectivul laboratorului este EF Core (`DbContext`, `DbSet`, `OnModelCreating`), iar maparea manuală a rezultatelor adaugă cod fără valoare pedagogică.

### Upsert după matricol (aleasă) vs. cheie EF în domeniu

* Bine, pentru că domeniul rămâne pur și repository-ul decide singur între inserare și actualizare.
* Rău, pentru că repository-ul face o interogare în plus înainte de scriere (una singură, cu `Include`).

### Model de citire separat (aleasă) vs. citire prin domeniu

* Bine, pentru că citirea nu impune invarianți și poate reprezenta notele lipsă.
* Rău, pentru că există două reprezentări ale aceleiași tabele (entitate și rând de citire).

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 12 (Persistence)

[2] Microsoft Learn, [Creating and configuring a model (EF Core)](https://learn.microsoft.com/ef/core/modeling/)

[3] Microsoft Learn, [Quickstart: Run SQL Server Linux container images with Docker](https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker)

[4] Microsoft Learn, [Safe storage of app secrets in development (user secrets)](https://learn.microsoft.com/aspnet/core/security/app-secrets)
