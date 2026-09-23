# ADR-0001: Domeniul este modelat printr-un sistem de tipuri închis, cu erorile transportate ca valori (`Result`), fără excepții și fără stări invalide

* **Stare**: Acceptată
* **Data**: 2026-09-22
* **Lucrarea**: 2 – Crearea unui sistem de tipuri pentru un model DDD ([README](../../README.md))
* **Extinsă de**: [ADR-0002](../../../Lucrarea-03/docs/adr/0002-operatii-pure-compuse-in-workflow.md) (operații și workflow)

## Context și problemă

Lucrarea 2 introduce primele două elemente ale unui model DDD: obiectele-valoare (nota, numărul matricol) și entitatea cu stările ei (examenul). Tot ce urmează în Lucrările 3–8 (operații, persistență, API, mesagerie) se construiește pe aceste tipuri, deci felul în care sunt definite decide cât de mult poate garanta compilatorul și cât rămâne pe seama disciplinei.

Versiunea anterioară a exemplului (pe .NET 8) avea următoarele probleme:

* Obiectele-valoare validau pe **două căi**: un constructor care arunca excepție și o metodă `TryParse`. Apelantul trebuia să știe pe care s-o folosească, iar cele două puteau diverge.
* Entitatea avea o stare **`Invalid`**: eșecul validării era modelat ca stare a examenului, deci un examen „invalid” putea fi transmis mai departe operațiilor următoare.
* Stările erau legate printr-o **interfață-marcaj deschisă** (`IExam`): orice clasă o putea implementa, deci fiecare `switch` avea nevoie de un `default` care arunca `NotImplementedException` – cod netestat, prezent în cinci locuri.
* `decimal.TryParse` folosea **cultura curentă**: pe un sistem ro-RO, „7.5” devenea 75.
* Un constructor privat fără parametri crea instanțe `Grade` cu `Value = 0`, adică o notă în afara domeniului de valori, iar expresia regulată era interpretată la fiecare apel.
* Nu exista nicio bibliotecă de tipuri funcționale, deci fiecare laborator improviza altfel raportarea erorilor (liste de `string`, excepții, `bool` + `out`).

## Factori de decizie

* **Pedagogic**: Lucrarea 2 trebuie să arate cum tipurile fac stările ilegale nereprezentabile (Wlaschin, cap. 4–6), fără a introduce încă operații sau workflow-uri.
* **O singură cale de construire** pentru fiecare tip, cu toate regulile într-un singur loc.
* **Toate erorile unei intrări raportate deodată**, nu prima întâlnită: utilizatorul corectează o singură dată.
* **Independență de cultură**: același cod trebuie să dea același rezultat pe ro-RO și en-US.
* **Fără dependențe externe în domeniu**: studenții copiază proiectele ca punct de plecare pentru proiectul de semestru.
* **.NET 10 / C# 14 ca cerință a cursului**: sintaxa aleasă trebuie să fie chiar poarta care impune versiunea (vezi „De ce .NET 10?” în [README-ul rădăcină](../../../README.md)).

## Opțiuni considerate

1. **Nucleu funcțional propriu** (`Examples.Functional`) + obiecte-valoare cu constructori inteligenți + ierarhii de stări închise.
2. **Bibliotecă existentă** de tipuri funcționale (LanguageExt).
3. **Stil orientat pe obiecte clasic**: constructori care aruncă excepții, `TryParse`, stare `Invalid` (varianta anterioară).

## Decizie

Opțiunea 1. Regulile concrete:

* `Result<TSuccess, TFailure>` este un **record abstract cu constructor privat** și două cazuri imbricate, `Ok(TSuccess Value)` și `Error(TFailure Value)`. `Match(ok, error)` este singurul loc din nucleu cu `throw new UnreachableException()`. Conversiile implicite din `TSuccess` și `TFailure` permit `return new GradeError.OutOfRange(value);` fără ambalare explicită.
* Combinatorii (`IsOk`, `Map`, `Bind`, `MapError`, `Ensure`, `Tap`, `GetValueOrThrow`) sunt **membri de extensie C# 14**, într-un bloc `extension<TSuccess, TFailure>(Result<TSuccess, TFailure> result)`.
* `Result.Combine` (cu 2 și 3 argumente) este **aplicativ**: păstrează toate erorile, spre deosebire de `Bind`, care se oprește la prima. `Traverse`/`Sequence` fac același lucru pentru colecții.
* `Option<T>` (`Some`/`None`) modelează absența legitimă (o notă finală care nu există), `Option.FromNullable` traduce coloanele nule ale bazei de date, `Unit` înlocuiește `void`.
* Obiectele-valoare sunt `sealed record` cu **constructor privat care doar atribuie** și metode statice `Create(decimal)` / `Parse(string?)` care întorc `Result<T, TError>`. Nu există `TryParse`, nu se aruncă excepții pentru intrări greșite.
* `Grade.Value { get; private init => field = Normalize(value); }` folosește cuvântul cheie **`field`** (C# 14): normalizarea (două decimale, rotunjire `AwayFromZero`) se întâmplă în singurul punct de scriere.
* `Grade.Parse` folosește `CultureInfo.InvariantCulture` (punctul este separatorul decimal, indiferent de sistem).
* `StudentRegistrationNumber` validează cu `[GeneratedRegex("^LM[0-9]{5}$")]` pe o proprietate parțială statică (expresie compilată la build).
* Stările entității: `abstract record Exam` cu constructor privat și cazurile `sealed record` imbricate `Unvalidated`, `Validated`, `Calculated`, `Published`. **Nu există o stare `Invalid`**: eșecul validării nu este o stare a examenului, ci o eroare transportată în `Result`. `Exam.Match` are exact patru ramuri.
* Erorile sunt **ierarhii închise** cu același tipar: `GradeError { NotANumber, OutOfRange }`, `ValidationError { InvalidRegistrationNumber, StudentNotFound, DuplicateRegistrationNumber, InvalidExamGrade, InvalidActivityGrade }` (cu `Code` și `ToMessage()`), `PublishExamError { Validation }`. `InvalidRegistrationNumberFormat(string? Raw)` este un record simplu, pentru că are un singur caz.
* Aplicația consolă din Lucrarea 2 exersează **doar tipurile**: fiecare linie brută trece prin `Result.Combine` (matricol, notă examen, notă activitate), liniile prin `Traverse`, iar existența studentului este simulată aleator – validarea reală vine în Lucrarea 3.

### Cum se reflectă în cod

| Regulă | Fișier |
|---|---|
| `Result` închis, `Match`, conversii implicite, `Combine` aplicativ | [Result.cs](../../Exemple/Examples.Functional/Result.cs) |
| Combinatori ca bloc `extension(...)` | [ResultExtensions.cs](../../Exemple/Examples.Functional/ResultExtensions.cs) |
| `Sequence` / `Traverse` pe colecții | [ResultCollectionExtensions.cs](../../Exemple/Examples.Functional/ResultCollectionExtensions.cs) |
| `Option<T>`, `Option.FromNullable` | [Option.cs](../../Exemple/Examples.Functional/Option.cs) |
| `Create`/`Parse`, `field`, cultură invariantă, `Average` | [Grade.cs](../../Exemple/Examples.Domain/ValueObjects/Grade.cs) |
| Ierarhie de eroare per obiect-valoare | [GradeError.cs](../../Exemple/Examples.Domain/ValueObjects/GradeError.cs) |
| `[GeneratedRegex]`, `Create(string?)` | [StudentRegistrationNumber.cs](../../Exemple/Examples.Domain/ValueObjects/StudentRegistrationNumber.cs) |
| Stări închise, fără `Invalid`, `Match` cu 4 ramuri | [Exam.cs](../../Exemple/Examples.Domain/States/Exam.cs) |
| Erori de validare cu `Code` și `ToMessage()` | [ValidationError.cs](../../Exemple/Examples.Domain/Errors/ValidationError.cs) |
| `Traverse` + `Combine` la marginea aplicației | [Program.cs](../../Exemple/Examples.ConsoleApp/Program.cs) |
| Poarta .NET 10 / C# 14 (`net10.0`, `LangVersion 14`, gardă SDK) | [Directory.Build.props](../../Exemple/Directory.Build.props), [Directory.Build.targets](../../Exemple/Directory.Build.targets), [global.json](../../../global.json) |

## Consecințe

### Pozitive

* O instanță de `Grade` sau `StudentRegistrationNumber` există **doar dacă este validă**; nu mai există `Value = 0` accidental și nici o a doua cale de validare care să divergă.
* Toate erorile unei linii (matricol și două note) și ale tuturor liniilor sunt raportate **deodată, în ordinea intrării**.
* Aceeași conversie de text dă același rezultat pe orice cultură.
* Domeniul nu are nicio dependență NuGet; nucleul funcțional are șapte fișiere și se poate citi integral într-o oră de laborator.
* Sintaxa folosită (blocuri de extensie, `field`) **nu compilează sub C# 14**, deci impune .NET 10 împreună cu `global.json`, `Directory.Build.props` și gardă din `Directory.Build.targets`.
* Același nucleu `Examples.Functional` este identic în Lucrările 2–8 (verificat automat de `tools/Check-LabConsistency.ps1`).

### Negative / compromisuri

* C# nu verifică exhaustivitatea pe ierarhii de record-uri: `Match` păstrează un `_ => throw new UnreachableException()`. Ierarhia este închisă prin constructorul privat, deci ramura este de neatins, dar compilatorul nu știe asta.
* Mai multe tipuri mici (câte un tip de eroare pentru fiecare obiect-valoare) – prețul plătit pentru claritate.
* **Limitare a compilatorului din SDK 10.0.4xx**: un argument de tip explicit pe un membru de extensie generic (de exemplu `.MapError<ValidationError>(...)`) nu se rezolvă (CS1061). Convenția din cod este conversia explicită în lambda: `.MapError(e => (ValidationError)new ValidationError.InvalidExamGrade(...))`. De reverificat la actualizările SDK-ului.
* `Result` este un record-clasă, nu o structură: o alocare per rezultat. Irelevant la scara laboratorului; un `struct` ar fi introdus un al treilea caz ilegal (`default`) și ar fi împiedicat potrivirea pe subtipuri.
* Studenții obișnuiți cu excepțiile au de învățat stilul „railway”: eroarea se propagă prin valoare, nu prin salt.

## Argumente pro și contra opțiunilor

### Opțiunea 1 – nucleu propriu + tipuri închise (aleasă)

* Bine, pentru că nu introduce dependențe și API-ul este exact cât se predă.
* Bine, pentru că `Combine`/`Traverse` fac vizibilă diferența dintre compunerea aplicativă (toate erorile) și cea monadică (prima eroare).
* Bine, pentru că ilustrează concret C# 14 și motivează cerința de versiune.
* Rău, pentru că reinventează un tip standard și nu oferă tot ce are o bibliotecă matură (LINQ asincron, `Validation`, `Either`).

### Opțiunea 2 – LanguageExt

* Bine, pentru că este completă, testată și folosită în producție.
* Rău, pentru că versiunea 5 era încă beta la data deciziei, iar versiunea 4 are un API foarte mare – curba de învățare depășește conceptul predat.
* Rău, pentru că numele `Map`/`Bind`/`Match` ar fi intrat în conflict cu blocurile de extensie proprii (CS0121) dacă am fi combinat cele două abordări.

### Opțiunea 3 – excepții + `TryParse` + stare `Invalid` (varianta anterioară)

* Bine, pentru că este familiară și cere puțin cod nou.
* Rău, pentru că două căi de validare pot diverge, iar excepția se oprește la prima eroare.
* Rău, pentru că eșecul devine o stare a entității, deci un examen „invalid” poate fi transmis mai departe.
* Rău, pentru că un `switch` pe o interfață deschisă are întotdeauna o ramură `default` netestată.

## Referințe

[1] Scott Wlaschin, *Domain Modeling Made Functional*, Pragmatic Bookshelf, 2018 – cap. 4 (Understanding Types), 5 (Domain Modeling with Types), 6 (Integrity and Consistency in the Domain), 10 (Working with Errors)

[2] Scott Wlaschin, [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)

[3] Microsoft Learn, [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14) – membri de extensie, cuvântul cheie `field`

[4] Microsoft Learn, [Regular expression source generators](https://learn.microsoft.com/dotnet/standard/base-types/regular-expression-source-generators)

[5] [MADR – Markdown Architectural Decision Records](https://adr.github.io/madr/) (formatul acestui document)
