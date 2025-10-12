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

**Durată**: 2 ore

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

**Studiați implementarea workflow-ului de publicare a notelor** pentru a identifica următoarele pattern-uri:

#### **Pattern 1: Value Objects Imutabile**

```csharp
// Exemplu: StudentRegistrationNumber
public record StudentRegistrationNumber
{
    private static readonly Regex ValidPattern = new("^LM[0-9]{5}$");
    public string Value { get; }
    
    private StudentRegistrationNumber(string value) { /* validare */ }
    public static bool TryParse(string stringValue, out StudentRegistrationNumber? result) { /* ... */ }
    public override string ToString() => Value;
}
```

**Pattern observat**:
- Constructor privat pentru control total
- Validare în constructor
- Metodă statică `TryParse` pentru conversie sigură
- Imutabilitate (doar `get`)
- Override `ToString()` pentru serializare

#### **Pattern 2: Entități de Stare (State Pattern cu Records)**

```csharp
// Exemplu: Exam states
public interface IExam { }

public record UnvalidatedExam(IReadOnlyCollection<UnvalidatedStudentGrade> GradeList) : IExam;
public record ValidatedExam(IReadOnlyCollection<ValidatedStudentGrade> GradeList) : IExam;
public record CalculatedExam(IReadOnlyCollection<CalculatedStudentGrade> GradeList) : IExam;
public record PublishedExam(IReadOnlyCollection<CalculatedStudentGrade> GradeList, 
                            string Csv, 
                            DateTime PublishedDate) : IExam;
public record InvalidExam(IReadOnlyCollection<UnvalidatedStudentGrade> GradeList, 
                          IEnumerable<string> Reasons) : IExam;
```

**Pattern observat**:
- Interfață goală pentru tipul de bază
- Fiecare stare = record separat
- Constructor `internal` pentru controlul instanțierii
- Imutabilitate completă

#### **Pattern 3: Operații de Domeniu (Domain Operations)**

```csharp
internal abstract class ExamOperation : ExamOperation<object>
{
    internal IExam Transform(IExam exam) => exam switch
    {
        UnvalidatedExam unvalidatedExam => OnUnvalidated(unvalidatedExam),
        ValidatedExam validExam => OnValid(validExam),
        CalculatedExam calculatedExam => OnCalculated(calculatedExam),
        InvalidExam invalidExam => OnInvalid(invalidExam),
        PublishedExam publishedExam => OnPublished(publishedExam),
        _ => throw new InvalidExamStateException(exam.GetType().Name)
    };

    protected virtual IExam OnUnvalidated(UnvalidatedExam exam) => exam;
    protected virtual IExam OnValid(ValidatedExam exam) => exam;
    // ... alte metode virtuale
}
```

**Pattern observat**:
- Pattern matching pentru procesarea diferitelor stări
- Metode virtuale pentru extensibilitate
- Fiecare operație = clasă separată care extinde `ExamOperation`
- Default behavior: returnează același obiect (identity)

#### **Pattern 4: Workflow = Compoziția Operațiilor**

```csharp
public IExamPublishedEvent Execute(PublishExamCommand command, 
                                   Func<StudentRegistrationNumber, bool> checkStudentExists)
{
    UnvalidatedExam unvalidatedGrades = new(command.InputExamGrades);
    
    IExam exam = new ValidateExamOperation(checkStudentExists).Transform(unvalidatedGrades);
    exam = new CalculateExamOperation().Transform(exam);
    exam = new PublishExamOperation().Transform(exam);
    
    return exam.ToEvent();
}
```

**Pattern observat**:
- Workflow = pipeline de transformări
- Fiecare operație transformă starea entității
- Dependency injection prin constructor
- Rezultatul final = conversie la eveniment

### Sarcina 2.2: Configurarea GitHub Copilot pentru Pattern-uri DDD

**Copiați fișierul `copilot-instructions.md`** în rădăcina proiectului.

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
   - Are metodă `TryParse`
   - Properties sunt `{ get; }` only
   - Are validare în constructor

4. **Testați manual** în Program.cs:
```csharp
// Test valid
if (CourseCode.TryParse("PSSC", out var code))
    Console.WriteLine($"Valid: {code}");

// Test invalid
if (!CourseCode.TryParse("invalid", out var invalid))
    Console.WriteLine("Correctly rejected invalid input");
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
// State flow:
// Unvalidated[Entity] → Validated[Entity] → [IntermediateState] → [FinalState]
//                    ↘ Invalid[Entity]
//
// States needed:
// 1. Unvalidated[Entity]: Raw input with string properties: [list properties]
// 2. Validated[Entity]: After validation with value objects: [list typed properties]
// 3. [IntermediateState]: [description and properties]
// 4. [FinalState]: [description and properties]
// 5. Invalid[Entity]: When validation/processing fails, contains Reasons
//
// Each state implements I[Entity] interface
// Use internal constructors and IReadOnlyCollection for lists

public static class [EntityName]
{
    // Copilot va genera interface-ul și toate stările...
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create entity states for ExamScheduling following the pattern from copilot-instructions.md
// 
// State flow:
// UnvalidatedExamScheduling → ValidatedExamScheduling → RoomAllocatedExamScheduling → PublishedExamScheduling
//                          ↘ InvalidExamScheduling
//
// States needed:
// 1. UnvalidatedExamScheduling: Raw input with properties: courseCode (string), proposedDate1/2/3 (string), duration (string), expectedStudents (string)
// 2. ValidatedExamScheduling: After validation with: courseCode (CourseCode), proposedDates (IReadOnlyList<ExamDate>), duration (Duration), expectedStudents (Capacity)
// 3. RoomAllocatedExamScheduling: After room allocation with: courseCode, selectedDate (ExamDate), duration, room (RoomNumber), roomCapacity (Capacity)
// 4. PublishedExamScheduling: After publishing with all above plus: publishedAt (DateTime), enrolledStudents (Capacity)
// 5. InvalidExamScheduling: When validation fails with: courseCode (string), reasons (IEnumerable<string>)
//
// Each state implements IExamScheduling interface
// Use internal constructors and IReadOnlyCollection for lists

public static class ExamScheduling
{
    // Copilot va genera...
}
```

---

## Partea 3: Implementarea Operațiilor și Workflow-ului (60 minute)

### Sarcina 3.1: Implementarea Operației de Validare

**Obiectiv**: Creați operația care transformă starea `Unvalidated` în `Validated` sau `Invalid`.

**Pași**:

1. **Identificați dependențele externe** necesare (verificări în baze de date, servicii externe)
2. **Scrieți comentariul pentru Copilot**:

```csharp
// Create Validate[Entity]Operation following the pattern from copilot-instructions.md
//
// This operation transforms Unvalidated[Entity] to either Validated[Entity] or Invalid[Entity]
//
// Dependencies (inject via constructor):
// - Func<[ValueObject], bool> [checkSomething]: [description of what it checks]
// - Func<[ValueObject], [ResultType]> [getSomething]: [description]
//
// Validation steps:
// 1. Parse each string field to its value object using TryParse
// 2. If parsing fails, add error to list: "Invalid [field] ([value])"
// 3. For fields requiring external validation, call dependency function
// 4. If external check fails, add error: "[Entity] not found/invalid ([value])"
// 5. If any errors, return Invalid[Entity] with all reasons
// 6. If no errors, return Validated[Entity] with all parsed value objects
//
// Override OnUnvalidated method only

internal sealed class Validate[Entity]Operation : [Entity]Operation
{
    // Copilot va genera...
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create ValidateExamSchedulingOperation following the pattern from copilot-instructions.md
//
// This operation transforms UnvalidatedExamScheduling to either ValidatedExamScheduling or InvalidExamScheduling
//
// Dependencies (inject via constructor):
// - Func<CourseCode, bool> checkCourseExists: Verifies course exists in catalog
// - Func<CourseCode, DateTime> getCourseEndDate: Gets when course ends for date validation
//
// Validation steps:
// 1. Parse courseCode string to CourseCode using TryParse
// 2. If parsing fails, add error: "Invalid course code ([value])"
// 3. If parsed, check if course exists using checkCourseExists
// 4. If not exists, add error: "Course not found ([code])"
// 5. Parse all three proposed dates to ExamDate
// 6. For each date, verify it's at least 7 days after course end date
// 7. Parse duration string to Duration value object
// 8. Parse expectedStudents to Capacity value object
// 9. If any errors collected, return InvalidExamScheduling with reasons
// 10. If no errors, return ValidatedExamScheduling with all value objects
//
// Override OnUnvalidated method only

internal sealed class ValidateExamSchedulingOperation : ExamSchedulingOperation
{
    // Copilot va genera...
}
```

---

### Sarcina 3.2: Implementarea Operațiilor de Business Logic

**Obiectiv**: Creați 2-3 operații care implementează regulile de business și transformă entitatea prin stările intermediare.

**Template comentariu**:

```csharp
// Create [OperationName] following the pattern from copilot-instructions.md
//
// This operation transforms [SourceState] to [TargetState]
//
// Dependencies (if any):
// - Func<[Input], [Output]> [dependencyName]: [description]
//
// Business logic:
// - [BUSINESS RULE 1]
// - [BUSINESS RULE 2]
// - [CALCULATION/TRANSFORMATION if applicable]
//
// Override On[SourceState] method
// Use LINQ Select to transform each item if working with collections
// Return new [TargetState] with transformed data

internal sealed class [OperationName] : [Entity]Operation
{
    // Copilot va genera...
}
```

**Exemplu pentru Alocare Cămin - Calculare Punctaj**:

```csharp
// Create CalculateScoreOperation following the pattern from copilot-instructions.md
//
// This operation transforms ValidatedApplication to ScoredApplication
//
// Dependencies:
// - decimal incomeThreshold (passed via constructor): Threshold for income points calculation
//
// Business logic:
// - Calculate grade points: (averageGrade - 5.00) * 8, max 40 points
// - Calculate distance points: (distance / 10) * 3, max 30 points
// - Calculate income points: if income < threshold then 20 points, else 0
// - Calculate special situation points: if hasSpecialSituation then 10 points, else 0
// - Total score = sum of all points (max 100)
//
// Override OnValidated method
// For each ValidatedApplication, calculate total score and create ScoredApplication
// Return new ScoredApplication with student info and calculated AllocationScore

internal sealed class CalculateScoreOperation : DormitoryApplicationOperation
{
    // Copilot va genera...
}
```

---

### Sarcina 3.3: Implementarea Evenimentelor

**Obiectiv**: Definiți evenimentele de succes și eșec pentru workflow.

**Template comentariu**:

```csharp
// Create [Entity][Action]Event following the pattern from copilot-instructions.md
//
// Define interface I[Entity][Action]Event as base type
//
// Success event: [Entity][Action]SucceededEvent
// Properties:
// - [Property1]: [Type] - [description]
// - [Property2]: [Type] - [description]
// - Timestamp: DateTime
//
// Failure event: [Entity][Action]FailedEvent
// Properties:
// - Reasons: IEnumerable<string> - List of all errors
//
// Extension method ToEvent(this I[Entity] entity):
// - Pattern match on all entity states
// - Unvalidated/Intermediate states → FailedEvent with "Unexpected state" message
// - Invalid state → FailedEvent with reasons from Invalid state
// - Final success state → SucceededEvent with relevant data
// - Default case → FailedEvent with "Unknown state" message

public static class [Entity][Action]Event
{
    // Copilot va genera...
}
```

---

### Sarcina 3.4: Implementarea Workflow-ului

**Obiectiv**: Compuneți toate operațiile într-un workflow complet.

**Template comentariu**:

```csharp
// Create [Action][Entity]Workflow following the pattern from copilot-instructions.md
//
// Input: [Command]Command containing [description of command data]
//
// Dependencies (all as Execute method parameters):
// - Func<[Type], [ReturnType]> [dep1]: [description]
// - Func<[Type], [ReturnType]> [dep2]: [description]
// - [other dependencies]
//
// Pipeline steps:
// 1. Create Unvalidated[Entity] from command input data
// 2. Transform using Validate[Entity]Operation (pass [dependencies])
// 3. Transform using [BusinessOperation1] (pass [dependencies] if needed)
// 4. Transform using [BusinessOperation2] (pass [dependencies] if needed)
// 5. Transform using [FinalOperation]
// 6. Convert final I[Entity] to I[Entity][Action]Event using ToEvent()
//
// Return I[Entity][Action]Event (success or failure)
//
// NO business logic in workflow, only composition of operations

public class [Action][Entity]Workflow
{
    // Copilot va genera...
}
```

**Exemplu pentru Domeniul Examene**:

```csharp
// Create ScheduleExamWorkflow following the pattern from copilot-instructions.md
//
// Input: ScheduleExamCommand containing: courseCode, proposedDate1/2/3, duration, expectedStudents
//
// Dependencies (all as Execute method parameters):
// - Func<CourseCode, bool> checkCourseExists: Verify course is in catalog
// - Func<CourseCode, DateTime> getCourseEndDate: Get course end date
// - Func<ExamDate, Duration, Capacity, IEnumerable<RoomNumber>> findAvailableRooms: Find rooms for exam
// - Func<RoomNumber, ExamDate, Duration, bool> reserveRoom: Reserve selected room
//
// Pipeline steps:
// 1. Create UnvalidatedExamScheduling from command data
// 2. Transform using ValidateExamSchedulingOperation (pass checkCourseExists, getCourseEndDate)
// 3. Transform using AllocateRoomOperation (pass findAvailableRooms, reserveRoom)
// 4. Transform using PublishExamOperation (no dependencies)
// 5. Convert final IExamScheduling to IExamScheduledEvent using ToEvent()
//
// Return IExamScheduledEvent (success or failure)

public class ScheduleExamWorkflow
{
    // Copilot va genera...
}
```

---

## Partea 4: Testare și Demonstrație (30 minute)

### Sarcina 4.1: Crearea Aplicației Console

**Obiectiv**: Demonstrați workflow-ul complet cu date de test.

**Template comentariu pentru Program.cs**:

```csharp
// Create console application demonstrating [WorkflowName]
//
// Steps:
// 1. Display application title and description
// 2. Create 3-5 sample commands with test data:
//    - At least 2 valid cases (should succeed)
//    - At least 1 invalid case (should fail with validation errors)
//    - At least 1 edge case
// 3. Create mock dependencies that return hardcoded values:
//    - [Dependency 1]: returns true/false based on [condition]
//    - [Dependency 2]: returns [type] based on [input]
// 4. For each test case:
//    - Display input data
//    - Execute workflow
//    - Pattern match on result event type
//    - Display success data OR failure reasons
//    - Separate each test with visual divider
// 5. Wait for user input before exit
//
// Use clear console output with emojis/symbols for visual feedback

class Program
{
    static void Main(string[] args)
    {
        // Copilot va genera...
    }
}
```

---

### Sarcina 4.2: Validarea cu AI

**După ce ați implementat tot codul**, folosiți AI pentru code review:

**Prompt pentru ChatGPT/Claude**:
```
Analizează codul meu DDD și verifică dacă respectă pattern-urile:

[ATTACH YOUR CODE FILES]

Verifică:
1. Value objects: constructor privat, TryParse, immutabilitate, validare
2. Entity states: interface, records, internal constructors, IReadOnlyCollection
3. Operations: pattern matching complet, single responsibility, dependencies injectate
4. Workflow: doar compoziție, fără business logic
5. Events: success/failure, conversie cu pattern matching
6. Naming conventions: comenzi/events/operations corect denumite

Pentru fiecare problemă găsită:
- Explică ce e greșit
- Arată codul corect
- Explică de ce pattern-ul DDD necesită această abordare
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

**Next steps după laborator**:
- Extindeți cu bounded contexts suplimentare
- Implementați comunicarea între contexte
- Explorați Event Sourcing pentru audit
- Încercați CQRS pentru separare read/write

