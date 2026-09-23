# ADR-0004: Workflow-ul este expus printr-un API minimal cu rezultate tipizate; `Result` se mapează la 200 / 400 `ValidationProblemDetails`, iar documentul OpenAPI este generat nativ

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 6 – Folosirea unui model DDD pentru a implementa un API Web ([README](../../README.md))
* **Extinde**: [ADR-0003](../../../Lucrarea-05/docs/adr/0003-persistenta-sql-first-la-marginile-workflow-ului.md) (persistență)
* **Extinsă de**: [ADR-0005](../../../Lucrarea-07/docs/adr/0005-contracte-partajate-si-client-http-rezilient.md) (comunicare sincronă), [ADR-0006](../../../Lucrarea-08/docs/adr/0006-evenimente-de-integrare-prin-service-bus-si-cloudevents.md) (comunicare asincronă)

## Context și problemă

Lucrarea 6 expune workflow-ul de publicare a notelor prin HTTP: `POST /grades` primește notele brute și întoarce catalogul sau erorile de validare, `GET /grades` întoarce catalogul curent. API-ul este graniță: primește DTO-uri, le traduce în comanda domeniului și traduce `Result` în răspunsuri HTTP.

Versiunea anterioară a exemplului avea următoarele probleme:

* Un controller MVC cu `[HttpGet("getAllGrades")]` (verb în URL), răspuns de **tip anonim** (schemă OpenAPI goală), succes întors ca `Ok()` gol (CSV-ul se pierdea) și eșec ca `BadRequest(IEnumerable<string>)`, fără o structură standard de eroare.
* DTO-ul de intrare avea `[Range(1, 10)]`, care **contrazicea** regula domeniului `(0, 10]`, `[Required]` pe un `decimal` ne-nullable (fără efect) și un câmp lipsă devenea `0`.
* `UseAuthorization()` fără nicio autentificare configurată, `AddHttpClient()` nefolosit, repository-uri *transient* peste un `DbContext` *scoped*.
* Generatorul de document OpenAPI al Swashbuckle plus `AddEndpointsApiExplorer`, deși ASP.NET Core generează documentul nativ din .NET 9.
* `class Program { static void Main }`, profiluri IIS Express.

## Factori de decizie

* **Endpoint = funcție**: aceeași idee ca operațiile din domeniu; tipul de retur trebuie să enumere răspunsurile posibile.
* **Regulile de validare într-un singur loc** (domeniul); API-ul doar traduce, nu re-validează.
* **Format standard de eroare** (RFC 9457 Problem Details) atât pentru 400, cât și pentru 500.
* **Document OpenAPI complet**, pentru că studenții testează din Swagger UI.
* Șabloanele implicite din .NET 10 și decizia titularului de laborator (2026-09-22): API minimal, nu controllere.

## Opțiuni considerate

1. **API minimal cu `TypedResults`** și tip de retur `Results<Ok<T>, ValidationProblem>`.
2. **Controllere MVC** (`[ApiController]`, `ActionResult<T>`, `[ProducesResponseType]`).
3. **API minimal cu `IResult` netipizat**.

Subdecizii: document OpenAPI nativ (`Microsoft.AspNetCore.OpenApi`) plus pachetul Swagger UI doar pentru interfață **sau** generatorul complet Swashbuckle **sau** Scalar; validare cu DataAnnotations pe DTO **sau** doar în domeniu.

## Decizie

Opțiunea 1, cu OpenAPI nativ și validare doar în domeniu. Regulile concrete:

* `Program.cs` cu instrucțiuni de nivel superior: `AddGradesData`, `AddScoped<PublishExamWorkflow>`, `AddSingleton(TimeProvider.System)`, `ConfigureHttpJsonOptions` cu `RespectNullableAnnotations = true`, `AddProblemDetails` (cu `traceId` în extensii), `AddOpenApi()`. Pipeline: `UseExceptionHandler`, `UseStatusCodePages`, `UseHttpsRedirection`, în *Development* `MapOpenApi()` (`/openapi/v1.json`) și `UseSwaggerUI` (`/swagger`), apoi `MapGrades()`. **Fără** `AddControllers`, **fără** `UseAuthorization`.
* `GradesEndpoints`: `MapGroup("/grades").WithTags("Grades")`; `POST /grades` întoarce `Results<Ok<PublishGradesResponse>, ValidationProblem>`; `GET /grades` întoarce `Ok<IReadOnlyList<StudentGradeRow>>` direct din `IGradesQuery` (**ocolește domeniul**, vezi ADR-0003).
* `Result` → HTTP într-un singur loc: `result.Match(published => TypedResults.Ok(new PublishGradesResponse(Csv, PublishedAt)), error => error.ToValidationProblem())`. `ToValidationProblem` grupează `ValidationError` după `Code` și pune mesajele ca valori, cu titlul „Catalogul nu a putut fi publicat.”.
* **Excepțiile de infrastructură nu se prind** în endpoint: `UseExceptionHandler` le transformă în 500 `application/problem+json` cu `traceId`.
* DTO-ul de intrare `InputGrade(string? RegistrationNumber, decimal? Exam, decimal? Activity)` **nu are DataAnnotations**: toate câmpurile sunt opționale la nivel de formă, iar `ToUnvalidated()` (bloc de extensie) le convertește în text cu `CultureInfo.InvariantCulture` și le predă domeniului, care raportează lipsa sau invaliditatea ca `ValidationError`. `RespectNullableAnnotations` rămâne activ pentru eventualele contracte cu proprietăți ne-nullable.
* Răspunsul de succes este un record tipizat, `PublishGradesResponse(string Csv, DateTimeOffset PublishedAt)`, nu un tip anonim.
* OpenAPI: `Microsoft.AspNetCore.OpenApi` **generează** documentul; `Swashbuckle.AspNetCore.SwaggerUI` doar **servește interfața** peste `/openapi/v1.json`.
* Un singur profil de lansare, `https` (`https://localhost:7195`), cu `launchUrl: swagger`.
* Durate de viață: `DbContext`, repository-uri, interogare și workflow *scoped*; `TimeProvider` *singleton*.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| Înregistrări, `ProblemDetails`, OpenAPI nativ, pipeline fără autorizare | [Program.cs](../../Exemple/Examples.Api/Program.cs) |
| `MapGroup("/grades")`, `Results<Ok<T>, ValidationProblem>`, `Match` → HTTP, GET prin `IGradesQuery` | [GradesEndpoints.cs](../../Exemple/Examples.Api/Endpoints/GradesEndpoints.cs) |
| DTO fără DataAnnotations | [InputGrade.cs](../../Exemple/Examples.Api/Models/InputGrade.cs) |
| Conversie DTO → `UnvalidatedStudentGrade` cu cultură invariantă | [InputGradeMapping.cs](../../Exemple/Examples.Api/Mapping/InputGradeMapping.cs) |
| `PublishExamError` → `ValidationProblem` grupat după `Code` | [PublishExamErrorMapping.cs](../../Exemple/Examples.Api/Mapping/PublishExamErrorMapping.cs) |
| Modelul de citire folosit de GET | [StudentGradeRow.cs](../../Exemple/Examples.Data/Queries/StudentGradeRow.cs) |
| Pachete: `Microsoft.AspNetCore.OpenApi` + Swagger UI | [Examples.Api.csproj](../../Exemple/Examples.Api/Examples.Api.csproj) |
| Un singur profil `https`, `launchUrl: swagger` | [launchSettings.json](../../Exemple/Examples.Api/Properties/launchSettings.json) |

## Consecințe

### Pozitive

* Tipul de retur al endpoint-ului **enumeră răspunsurile posibile** și compilatorul verifică fiecare ramură – aceeași idee „stări ilegale nereprezentabile” ca în domeniu, aplicată la HTTP. Schema OpenAPI rezultă completă fără atribute.
* 400 are un format standard (`ValidationProblemDetails`), consumabil de orice client; 500 are același format, cu `traceId` pentru corelare.
* Regulile de business există într-un singur loc: o schimbare a lui `Grade.Maximum` se propagă automat în API, fără a corecta atribute.
* `Program.cs` are 34 de linii și se citește de sus în jos.

### Negative / compromisuri

* Studenții care cunosc MVC întâlnesc un stil nou; README-ul explică corespondența (grup de rute ↔ controller, funcție ↔ acțiune).
* Erorile de **formă** ale corpului JSON (JSON invalid, tip greșit) produc 400 din framework, într-un format diferit de `ValidationProblem`-ul domeniului.
* Pachetul Swagger UI este o dependență doar pentru interfață; poate fi înlocuit cu Scalar fără a atinge generarea documentului.
* Lipsa matricolului ajunge la domeniu (care o raportează ca `InvalidRegistrationNumber`), nu este respinsă de serializator – intenționat, ca toate erorile unei cereri să vină din același loc.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – API minimal cu `TypedResults` (aleasă)

* Bine, pentru că răspunsurile sunt un tip-uniune verificat la compilare și documentat automat.
* Bine, pentru că este șablonul implicit în .NET 10 și cel mai apropiat de stilul funcțional al domeniului.
* Rău, pentru că pentru API-uri mari organizarea în fișiere de endpoint-uri cere disciplină (nu există convenția de clasă a controllerelor).

### Opțiunea 2 – controllere MVC

* Bine, pentru că este stilul cel mai răspândit în tutoriale și în cod existent.
* Rău, pentru că răspunsurile se documentează prin atribute `[ProducesResponseType]` care pot minți, iar `ActionResult<T>` nu enumeră ramurile de eroare.

### Opțiunea 3 – API minimal cu `IResult`

* Bine, pentru că este cel mai scurt de scris.
* Rău, pentru că pierde atât verificarea la compilare, cât și schema OpenAPI a răspunsurilor.

### OpenAPI nativ + Swagger UI (aleasă) vs. generator Swashbuckle vs. Scalar

* Bine, pentru că generatorul nativ este întreținut de echipa ASP.NET Core și suportă transformatoare de document (folosite în Lucrarea 8).
* Bine, pentru că vocabularul „Swagger” din README-uri rămâne valid – interfața este chiar Swagger UI.
* Rău, pentru că pachetul de interfață provine tot din familia Swashbuckle, ceea ce poate crea confuzie cu generatorul înlocuit.

### Validare doar în domeniu (aleasă) vs. DataAnnotations pe DTO

* Bine, pentru că nu există două seturi de reguli care să divergă (exact bug-ul `[Range(1, 10)]` din versiunea anterioară).
* Rău, pentru că API-ul nu respinge nimic „devreme”: fiecare cerere ajunge la workflow, inclusiv cele evident greșite.

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 11 (Serialization)

[2] Microsoft Learn, [Minimal APIs în ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)

[3] Microsoft Learn, [Create responses in Minimal API applications – `TypedResults`, `Results<T1, T2>`](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses)

[4] Microsoft Learn, [Generate OpenAPI documents (Microsoft.AspNetCore.OpenApi)](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi)

[5] Microsoft Learn, [Handle errors in ASP.NET Core APIs – Problem Details](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)

[6] IETF, [RFC 9457 – Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
