# Lucrarea 7: Comunicare sincronă

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: implementarea și apelarea unui API

**Sarcina 1**

Analizați și rulați soluția din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`). Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

Realizați un nou API (care reprezintă contextul de livrări) pe care să îl apelați la finalul procesării comenzii. Configurați politica de reîncercări, folosind `Microsoft.Extensions.Http.Resilience`, astfel încât apelul să reîncerce orice eroare tranzitorie de 3 ori la interval de timp exponențiale.

## Contracte partajate

DTO-urile schimbate între API-uri stau într-un proiect separat, `Examples.Contracts`. Nu referențiați un proiect web din altul ca să-i reutilizați un model: controller-ele/endpoint-urile lui ar fi încărcate ca *application part* și în API-ul apelant — motivul pentru care o versiune anterioară a acestui exemplu avea nevoie de un filtru care să ascundă din documentația OpenAPI endpoint-urile "împrumutate".

## Rulare

Porniți mai întâi `Examples.ReportGenerator` (`https://localhost:7286`), apoi `Examples.Api`. Adresa e citită din configurație (`ReportApi:BaseAddress` în `appsettings.json`), nu e hard-codată.

```
dotnet run --project Exemple/Examples.ReportGenerator
dotnet run --project Exemple/Examples.Api
```

## GitHub Copilot

## HttpClient tipizat

HttpClient tipizat este o caracteristică în .NET Core care vă permite să definiți și să injectați instanțe HttpClient puternic tipizate. Această abordare oferă o mai bună încapsulare și o testare mai ușoară comparativ cu utilizarea directă a HttpClient-ului implicit. Se bazează pe injecția de dependențe pentru a gestiona ciclul de viață al instanțelor HttpClient, asigurând o utilizare eficientă a resurselor și evitând capcanele comune, cum ar fi epuizarea socket-urilor.

### Configurarea unui HttpClient tipizat

Pentru a configura un HttpClient tipizat, urmați pașii de mai jos:

1. **Definiți o clasă pentru HttpClient tipizat**:
    ```csharp
    public class MyApiClient
    {
        private readonly HttpClient _httpClient;

        public MyApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDataAsync()
        {
            var response = await _httpClient.GetAsync("endpoint");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
    ```

2. **Configurați HttpClient tipizat în `Program.cs`**:
    ```csharp
    builder.Services.AddHttpClient<MyApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    ```

3. **Injectați clientul tipizat într-un endpoint minimal**:
    ```csharp
    app.MapGet("/data", async (MyApiClient client) => await client.GetDataAsync());
    ```

## Microsoft.Extensions.Http.Resilience

`Microsoft.Extensions.Http.Resilience` este pachetul actual pentru reziliența apelurilor HTTP în .NET (construit peste Polly v8); înlocuiește vechiul `Microsoft.Extensions.Http.Polly` + `AddPolicyHandler`. Oferă `AddResilienceHandler` pentru o politică personalizată sau `AddStandardResilienceHandler` pentru un set implicit (retry + circuit breaker + timeout).

### Configurarea unei politici de reîncercare

1. **Adăugați pachetul în proiectul dvs.**:
    ```bash
    dotnet add package Microsoft.Extensions.Http.Resilience
    ```

2. **Configurați politica în `Program.cs`**:
    ```csharp
    builder.Services.AddHttpClient<MyApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com/");
    })
    .AddResilienceHandler("my-api", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        });
    });
    ```

   Politica de mai sus reîncearcă de 3 ori erorile tranzitorii (5xx, timeout, excepții de rețea), cu întârziere exponențială și *jitter* — exact configurația din exemplul acestei lucrări (`Examples.Api/Clients/ServiceCollectionExtensions.cs`), unde numărul de reîncercări și întârzierea de bază sunt legate din configurație (`ReportApi:Retry`) în loc să fie valori fixe în cod.

3. **Injectați clientul tipizat** ca mai sus.

Intervalul de timp dintre reîncercări crește exponențial (600 ms, 1,2 s, 2,4 s, cu jitter — la fel ca în exemplu), nu doar constant.

## Decizii de arhitectură

Alegerile de proiectare ale acestui laborator – contractele partajate într-o bibliotecă separată, clientul HTTP tipizat cu reziliență, propagarea eșecului dependenței – sunt documentate în [ADR-0005](docs/adr/0005-contracte-partajate-si-client-http-rezilient.md), care continuă [ADR-0004](../Lucrarea-06/docs/adr/0004-api-minimal-cu-rezultate-tipizate.md) din Lucrarea 6.

## Referințe

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/), Pragmatic Bookshelf, 2018 — cap. 3 (comunicarea între contexte delimitate), cap. 11 (DTO-uri la graniță)

[2] Microsoft Documentation, [Clienți HttpClient tipizați](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory#typed-clients)

[3] Microsoft Documentation, [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience)