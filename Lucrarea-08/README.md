# Lucrarea 8: Comunicare asincronă între contexte folosind evenimente

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: publicarea evenimentelor de domeniu către alte contexte prin **Azure Service Bus** (topic + subscripții), în format **CloudEvents**; consumarea lor într-un serviciu worker (`BackgroundService`); rularea locală cu **emulatorul Azure Service Bus** (Docker), fără a avea nevoie de un namespace Azure real.

**Sarcina 1**

Necesită baza de date din [Lucrarea 5](../Lucrarea-05/README.md). Analizați și rulați soluția din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`). Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă: proiectele `Examples.Events` (abstracții independente de transport), `Examples.Events.ServiceBus` (implementarea peste Azure Service Bus) și `Examples.Accommodation.EventProcessor` (worker-ul care ascultă evenimentele).

**Rulare**

```
copy ..\.env.example ..\.env
docker compose -f ..\compose.yaml up -d
dotnet run --project Exemple/Examples.Api
dotnet run --project Exemple/Examples.Accommodation.EventProcessor
```

`docker compose` pornește SQL Server și emulatorul Service Bus cu topicul `grades` și subscripția `accommodation`, definite în [infra/servicebus/Config.json](../infra/servicebus/Config.json). Un `POST /grades` către API publică notele și trimite evenimentul; worker-ul îl preia și afișează un mesaj în consolă.

Connection string-ul emulatorului este o valoare publică, aceeași pentru toată lumea:
```
Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```
Nu puneți niciodată un connection string real (către un namespace Azure adevărat) într-un fișier `appsettings.json` — folosiți `dotnet user-secrets` sau o variabilă de mediu (`ConnectionStrings__ServiceBus`).

**Sarcina 2**

Adăugați în [infra/servicebus/Config.json](../infra/servicebus/Config.json) o coadă `orders` și un topic `order-placed` cu subscripțiile `invoicing` și `shipping`. Implementați un emițător și un receptor ce comunică prin coadă (1-la-1), apoi comunicare 1-la-mai-mulți folosind topicul (vezi referințele [2] și [3]).

**Sarcina 3**

În contextul workflow-ului pentru plasarea unei comenzi realizați următoarele:
* generați un eveniment care să indice faptul că o comandă a fost preluată
* procesați evenimentul pentru a genera factura (se va apela procesul de generare a facturii)
* procesați evenimentul pentru a iniția livrarea (se va apela procesul de livrare)
* trebuie implementat la alegere fie workflow-ul pentru a genera factura fie cel pentru a iniția livrarea

## Concepte

* **Livrare cel-puțin-o-dată (at-least-once) și idempotență**: un mesaj poate fi livrat de mai multe ori (de exemplu după o eroare de rețea între procesarea și confirmarea lui); handler-ele trebuie scrise ca reluarea aceleiași procesări să nu strice starea.
* **Mesaje moarte (dead-letter)**: un mesaj pe care handler-ul nu-l poate trata (format nedecodabil, niciun handler înregistrat pentru tipul lui) este mutat explicit în coada de mesaje moarte a subscripției, nu doar ignorat — vedeți `EventProcessingResult` (`Completed`/`Retry`/`Failed`) și modul în care `ServiceBusTopicEventListener` decontează fiecare rezultat.
* **Rutare după tipul evenimentului**: fiecare eveniment de integrare (`IIntegrationEvent`) își declară propriul `EventType` (un șir stabil, versionat, de exemplu `"pssc.grades.published.v1"`), folosit pentru rutare — nu numele clasei C#, care s-ar putea schimba fără să anunțe consumatorii.
* **Salvarea în bază urmată de trimiterea evenimentului nu este o singură tranzacție**: dacă aplicația se oprește exact între cele două, evenimentul se pierde. Soluția completă (un *outbox* tranzacțional) depășește scopul acestui laborator — e menționată aici ca următor pas posibil.

## Referințe

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/), Pragmatic Bookshelf, 2018 — cap. 3 (comunicarea între contexte delimitate), cap. 11

[2] Microsoft Documentation, [Ghid de start rapid: cozi Azure Service Bus (.NET)](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues)

[3] Microsoft Documentation, [Ghid de start rapid: topicuri și subscripții (.NET)](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions)

[4] Microsoft Documentation, [Emulatorul Azure Service Bus](https://learn.microsoft.com/azure/service-bus-messaging/overview-emulator)

[5] [CloudEvents — specificația formatului de eveniment](https://cloudevents.io/)
