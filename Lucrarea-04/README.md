# Lucrarea 4: Proiectarea Asistată de AI a Sistemelor Software Complexe

**Context**: Proiectarea unui sistem Domain Driven Design prin explorare rapidă folosind instrumente AI

**Obiective**:
- Analiza unui domeniu nou folosind AI pentru descoperirea conceptelor DDD
- Aplicarea Event Storming pentru identificarea evenimentelor domeniului
- Implementarea unui sistem de tipuri și workflow-uri folosind GitHub Copilot
- Dezvoltarea abilităților de prompt engineering pentru design software
- Înțelegerea echilibrului între asistență AI și expertiză în domeniu

**Prerequisite**:
- Lucrările 2 și 3 completate (sisteme de tipuri DDD și workflow-uri)
- Acces la GitHub Copilot sau alternative (ChatGPT, Claude, etc.)
- Cunoștințe despre bounded contexts și domain modeling

**Durată**: 2 ședințe de laborator (2 × 2 ore). Ședința 1: Părțile 1-2 (105 min). Ședința 2: Părțile 3-5 (120 min). Cu o singură ședință: Partea 1 ca temă pre-laborator, Partea 5 redusă la 15 minute.

---

## Partea 1: Event Storming și Descoperirea Domeniului (45 minute)

### Sarcina 1.1: Alegerea Domeniului

**Lucrați în echipe de 3 persoane.** Alegeți unul din următoarele domenii:

#### **Opțiunea A: Sistem de Gestionare a Sesiunii de Examene**

**Descriere**: Un sistem pentru planificarea, desfășurarea și finalizarea sesiunii de examene la nivel de facultate.

**Actori principali**:
- Secretariat (planifică examene, alocă săli)
- Profesori (propun date, corectează, publică note)
- Studenți (se înscriu, vizualizează rezultate, contestă)
- Administrator sistem (gestionează capacități, conflicte)

**Scenarii cheie**:
- Profesorul propune 3 date posibile pentru examen
- Secretariatul validează disponibilitatea sălilor
- Studenții se înscriu la examene (cu restricții: max 2 examene/zi)
- Profesorul introduce notele și le publică
- Studentul contestă nota în termen de 48h
- Sistemul generează rapoarte pentru promovabilitate

#### **Opțiunea B: Sistem de Alocare a Locurilor în Cămin**

**Descriere**: Un sistem pentru gestionarea procesului de cazare în căminele studențești.

**Actori principali**:
- Studenți (aplică pentru cazare, aleg preferințe)
- Administrator cămin (verifică eligibilitate, alocă camere)
- Comisie de cazare (aprobă cereri speciale, rezolvă litigii)
- Sistem de plăți (verifică plata taxelor)

**Scenarii cheie**:
- Studentul depune cerere de cazare cu documente justificative
- Sistemul calculează punctajul (distanță, medie, venit, situații speciale)
- Administrator verifică documentele și eligibilitatea
- Sistemul generează lista de alocare bazată pe punctaj
- Studentul acceptă/refuză locul alocat
- La refuz, locul este realocat următorului din listă
- Sistemul gestionează schimbările de cameră după cazare

#### **Opțiunea C: Sistem de Rezervare a Spațiilor de Studiu**

**Descriere**: Un sistem pentru rezervarea sălilor de studiu, a sălilor de grup și a echipamentelor din bibliotecă/cămin.

**Actori principali**:
- Studenți (rezervă spații, echipamente)
- Bibliotecar/Administrator (aprobă, monitorizează utilizarea)
- Sistem automat (gestionează disponibilitatea, trimite reminder-e)

**Scenarii cheie**:
- Studentul caută spații disponibile (filtru: capacitate, echipament, interval orar)
- Sistemul verifică regulile (max 2h/zi pentru spații individuale, max 4h pentru săli de grup)
- Rezervarea este confirmată automat sau necesită aprobare pentru intervale lungi
- Sistemul trimite reminder cu 30 min înainte
- La absență (> 15 min întârziere), rezervarea se anulează automat
- Penalizări pentru anulări repetate (3 no-show-uri = blocare 1 săptămână)
- Sistemul generează statistici de utilizare

### Sarcina 1.2: Event Storming Asistat de AI

**Pas 1: Brainstorming inițial (15 minute)**

Lucrați în echipă pentru a identifica evenimentele domeniului. Folosiți post-it-uri fizice sau un tool digital (Miro, Mural).

**Prompt template pentru AI** (folosiți ChatGPT/Claude pentru a valida și extinde lista):

```
Sunt în procesul de Event Storming pentru un sistem de [DESCRIERE DOMENIU].

Actorii principali sunt:
- [ACTOR 1]: [rol]
- [ACTOR 2]: [rol]
- ...

Scenariile cheie includ:
- [SCENARIU 1]
- [SCENARIU 2]
- ...

Ajută-mă să identific evenimentele de domeniu (domain events) care ar putea apărea în acest sistem. 
Pentru fiecare eveniment, specifică:
1. Numele evenimentului (la trecut, ex: "ExamenPlanificat", "CazareAprobată")
2. Cine/ce declanșează evenimentul
3. Ce date sunt asociate evenimentului
4. Ce alte evenimente ar putea urma

Formatul: EventName | Trigger | Data | Subsequent Events
```

**Pas 2: Organizarea evenimentelor (10 minute)**

Aranjați evenimentele în ordine cronologică și grupați-le în **bounded contexts** (contexte delimitate).

**Prompt pentru validare**:

```
Am identificat următoarele evenimente pentru domeniul [NUME DOMENIU]:
[LISTA DE EVENIMENTE]

Ajută-mă să le organizez în bounded contexts logice. 
Pentru fiecare context, specifică:
- Numele contextului
- Evenimentele care aparțin contextului
- Responsabilitățile contextului
- Cum comunică cu alte contexte (evenimente partajate, comenzi)
```

**Pas 3: Identificarea comenzilor și agregărilor (20 minute)**

Pentru fiecare eveniment, identificați:
- **Comanda** care declanșează evenimentul
- **Agregarea** (aggregate) care procesează comanda
- **Regulile de business** care trebuie validate

**Prompt pentru extragere**:

```
Pentru bounded contextul [NUME CONTEXT] cu evenimentele:
[LISTA EVENIMENTE]

Ajută-mă să identific:
1. Comenzile care declanșează fiecare eveniment (ex: PlaseazăComandă → ComandăPlasată)
2. Agregările care gestionează logica de business
3. Regulile de validare pentru fiecare comandă
4. Invarianții care trebuie menținuți de fiecare agregare

Folosește principiile Domain Driven Design și oferă exemple în C#.
```

**Deliverable Partea 1**: Document/diagram cu evenimentele identificate, organizate în bounded contexts, cu comenzi și agregări asociate.

---

## Partea 2: Implementarea Sistemului de Tipuri cu GitHub Copilot (60 minute)

### Sarcina 2.1: Analiza Pattern-urilor din Codul de Referință

**Studiați implementarea workflow-ului de publicare a notelor** din [Lucrarea 3](../Lucrarea-03/Exemple/Examples.Domain/) pentru a identifica următoarele pattern-uri:

#### **Pattern 1: Obiecte-Valoare Imutabile, cu Smart Constructor**

```csharp
// Exemplu: StudentRegistrationNumber (Examples.Domain/ValueObjects/StudentRegistrationNumber.cs)
public sealed partial record StudentRegistrationNumber
{
    [GeneratedRegex("^LM[0-9]{5}$")]
    private static partial Regex Pattern { get; }
    public string Value { get; }

    private StudentRegistrationNumber(string value) => Value = value;

    public static Result<StudentRegistrationNumber, InvalidRegistrationNumberFormat> Create(string? raw) =>
        raw is not null && Pattern.IsMatch(raw) ? new StudentRegistrationNumber(raw) : new InvalidRegistrationNumberFormat(raw);

    public override string ToString() => Value;
}
```

**Pattern observat**:
- Constructor privat pentru control total
- Metodă statică `Create` (sau `Parse` pentru text) care întoarce `Result<T, TError>` — **niciodată nu aruncă excepție** pentru date de intrare nevalide
- Imutabilitate (doar `get`)
- Override `ToString()` pentru serializare

#### **Pattern 2: Stări ale Entității (Tip-Sumă Închis)**

```csharp
// Exemplu: Exam states (Examples.Domain/States/Exam.cs)
public abstract record Exam
{
    private Exam() { }

    public sealed record Unvalidated(IReadOnlyList<UnvalidatedStudentGrade> Grades) : Exam;
    public sealed record Validated(IReadOnlyList<ValidatedStudentGrade> Grades) : Exam;
    public sealed record Calculated(IReadOnlyList<CalculatedStudentGrade> Grades) : Exam;
    public sealed record Published(IReadOnlyList<CalculatedStudentGrade> Grades, string Csv, DateTimeOffset PublishedAt) : Exam;

    public TResult Match<TResult>(Func<Unvalidated, TResult> unvalidated, Func<Validated, TResult> validated,
        Func<Calculated, TResult> calculated, Func<Published, TResult> published) => this switch { /* ... */ };
}
```

**Pattern observat**:
- `abstract record` cu constructor **privat** = tip-sumă închis, nu o interfață deschisă
- Fiecare stare = înregistrare imbricată `sealed`
- **Nicio stare `Invalid`**: un eșec de validare nu e o stare a examenului, e o eroare (vezi Pattern 4)
- `Match` oferă potrivire exhaustivă, verificată de compilator

#### **Pattern 3: Operații de Domeniu (Funcții Pure ca Membri de Extensie)**

```csharp
// Exemplu: Examples.Domain/Operations/ExamCalculation.cs
public static class ExamCalculation
{
    extension(Exam.Validated exam)
    {
        public Exam.Calculated Calculate() => new([.. exam.Grades.Select(CalculateGrade)]);
    }
}
```

**Pattern observat**:
- Membru de extensie C# 14 (`extension(...)`) pe **starea exactă** pe care o transformă — a apela `.Calculate()` pe o stare greșită e eroare de compilare, nu un no-op silențios
- Funcție **totală** (nu poate eșua) → întoarce direct starea următoare; o funcție care poate eșua întoarce `Result<StareUrmătoare, IReadOnlyList<Eroare>>`
- Fără clase de bază, fără metode virtuale `OnXxx`

#### **Pattern 4: Workflow = Compoziția Operațiilor cu `Result`**

```csharp
// Examples.Domain/Workflows/PublishExamWorkflow.cs
public static Result<Exam.Published, PublishExamError> Publish(
    PublishExamCommand command, IReadOnlySet<StudentRegistrationNumber> knownStudents, DateTimeOffset now) =>
    new Exam.Unvalidated(command.Grades)
        .Validate(knownStudents)
        .MapError(errors => (PublishExamError)new PublishExamError.Validation(errors))
        .Map(validated => validated.Calculate())
        .Map(calculated => calculated.Publish(now));
```

**Pattern observat**:
- Workflow = pipeline de transformări compuse cu `Map`/`Bind` (stil *railway-oriented programming*)
- Ramura de eroare (`Result.Error`) sare direct la final — pașii următori nu se execută
- Dependențele (aici `knownStudents`, `now`) sunt parametri, nu câmpuri injectate prin constructor
- Rezultatul final e tot un `Result`: succes = eveniment de domeniu, eșec = eroare de domeniu (nu un eveniment de eșec)

### Sarcina 2.2: Configurarea GitHub Copilot pentru Pattern-uri DDD

**Copiați fișierul [`copilot-instructions.md`](copilot-instructions.md)** în `.github/copilot-instructions.md` din repository-ul echipei — de acolo îl citesc automat GitHub Copilot în VS Code, Visual Studio și Rider.

### Sarcina 2.3: Implementarea Value Objects cu Copilot

**Obiectiv**: Creați 3-5 value objects pentru domeniul vostru folosind GitHub Copilot.

**Pași**:

1. **Identificați value objects necesare** din evenimentele și comenzile identificate în Event Storming
2. **Pentru fiecare value object**, scrieți un comentariu descriptiv urmând acest template:

```csharp
// Create value object for [NUME] representing [DESCRIERE]
// Validation rules:
// - [REGULĂ 1]
// - [REGULĂ 2]
// - [REGULĂ 3]
// Valid examples: [EX1], [EX2]
// Invalid examples: [EX_INVALID1], [EX_INVALID2]
// Follow the pattern from copilot-instructions.md
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create value object for CourseCode representing the unique identifier of a university course
// Validation rules:
// - Format: 2-4 uppercase letters optionally followed by a digit
// - Examples: "PSSC", "BD", "POO2", "MATH"
// - Must not be empty or whitespace
// Valid examples: "PSSC", "BD", "MATH1"
// Invalid examples: "pssc" (lowercase), "P" (too short), "ABCDE" (too long), ""
// Follow the pattern from copilot-instructions.md

public record CourseCode
{
    // Copilot va genera implementarea...
}
```

3. **Verificați codul generat** asigurându-vă că:
   - Constructor este `private`
   - Are metodă statică `Create` (sau `Parse`) care întoarce `Result<T, TError>` — nu aruncă excepție
   - Properties sunt `{ get; }` only
   - Nu are validare cu efecte externe în constructor (doar format/structură)

4. **Testați manual** în Program.cs:
```csharp
// Cazul valid
Result<CourseCode, CourseCodeError> valid = CourseCode.Create("PSSC");
Console.WriteLine(valid.Match(code => $"Valid: {code}", error => $"Neașteptat: {error}"));

// Cazul invalid
Result<CourseCode, CourseCodeError> invalid = CourseCode.Create("invalid");
Console.WriteLine(invalid.Match(code => $"Neașteptat: {code}", error => "Respins corect"));
```

---

### Sarcina 2.4: Implementarea Entity States

**Obiectiv**: Definiți toate stările pe care le parcurge entitatea principală din workflow-ul vostru.

**Pași**:

1. **Identificați stările necesare** bazat pe flow-ul evenimentelor din Event Storming
2. **Scrieți comentariul pentru Copilot**:

```csharp
// Create entity states for [ENTITY_NAME] following the pattern from copilot-instructions.md
//
// State flow (no Invalid state: a failed transition is a Result error, not a state):
// [Entity].Unvalidated → [Entity].Validated → [Entity].[IntermediateState] → [Entity].[FinalState]
//
// States needed (nested sealed records inside an abstract record [Entity] with a private constructor):
// 1. Unvalidated: Raw input with string properties: [list properties]
// 2. Validated: After validation with value objects: [list typed properties]
// 3. [IntermediateState]: [description and properties]
// 4. [FinalState]: [description and properties]
//
// Provide a Match<TResult> fold on [Entity] for exhaustive handling.
// Use IReadOnlyList<T>/IReadOnlySet<T> for collections.

public abstract record [EntityName]
{
    // Copilot va genera constructorul privat, stările imbricate și Match...
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create entity states for ExamScheduling following the pattern from copilot-instructions.md
//
// State flow (no Invalid state):
// ExamScheduling.Unvalidated → ExamScheduling.Validated → ExamScheduling.RoomAllocated → ExamScheduling.Published
//
// States needed:
// 1. Unvalidated: Raw input with properties: courseCode (string), proposedDate1/2/3 (string), duration (string), expectedStudents (string)
// 2. Validated: After validation with: courseCode (CourseCode), proposedDates (IReadOnlyList<ExamDate>), duration (Duration), expectedStudents (Capacity)
// 3. RoomAllocated: After room allocation with: courseCode, selectedDate (ExamDate), duration, room (RoomNumber), roomCapacity (Capacity)
// 4. Published: After publishing with all above plus: publishedAt (DateTimeOffset), enrolledStudents (Capacity)
//
// Provide a Match<TResult> fold. Use IReadOnlyList<T> for collections.

public abstract record ExamScheduling
{
    // Copilot va genera...
}
```

---

## Partea 3: Implementarea Operațiilor și Workflow-ului (60 minute)

### Sarcina 3.1: Implementarea Operației de Validare

**Obiectiv**: Creați funcția care transformă starea `Unvalidated` în `Validated`, sau întoarce lista de erori.

**Pași**:

1. **Identificați dependențele externe** necesare (verificări în baze de date, servicii externe)
2. **Scrieți comentariul pentru Copilot**:

```csharp
// Create a Validate extension member on [Entity].Unvalidated, following the pattern from copilot-instructions.md
//
// This function transforms [Entity].Unvalidated to Result<[Entity].Validated, IReadOnlyList<ValidationError>>
//
// Dependencies (as function parameters, not constructor fields):
// - IReadOnlySet<[ValueObject]> known: [description of what it checks]
//
// Validation steps:
// 1. Parse each string field to its value object using Create/Parse
// 2. Combine all per-field results with Result.Combine so every error is reported, not just the first
// 3. For fields requiring an existence check, use .Ensure(known.Contains, value => new ValidationError.NotFound(value))
// 4. On success, return [Entity].Validated with all parsed value objects
//
// Write it as: extension([Entity].Unvalidated entity) { public Result<...> Validate(...) => ... }

public static class [Entity]Validation
{
    // Copilot va genera...
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create a Validate extension member on ExamScheduling.Unvalidated, following the pattern from copilot-instructions.md
//
// This function transforms ExamScheduling.Unvalidated to Result<ExamScheduling.Validated, IReadOnlyList<ValidationError>>
//
// Dependencies (as function parameters):
// - IReadOnlySet<CourseCode> existingCourses: Courses in the catalog
// - IReadOnlyDictionary<CourseCode, DateTimeOffset> courseEndDates: When each course ends, for date validation
//
// Validation steps:
// 1. Parse courseCode string to CourseCode using Create
// 2. Ensure the parsed CourseCode exists in existingCourses
// 3. Parse all three proposed dates to ExamDate
// 4. For each date, verify it's at least 7 days after the course's end date
// 5. Parse duration string to Duration, expectedStudents string to Capacity
// 6. Combine all of the above with Result.Combine so every error is reported
// 7. On success, return ExamScheduling.Validated with all parsed value objects

public static class ExamSchedulingValidation
{
    // Copilot va genera...
}
```

---

### Sarcina 3.2: Implementarea Operațiilor de Business Logic

**Obiectiv**: Creați 2-3 funcții care implementează regulile de business și transformă entitatea prin stările intermediare.

**Template comentariu**:

```csharp
// Create a [Verb] extension member on [Entity].[SourceState], following the pattern from copilot-instructions.md
//
// This function transforms [Entity].[SourceState] to [Entity].[TargetState] (total: cannot fail)
// — or to Result<[Entity].[TargetState], IReadOnlyList<[Error]>> if it can fail
//
// Dependencies (if any, as function parameters):
// - [Type] [dependencyName]: [description]
//
// Business logic:
// - [BUSINESS RULE 1]
// - [BUSINESS RULE 2]
// - [CALCULATION/TRANSFORMATION if applicable]
//
// Write it as: extension([Entity].[SourceState] entity) { public [Entity].[TargetState] [Verb]([params]) => ... }
// Use LINQ Select to transform each item if working with collections

public static class [Entity][Verb]
{
    // Copilot va genera...
}
```

**Exemplu pentru Alocare Cămin - Calculare Punctaj**:

```csharp
// Create a Score extension member on DormitoryApplication.Validated, following the pattern from copilot-instructions.md
//
// This function transforms DormitoryApplication.Validated to DormitoryApplication.Scored (total: cannot fail)
//
// Dependencies (as a function parameter):
// - decimal incomeThreshold: Threshold for income points calculation
//
// Business logic:
// - Grade points: (averageGrade - 5.00) * 8, max 40 points
// - Distance points: (distance / 10) * 3, max 30 points
// - Income points: if income < threshold then 20 points, else 0
// - Special situation points: if hasSpecialSituation then 10 points, else 0
// - Total score = sum of all points (max 100)
//
// Write it as: extension(DormitoryApplication.Validated application) { public DormitoryApplication.Scored Score(decimal incomeThreshold) => ... }

public static class DormitoryApplicationScoring
{
    // Copilot va genera...
}
```

---

### Sarcina 3.3: Implementarea Evenimentelor și Erorilor

**Obiectiv**: Definiți evenimentul de succes și eroarea de domeniu pentru workflow.

**Template comentariu**:

```csharp
// Create [Entity][Action]Event and [Action][Entity]Error, following the pattern from copilot-instructions.md
//
// Event (success only, no failure event):
// public sealed record [Entity][Action]Event([Property1] [Type], [Property2] [Type], DateTimeOffset [Action]At);
//
// Error (closed hierarchy, one case per distinct failure reason):
// public abstract record [Action][Entity]Error
// {
//     private [Action][Entity]Error() { }
//     public sealed record Validation(IReadOnlyList<ValidationError> Errors) : [Action][Entity]Error;
//     // add other cases here if the workflow has other ways to fail
//
//     public IReadOnlyList<string> ToMessages() => this switch { ... };
// }
//
// Extension method ToEvent(this [Entity].[FinalState] entity):
// - Produces the event directly from the final state (no pattern matching over failure states needed —
//   failure never reaches this point, it already left the pipeline as a Result.Error)

public sealed record [Entity][Action]Event(/* ... */);
public abstract record [Action][Entity]Error { /* Copilot va genera... */ }
```

---

### Sarcina 3.4: Implementarea Workflow-ului

**Obiectiv**: Compuneți toate operațiile într-un workflow complet.

**Template comentariu**:

```csharp
// Create the [Action][Entity]Workflow pure core, following the pattern from copilot-instructions.md
//
// Input: [Command]Command containing [description of command data]
//
// Dependencies (all as function parameters, not constructor fields):
// - [Type] [dep1]: [description]
// - DateTimeOffset now: the clock, injected, never DateTime.Now inside this function
//
// Pipeline steps (Map/Bind-chained, railway-oriented):
// 1. Create [Entity].Unvalidated from command input data
// 2. .Validate([dependencies]) -> Result<[Entity].Validated, IReadOnlyList<ValidationError>>
// 3. .MapError(errors => new [Action][Entity]Error.Validation(errors))
// 4. .Map(validated => validated.[BusinessOperation1]([dependencies]))
// 5. .Map(intermediate => intermediate.[FinalOperation](now))
//
// Return Result<[Entity][Action]Event, [Action][Entity]Error>
// (map the final state to the event with .Map(final => final.ToEvent()))
//
// NO business logic in this function — only composition. Never catch exceptions here.

public static class [Action][Entity]Workflow
{
    public static Result<[Entity][Action]Event, [Action][Entity]Error> Publish(/* command, dependencies, now */)
    {
        // Copilot va genera...
    }
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create the ScheduleExamWorkflow pure core, following the pattern from copilot-instructions.md
//
// Input: ScheduleExamCommand containing: courseCode, proposedDate1/2/3, duration, expectedStudents
//
// Dependencies (as function parameters):
// - IReadOnlySet<CourseCode> existingCourses, IReadOnlyDictionary<CourseCode, DateTimeOffset> courseEndDates
// - Func<ExamDate, Duration, Capacity, IReadOnlyList<RoomNumber>> findAvailableRooms
// - DateTimeOffset now
//
// Pipeline steps:
// 1. Create ExamScheduling.Unvalidated from command data
// 2. .Validate(existingCourses, courseEndDates) -> Result<ExamScheduling.Validated, IReadOnlyList<ValidationError>>
// 3. .MapError(errors => new ScheduleExamError.Validation(errors))
// 4. .Map(validated => validated.AllocateRoom(findAvailableRooms))
// 5. .Map(allocated => allocated.Publish(now))
// 6. .Map(published => published.ToEvent())
//
// Return Result<ExamScheduledEvent, ScheduleExamError>

public static class ScheduleExamWorkflow
{
    public static Result<ExamScheduledEvent, ScheduleExamError> Publish(/* ... */)
    {
        // Copilot va genera...
    }
}
```

---

## Partea 4: Testare și Demonstrație (30 minute)

### Sarcina 4.1: Crearea Aplicației Console

**Obiectiv**: Demonstrați workflow-ul complet cu date de test.

**Template comentariu pentru Program.cs**:

```csharp
// Create a top-level-statements console application demonstrating [WorkflowName]
//
// Steps:
// 1. Display application title and description
// 2. Create 3-5 sample commands with test data:
//    - At least 2 valid cases (should succeed)
//    - At least 1 invalid case (should fail with validation errors)
//    - At least 1 edge case
// 3. Create mock dependencies as hardcoded in-memory values (no EF Core, no ASP.NET at this stage):
//    - [Dependency 1]: a fixed IReadOnlySet<...>/IReadOnlyDictionary<...>
// 4. For each test case:
//    - Display input data
//    - Call the workflow's static Publish function
//    - result.Match(event => "success text", error => string.Join(Environment.NewLine, error.ToMessages()))
//    - Display success data OR the failure messages
//    - Separate each test with a visual divider
//
// Use clear console output with emojis/symbols for visual feedback

// Copilot va genera...
```

---

### Sarcina 4.2: Validarea cu AI

**După ce ați implementat tot codul**, folosiți AI pentru code review:

**Prompt pentru ChatGPT/Claude**:
```
Analizează codul meu DDD și verifică dacă respectă pattern-urile:

[ATTACH YOUR CODE FILES]

Verifică:
1. Value objects: constructor privat, `Create`/`Parse` care întorc `Result` (nu aruncă excepție), imutabilitate
2. Entity states: `abstract record` cu constructor privat, stări imbricate `sealed`, fără stare `Invalid`, `Match` exhaustiv
3. Operations: membri de extensie C# 14 pe starea exactă transformată, single responsibility, dependențe ca parametri de funcție
4. Workflow: doar compoziție (`Map`/`Bind`), fără business logic, fără try/catch
5. Events/Errors: eveniment doar pentru succes; eroare ca ierarhie închisă, nu ca eveniment de eșec
6. Naming conventions: comenzi/events/operations corect denumite

Pentru fiecare problemă găsită:
- Explică ce e greșit
- Arată codul corect
- Explică de ce pattern-ul DDD funcțional necesită această abordare
```

---

### Sarcina 4.3: Teste unitare (xunit.v3)

**Obiectiv**: Scrieți câteva teste care fixează comportamentul funcțiilor pure implementate mai sus — sunt directe de testat pentru că nu au dependențe ascunse (baze de date, timp de sistem).

**Template comentariu**:

```csharp
// Create xunit.v3 tests for [Entity]Validation.Validate, following the pattern from copilot-instructions.md
//
// Test cases:
// 1. All fields valid, student/course known -> Ok, correct number of items
// 2. Every field invalid at once -> Error with one ValidationError per invalid field, in order (Result.Combine keeps them all)
// 3. A boundary case specific to your domain (e.g. exactly on a threshold)
//
// Use plain Assert (no FluentAssertions). Name tests Method_Scenario_ExpectedResult.

public sealed class [Entity]ValidationTests
{
    // Copilot va genera...
}
```

---

## Partea 5: Prezentare și Discuții (30 minute)

### Sarcina 5.1: Pregătirea Prezentării

Fiecare echipă prezintă (10 minute):

**Structură prezentare**:
1. **Domeniul și Event Storming** (2 min)
   - Ce domeniu ați ales
   - Bounded contexts identificate  
   - Diagrama cu evenimente

2. **Implementarea** (5 min)
   - Value objects create (3-5)
   - States și flow-ul entității
   - Operații implementate (validare + 2-3 business)
   - Workflow-ul complet

3. **Demo live** (2 min)
   - Rulare aplicație console
   - Caz valid (success)
   - Caz invalid (failure cu erori)

4. **AI Usage** (1 min)
   - Ce prompturi au funcționat bine
   - Ce a trebuit corectat manual
   - O lecție învățată

---

### Sarcina 5.2: Reflecție și Discuție

**Discutați în echipă și prezentați**:

1. **Când a ajutat AI?**
   - Generare boilerplate
   - Sugestii de validare
   - Pattern matching complet

2. **Când a fost nevoie de intervenție umană?**
   - Business rules complexe
   - Dependencies injection
   - Edge cases

3. **Ce ați învățat despre DDD?**
   - Importanța immutabilității
   - Separarea responsabilităților
   - Type safety și compiler-enforced correctness

## Evaluare

| Criteriu | Punctaj | Descriere |
|----------|---------|-----------|
| **Event Storming** | 15% | Calitatea identificării evenimentelor și bounded contexts |
| **Obiecte-valoare** | 20% | `Create`/`Parse` care întorc `Result`, imutabilitate, fără excepții pentru date de intrare nevalide |
| **Stări ale entității** | 20% | Tip-sumă închis, tranziții logice, fără stare `Invalid` |
| **Operații** | 20% | Funcții pure, o singură responsabilitate, dependențe ca parametri |
| **Workflow** | 15% | Compunere corectă cu `Map`/`Bind`, fără business logic în workflow |
| **Utilizare AI** | 10% | Prompturi eficiente, validare critică a rezultatelor |

## Concluzii

Acest laborator v-a introdus în:
- **Domain-Driven Design** cu pattern-uri practice și reutilizabile
- **AI-Assisted Development** cu focus pe prompt engineering
- **Functional Programming** prin immutabilitate și funcții pure
- **Type-Driven Development** unde compilatorul previne erori

**Recomandări finale**:
- Folosiți AI ca asistent, nu ca înlocuitor
- Validați întotdeauna sugestiile AI
- Construiți o bibliotecă de prompturi eficiente
- Documentați deciziile de design

## Continuare opțională (după Lucrările 5-8)

Domeniul construit aici e voit izolat (consolă, dependențe simulate în memorie). Dacă echipa vrea să-l ducă mai departe:
- **Persistență** — adăugați repository-uri reale peste EF Core, ca în [Lucrarea 5](../Lucrarea-05/README.md)
- **API** — expuneți workflow-ul printr-un API minimal, ca în [Lucrarea 6](../Lucrarea-06/README.md)
- **Comunicare între contexte** — publicați evenimentul de succes către alt context, ca în [Lucrarea 8](../Lucrarea-08/README.md)
- Explorați Event Sourcing pentru audit
- Încercați CQRS pentru separarea citire/scriere

