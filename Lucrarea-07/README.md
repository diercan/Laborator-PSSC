# Lucrarea 7: Comunicare sincrona

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: implementarea si apelarea unui API

**Sarcina 1**

Analizați și rulați soluția din directorul exemple. Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

**Sarcina 2**

Realizați un nou API (care reprezintă contextul de livrări) pe care să îl apelați la finalul procesării comenzii. Configurați politica de reîncercări astfel încât apelul să reîncerce orice eroare tranzitorie de 3 ori la interval de timp exponențiale.

# GitHub Copilot

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

2. **Configurați HttpClient tipizat în `Startup.cs`**:
    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<MyApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
    ```

3. **Utilizați HttpClient tipizat în controller sau serviciu**:
    ```csharp
    public class MyController : ControllerBase
    {
        private readonly MyApiClient _myApiClient;

        public MyController(MyApiClient myApiClient)
        {
            _myApiClient = myApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _myApiClient.GetDataAsync();
            return Ok(data);
        }
    }
    ```

## Biblioteca Polly

    Polly este o bibliotecă .NET care oferă mecanisme de gestionare a rezilienței și a tranzienței, cum ar fi retry, circuit breaker, timeout și bulkhead isolation. În contextul configurării unei politici de retry pentru un HttpClient, Polly poate fi utilizată pentru a reîncerca apelurile HTTP care eșuează din cauza erorilor tranzitorii.

### Configurarea unei politici de retry cu Polly

Pentru a configura o politică de retry folosind Polly, urmați pașii de mai jos:

1. **Adăugați pachetul Polly în proiectul dvs.**:
    ```bash
    dotnet add package Polly
    ```

2. **Configurați politica de retry în `Startup.cs`**:
    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<MyApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    }
    ```

3. **Utilizați HttpClient tipizat în controller sau serviciu**:
    ```csharp
    public class MyController : ControllerBase
    {
        private readonly MyApiClient _myApiClient;

        public MyController(MyApiClient myApiClient)
        {
            _myApiClient = myApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var data = await _myApiClient.GetDataAsync();
            return Ok(data);
        }
    }
    ```

În exemplul de mai sus, politica de retry este configurată pentru a reîncerca de 3 ori apelurile care eșuează din cauza unei `HttpRequestException` sau a unui răspuns cu codul de stare `InternalServerError`. Intervalul de timp dintre reîncercări crește exponențial (2, 4, 8 secunde).

### Exemple de utilizare a Polly

#### Politica de retry simplă
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .RetryAsync(3);

await retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("endpoint"));
```

#### Politica de retry cu backoff exponențial
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("endpoint"));
```

#### Politica de retry cu jitter (interval de timp aleatoriu)
```csharp
var jitterer = new Random();
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

await retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("endpoint"));
```