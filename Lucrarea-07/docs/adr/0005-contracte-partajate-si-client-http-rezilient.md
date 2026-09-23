# ADR-0005: Comunicarea sincronă între contexte folosește contracte partajate într-o bibliotecă separată și un client HTTP tipizat cu reziliență standard

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 7 – Comunicare sincronă ([README](../../README.md))
* **Extinde**: [ADR-0004](../../../Lucrarea-06/docs/adr/0004-api-minimal-cu-rezultate-tipizate.md) (API Web)
* **Vezi și**: [ADR-0006](../../../Lucrarea-08/docs/adr/0006-evenimente-de-integrare-prin-service-bus-si-cloudevents.md) – alternativa asincronă la aceeași problemă

## Context și problemă

Lucrarea 7 introduce al doilea context: un API de rapoarte (`Examples.ReportGenerator`) pe care API-ul de note îl apelează după publicarea catalogului, pentru raportul de semestru și pentru calculul burselor. Cele două API-uri rulează ca procese separate și comunică prin HTTP.

Versiunea anterioară a exemplului avea următoarele probleme:

* API-ul de note **referenția proiectul web** al generatorului de rapoarte doar pentru a refolosi un DTO. ASP.NET Core încărca astfel și controllerele acelui proiect ca *application parts* în API-ul de note, iar exemplul le ascundea din documentul OpenAPI cu un filtru de document – un simptom tratat în loc de cauză.
* URL-ul de bază al generatorului de rapoarte era hard-codat în cod.
* Reziliența folosea vechiul pachet de integrare Polly (`AddPolicyHandler`), cu 3 reîncercări la **interval constant** de 600 ms, deși README-ul cerea întârziere exponențială.
* `HttpResponseMessage` nu era eliberat, `CancellationToken` nu era transmis, generatorul de rapoarte loga întreg CSV-ul primit.

## Factori de decizie

* **Contractul dintre contexte trebuie să fie explicit și fără logică** (Wlaschin, cap. 3 și 11): tipuri simple, serializabile, deținute în comun.
* **Un proiect web nu se referențiază din alt proiect web.**
* **Reziliența este configurație, nu cod**: erorile tranzitorii se reîncearcă exponențial, cu *jitter*, cu limită de timp.
* **Eșecul dependenței nu se maschează**: dacă rapoartele nu pot fi generate, apelantul trebuie să afle.
* Ambele API-uri trebuie să pornească dintr-un singur F5.

## Opțiuni considerate

1. **Bibliotecă de contracte** (`Examples.Contracts`, fără dependențe) + **client tipizat** cu `Microsoft.Extensions.Http.Resilience`.
2. **Referință la proiectul web** al celuilalt API + filtru OpenAPI (varianta anterioară).
3. **Duplicarea DTO-urilor** în fiecare API, fără proiect comun.
4. **Client generat din documentul OpenAPI** (Kiota, NSwag).

Pentru reziliență: `Microsoft.Extensions.Http.Resilience` **sau** vechiul pachet Polly cu `AddPolicyHandler` **sau** fără reîncercări.

## Decizie

Opțiunea 1. Regulile concrete:

* `Examples.Contracts/Reports` conține `ExamPublishedReport(string Csv, DateTimeOffset PublishedAt)` și `ReportAcknowledgement(string Message, DateTimeOffset ReceivedAt)`. Proiectul **nu are referințe** și **nu conține logică**; este referențiat de `Examples.Api` și de `Examples.ReportGenerator`.
* `ReportApiClient(HttpClient)` este un **client tipizat** cu două metode, `GenerateSemesterReportAsync` și `CalculateScholarshipAsync`, care fac `POST report/semester-report` și `POST report/scholarship` cu `PostAsJsonAsync`, `EnsureSuccessStatusCode` și `ReadFromJsonAsync<ReportAcknowledgement>`. Răspunsul este eliberat (`using`), `CancellationToken` este transmis.
* `AddReportApiClient(services, configuration)`: `ReportApiOptions` (`BaseAddress` `[Required]`; `Retry.MaxRetryAttempts` implicit 3, `[Range(0, 10)]`; `Retry.BaseDelayMilliseconds` implicit 600, `[Range(50, 10_000)]`) legate din secțiunea `ReportApi` cu `ValidateDataAnnotations().ValidateOnStart()`; `AddHttpClient<ReportApiClient>` cu `BaseAddress` din opțiuni; `AddResilienceHandler("report-api")` cu `AddRetry(HttpRetryStrategyOptions { MaxRetryAttempts, BackoffType = Exponential, UseJitter = true, Delay = întârzierea de bază })` urmat de `AddTimeout(5 s)` per încercare.
* În `POST /grades`, pe ramura de succes, `published.ToReport()` este trimis către **ambele** endpoint-uri **concurent** (`Task.WhenAll`), apoi se întoarce 200. Excepțiile (`HttpRequestException`, expirarea) **nu se prind**: după epuizarea reîncercărilor ajung la `UseExceptionHandler` și devin 500 `ProblemDetails`.
* `Examples.ReportGenerator` este un API minimal separat: `MapGroup("/report")`, două `POST` care doar loghează (prin `LoggerMessage`, fără conținutul CSV) și confirmă cu `ReportAcknowledgement`; rulează pe `https://localhost:7286`.
* `Examples.slnLaunch.user` (urmărit în git prin excepția din `.gitignore`) definește profilul „Api + ReportGenerator”, care pornește generatorul de rapoarte primul.
* Reîncercarea cererilor `POST` este acceptată pentru că endpoint-urile de raport sunt idempotente (doar loghează). Pentru ținte cu efecte secundare, `HttpRetryStrategyOptions.DisableForUnsafeHttpMethods()` dezactivează reîncercarea pe metodele nesigure.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| Contracte fără logică, fără dependențe | [ExamPublishedReport.cs](../../Exemple/Examples.Contracts/Reports/ExamPublishedReport.cs), [ReportAcknowledgement.cs](../../Exemple/Examples.Contracts/Reports/ReportAcknowledgement.cs), [Examples.Contracts.csproj](../../Exemple/Examples.Contracts/Examples.Contracts.csproj) |
| Client tipizat, răspuns eliberat, `CancellationToken` | [ReportApiClient.cs](../../Exemple/Examples.Api/Clients/ReportApiClient.cs) |
| Opțiuni validate la pornire | [ReportApiOptions.cs](../../Exemple/Examples.Api/Clients/ReportApiOptions.cs) |
| `AddHttpClient<T>` + `AddResilienceHandler` (retry exponențial + jitter + timeout) | [ServiceCollectionExtensions.cs](../../Exemple/Examples.Api/Clients/ServiceCollectionExtensions.cs) |
| Apeluri concurente pe succes, excepțiile se propagă | [GradesEndpoints.cs](../../Exemple/Examples.Api/Endpoints/GradesEndpoints.cs) |
| `ExamPublishedEvent` → `ExamPublishedReport` la graniță | [ExamPublishedEventMapping.cs](../../Exemple/Examples.Api/Mapping/ExamPublishedEventMapping.cs) |
| Secțiunea `ReportApi` (adresă, reîncercări) | [appsettings.json](../../Exemple/Examples.Api/appsettings.json) |
| Al doilea API: doar loghează și confirmă | [Program.cs](../../Exemple/Examples.ReportGenerator/Program.cs), [ReportEndpoints.cs](../../Exemple/Examples.ReportGenerator/Endpoints/ReportEndpoints.cs) |
| Profil de lansare cu ambele API-uri | [Examples.slnLaunch.user](../../Exemple/Examples.slnLaunch.user) |

## Consecințe

### Pozitive

* Nu mai există controllere „împrumutate” și nici filtru OpenAPI care să le ascundă.
* Pipeline-ul de reziliență este cel standard din .NET (Polly v8 sub capotă), configurabil din `appsettings.json`, cu întârzieri de aproximativ 600 ms, 1,2 s și 2,4 s plus *jitter* – exact ce cere README-ul.
* Contractul există într-un singur loc și se versionează împreună cu ambele API-uri.
* Cele două apeluri independente rulează în paralel; durata pe succes este cea a celui mai lent, nu suma.

### Negative / compromisuri

* **Cuplare la compilare**: o schimbare de contract cere reconstruirea ambelor API-uri. Acceptabil într-un depozit de laborator; în producție contractele s-ar publica ca pachet versionat.
* **Disponibilitatea `POST /grades` depinde de generatorul de rapoarte**: notele sunt deja salvate în baza de date când apelul HTTP eșuează, deci clientul primește 500 deși publicarea a reușit parțial. Aceasta este motivația directă pentru comunicarea asincronă din Lucrarea 8 (ADR-0006).
* În cazul cel mai rău (patru încercări cu limită de 5 s fiecare, plus întârzierile), cererea poate dura peste 20 de secunde înainte de 500.
* Generatorul de rapoarte nu are `ProblemDetails` sau gestionare de excepții – este intenționat minimal.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – bibliotecă de contracte + client tipizat (aleasă)

* Bine, pentru că graniță dintre contexte este un proiect vizibil, cu tipuri simple.
* Bine, pentru că `IHttpClientFactory` gestionează durata de viață a conexiunilor, iar reziliența se adaugă declarativ.
* Rău, pentru că ambele API-uri trebuie reconstruite la o schimbare de contract.

### Opțiunea 2 – referință la proiectul web (varianta anterioară)

* Bine, pentru că nu cere un proiect în plus.
* Rău, pentru că încarcă endpoint-urile celuilalt API în procesul apelantului și trage după sine toate dependențele web ale acestuia.

### Opțiunea 3 – DTO-uri duplicate

* Bine, pentru că fiecare API este complet independent la compilare.
* Rău, pentru că cele două copii pot devia fără nicio avertizare, iar laboratorul vrea să arate explicit conceptul de contract.

### Opțiunea 4 – client generat din OpenAPI (Kiota, NSwag)

* Bine, pentru că este abordarea corectă între echipe care nu împart cod și ține clientul în sincron cu documentul.
* Rău, pentru că adaugă o unealtă de generare și cod generat greu de citit – prea mult pentru un laborator de două ore.

### `Microsoft.Extensions.Http.Resilience` (aleasă) vs. `AddPolicyHandler` (Polly v7) vs. fără reîncercări

* Bine, pentru că este pachetul recomandat de Microsoft, cu strategii tipizate (`HttpRetryStrategyOptions`) și integrare cu opțiunile.
* Rău, pentru că multe exemple online încă folosesc API-ul Polly v7, iar `AddPolicyHandler` este marcat învechit.
* Fără reîncercări ar fi mai simplu, dar ar contrazice obiectivul laboratorului (comunicare sincronă robustă).

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 3 (Communication between bounded contexts), 11 (Serialization)

[2] Microsoft Learn, [Clienți `HttpClient` tipizați (`IHttpClientFactory`)](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory#typed-clients)

[3] Microsoft Learn, [Build resilient HTTP apps – `Microsoft.Extensions.Http.Resilience`](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)

[4] [Polly](https://www.pollydocs.org/) – strategii de reziliență folosite sub capotă

[5] Microsoft Learn, [Options pattern – validare la pornire](https://learn.microsoft.com/dotnet/core/extensions/options)
