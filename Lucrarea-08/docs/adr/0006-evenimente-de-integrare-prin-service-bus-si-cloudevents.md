# ADR-0006: Evenimentele de domeniu sunt publicate către alte contexte ca evenimente de integrare în format CloudEvents prin Azure Service Bus (topic + subscripții) și consumate de un worker, fără outbox

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 8 – Comunicare asincronă între contexte folosind evenimente ([README](../../README.md))
* **Extinde**: [ADR-0004](../../../Lucrarea-06/docs/adr/0004-api-minimal-cu-rezultate-tipizate.md) (API Web)
* **Vezi și**: [ADR-0005](../../../Lucrarea-07/docs/adr/0005-contracte-partajate-si-client-http-rezilient.md) – alternativa sincronă, ale cărei limite motivează acest ADR

## Context și problemă

Lucrarea 8 înlocuiește apelul HTTP sincron din Lucrarea 7 cu publicarea unui eveniment: după ce catalogul este salvat, API-ul de note anunță „notele au fost publicate”, iar un serviciu al contextului de cazare (`Examples.Accommodation.EventProcessor`) reacționează independent. API-ul nu mai depinde de disponibilitatea consumatorilor și pot apărea consumatori noi fără a-l modifica.

Versiunea anterioară a exemplului avea următoarele probleme:

* `IEventHandler` primea direct un `CloudEvent`: **transportul se scurgea în codul de business**. Rutarea se făcea după `typeof(T).Name`, deci redenumirea unei clase rupea consumatorii fără avertizare.
* Mesajele fără handler **nu erau niciodată decontate** (buclă de relivrare); un `catch` generic trata și anularea ca mesaj mort; `ContentType`-ul CloudEvents era pierdut; `Source` hard-codat; `DateTimeOffset.Now`.
* Worker-ul era un `IHostedService` cu `Console.WriteLine`, topic și subscripție hard-codate, fără `appsettings.json`.
* SDK-ul `Azure.Messaging.ServiceBus` 7.5 (2021) **nu se putea conecta la emulator**; `GradesPublishedEvent.ToString()` scria la consolă; contractul expunea un `List<T>` mutabil.
* O **cheie SAS reală** ajunsese în directorul de lucru (nu și în istoricul git): secretele trebuiau scoase din `appsettings.json`.

## Factori de decizie

* **Decuplarea disponibilității** între contexte (lecția din ADR-0005).
* **Handler-ul de business nu depinde de Service Bus sau CloudEvents**: primește un record și întoarce un rezultat.
* **Semantică la-cel-puțin-o-dată** cu mesaje moarte explicite, nu ignorate.
* **Rulare locală fără cont Azure**: emulatorul Service Bus în Docker.
* **Fără secrete în depozit.**
* Tema cursului este Azure Service Bus; formatul de eveniment trebuie să fie un standard, nu unul propriu.

## Opțiuni considerate

1. **Abstracții proprii agnostice de transport** (`Examples.Events`) + **adaptor Service Bus** cu CloudEvents în mod structurat.
2. **SDK-ul Service Bus direct** în API și în worker, fără abstracții (varianta anterioară, parțial).
3. **Framework de mesagerie** (MassTransit, NServiceBus, Wolverine).
4. **Menținerea apelurilor HTTP sincrone** din Lucrarea 7.

Subdecizii: topic cu subscripții **sau** coadă; plic CloudEvents **sau** JSON propriu; outbox tranzacțional **sau** fără (documentat).

## Decizie

Opțiunea 1, cu topic + subscripții, CloudEvents și fără outbox. Regulile concrete:

* `Examples.Events` (**zero dependențe**): `IIntegrationEvent { static abstract string EventType }` (numele stabil, versionat, al evenimentului – rezolvat la compilare, nu prin reflecție); `IEventSender.SendAsync<TEvent>(TopicName, TEvent, ct)`; `IEventHandler<in TEvent>.HandleAsync(...)` → `EventProcessingResult { Completed, Retry, Failed }`; `IEventListener : IAsyncDisposable` cu `StartAsync(topic, subscription, ct)` idempotent și `StopAsync`; `TopicName` și `SubscriptionName` ca `readonly record struct` cu gardă pe șir gol.
* Contractul `GradesPublishedEvent(PublishedAt, IReadOnlyList<StudentGradeDto> Grades)` cu `EventType = "pssc.grades.published.v1"` stă în `Examples.Contracts` (care referențiază `Examples.Events`). Este **distinct** de evenimentul de domeniu `ExamPublishedEvent`: maparea `ToIntegrationEvent()` (bloc de extensie în API) despachetează obiectele-valoare și transformă `Option<Grade>` în `decimal?`.
* `ServiceBusTopicEventSender` construiește un `CloudEvent` cu `Id = Guid.NewGuid()`, `Type = TEvent.EventType`, `Source` din opțiuni (`ServiceBusEventsOptions.Source`, implicit `urn:pssc:examples-api`), `Time` din `TimeProvider`, `Subject` = numele topicului, `DataContentType = application/json`, codat în **mod structurat JSON** cu `JsonEventFormatter` (`application/cloudevents+json`). `ServiceBusMessage` primește `MessageId = Id`, `ContentType` și `Subject = EventType`. Un `ServiceBusSender` per topic, ținut în cache.
* `ServiceBusTopicEventListener`: rute într-un `FrozenDictionary<string, IEventDispatcher>` după `EventType` (un duplicat eșuează la construcție); `SemaphoreSlim` pentru un `StartAsync` idempotent; `ServiceBusProcessor` cu `AutoCompleteMessages = false` și `MaxConcurrentCalls = 2`. Decontare: mesaj nedecodabil → dead-letter `InvalidCloudEvent`; tip fără handler → dead-letter `NoHandler`; `Completed` → `Complete`; `Retry` → `Abandon` (relivrare); `Failed` → dead-letter `HandlerFailed`. **O excepție a handler-ului nu este prinsă**: mesajul este abandonat de procesor și relivrat, iar broker-ul îl mută în mesaje moarte după `MaxDeliveryCount`.
* `EventDispatcher<TEvent>` deserializează cu `JsonSerializerOptions.Web` (`null` → `Failed`, corupție, nu eroare tranzitorie) și rezolvă handler-ul **scoped, per mesaj**, prin `IServiceScopeFactory`.
* Înregistrare: `AddServiceBusEventSender(configuration)` și `AddServiceBusEventListener().AddHandler<GradesPublishedEvent, GradesPublishedEventHandler>()`; `ServiceBusClient` vine din `AddAzureClients(... AddServiceBusClient(ConnectionStrings:ServiceBus))`.
* În API, `MessagingOptions.GradesTopic` (secțiunea `Messaging`, valoare `grades`) este validată la pornire; publicarea se face în endpoint **după** succesul workflow-ului, deci după salvarea în baza de date: `events.SendAsync(new TopicName(...), published.ToIntegrationEvent(), ct)`.
* Worker-ul `Examples.Accommodation.EventProcessor` folosește gazda generică (`Host.CreateApplicationBuilder`), `EventProcessorOptions { TopicName, SubscriptionName }` `[Required]` din secțiunea `EventProcessor` (`grades` / `accommodation`), `Worker : BackgroundService` (pornește ascultătorul, așteaptă oprirea, `StopAsync` → `listener.StopAsync`) și `GradesPublishedEventHandler`, care loghează prin `LoggerMessage` și întoarce `Completed`.
* Local: `compose.yaml` pornește `sqlserver` (SQL Server 2025), `sqlserver-init` (rulează scriptul din `Lucrarea-08/SQL`) și `servicebus-emulator` cu `infra/servicebus/Config.json` – topic `grades`, subscripție `accommodation`, `MaxDeliveryCount 5`, `LockDuration PT1M`, TTL `PT1H`, mesaje moarte la expirare; porturi 5672 (AMQP) și 5300 (sănătate). Variabilele de mediu vin din `.env`, copiat din `.env.example`.
* Secrete: connection string-ul emulatorului (o constantă publică) stă în `appsettings.Development.json`; orice altă valoare vine din `dotnet user-secrets` sau din variabila `ConnectionStrings__ServiceBus`. **Nicio cheie reală în depozit.**
* **Fără outbox**: salvarea în baza de date și trimiterea evenimentului sunt două operații separate; dacă procesul se oprește între ele, evenimentul se pierde. Limitarea este documentată în README ca pas următor.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| Abstracții fără dependențe: `static abstract EventType`, `EventProcessingResult` | [IIntegrationEvent.cs](../../Exemple/Examples.Events/IIntegrationEvent.cs), [IEventSender.cs](../../Exemple/Examples.Events/IEventSender.cs), [IEventHandler.cs](../../Exemple/Examples.Events/IEventHandler.cs), [IEventListener.cs](../../Exemple/Examples.Events/IEventListener.cs), [EventProcessingResult.cs](../../Exemple/Examples.Events/EventProcessingResult.cs), [TopicName.cs](../../Exemple/Examples.Events/TopicName.cs) |
| Contract de integrare versionat, separat de evenimentul de domeniu | [GradesPublishedEvent.cs](../../Exemple/Examples.Contracts/Events/GradesPublishedEvent.cs), [StudentGradeDto.cs](../../Exemple/Examples.Contracts/Models/StudentGradeDto.cs) |
| CloudEvents mod structurat, `TimeProvider`, `Source` din opțiuni | [ServiceBusTopicEventSender.cs](../../Exemple/Examples.Events.ServiceBus/ServiceBusTopicEventSender.cs), [ServiceBusEventsOptions.cs](../../Exemple/Examples.Events.ServiceBus/ServiceBusEventsOptions.cs) |
| Rutare `FrozenDictionary`, start idempotent, dead-letter cu motiv, excepțiile se propagă | [ServiceBusTopicEventListener.cs](../../Exemple/Examples.Events.ServiceBus/ServiceBusTopicEventListener.cs) |
| Deserializare `JsonSerializerOptions.Web`, handler scoped per mesaj | [EventDispatcher.cs](../../Exemple/Examples.Events.ServiceBus/EventDispatcher.cs) |
| `AddServiceBusEventSender`, `AddServiceBusEventListener().AddHandler<,>()` | [ServiceCollectionExtensions.cs](../../Exemple/Examples.Events.ServiceBus/ServiceCollectionExtensions.cs) |
| `ServiceBusClient` din `ConnectionStrings:ServiceBus`, `MessagingOptions` validate | [Program.cs](../../Exemple/Examples.Api/Program.cs), [MessagingOptions.cs](../../Exemple/Examples.Api/Messaging/MessagingOptions.cs), [appsettings.json](../../Exemple/Examples.Api/appsettings.json) |
| Publicare după salvarea în bază | [GradesEndpoints.cs](../../Exemple/Examples.Api/Endpoints/GradesEndpoints.cs) |
| `ExamPublishedEvent` → `GradesPublishedEvent` | [ExamPublishedEventMapping.cs](../../Exemple/Examples.Api/Mapping/ExamPublishedEventMapping.cs) |
| Worker `BackgroundService`, opțiuni validate, handler care doar loghează | [Program.cs](../../Exemple/Examples.Accommodation.EventProcessor/Program.cs), [Worker.cs](../../Exemple/Examples.Accommodation.EventProcessor/Worker.cs), [EventProcessorOptions.cs](../../Exemple/Examples.Accommodation.EventProcessor/EventProcessorOptions.cs), [GradesPublishedEventHandler.cs](../../Exemple/Examples.Accommodation.EventProcessor/GradesPublishedEventHandler.cs), [appsettings.json](../../Exemple/Examples.Accommodation.EventProcessor/appsettings.json) |
| Emulator + SQL Server local, topic/subscripție, `MaxDeliveryCount` | [compose.yaml](../../../compose.yaml), [Config.json](../../../infra/servicebus/Config.json), [.env.example](../../../.env.example) |
| Profil de lansare cu API și worker | [Examples.slnLaunch.user](../../Exemple/Examples.slnLaunch.user) |

## Consecințe

### Pozitive

* API-ul de note **nu depinde de disponibilitatea consumatorilor**; `POST /grades` răspunde 200 imediat după publicarea evenimentului.
* Un consumator nou înseamnă o subscripție nouă în `Config.json` și un handler nou – fără modificări în API.
* Handler-ul de business este cod .NET obișnuit, testabil fără Service Bus.
* Mesajele otrăvite ajung în coada de mesaje moarte **cu un motiv** (`InvalidCloudEvent`, `NoHandler`, `HandlerFailed`), iar erorile tranzitorii sunt relivrate de broker până la `MaxDeliveryCount`.
* Totul rulează local, cu `docker compose up -d` și două procese .NET.

### Negative / compromisuri

* **Consistență eventuală**: contextul de cazare vede notele cu întârziere.
* **Salvarea și publicarea nu sunt atomice**: o oprire între ele pierde evenimentul. Soluția (outbox tranzacțional) depășește scopul laboratorului și este menționată ca pas următor.
* **Consumatorii trebuie să fie idempotenți**: același eveniment poate fi livrat de mai multe ori.
* Versionarea contractului este responsabilitatea echipei (sufixul `.v1` din `EventType`); o schimbare incompatibilă cere un tip nou.
* Limitele emulatorului: doar AMQP peste TCP, fără persistență la repornire, TTL maxim o oră, WSL 2 necesar pe Windows; emulatorul își ține starea în același container SQL Server.
* `ServiceBusEventsOptions` este legată fără validare la pornire (are o valoare implicită validă); `Id`-ul CloudEvent este GUID aleator, nu ordonat – suficient pentru identificare, fără garanție de ordine.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – abstracții proprii + adaptor Service Bus cu CloudEvents (aleasă)

* Bine, pentru că `Examples.Events` arată exact conceptele predate (eveniment, handler, rezultat de procesare) fără zgomot de transport.
* Bine, pentru că plicul CloudEvents oferă metadate standard (`id`, `type`, `source`, `time`) și rutare după tip, interoperabile cu alte platforme.
* Rău, pentru că adaptorul (ascultător, dispecer, decontare) este cod propriu de întreținut.

### Opțiunea 2 – SDK direct, fără abstracții

* Bine, pentru că este cel mai puțin cod la început.
* Rău, pentru că handler-ele depind de `ServiceBusReceivedMessage`/`CloudEvent`, iar politica de decontare se repetă în fiecare consumator – exact starea anterioară.

### Opțiunea 3 – MassTransit, NServiceBus, Wolverine

* Bine, pentru că oferă outbox, reîncercări, sagas și mai multe transporturi gata făcute.
* Rău, pentru că ascund tocmai mecanismele pe care laboratorul vrea să le arate (topic, subscripție, decontare, mesaje moarte), iar unele au licențe comerciale.

### Opțiunea 4 – HTTP sincron (Lucrarea 7)

* Bine, pentru că este simplu și răspunsul este imediat.
* Rău, pentru că disponibilitatea API-ului depinde de fiecare consumator, iar un consumator nou cere modificarea apelantului.

### Topic + subscripții (aleasă) vs. coadă

* Bine, pentru că același eveniment ajunge la mai mulți consumatori, fiecare cu propria coadă de mesaje moarte.
* Rău, pentru că o coadă ar fi fost suficientă pentru un singur consumator; topicul anticipează consumatorii din sarcinile de laborator.

### Fără outbox (aleasă) vs. outbox tranzacțional

* Bine, pentru că laboratorul rămâne concentrat pe mesagerie, nu pe tranzacții distribuite.
* Rău, pentru că exemplul nu este corect în toate cazurile de defectare – limitare făcută explicită în README și aici.

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 3 (Communication between bounded contexts), 11 (Serialization)

[2] Microsoft Learn, [Ghid de start rapid: topicuri și subscripții Azure Service Bus (.NET)](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions)

[3] Microsoft Learn, [Emulatorul Azure Service Bus](https://learn.microsoft.com/azure/service-bus-messaging/overview-emulator)

[4] Microsoft Learn, [Cozi de mesaje moarte în Service Bus](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-dead-letter-queues)

[5] [CloudEvents – specificația v1.0](https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md) și [formatul JSON](https://github.com/cloudevents/spec/blob/main/cloudevents/formats/json-format.md); [SDK .NET](https://github.com/cloudevents/sdk-csharp)

[6] Microsoft Learn, [Background tasks with hosted services (`BackgroundService`)](https://learn.microsoft.com/dotnet/core/extensions/workers)

[7] Chris Richardson, [Pattern: Transactional outbox](https://microservices.io/patterns/data/transactional-outbox.html)
