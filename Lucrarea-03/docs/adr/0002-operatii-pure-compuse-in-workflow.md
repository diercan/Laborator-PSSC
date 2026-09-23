# ADR-0002: Comportamentul domeniului este exprimat ca operații pure definite pe starea exactă pe care o transformă și compuse într-un workflow pe model „railway”

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 3 – Implementarea unui „workflow” DDD ([README](../../README.md))
* **Extinde**: [ADR-0001](../../../Lucrarea-02/docs/adr/0001-sistem-de-tipuri-inchis-cu-erori-ca-valori.md) (sistemul de tipuri)
* **Extinsă de**: [ADR-0003](../../../Lucrarea-05/docs/adr/0003-persistenta-sql-first-la-marginile-workflow-ului.md) (persistență)

## Context și problemă

Lucrarea 3 adaugă comportamentul peste tipurile din Lucrarea 2: validarea notelor brute, calculul notei finale și publicarea catalogului, înlănțuite într-un workflow care se încheie cu un eveniment sau cu o eroare. Este și primul laborator cu teste automate.

Versiunea anterioară a exemplului avea următoarele probleme:

* O **ierarhie de clase-operație** (`DomainOperation<TEntity, TState, TResult>` → `ExamOperation<TState>` → `ExamOperation`) cu zece membri de redirecționare care nu făceau decât să ascundă un parametru, plus câte o clasă pentru fiecare operație.
* Operațiile erau definite pe **orice stare**: pentru stările pe care nu le tratau, întorceau starea neschimbată. Un pipeline ordonat greșit compila și eșua tăcut.
* Erorile de validare se acumulau prin **mutarea unei `List<string>`** transmise pe trei niveluri de apel.
* `DateTime.Now` în operația de publicare; CSV-ul era construit cu `Aggregate`, iar rezultatul era aruncat (fără antet, fără tratarea separatorului).
* Workflow-ul prindea `catch (Exception)` și întorcea „Unexpected error”: eșecurile de infrastructură deveneau erori de validare.
* Exista un **eveniment de eșec** (`ExamPublishFailedEvent`) alături de cel de succes, deși eșecul nu este un fapt petrecut în domeniu.
* Copiile domeniului din Lucrările 3, 5, 6, 7 și 8 divergeau (semnături diferite, vizibilități diferite, membri prezenți doar în unele laboratoare).

## Factori de decizie

* **Ordinea pașilor garantată de compilator**, nu de disciplină.
* **Nucleu pur**: fără I/O, fără ceas, fără logging în logica de business, ca să fie testabilă cu valori, nu cu mock-uri.
* **Evenimentele descriu fapte petrecute**; erorile descriu de ce nu s-a putut produce evenimentul (Wlaschin, cap. 7 și 10).
* **Un singur domeniu pentru Lucrările 3–8**, verificat automat, ca să nu mai apară derivă între copii.
* Sintaxa C# 14 (bloc `extension` pe tipul stării) exprimă direct ideea „operația aparține stării pe care o transformă”.

## Opțiuni considerate

1. **Module statice cu blocuri `extension(Exam.<Stare>)`**, un bloc pentru fiecare stare de intrare.
2. **Metode statice obișnuite** (`ExamValidation.Validate(Exam.Unvalidated exam, ...)`).
3. **Ierarhie de clase-operație** (varianta anterioară).

Pentru workflow: (a) un singur fișier cu nucleu static pur plus `ExecuteAsync` pe porturi, identic în Lucrările 3–8; (b) o funcție statică în Lucrarea 3 și o clasă separată începând cu Lucrarea 5.

Pentru teste: xunit.v3 pe Microsoft.Testing.Platform; xunit 2 pe VSTest cu FluentAssertions; MSTest sau TUnit.

## Decizie

Opțiunea 1, cu varianta (a) pentru workflow și xunit.v3 pentru teste. Regulile concrete:

* `ExamValidation` definește `extension(Exam.Unvalidated exam)` → `Validate(IReadOnlySet<StudentRegistrationNumber> knownStudents)` care întoarce `Result<Exam.Validated, IReadOnlyList<ValidationError>>`: `Traverse(ValidateGrade)` (fiecare linie, fiecare câmp, toate erorile) → `Bind(RejectDuplicates)` (regulă între linii) → `Map(new Exam.Validated)`. `ValidateGrade` combină aplicativ (`Result.Combine`) matricolul (`Create` → `MapError` → `Ensure(knownStudents.Contains, StudentNotFound)`), nota de examen și nota de activitate.
* `ExamCalculation` definește `extension(Exam.Validated exam)` → `Calculate()` care întoarce `Exam.Calculated` (funcție totală, nu poate eșua). Regula de business `FinalGrade(exam, activity)` întoarce `Option<Grade>`: media doar dacă ambele componente sunt ≥ `Grade.PassingThreshold`, altfel `None`.
* `ExamPublishing` definește `extension(Exam.Calculated exam)` → `Publish(DateTimeOffset publishedAt)` și `extension(Exam.Published exam)` → `ToEvent()`. Momentul publicării este **parametru**, nu citit din ceas. `GradesCsv.Render` produce CSV cu antetul `RegistrationNumber,ExamGrade,ActivityGrade,FinalGrade`, fără newline final; nota finală lipsă este câmp gol.
* **Nucleul pur** `PublishExamWorkflow.Publish(command, knownStudents, now)` este static: `new Exam.Unvalidated(command.Grades).Validate(knownStudents).MapError(errors => (PublishExamError)new PublishExamError.Validation(errors)).Map(v => v.Calculate()).Map(c => c.Publish(now))`. Dependențele (mulțimea studenților cunoscuți, momentul) sunt **valori**, nu servicii.
* Același fișier conține și `ExecuteAsync(command, cancellationToken)` peste porturile `IStudentsRepository` și `IGradesRepository` plus `TimeProvider` (secvența impur → pur → impur), folosit începând cu Lucrarea 5. **În Lucrarea 3, aplicația consolă apelează doar `Publish`**, cu patru numere matricole ținute în memorie și `DateTimeOffset.UtcNow`; porturile există, dar nu au încă nicio implementare.
* **Evenimente doar pe ramura de succes**: `ExamPublishedEvent(Grades, Csv, PublishedAt)`. Eșecul este `PublishExamError.Validation(errors)`, o ierarhie închisă cu `ToMessages()`. `ToString()` este `sealed override`: fără `sealed`, fiecare record derivat și-ar sintetiza propriul `ToString()` și l-ar ascunde pe cel al bazei (bug găsit și corectat în timpul refactorizării).
* În domeniu **nu** există `try/catch` generic, `ILogger` sau `DateTime.Now`. Excepțiile de infrastructură se propagă la margine.
* `Examples.Domain.Tests` folosește **xunit.v3** în modul Microsoft.Testing.Platform (`OutputType Exe`, `test.runner` în `global.json`), `Assert` din xunit (fără FluentAssertions, care are licență comercială din versiunea 8) și fake-uri scrise de mână: `FixedTimeProvider`, `FakeStudentsRepository`, `RecordingGradesRepository`. Sunt 21 de metode de test (13 `[Fact]`, 8 `[Theory]`, circa 40 de cazuri), toate pe nucleul pur.
* `Examples.Domain` și `Examples.Domain.Tests` sunt **identice** în Lucrările 3–8 (seturile „Domain” și „Domain.Tests” din `tools/lab-consistency.json`); o corecție se face o dată și se propagă cu `tools/Sync-Labs.ps1`.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| `Validate` doar pe `Exam.Unvalidated`; `Traverse` + `Combine` + `RejectDuplicates` | [ExamValidation.cs](../../Exemple/Examples.Domain/Operations/ExamValidation.cs) |
| `Calculate` doar pe `Exam.Validated`; regula `FinalGrade` → `Option<Grade>` | [ExamCalculation.cs](../../Exemple/Examples.Domain/Operations/ExamCalculation.cs) |
| `Publish(now)` doar pe `Exam.Calculated`; `ToEvent()` doar pe `Exam.Published` | [ExamPublishing.cs](../../Exemple/Examples.Domain/Operations/ExamPublishing.cs) |
| CSV cu antet, fără newline final | [GradesCsv.cs](../../Exemple/Examples.Domain/Operations/GradesCsv.cs) |
| Nucleu static pur `Publish` + `ExecuteAsync` pe porturi | [PublishExamWorkflow.cs](../../Exemple/Examples.Domain/Workflows/PublishExamWorkflow.cs) |
| Porturi fără implementare în Lucrarea 3 | [IStudentsRepository.cs](../../Exemple/Examples.Domain/Repositories/IStudentsRepository.cs), [IGradesRepository.cs](../../Exemple/Examples.Domain/Repositories/IGradesRepository.cs) |
| Comanda de intrare cu `RegistrationNumbers` derivat | [PublishExamCommand.cs](../../Exemple/Examples.Domain/Commands/PublishExamCommand.cs) |
| Eveniment doar pe succes | [ExamPublishedEvent.cs](../../Exemple/Examples.Domain/Events/ExamPublishedEvent.cs) |
| Eroare de workflow închisă, `sealed override ToString()` | [PublishExamError.cs](../../Exemple/Examples.Domain/Errors/PublishExamError.cs) |
| Acumularea erorilor în ordinea intrării (test) | [ExamValidationTests.cs](../../Exemple/Examples.Domain.Tests/Operations/ExamValidationTests.cs) |
| Workflow testat cu fake-uri, fără mock-uri | [PublishExamWorkflowTests.cs](../../Exemple/Examples.Domain.Tests/Workflows/PublishExamWorkflowTests.cs), [FixedTimeProvider.cs](../../Exemple/Examples.Domain.Tests/Workflows/FixedTimeProvider.cs) |
| xunit.v3 pe Microsoft.Testing.Platform | [Examples.Domain.Tests.csproj](../../Exemple/Examples.Domain.Tests/Examples.Domain.Tests.csproj), [global.json](../../../global.json) |
| Consola folosește doar nucleul pur, studenți în memorie | [Program.cs](../../Exemple/Examples.ConsoleApp/Program.cs) |
| Domeniu identic în Lucrările 3–8 | [lab-consistency.json](../../../tools/lab-consistency.json) |

## Consecințe

### Pozitive

* `Validate` există doar pe `Unvalidated`, `Calculate` doar pe `Validated`, `Publish` doar pe `Calculated`: **un pipeline ordonat greșit nu compilează** (`exam.Calculate()` pe un `Exam.Unvalidated` este eroare de compilare), nu eșuează tăcut.
* Ramura de eroare sare peste restul pipeline-ului fără cod suplimentar (`Map` pe `Error` este identitate) – modelul „railway”.
* Nucleul pur se testează cu valori de intrare și ieșire; același set de teste rulează neschimbat în Lucrările 3–8.
* Toate erorile sunt raportate în ordinea intrării, pe toate liniile și toate câmpurile (testul `Validate_accumulates_every_error_across_every_row_and_field_in_order`).
* Un singur domeniu pentru șase laboratoare: o corecție (de exemplu `sealed override ToString()`) se propagă peste tot.

### Negative / compromisuri

* Blocurile `extension` sunt sintaxă nouă; studenții o întâlnesc prima dată aici. Convenția „conversie explicită în lambda” la `MapError` (vezi ADR-0001) trebuie explicată.
* În Lucrarea 3 studentul vede două porturi (`Repositories/`) și un `ExecuteAsync` pe care aplicația consolă nu le folosește. Este costul acceptat al unui domeniu comun; README-ul și comentariile din cod explică de ce.
* `RejectDuplicates` rulează după validarea fiecărei linii: un matricol duplicat cu o notă invalidă raportează întâi nota. Acceptat – toate erorile ajung oricum la utilizator.
* Microsoft.Testing.Platform schimbă comanda de testare (`dotnet test --solution ...`, fără filtrele VSTest); Visual Studio 2026 și Rider îl suportă nativ.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – blocuri `extension` pe starea de intrare (aleasă)

* Bine, pentru că pipeline-ul se citește în ordinea execuției (`exam.Validate(...).Map(v => v.Calculate())`) și tipul receptorului documentează precondiția.
* Bine, pentru că tranzițiile ilegale sunt erori de compilare.
* Rău, pentru că depinde de C# 14 și de o limitare a compilatorului la argumente de tip explicite.

### Opțiunea 2 – metode statice obișnuite

* Bine, pentru că este echivalentă funcțional și nu cere sintaxă nouă.
* Rău, pentru că pipeline-ul se citește invers (`Publish(Calculate(Validate(exam)))`) sau cere variabile intermediare, și nu ilustrează C# 14 – cerință a cursului.

### Opțiunea 3 – ierarhie de clase-operație (varianta anterioară)

* Bine, pentru că seamănă cu tiparele orientate pe obiecte cunoscute.
* Rău, pentru că operațiile „identitate” pe stările greșite ascund erorile de ordonare, iar cei zece membri de redirecționare nu adaugă nicio garanție.

### Workflow (a) – un fișier, nucleu static + `ExecuteAsync` (aleasă)

* Bine, pentru că domeniul rămâne identic din Lucrarea 3 până în Lucrarea 8 și testele nu se dublează.
* Rău, pentru că în Lucrarea 3 apar membri nefolosiți încă.

### Workflow (b) – funcție statică în 3, clasă în 5

* Bine, pentru că fiecare laborator arată exact cât are nevoie.
* Rău, pentru că reintroduce două copii ale domeniului care evoluează separat – exact problema care a generat deriva istorică.

### Teste – xunit.v3 pe Microsoft.Testing.Platform (aleasă)

* Bine, pentru că este platforma implicită în .NET 10 și rulează nativ în Visual Studio 2026; `Assert` acoperă tot ce se testează aici.
* Rău, pentru că tutorialele online încă arată xunit 2 pe VSTest, iar FluentAssertions (folosit în multe proiecte) nu mai este liber pentru uz comercial din versiunea 8.

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 7 (Modeling Workflows as Pipelines), 8 (Understanding Functions), 9 (Implementation: Composing a Pipeline), 10 (Working with Errors)

[2] Scott Wlaschin, [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)

[3] Microsoft Learn, [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14) – membri de extensie

[4] xUnit.net, [Getting started with xUnit.net v3 and Microsoft.Testing.Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)

[5] Microsoft Learn, [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
