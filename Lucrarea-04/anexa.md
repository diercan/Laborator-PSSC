> **Notă**: unele exemple de cod din această anexă folosesc încă stilul anterior (constructor `private` + `TryParse` care aruncă/întoarce `bool`, stare `Invalid[Entity]`, clase `Operation` cu moștenire). Stilul curent al laboratorului — cel pe care trebuie să-l urmați — este descris în [`copilot-instructions.md`](copilot-instructions.md) și implementat în [Lucrarea 3](../Lucrarea-03/Exemple/Examples.Domain/): obiecte-valoare cu `Create`/`Parse` care întorc `Result`, stări fără `Invalid`, operații ca membri de extensie. Unde exemplele de mai jos diferă, adaptați-le la stilul curent.

## ANEXA A: Exemple Complete de Prompturi pentru Fiecare Domeniu

### A.1: Domeniul Gestionare Examene

#### Value Objects

**CourseCode**
```csharp
// Create value object for CourseCode representing unique course identifier
// Validation rules:
// - Format: 2-4 uppercase letters optionally followed by single digit
// - Examples: "PSSC", "BD", "POO2", "MATH1"
// - Must not be empty or whitespace
// Valid examples: "PSSC", "BD", "MATH1"
// Invalid examples: "pssc" (lowercase), "P" (too short), "ABCDEFG" (too long)
// Follow the pattern from copilot-instructions.md
```

**ExamDate**
```csharp
// Create value object for ExamDate representing valid exam session date
// Validation rules:
// - Must be future date
// - Must be within exam sessions: June 1-July 15 OR January 15-February 28
// - Cannot be Saturday or Sunday
// - Format: DateTime (date only, no time)
// Valid examples: "2025-06-15" (Monday in June), "2025-01-20" (Monday in Jan)
// Invalid examples: "2024-12-15" (not in session), "2025-06-14" (Saturday)
// Follow the pattern from copilot-instructions.md
// Include method: IsInSameWeek(ExamDate other) returns bool
```

**RoomNumber**
```csharp
// Create value object for RoomNumber representing university room
// Validation rules:
// - Format: BuildingLetter + Floor + RoomNumber (e.g., "A301", "C205")
// - Building: single letter A-D
// - Floor: digit 0-4
// - Room: two digits 01-99
// Valid examples: "A301", "B205", "C101", "D499"
// Invalid examples: "Z301" (invalid building), "A5" (incomplete), "A801" (invalid floor)
// Follow the pattern from copilot-instructions.md
```

#### Entity States

```csharp
// Create entity states for ExamScheduling following the pattern from copilot-instructions.md
// 
// State flow:
// UnvalidatedExamScheduling → ValidatedExamScheduling → RoomAllocatedExamScheduling → PublishedExamScheduling → ClosedExamScheduling
//                          ↘ InvalidExamScheduling
//
// States needed:
// 1. UnvalidatedExamScheduling: Raw input
//    Properties: courseCode (string), proposedDate1 (string), proposedDate2 (string), proposedDate3 (string), duration (string), expectedStudents (string)
//
// 2. ValidatedExamScheduling: After validation
//    Properties: courseCode (CourseCode), proposedDates (IReadOnlyList<ExamDate>), duration (Duration), expectedStudents (Capacity)
//
// 3. RoomAllocatedExamScheduling: After room allocation
//    Properties: courseCode (CourseCode), selectedDate (ExamDate), duration (Duration), room (RoomNumber), roomCapacity (Capacity)
//
// 4. PublishedExamScheduling: After publishing to students
//    Properties: courseCode, selectedDate, duration, room, roomCapacity, enrolledStudents (Capacity), publishedAt (DateTime)
//
// 5. ClosedExamScheduling: After exam completed
//    Properties: courseCode, selectedDate, room, enrolledStudents, attendedStudents (Capacity), closedAt (DateTime)
//
// 6. InvalidExamScheduling: Validation/processing failed
//    Properties: courseCode (string), reasons (IEnumerable<string>)
//
// All states implement IExamScheduling interface
// Use internal constructors

public static class ExamScheduling
{
    // Copilot will generate...
}
```

#### Workflow

```csharp
// Create ScheduleExamWorkflow following the pattern from copilot-instructions.md
//
// Input: ScheduleExamCommand with properties: courseCode, proposedDate1/2/3, duration, expectedStudents (all strings)
//
// Dependencies (Execute method parameters):
// - Func<CourseCode, bool> checkCourseExists: Returns true if course exists in catalog
// - Func<CourseCode, DateTime> getCourseEndDate: Returns date when course ends
// - Func<ExamDate, Duration, Capacity, IEnumerable<RoomNumber>> findAvailableRooms: Returns available rooms for given date/duration/capacity
// - Func<RoomNumber, ExamDate, Duration, bool> reserveRoom: Reserves room, returns true if successful
//
// Pipeline:
// 1. Create UnvalidatedExamScheduling from command
// 2. Apply ValidateExamSchedulingOperation with checkCourseExists and getCourseEndDate
// 3. Apply AllocateRoomOperation with findAvailableRooms and reserveRoom
// 4. Apply PublishExamOperation (no dependencies)
// 5. Convert result to IExamScheduledEvent using ToEvent()
//
// Return IExamScheduledEvent

public class ScheduleExamWorkflow
{
    // Copilot will generate...
}
```

---

### A.2: Domeniul Alocare Cămin

#### Value Objects

**StudentId (CNP)**
```csharp
// Create value object for StudentId representing Romanian CNP (personal numeric code)
// Validation rules:
// - Exactly 13 digits
// - First digit: 1-8 (gender and century)
// - Digits 2-7: valid birth date YYMMDD
// - Digits 8-9: county code 01-52
// - Last digit: checksum using modulo 11 algorithm
// Valid examples: "1990512345678", "2950312123456"
// Invalid examples: "123" (too short), "9999999999999" (invalid format)
// Follow the pattern from copilot-instructions.md
```

**AverageGrade**
```csharp
// Create value object for AverageGrade representing student's grade average
// Validation rules:
// - Range: 5.00 to 10.00 (Romanian grading scale)
// - Precision: 2 decimal places
// - Type: decimal
// Valid examples: 9.50, 7.25, 10.00, 5.00
// Invalid examples: 4.99 (too low), 10.01 (too high), 8.567 (too many decimals)
// Follow the pattern from copilot-instructions.md
// Include method: ToScorePoints() that returns int (converts to 0-40 points scale)
```

**AllocationScore**
```csharp
// Create value object for AllocationScore representing total dormitory allocation score
// Validation rules:
// - Range: 0 to 100 points
// - Precision: 2 decimal places
// - Type: decimal
// Valid examples: 85.50, 42.75, 100.00, 0.00
// Invalid examples: -5.0 (negative), 101.0 (exceeds max)
// Follow the pattern from copilot-instructions.md
// Include method: CompareTo(AllocationScore other) for sorting students
```

#### Entity States

```csharp
// Create entity states for DormitoryApplication following the pattern from copilot-instructions.md
//
// State flow:
// UnvalidatedApplication → ValidatedApplication → ScoredApplication → AllocatedApplication → ConfirmedApplication
//                       ↘ InvalidApplication                       ↘ UnallocatedApplication ↘ RejectedApplication
//
// States:
// 1. UnvalidatedApplication: Raw input
//    Properties: studentId (string), name (string), averageGrade (string), distanceFromHome (string), 
//                monthlyIncome (string), hasSpecialSituation (string), preferredBuildings (string)
//
// 2. ValidatedApplication: After validation
//    Properties: studentId (StudentId), name (StudentName), averageGrade (AverageGrade), 
//                distance (Distance), monthlyIncome (MonthlyIncome), hasSpecialSituation (bool), 
//                preferredBuildings (IReadOnlyList<BuildingCode>)
//
// 3. ScoredApplication: After score calculation
//    Properties: studentId, name, score (AllocationScore), averageGrade, distance, monthlyIncome, 
//                hasSpecialSituation, preferredBuildings
//
// 4. AllocatedApplication: After room allocation
//    Properties: studentId, name, score, allocatedRoom (RoomId), building (BuildingCode), 
//                allocatedAt (DateTime), deadlineToConfirm (DateTime)
//
// 5. ConfirmedApplication: After student confirms
//    Properties: studentId, allocatedRoom, confirmedAt (DateTime), moveInDate (DateTime)
//
// 6. UnallocatedApplication: No room available
//    Properties: studentId, name, score, reason (string - why no room found)
//
// 7. RejectedApplication: Student rejected or missed deadline
//    Properties: studentId, rejectedAt (DateTime), reason (string)
//
// 8. InvalidApplication: Validation failed
//    Properties: studentId (string), reasons (IEnumerable<string>)

public static class DormitoryApplication
{
    // Copilot will generate...
}
```

#### Operations

**CalculateScoreOperation**
```csharp
// Create CalculateScoreOperation following the pattern from copilot-instructions.md
//
// Transform ValidatedApplication to ScoredApplication
//
// Dependencies:
// - decimal incomeThreshold (constructor parameter): Threshold for income-based points
//
// Score calculation formula:
// - Grade points: (averageGrade.Value - 5.00) * 8, maximum 40 points
// - Distance points: min((distance.Value / 10) * 3, 30), maximum 30 points
// - Income points: if monthlyIncome.Value < incomeThreshold then 20 else 0
// - Special situation points: if hasSpecialSituation then 10 else 0
// - Total = sum of all (maximum 100 points)
//
// For each ValidatedApplication:
// 1. Calculate all point components
// 2. Sum to get total score
// 3. Create AllocationScore value object
// 4. Return new ScoredApplication with calculated score
//
// Override OnValidated method

internal sealed class CalculateScoreOperation : DormitoryApplicationOperation
{
    // Copilot will generate...
}
```

---

### A.3: Domeniul Rezervare Spații Studiu

#### Value Objects

**SpaceId**
```csharp
// Create value object for SpaceId representing study space identifier
// Validation rules:
// - Format: Building-Floor-Type-Number (e.g., "LIB-2-IND-05", "DORM-3-GRP-12")
// - Building: "LIB" (library) or "DORM" (dormitory)
// - Floor: digit 0-5
// - Type: "IND" (individual), "GRP" (group), "LAB" (lab)
// - Number: two digits 01-99
// Valid examples: "LIB-2-IND-05", "DORM-3-GRP-12", "LIB-1-LAB-01"
// Invalid examples: "LIB-2-05" (missing type), "MALL-1-IND-01" (invalid building)
// Follow the pattern from copilot-instructions.md
```

**TimeSlot**
```csharp
// Create value object for TimeSlot representing reservation time period
// Validation rules:
// - StartTime and EndTime both DateTime
// - StartTime must be in future
// - Duration: minimum 30 minutes, maximum 4 hours
// - Operating hours: 08:00 to 22:00
// - StartTime must be on hour or half-hour (e.g., 10:00, 10:30, not 10:15)
// Valid examples: start=2025-06-15 10:00, end=2025-06-15 12:00
// Invalid examples: start=2025-06-15 23:00 (outside hours), duration=5 hours (too long)
// Follow the pattern from copilot-instructions.md
// Include methods: GetDuration() returns TimeSpan, OverlapsWith(TimeSlot other) returns bool
```

#### Entity States

```csharp
// Create entity states for SpaceReservation following the pattern from copilot-instructions.md
//
// State flow:
// UnvalidatedReservation → ValidatedReservation → ConfirmedReservation → ActiveReservation → CompletedReservation
//                       ↘ InvalidReservation    ↘ PendingApprovalReservation              ↘ NoShowReservation
//                                                                                           ↘ CancelledReservation
//
// States:
// 1. UnvalidatedReservation: Raw input
//    Properties: studentId (string), spaceId (string), startTime (string), endTime (string), purpose (string)
//
// 2. ValidatedReservation: After validation
//    Properties: studentId (StudentId), spaceId (SpaceId), timeSlot (TimeSlot), purpose (ReservationPurpose)
//
// 3. PendingApprovalReservation: Needs manual approval (long duration)
//    Properties: studentId, spaceId, timeSlot, purpose, requestedAt (DateTime)
//
// 4. ConfirmedReservation: Approved and confirmed
//    Properties: studentId, spaceId, timeSlot, purpose, confirmedAt (DateTime), reminderSent (bool)
//
// 5. ActiveReservation: Student checked in
//    Properties: studentId, spaceId, timeSlot, checkedInAt (DateTime)
//
// 6. CompletedReservation: Reservation completed normally
//    Properties: studentId, spaceId, timeSlot, checkedInAt, checkedOutAt (DateTime)
//
// 7. NoShowReservation: Student didn't show up
//    Properties: studentId, spaceId, timeSlot, markedNoShowAt (DateTime)
//
// 8. CancelledReservation: Cancelled by student or system
//    Properties: studentId, spaceId, timeSlot, cancelledAt (DateTime), reason (string)
//
// 9. InvalidReservation: Validation failed
//    Properties: studentId (string), reasons (IEnumerable<string>)

public static class SpaceReservation
{
    // Copilot will generate...
}
```

---

## ANEXA B: Greșeli Comune și Soluții

Vedeți [ANEXA P: Common Mistakes and Solutions](#anexa-p-common-mistakes-and-solutions) pentru lista completă de greșeli frecvente cu exemple înainte/după (aceleași categorii de greșeli erau descrise aici, pe scurt și în română; ANEXA P le acoperă mai detaliat).

---

## ANEXA C: Checklist Final

### Înainte de Predare

**Obiecte-valoare:**
- [ ] Toate au constructor `private`
- [ ] Toate au metodă statică `Create`/`Parse` care întoarce `Result<T, TError>` — nicio metodă nu aruncă excepție pentru date de intrare nevalide
- [ ] Toate proprietățile sunt `{ get; }` only
- [ ] `ToString()` override implementat

**Stările entității:**
- [ ] `abstract record` cu constructor `private` (tip-sumă închis, nu interfață)
- [ ] Toate stările sunt înregistrări imbricate `sealed`
- [ ] Collections sunt `IReadOnlyList<T>`/`IReadOnlySet<T>`
- [ ] **Nu** există o stare `Invalid[Entity]` — eșecurile sunt valori `Result.Error`
- [ ] Există un fold `Match<TResult>` pentru potrivire exhaustivă

**Operații:**
- [ ] Scrise ca membri de extensie C# 14 pe starea exactă pe care o transformă
- [ ] Fără clasă de bază `DomainOperation`/metode virtuale `OnXxx`
- [ ] Dependențele sunt parametri de funcție, nu câmpuri injectate prin constructor
- [ ] Validarea adună toate erorile (`Result.Combine`/`Traverse`), nu se oprește la prima
- [ ] Fiecare operație = o singură responsabilitate

**Workflow:**
- [ ] Nucleul pur e o funcție statică (`Map`/`Bind`-compusă), fără business logic în afara ei
- [ ] Primește dependențele (inclusiv ceasul) ca parametri
- [ ] Creează entitatea `Unvalidated` la început
- [ ] Se încheie cu conversia stării finale la eveniment (`ToEvent()`)
- [ ] Nu conține `try`/`catch`

**Aplicație Console:**
- [ ] Compilează fără erori sau avertismente
- [ ] Are cel puțin 3 cazuri de test
- [ ] Afișează clar rezultatele (`result.Match(...)`)
- [ ] Dependențele simulate sunt valori fixe în memorie (fără EF Core, fără ASP.NET)

**Documentație:**
- [ ] README.md completat
- [ ] Event Storming diagram inclus
- [ ] Bounded contexts documentate
- [ ] Prompturi AI salvate

---

## ANEXA A: Pattern-uri Avansate pentru GitHub Copilot

### A.1: Generarea Automată de Excepții de Domeniu

**Context**: Fiecare value object și operație necesită excepții specifice domeniului.

**Prompt template**:

```csharp
// Generate domain exception for [EntityName]
// Following the pattern: Invalid[EntityName]Exception
// Should inherit from DomainException or Exception
// Constructor should accept descriptive message
// Include any relevant context properties
```

**Exemplu generat**:

```csharp
public class InvalidExamDateException : DomainException
{
    public InvalidExamDateException(string message) : base(message) { }
    
    public InvalidExamDateException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### A.2: Builders pentru Date de Test

**Prompt pentru generare**:

```csharp
// Create test data builder for [Entity] following Builder pattern
// Methods:
// - With[Property]([Type] value) - set property
// - WithValid[Property]() - set valid default
// - WithInvalid[Property]() - set invalid value for testing
// - Build() - return Unvalidated[Entity]
// 
// Include at least 3 preset configurations:
// - ValidDefault() - all valid data
// - InvalidAll() - all invalid data  
// - Partial() - some fields valid, some invalid
```

**Exemplu**:

```csharp
public class ExamSchedulingBuilder
{
    private string _courseCode = "PSSC";
    private string _proposedDate = "2025-06-15";
    private string _duration = "120";
    
    public ExamSchedulingBuilder WithCourseCode(string code)
    {
        _courseCode = code;
        return this;
    }
    
    public ExamSchedulingBuilder WithInvalidCourseCode()
    {
        _courseCode = "INVALID CODE!";
        return this;
    }
    
    public static ExamSchedulingBuilder ValidDefault() => new ExamSchedulingBuilder();
    
    public UnvalidatedExamScheduling Build() => new(_courseCode, _proposedDate, _duration);
}
```

### A.3: Logging și Observability

**Prompt**:

```csharp
// Add structured logging to [OperationName] using Microsoft.Extensions.Logging
// Log points:
// - Entry: log input state type
// - Before validation: log item count
// - After validation: log success/failure counts
// - Exit: log result state type
// 
// Use appropriate log levels:
// - Information: normal flow
// - Warning: validation failures
// - Error: unexpected exceptions
//
// Include relevant properties in structured logs
```

### A.4: Metrici și Performance

**Prompt pentru instrumentare**:

```csharp
// Add performance metrics to [WorkflowName] using System.Diagnostics
// Measure:
// - Total workflow execution time
// - Time per operation
// - Number of items processed
// - Success/failure rates
//
// Create custom ActivitySource named "[Domain].Workflows"
// Use Activity.StartActivity for each operation
// Add tags for relevant context (entity count, state transitions)
```

---

## ANEXA B: Scenarii Complete pentru Fiecare Domeniu

### B.1: Sistem de Gestionare a Sesiunii de Examene - Detalii Complete

#### Value Objects Necesare

1. **CourseCode**
   ```csharp
   // Create value object for CourseCode
   // Rules:
   // - Format: 2-4 uppercase letters followed by optional digit
   // - Examples: "PSSC", "BD", "POO2"
   // - Must exist in course catalog (check via dependency)
   ```

2. **ExamDate**
   ```csharp
   // Create value object for ExamDate  
   // Rules:
   // - Must be in future
   // - Must be in exam session (June 1-July 15 or Jan 15-Feb 28)
   // - Cannot be weekend
   // - Must be at least 7 days after course end
   // Include method: IsInSameWeek(ExamDate other)
   ```

3. **RoomNumber**
   ```csharp
   // Create value object for RoomNumber
   // Rules:
   // - Format: Building letter + floor + room (e.g., "A301", "C201")
   // - Building must be valid (A, B, C, D)
   // - Floor must be 0-4
   // - Room number must be 01-99
   ```

4. **Duration**
   ```csharp
   // Create value object for Duration (exam duration in minutes)
   // Rules:
   // - Minimum: 60 minutes
   // - Maximum: 180 minutes
   // - Must be multiple of 15
   // Include method: ToHoursAndMinutes() returns (int hours, int minutes)
   ```

5. **Capacity**
   ```csharp
   // Create value object for Capacity (room/enrolled students)
   // Rules:
   // - Minimum: 1
   // - Maximum: 500
   // - Must be positive integer
   // Include methods: 
   // - CanAccommodate(Capacity required) returns bool
   // - UtilizationPercent(Capacity used) returns decimal
   ```

#### Entity States și Tranziții

```csharp
// Create entity states for ExamScheduling
// State machine:
// Unvalidated → Validated → RoomAllocated → Published → Closed
//            ↘ Invalid
//
// States:
public static class ExamScheduling
{
    public interface IExamScheduling { }
    
    // Unvalidated - raw input from professor
    public record UnvalidatedExamScheduling(
        string CourseCode,
        string ProposedDate1,
        string ProposedDate2,
        string ProposedDate3,
        string Duration,
        string ExpectedStudents) : IExamScheduling;
    
    // Validated - all data validated
    public record ValidatedExamScheduling(
        CourseCode CourseCode,
        IReadOnlyList<ExamDate> ProposedDates,
        Duration Duration,
        Capacity ExpectedStudents) : IExamScheduling;
    
    // RoomAllocated - room assigned by secretariat
    public record RoomAllocatedExamScheduling(
        CourseCode CourseCode,
        ExamDate SelectedDate,
        Duration Duration,
        RoomNumber Room,
        Capacity RoomCapacity,
        Capacity ExpectedStudents) : IExamScheduling;
    
    // Published - visible to students for enrollment
    public record PublishedExamScheduling(
        CourseCode CourseCode,
        ExamDate Date,
        Duration Duration,
        RoomNumber Room,
        Capacity RoomCapacity,
        Capacity EnrolledStudents,
        DateTime PublishedAt) : IExamScheduling;
    
    // Closed - exam finished, ready for grading
    public record ClosedExamScheduling(
        CourseCode CourseCode,
        ExamDate Date,
        RoomNumber Room,
        Capacity EnrolledStudents,
        Capacity AttendedStudents,
        DateTime ClosedAt) : IExamScheduling;
    
    // Invalid - validation or allocation failed
    public record InvalidExamScheduling(
        string CourseCode,
        IEnumerable<string> Reasons) : IExamScheduling;
}
```

#### Operations Detaliate

**1. ValidateExamProposalOperation**

```csharp
// Create ValidateExamProposalOperation
// Dependencies:
// - Func<CourseCode, bool> checkCourseExists
// - Func<CourseCode, DateTime> getCourseEndDate
// - Func<ExamDate, bool> checkDateAvailable
//
// Validation steps per proposal:
// 1. Parse and validate course code
// 2. Validate all 3 proposed dates
// 3. Check dates are at least 7 days after course end
// 4. Verify dates don't conflict with other exams for same students
// 5. Parse duration and expected students
//
// If any validation fails, create InvalidExamScheduling
// If all pass, create ValidatedExamScheduling
```

**2. AllocateRoomOperation**

```csharp
// Create AllocateRoomOperation  
// Dependencies:
// - Func<ExamDate, Duration, Capacity, IEnumerable<RoomNumber>> findAvailableRooms
// - Func<RoomNumber, ExamDate, Duration, bool> reserveRoom
//
// Business logic:
// 1. Try proposed dates in order
// 2. For each date, find rooms with sufficient capacity
// 3. Prefer rooms with capacity closest to expected students (minimize waste)
// 4. If room found and reserved, create RoomAllocatedExamScheduling
// 5. If no room available for any date, create InvalidExamScheduling
```

**3. PublishToStudentsOperation**

```csharp
// Create PublishToStudentsOperation
// No external dependencies
//
// Business logic:
// 1. Verify date is at least 14 days in future (publishing rules)
// 2. Set PublishedAt to current timestamp
// 3. Initialize EnrolledStudents to 0
// 4. Create PublishedExamScheduling
```

**4. EnrollStudentOperation**

```csharp
// Create EnrollStudentOperation
// Dependencies:
// - Func<StudentRegistrationNumber, CourseCode, bool> checkStudentEligible
// - Func<StudentRegistrationNumber, ExamDate, IEnumerable<ExamDate>> getStudentExams
//
// Business logic:
// 1. Check student is eligible for course
// 2. Check student doesn't have > 2 exams same day
// 3. Check student doesn't have exam previous or next day
// 4. Check room capacity not exceeded
// 5. If all checks pass, increment EnrolledStudents
// 6. Return updated PublishedExamScheduling
```

**5. CloseExamOperation**

```csharp
// Create CloseExamOperation
// Dependencies:
// - Func<CourseCode, ExamDate, int> getAttendanceCount
//
// Business logic:
// 1. Can only close on or after exam date
// 2. Get actual attendance from external system
// 3. Set ClosedAt timestamp
// 4. Create ClosedExamScheduling
```

#### Workflow Complet

```csharp
// Create ScheduleExamWorkflow
// Input: ScheduleExamCommand
// Dependencies: all operation dependencies
//
// Pipeline:
// 1. ValidateExamProposalOperation
// 2. AllocateRoomOperation  
// 3. PublishToStudentsOperation
// 4. Convert to ExamScheduledEvent
//
// Return: IExamScheduledEvent
```

#### Events

```csharp
// Create ExamScheduledEvent
// Success event properties:
// - CourseCode
// - SelectedDate
// - Room
// - Capacity
// - PublishedAt
//
// Failure event properties:
// - CourseCode (if parsed)
// - Reasons (list of all validation errors)
```

### B.2: Sistem de Alocare Cămin - Detalii Complete

#### Value Objects Necesare

1. **StudentId**
   ```csharp
   // Create value object for StudentId (CNP-based)
   // Rules:
   // - Exactly 13 digits
   // - Valid CNP format (checksum algorithm)
   // - Cannot be all zeros or all same digit
   ```

2. **AverageGrade**
   ```csharp
   // Create value object for AverageGrade
   // Rules:
   // - Range: 5.00 to 10.00
   // - Precision: 2 decimals
   // - Used for score calculation
   // Include method: ToScorePoints() returns int (0-40)
   ```

3. **Distance**
   ```csharp
   // Create value object for Distance (from home to university in km)
   // Rules:
   // - Minimum: 0
   // - Maximum: 1000
   // - Precision: 1 decimal
   // Include method: ToScorePoints() returns int (0-30)
   ```

4. **MonthlyIncome**
   ```csharp
   // Create value object for MonthlyIncome (family income per person)
   // Rules:
   // - Minimum: 0
   // - Currency: RON
   // - Precision: 2 decimals
   // Include method: ToScorePoints(decimal threshold) returns int (0-20)
   ```

5. **RoomId**
   ```csharp
   // Create value object for RoomId
   // Rules:
   // - Format: Building + Floor + Room (e.g., "T1-3-215", "T2-1-104")
   // - Building: T1, T2, T3, T4, T5
   // - Floor: 0-10
   // - Room: 100-999
   ```

6. **AllocationScore**
   ```csharp
   // Create value object for AllocationScore
   // Rules:
   // - Range: 0-100
   // - Calculated from: grade (40) + distance (30) + income (20) + special (10)
   // - Precision: 2 decimals
   // Include method: CompareTo(AllocationScore other) for sorting
   ```

#### Entity States

```csharp
// Create entity states for DormitoryApplication
// State machine:
// Unvalidated → Validated → Scored → Allocated → Confirmed
//            ↘ Invalid        ↓         ↓
//                         Unallocated  Rejected
//
public static class DormitoryApplication
{
    public interface IDormitoryApplication { }
    
    public record UnvalidatedApplication(
        string StudentId,
        string Name,
        string AverageGrade,
        string DistanceFromHome,
        string MonthlyIncomePerPerson,
        string HasSpecialSituation,
        string PreferredBuildings,
        string DocumentsAttached) : IDormitoryApplication;
    
    public record ValidatedApplication(
        StudentId StudentId,
        StudentName Name,
        AverageGrade AverageGrade,
        Distance DistanceFromHome,
        MonthlyIncome MonthlyIncome,
        bool HasSpecialSituation,
        IReadOnlyList<BuildingCode> PreferredBuildings,
        bool DocumentsComplete) : IDormitoryApplication;
    
    public record ScoredApplication(
        StudentId StudentId,
        StudentName Name,
        AllocationScore Score,
        AverageGrade AverageGrade,
        Distance Distance,
        MonthlyIncome Income,
        bool HasSpecialSituation,
        IReadOnlyList<BuildingCode> PreferredBuildings) : IDormitoryApplication;
    
    public record AllocatedApplication(
        StudentId StudentId,
        StudentName Name,
        AllocationScore Score,
        RoomId AllocatedRoom,
        BuildingCode Building,
        DateTime AllocatedAt,
        DateTime DeadlineToConfirm) : IDormitoryApplication;
    
    public record ConfirmedApplication(
        StudentId StudentId,
        RoomId Room,
        DateTime ConfirmedAt,
        DateTime MoveInDate) : IDormitoryApplication;
    
    public record RejectedApplication(
        StudentId StudentId,
        DateTime RejectedAt,
        string Reason) : IDormitoryApplication;
    
    public record UnallocatedApplication(
        StudentId StudentId,
        StudentName Name,
        AllocationScore Score,
        string Reason) : IDormitoryApplication;
    
    public record InvalidApplication(
        string StudentId,
        IEnumerable<string> Reasons) : IDormitoryApplication;
}
```

#### Operations Complete

**1. ValidateApplicationOperation**
```csharp
// Dependencies:
// - Func<StudentId, bool> checkStudentExists
// - Func<StudentId, bool> checkStudentEligible (not already in dorm, not graduated)
// - Func<StudentId, bool> checkDocumentsUploaded
//
// Validations:
// - Parse all value objects
// - Verify student exists and is eligible
// - Check documents are complete
// - Validate preferred buildings exist
```

**2. CalculateScoreOperation**
```csharp
// No dependencies (pure calculation)
//
// Formula:
// - Grade points: (average - 5.00) * 8 (max 40)
// - Distance points: min(distance / 10 * 3, 30)
// - Income points: income < threshold ? 20 : 0
// - Special situation: hasSpecial ? 10 : 0
// - Total = sum (max 100)
```

**3. AllocateRoomsOperation**
```csharp
// Dependencies:
// - Func<BuildingCode, IEnumerable<RoomId>> getAvailableRooms
// - Func<RoomId, bool> reserveRoom
//
// Algorithm:
// 1. Sort all ScoredApplications by score DESC
// 2. For each application:
//    a. Try preferred buildings in order
//    b. Find first available room
//    c. Reserve room
//    d. Set deadline = now + 7 days
// 3. If no room found, mark as Unallocated
```

**4. ConfirmAllocationOperation**
```csharp
// Dependencies:
// - Func<StudentId, bool> checkPaymentReceived
//
// Logic:
// - Check confirmation before deadline
// - Verify payment received
// - Set move-in date = start of semester
// - If past deadline or no payment, auto-reject
```

**5. HandleRejectionOperation**
```csharp
// Dependencies:
// - Func<RoomId, bool> releaseRoom
//
// Logic:
// - Release the allocated room
// - Get next student from unallocated list
// - Try to allocate released room to next student
```

### B.3: Sistem Rezervare Spații Studiu - Detalii Complete

#### Value Objects

1. **SpaceId**
   ```csharp
   // Format: Building-Floor-Type-Number
   // Example: "LIB-2-INDIVIDUAL-05", "DORM-3-GROUP-12"
   ```

2. **TimeSlot**
   ```csharp
   // Start and End time with validation:
   // - Must be in future
   // - Duration: 30min to 4 hours
   // - Within operating hours (8:00-22:00)
   // - Start must be on hour or half-hour
   ```

3. **ReservationDuration**
   ```csharp
   // Rules based on space type:
   // - Individual: max 2 hours
   // - Group: max 4 hours  
   // - Lab equipment: max 3 hours
   ```

4. **PenaltyStatus**
   ```csharp
   // Track no-shows:
   // - None: 0 no-shows
   // - Warning: 1-2 no-shows
   // - Suspended: 3+ no-shows (blocked for 1 week)
   // Include method: CanMakeReservation() returns bool
   ```

#### Entity States

```csharp
public static class SpaceReservation
{
    public interface ISpaceReservation { }
    
    public record UnvalidatedReservation(...) : ISpaceReservation;
    public record ValidatedReservation(...) : ISpaceReservation;
    public record PendingApprovalReservation(...) : ISpaceReservation;
    public record ConfirmedReservation(...) : ISpaceReservation;
    public record ActiveReservation(...) : ISpaceReservation;
    public record CompletedReservation(...) : ISpaceReservation;
    public record CancelledReservation(...) : ISpaceReservation;
    public record NoShowReservation(...) : ISpaceReservation;
    public record InvalidReservation(...) : ISpaceReservation;
}
```

#### Operations

1. ValidateReservationOperation
2. CheckAvailabilityOperation
3. ApproveReservationOperation (for long durations)
4. ActivateReservationOperation (when student checks in)
5. CompleteReservationOperation
6. HandleNoShowOperation (+ update penalty status)

---

## ANEXA C: Exerciții Suplimentare Avansate

### C.1: Saga Pattern pentru Procese Distribuite

**Context**: Unele workflow-uri necesită coordonarea între multiple bounded contexts.

**Exercițiu**:
```csharp
// Implement a saga for EnrollStudentInExam that coordinates:
// 1. ExamScheduling context - check capacity
// 2. StudentProfile context - check eligibility  
// 3. Payment context - charge enrollment fee
// 4. Notification context - send confirmation
//
// If any step fails, compensate previous steps:
// - Release reserved capacity
// - Refund payment
// - Send cancellation notification
//
// Use saga pattern with:
// - ISagaStep<TInput, TOutput> interface
// - Compensate() method for rollback
// - SagaOrchestrator to coordinate steps
```

### C.2: CQRS Pattern

**Exercițiu**:
```csharp
// Separate read and write models for ExamScheduling:
//
// Write model (commands):
// - ScheduleExamCommand
// - AllocateRoomCommand
// - PublishExamCommand
//
// Read model (queries):
// - GetExamsByDateQuery → ExamSummaryDto[]
// - GetExamByIdQuery → ExamDetailsDto
// - GetAvailableRoomsQuery → RoomAvailabilityDto[]
//
// Implement:
// 1. Command handlers (use existing operations)
// 2. Query handlers (optimized for reading)
// 3. Projection to update read model when commands succeed
```

### C.3: Event Sourcing

**Exercițiu**:
```csharp
// Implement event sourcing for DormitoryApplication:
//
// Events to store:
// - ApplicationSubmitted
// - ApplicationValidated  
// - ScoreCalculated
// - RoomAllocated
// - AllocationConfirmed
// - AllocationRejected
//
// Implement:
// 1. Event store (append-only log)
// 2. Aggregate rebuild from events
// 3. Snapshots for performance
// 4. Projections for current state queries
```

---

## ANEXA D: Troubleshooting Common Issues

### Issue 1: Copilot generează cod care nu compilează

**Simptom**: Tipuri lipsă, using-uri incorecte

**Soluție**:
```csharp
// At the top of each file, explicitly list:
using Examples.Domain.Models;
using Examples.Domain.Operations;
using Examples.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

// Then prompt Copilot with complete context:
// "Using the types from Examples.Domain.Models namespace, create..."
```

### Issue 2: Copilot nu respectă pattern-urile DDD

**Simptom**: Public constructors, mutable properties, validare în loc greșit

**Soluție**:
- Re-citește copilot-instructions.md înainte de fiecare sesiune
- Include comentarii explicite:
```csharp
// IMPORTANT: Constructor must be PRIVATE, validation in constructor
// IMPORTANT: Properties must be { get; } only (immutable)
// IMPORTANT: Use internal constructor for state records
```

### Issue 3: Workflow-ul nu gestionează corect toate stările

**Simptom**: Pattern matching incomplet, excepții neașteptate

**Soluție**:
```csharp
// Ensure switch expression has ALL state types:
public IExamPublishedEvent Execute(...)
{
    // ...
    return exam switch
    {
        UnvalidatedExam => /* handle */,
        ValidatedExam => /* handle */,
        CalculatedExam => /* handle */,
        PublishedExam => /* handle */,
        InvalidExam => /* handle */,
        _ => throw new InvalidOperationException($"Unexpected state: {exam.GetType().Name}")
    };
}
```

### Issue 4: TryParse method nu funcționează corect

**Simptom**: Excepții în loc de false, null reference exceptions

**Soluție template**:
```csharp
public static bool TryParse(string input, out MyValueObject? result)
{
    result = null; // CRITICAL: initialize to null
    
    if (string.IsNullOrWhiteSpace(input))
        return false;
    
    if (!IsValid(input))
        return false;
    
    try
    {
        result = new MyValueObject(input); // May throw from constructor
        return true;
    }
    catch
    {
        result = null;
        return false;
    }
}
```

---

## ANEXA E: Grading Rubric Detailat

*(Vedeți și [ANEXA N: Evaluation Rubric - Detailed Breakdown](#anexa-n-evaluation-rubric---detailed-breakdown) pentru o defalcare pe sub-criterii, în engleză, a acelorași ponderi.)*

### Partea 1: Event Storming (15 puncte)

| Criteriu | Excelent (5p) | Bun (3-4p) | Satisfăcător (1-2p) | Insuficient (0p) |
|----------|---------------|------------|---------------------|------------------|
| **Identificare evenimente** | 15+ evenimente relevante, descriere diversă și acoperă tot flow-ul | 10-14 evenimente, acoperire parțială | 5-9 evenimente, lipsesc scenarii importante | < 5 evenimente sau majoritatea irelevante |
| **Bounded contexts** | Contexts clar delimitați, responsabilități bine definite, comunicare explicită | 2-3 contexts, delimitare rezonabilă | 1-2 contexts, delimitare vagă | Nu sunt identificate contexte |
| **Comenzi și agregări** | Toate evenimentele au comenzi și agregări asociate, consistente | Majoritatea au, câteva lipsesc | Doar câteva identificate | Nu sunt identificate |

### Partea 2: Value Objects (20 puncte)

| Criteriu | 5p | 3-4p | 1-2p | 0p |
|----------|-----|------|------|-----|
| **Validare** | Toate validările în constructor, IsValid separat, excepții specifice | Majoritatea validărilor corecte | Validări incomplete | Fără validare |
| **TryParse** | Implementat corect, gestionează toate cazurile | Implementat, dar cu bugs minore | Implementare incompletă | Lipsește sau greșită |
| **Immutability** | Toate properties readonly, constructor privat | Majoritatea immutable | Câteva mutable | Mutable |
| **ToString** | Implementat pentru toate | Implementat parțial | Doar câteva | Lipsește |

### Partea 3: Entity States (20 puncte)

| Criteriu | 5p | 3-4p | 1-2p | 0p |
|----------|-----|------|------|-----|
| **State Pattern** | Interface + records pentru toate stările, internal constructors | Majoritatea corect | Câteva issues | Nu folosește pattern |
| **Tranziții logice** | Flow logic între stări, toate tranzițiile posibile | Majoritatea tranzițiilor | Lipsesc tranziții | Nu sunt clare stările |
| **Properties adecvate** | Fiecare stare are properties relevante, tipuri corecte | Majoritatea corecte | Properties lipsă sau greșite | Properties inadecvate |
| **Collections** | IReadOnlyCollection peste tot | Majoritatea readonly | Câteva List<> mutabile | Arrays sau List<> |

### Partea 4: Operations (20 puncte)

| Criteriu | 5p | 3-4p | 1-2p | 0p |
|----------|-----|------|------|-----|
| **Pattern matching** | Switch expression complet, toate stările | Majoritatea stărilor | Câteva stări lipsă | Nu folosește pattern matching |
| **Single Responsibility** | Fiecare operație face exact un lucru | Majoritatea focalizate | Câteva fac prea multe | Operations prea complexe |
| **Virtual methods** | Override doar metodele necesare, default identity | Majoritatea corecte | Prea multe override-uri | Nu extinde corect base |
| **Dependencies** | DI prin constructor, Func<> pentru externe | Majoritatea injectate | Câteva hardcoded | Fără DI |

### Partea 5: Workflow (15 puncte)

| Criteriu | 5p | 3-4p | 1-2p | 0p |
|----------|-----|------|------|-----|
| **Composition** | Pipeline clar, operații chainuite logic | Majoritatea chainuite | Câteva operații lipsă | Nu e pipeline |
| **Error handling** | Convert la event corect, gestionează toate stările | Majoritatea cazurilor | Câteva cazuri lipsă | Fără error handling |
| **Dependencies** | Toate injectate prin constructor | Majoritatea | Câteva hardcoded | Fără DI |

### Partea 6: Utilizare AI (10 puncte)

| Criteriu | 5p | 3-4p | 1-2p | 0p |
|----------|-----|------|------|-----|
| **Prompturi** | Prompturi clare, specifice, cu context | Majoritatea clare | Vagi sau prea generice | Copy-paste fără gândire |
| **Validare** | Verifică și corectează toate sugestiile AI | Verifică majoritatea | Acceptă orb unele | Acceptă tot fără verificare |

### Bonus (până la 10 puncte)

- **Tests** (5p): Unit tests pentru value objects și operations
- **Documentation** (3p): XML comments, README cu explicații
- **Advanced patterns** (2p): CQRS, Event Sourcing, Saga

---

## ANEXA F: Exemple de Cod Complet

Exemplele de cod complet (obiect-valoare, stare de entitate, operație, workflow) nu mai stau aici — foloseau stilul anterior (constructor `private` + `TryParse`, stare `Invalid`) și duplicau, prost actualizat, ce e deja implementat corect în laborator. Referința completă și la zi este [Lucrarea 3](../Lucrarea-03/Exemple/Examples.Domain/): `ValueObjects/StudentRegistrationNumber.cs`, `States/Exam.cs`, `Operations/ExamValidation.cs`, `Workflows/PublishExamWorkflow.cs`.

---

## ANEXA G: Resurse și Referințe

### Cărți Recomandate
1. **"Domain-Driven Design" by Eric Evans** - fundamentele DDD
2. **"Implementing Domain-Driven Design" by Vaughn Vernon** - implementare practică
3. **"Domain Modeling Made Functional" by Scott Wlaschin** - DDD cu programare funcțională

### Articole și Blog Posts
- [Microsoft - DDD Patterns](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Martin Fowler - Domain Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Domain-Driven Design Europe](https://dddeurope.com/resources/)

### Tools și Frameworks
- **GitHub Copilot** - AI pair programmer
- **ChatGPT/Claude** - pentru explorare arhitecturală
- **Miro/Mural** - pentru Event Storming virtual
- **PlantUML** - pentru diagrame automate
- **ArchUnit** - pentru validare arhitecturală automată

### Video Tutorials
- [Event Storming - Alberto Brandolini](https://www.youtube.com/watch?v=1i6QYvYhlYQ)
- [Domain-Driven Design Fundamentals - Pluralsight](https://www.pluralsight.com/courses/domain-driven-design-fundamentals)
- [GOTO Conferences - DDD Track](https://www.youtube.com/@GOTO-)

### GitHub Repositories cu Exemple
- [microsoft/eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [EventStore/samples](https://github.com/EventStore/samples)

---

## ANEXA H: FAQ - Întrebări Frecvente

### Q1: Când ar trebui să folosesc value objects vs simple strings?

**A**: Folosește value objects când:
- Valoarea are reguli de validare specifice
- Valoarea are comportament asociat (metode)
- Vrei să previi erori de tip (ex: confundarea unui ID de student cu un ID de cameră)
- Valoarea apare frecvent în domeniu și merită să fie tip explicit

### Q2: Câte operații ar trebui să am într-un workflow?

**A**: Depinde de complexitatea domeniului, dar în general:
- **Simple workflows**: 2-4 operații (validate → process → publish)
- **Medium workflows**: 5-7 operații (validate → enrich → calculate → allocate → notify → publish)
- **Complex workflows**: 8+ operații (consider splitting into multiple workflows sau folosește saga pattern)

Regula generală: dacă o operație devine prea complexă (>100 linii), împarte-o în mai multe operații.

### Q3: Când folosesc evenimente vs comenzi?

**A**:
- **Comenzi** (Commands): exprimă intenție, request pentru acțiune
  - Nume la imperativ: `ScheduleExam`, `AllocateRoom`
  - Pot fi respinse (validare)
  - Un singur handler
  
- **Evenimente** (Events): exprimă fapt împlinit, ceva care s-a întâmplat
  - Nume la trecut: `ExamScheduled`, `RoomAllocated`
  - Nu pot fi respinse (deja s-au întâmplat)
  - Multiple handlers posibile

### Q4: Cum gestionez relațiile între entități?

**A**: În DDD:
- **În cadrul aceluiași aggregate**: referință directă (object reference)
- **Între aggregates diferite**: referință prin ID (value object)
- **Între bounded contexts**: evenimente de integrare

Exemplu:
```csharp
// BAD - referință directă între aggregates
public record Reservation(Student Student, Room Room);

// GOOD - referință prin ID
public record Reservation(StudentId StudentId, RoomId RoomId);
```

### Q5: GitHub Copilot nu generează cod bun. Ce fac greșit?

**A**: Verifică:
1. **Context insuficient**: Copilot vede doar fișierul curent. Adaugă comentarii cu context:
   ```csharp
   // This follows the pattern from StudentRegistrationNumber.cs
   // Must have: private constructor, TryParse method, validation
   ```

2. **Prompturi vagi**: În loc de "create a class", folosește:
   ```csharp
   // Create value object for ExamDate with:
   // - Private constructor with validation
   // - Static TryParse method
   // - Immutable properties
   // - ToString override
   ```

3. **Nu validezi output-ul**: Copilot face greșeli. Verifică întotdeauna logica generată.

### Q6: Cum testez workflow-uri cu dependencies?

**A**: Folosește mock objects sau funcții simple:

```csharp
[Fact]
public void Execute_ValidData_ReturnsSuccessEvent()
{
    // Arrange
    var command = CreateValidCommand();
    Func<StudentId, bool> mockCheck = id => true; // All students exist
    var workflow = new AllocateDormitoryWorkflow();
    
    // Act
    var result = workflow.Execute(command, mockCheck, ...);
    
    // Assert
    result.Should().BeOfType<AllocationSucceededEvent>();
}
```

### Q7: Pot folosi AI pentru a genera teste?

**A**: Da, dar cu atenție:
```csharp
// Generate xUnit tests for CourseCode value object
// Test cases:
// 1. Valid codes: "PSSC", "BD", "POO2"
// 2. Invalid codes: "pssc" (lowercase), "ABC123" (too long), "A" (too short)
// 3. Edge cases: null, empty, whitespace
// Use plain Assert (xunit.v3, no FluentAssertions — see copilot-instructions.md)
```

AI-ul va genera teste comprehensive, dar verifică că acoperă toate edge cases-urile relevante.

### Q8: Cum organizez fișierele în proiect?

**A**: Structură recomandată:
```
Solution/
├── Domain/
│   ├── Models/
│   │   ├── ValueObjects/
│   │   │   ├── StudentId.cs
│   │   │   ├── CourseCode.cs
│   │   │   └── ...
│   │   ├── Entities/
│   │   │   ├── ExamScheduling.cs
│   │   │   └── ...
│   │   ├── Commands/
│   │   │   ├── ScheduleExamCommand.cs
│   │   │   └── ...
│   │   └── Events/
│   │       ├── ExamScheduledEvent.cs
│   │       └── ...
│   ├── Operations/
│   │   ├── DomainOperation.cs
│   │   ├── ExamOperation.cs
│   │   ├── ValidateExamOperation.cs
│   │   └── ...
│   ├── Workflows/
│   │   ├── ScheduleExamWorkflow.cs
│   │   └── ...
│   └── Exceptions/
│       ├── DomainException.cs
│       ├── InvalidCourseCodeException.cs
│       └── ...
├── Tests/
│   ├── ValueObjectsTests/
│   ├── OperationsTests/
│   └── WorkflowsTests/
└── ConsoleApp/
    └── Program.cs
```

### Q9: Care sunt greșelile comune de evitat?

**A**:
1. **Validare în mai multe locuri** - validează doar în constructor-ul value object-ului
2. **Value objects mutabile** - folosește `record` sau properties cu doar `{ get; }`
3. **Logică de business în workflow** - workflow-ul doar compune operații, nu conține logică
4. **Acceptare oarbă a codului generat de AI** - validează întotdeauna
5. **Aggregates prea mari** - dacă aggregatul are >10 properties, probabil ar trebui împărțit
6. **Ignorarea stărilor invalide** - modelează explicit starea `Invalid` cu motive

### Q10: Cum extind workflow-ul pentru cerințe noi?

**A**: Adaugă operații noi în pipeline:
```csharp
// Original workflow
IExam exam = new ValidateOperation().Transform(unvalidated);
exam = new CalculateOperation().Transform(exam);
exam = new PublishOperation().Transform(exam);

// Extended workflow - add notification
IExam exam = new ValidateOperation().Transform(unvalidated);
exam = new CalculateOperation().Transform(exam);
exam = new NotifyProfessorOperation().Transform(exam); // NEW
exam = new PublishOperation().Transform(exam);
```

Operațiile existente nu se modifică (Open-Closed Principle).

---

## Final Checklist pentru Studenți

Vedeți [ANEXA C: Checklist Final](#anexa-c-checklist-final), la începutul acestei anexe — aceeași listă, actualizată pentru stilul curent (fără `TryParse`, fără stare `Invalid`, operații ca membri de extensie).

**Succes la implementare! 🚀**

---

## ANEXA I: Exemple de Prompturi Eficiente pentru Fiecare Etapă

### I.1: Prompturi pentru Event Storming

#### Prompt pentru descoperirea evenimentelor inițiale:
```
Lucrez la un sistem [DOMENIU]. Actorii principali sunt [LISTA ACTORI].

Ajută-mă să generez o listă de evenimente de domeniu (domain events) care ar putea 
apărea în acest sistem. Pentru fiecare eveniment:
- Folosește timp trecut (ex: "CommandPlaced", nu "PlaceCommand")
- Grupează evenimente relacionate
- Indică evenimentele critice pentru business

Format tabel:
| Event Name | Triggered By | What Happened | Impact |
```

#### Prompt pentru rafinarea evenimentelor:
```
Am identificat următoarele evenimente preliminare:
[LISTA EVENIMENTE]

Ajută-mă să:
1. Identific evenimente redundante sau prea granulare
2. Sugerez evenimente lipsă bazate pe flow-ul complet
3. Grupez evenimentele în agregate logice
4. Identific cauzality chains (care evenimente declanșează alte evenimente)

Pentru fiecare grupare, explică raționamentul.
```

#### Prompt pentru identificarea bounded contexts:
```
Evenimente identificate:
[LISTA EVENIMENTE GRUPATE]

Ajută-mă să definesc bounded contexts folosind criterii DDD:
- Agregările care aparțin împreună
- Limbajul ubiquu specific fiecărui context
- Granițele între contexte (ce se comunică prin evenimente)
- Context map: upstream/downstream relationships

Oferă și un diagram Mermaid pentru vizualizare.
```

### I.2: Prompturi pentru Implementare Value Objects

#### Prompt basic pentru value object:
```csharp
// Create a value object for [NAME] representing [DESCRIPTION]
// 
// Constraints:
// - [CONSTRAINT 1]
// - [CONSTRAINT 2]
// - [FORMAT/PATTERN if applicable]
//
// Must follow pattern:
// - record type
// - private constructor with validation
// - static TryParse method
// - IsValid private method
// - ToString override
// - throw [SpecificException] on invalid input
//
// Example valid values: [EXAMPLES]
// Example invalid values: [COUNTER-EXAMPLES]
```

#### Prompt pentru value object cu logică complexă:
```csharp
// Create value object for [NAME] with advanced features:
//
// Validation rules:
// - [RULE 1]
// - [RULE 2]
// 
// Additional methods needed:
// - [METHOD 1]: [description]
// - [METHOD 2]: [description]
//
// Operators to implement:
// - [OPERATOR]: [behavior]
//
// Follow the pattern from Grade.cs which has:
// - operator + for averaging
// - Round() method
// - decimal Value property
```

#### Prompt pentru value object cu dependență externă:
```csharp
// Create value object for [NAME] that requires external validation
//
// Internal validation:
// - [RULES]
//
// External validation (cannot be in constructor):
// - Needs to check [EXTERNAL_DEPENDENCY]
// - This will be provided via dependency injection in the operation
//
// Design:
// - TryParse should only do format/structure validation
// - External validation happens in ValidateOperation
// - Include a separate method: IsValidFormat(string input)
```

### I.3: Prompturi pentru Entity States

#### Prompt pentru state machine complet:
```csharp
// Design entity states for [ENTITY_NAME] following state machine pattern
//
// Business flow:
// [STATE_1] → [STATE_2] → [STATE_3] → [FINAL_STATE]
//         ↘ [INVALID_STATE]
//
// For each state, define:
// 1. Properties specific to that state
// 2. Validation rules to transition TO this state
// 3. Possible transitions OUT of this state
//
// Follow pattern from Exam.cs:
// - Interface I[Entity]
// - Records for each state with internal constructor
// - IReadOnlyCollection for lists
// - Invalid state with Reasons property
//
// States needed:
// - Unvalidated[Entity]: [raw input properties]
// - Validated[Entity]: [validated value objects]
// - [IntermediateState1]: [properties for this stage]
// - [IntermediateState2]: [properties for this stage]
// - [FinalState]: [final properties + metadata]
// - Invalid[Entity]: [reasons for failure]
```

#### Prompt pentru state cu substates:
```csharp
// Design entity states for [ENTITY] with hierarchical states
//
// Main states: [LIST]
//
// State [X] has substates:
// - [SUBSTATE_1]: [when/why]
// - [SUBSTATE_2]: [when/why]
//
// Question: Should I model substates as:
// A) Separate record types (Pending, PendingManualReview, PendingPayment)
// B) Single record with status enum (Pending with PendingReason enum)
// C) Marker interfaces (IPendingApplication with subtypes)
//
// Recommend approach based on DDD principles and provide implementation.
```

### I.4: Prompturi pentru Operations

#### Prompt pentru validation operation:
```csharp
// Create [EntityName]ValidationOperation following ValidateExamOperation pattern
//
// Dependencies needed (constructor parameters):
// - Func<[Type1], bool> [checkSomething]: [description]
// - Func<[Type2], [ResultType]> [getSomething]: [description]
//
// Validation logic:
// 1. For each unvalidated item:
//    a. Parse and validate [Field1] using [ValueObject1].TryParse
//    b. Parse and validate [Field2] using [ValueObject2].TryParse  
//    c. Check external constraint: [description]
//    d. Check business rule: [description]
// 2. Collect all validation errors
// 3. If no errors: create Validated[Entity] with all validated fields
// 4. If errors: create Invalid[Entity] with reasons
//
// Helper methods to create:
// - ValidateAndParse[Field1]
// - ValidateAndParse[Field2]
// - Each helper adds errors to list and returns parsed value or null
```

#### Prompt pentru business logic operation:
```csharp
// Create [OperationName] that transforms [SourceState] to [TargetState]
//
// Business rules:
// - [RULE 1]: [detailed description]
// - [RULE 2]: [detailed description]
// - [CALCULATION if applicable]
//
// Implementation:
// - Override On[SourceState] method only
// - Use LINQ Select to transform each item
// - Create new [TargetState] record for each item
// - Handle edge cases: [list edge cases]
//
// Example transformation:
// Input: [example input]
// Output: [example output]
```

#### Prompt pentru orchestration operation:
```csharp
// Create [OperationName] that orchestrates multiple steps
//
// Dependencies:
// - [Dependency 1]
// - [Dependency 2]
//
// Steps:
// 1. [Step 1]: [what to do]
//    If fails: [failure handling]
// 2. [Step 2]: [what to do]
//    If fails: [failure handling]
// 3. [Step 3]: [what to do]
//
// Question: Should this be:
// A) Single operation with complex logic
// B) Multiple operations chained in workflow
// C) Saga pattern with compensation
//
// Recommend approach and implement.
```

### I.5: Prompturi pentru Workflow

#### Prompt pentru workflow complet:
```csharp
// Create [WorkflowName] following PublishExamWorkflow pattern
//
// Input: [CommandName] containing [list of input data]
//
// Dependencies (all injected via Execute method parameters):
// - [Dependency 1]: Func<[Input], [Output]> - [description]
// - [Dependency 2]: Func<[Input], [Output]> - [description]
//
// Pipeline:
// 1. Create Unvalidated[Entity] from command.InputData
// 2. Transform using [Operation1] (pass dependency1)
// 3. Transform using [Operation2] (no dependency)
// 4. Transform using [Operation3] (pass dependency2)
// 5. Convert final IExam state to I[Entity][Action]Event using ToEvent()
//
// Return type: I[Entity][Action]Event
//
// Key points:
// - Each operation Transform() takes previous result
// - Workflow has no business logic, only composition
// - Dependencies injected, not created in workflow
```

#### Prompt pentru workflow cu branching:
```csharp
// Create [WorkflowName] with conditional paths
//
// Flow:
// 1. Validate input
// 2. If [CONDITION]:
//    - Path A: [operations]
// 3. Else:
//    - Path B: [operations]
// 4. Merge: [final operations]
//
// Question: How to implement branching in DDD workflow?
// Options:
// A) Pattern matching after validation
// B) Separate operations that handle branching
// C) Two separate workflows
//
// Recommend and implement with explanation.
```

### I.6: Prompturi pentru Events

#### Prompt pentru event classes:
```csharp
// Create event classes for [Entity][Action] following ExamPublishedEvent pattern
//
// Event interface: I[Entity][Action]Event
//
// Success event: [Entity][Action]SucceededEvent
// Properties:
// - [Property 1]: [type] - [description]
// - [Property 2]: [type] - [description]
// - Timestamp: DateTime
//
// Failure event: [Entity][Action]FailedEvent  
// Properties:
// - Reasons: IEnumerable<string>
// - Optional: [PartialData] if some data was valid
//
// Extension method: ToEvent(this I[Entity] entity)
// - Pattern match on all entity states
// - Invalid/Unvalidated/Unexpected → FailedEvent
// - [FinalSuccessState] → SucceededEvent with relevant data
```

### I.7: Prompturi pentru Testing

#### Prompt pentru unit tests:
```csharp
// Generate xUnit unit tests for [ClassName]
//
// Test categories needed:
//
// 1. Constructor tests:
//    - Valid input → should create object
//    - Invalid input → should throw [SpecificException]
//    - Edge cases: [list]
//
// 2. TryParse tests (if applicable):
//    - Valid formats → should return true and parse correctly
//    - Invalid formats → should return false
//    - Null/empty/whitespace → should return false
//
// 3. Business logic tests:
//    - [Scenario 1] → [expected result]
//    - [Scenario 2] → [expected result]
//
// Use:
// - [Theory] with [InlineData] for parameterized tests
// - Plain Assert (xunit.v3, no FluentAssertions)
// - Arrange-Act-Assert pattern clearly separated
// - Descriptive test names: MethodName_Scenario_ExpectedResult
```

#### Prompt pentru integration tests:
```csharp
// Generate integration tests for [WorkflowName]
//
// Test scenarios:
//
// 1. Happy path:
//    - Input: [valid data]
//    - Mock dependencies: [return values]
//    - Expected: [SuccessEvent] with [properties]
//
// 2. Validation failure:
//    - Input: [invalid data in field X]
//    - Expected: [FailureEvent] with reason containing "[message]"
//
// 3. External dependency failure:
//    - Input: [valid data]
//    - Mock [dependency] returns false
//    - Expected: [FailureEvent] with appropriate reason
//
// 4. Edge cases:
//    - [Scenario]: [expected behavior]
//
// Create mocks using simple Func<> delegates, not mocking frameworks.
```

### I.8: Prompturi pentru Refactoring

#### Prompt pentru code review:
```
Review this [ClassName] implementation for DDD compliance:

[PASTE CODE]

Check for:
1. Immutability: Are all properties readonly? Constructor private?
2. Encapsulation: Is validation in the right place?
3. Single Responsibility: Does class do one thing well?
4. Naming: Does it follow ubiquitous language?
5. Dependencies: Are external dependencies injected properly?
6. Error handling: Are domain exceptions used?
7. Pattern compliance: Does it match our established patterns?

For each issue found:
- Explain why it's a problem
- Show the correct implementation
- Explain the DDD principle behind it
```

#### Prompt pentru performance optimization:
```csharp
// This [OperationName] is slow with large datasets:
[PASTE CODE]

Profile and suggest optimizations for:
1. LINQ queries that could be more efficient
2. Multiple iterations that could be combined
3. Unnecessary object allocations
4. String concatenation in loops

Maintain:
- Immutability of value objects
- Functional programming style
- Readability and maintainability

Show before/after comparison with estimated performance impact.
```

#### Prompt pentru extracting patterns:
```
I have implemented similar logic in multiple places:

[PASTE CODE SNIPPET 1]
[PASTE CODE SNIPPET 2]
[PASTE CODE SNIPPET 3]

Help me extract a reusable pattern:
1. Identify the common structure
2. Suggest an abstraction (base class, interface, or helper method)
3. Show how to refactor each case to use the abstraction
4. Ensure the abstraction follows DDD principles

Consider:
- Is this truly common behavior or just coincidental similarity?
- Would abstraction add value or just complexity?
- Can it be done without breaking immutability?
```

---

## ANEXA J: Debugging Strategies cu AI

### J.1: Când Codul Nu Compilează

#### Strategie: Iterative Fixing

```
Copilot generated this code but it doesn't compile:
[PASTE CODE]

Compilation errors:
[PASTE ERRORS]

Context:
- I'm using .NET 10 / C# 14
- Available types in project: [LIST KEY TYPES]
- This should follow pattern from [REFERENCE CLASS]

Fix the compilation errors while maintaining the intended functionality.
```

### J.2: Când Logica Este Incorectă

#### Strategie: Explain and Fix

```
This validation logic isn't working as expected:
[PASTE CODE]

Expected behavior: [DESCRIPTION]
Actual behavior: [WHAT'S HAPPENING]

Test cases:
✓ Input: [VALID CASE] → should pass, and it does
✗ Input: [INVALID CASE] → should fail, but it passes
✗ Input: [EDGE CASE] → crashes with [EXCEPTION]

Debug and fix the logic. Explain what was wrong.
```

### J.3: Când Pattern-ul Nu Este Urmat

#### Strategie: Pattern Enforcement

```
AI generated this but it doesn't follow our DDD patterns:
[PASTE CODE]

Our patterns require:
1. [PATTERN 1 from established code]
2. [PATTERN 2 from established code]

Reference implementation:
[PASTE REFERENCE CODE]

Refactor the generated code to match our patterns exactly.
Explain each change you make and why it's necessary.
```

### J.4: Când Performance Este Problemă

#### Strategie: Profile and Optimize

```
This operation is slow with [N] items:
[PASTE CODE]

Profiling shows:
- [BOTTLENECK 1]: [time/allocations]
- [BOTTLENECK 2]: [time/allocations]

Constraints:
- Must maintain immutability
- Must preserve functional style
- Cannot change public API

Suggest optimizations with code examples.
```

---

## ANEXA K: Advanced AI Techniques

### K.1: Chain-of-Thought Prompting

Pentru probleme complexe, cereți AI-ului să gândească step-by-step:

```
I need to implement [COMPLEX FEATURE].

Let's think through this step by step:

Step 1: What domain concepts are involved?
[Let AI respond]

Step 2: What value objects do we need?
[Let AI respond]

Step 3: What states will the entity go through?
[Let AI respond]

Step 4: What operations are needed for each transition?
[Let AI respond]

Step 5: Now implement the first value object: [NAME]
[Let AI implement]

Step 6: Review the implementation. Does it follow our patterns?
[Let AI review]

Continue this process for each component.
```

### K.2: Few-Shot Learning

Oferiți exemple pentru a învăța AI-ul pattern-ul vostru:

```
I want to create value objects following this pattern.

Example 1 - Simple value object:
[PASTE StudentRegistrationNumber COMPLETE CODE]

Example 2 - Value object with calculation:
[PASTE Grade COMPLETE CODE]

Example 3 - Value object with complex validation:
[PASTE ExamDate COMPLETE CODE]

Now create a value object for [NEW CONCEPT] following the same pattern:
- [Requirements]
- [Constraints]
```

### K.3: Socratic Method

Lăsați AI-ul să vă pună întrebări:

```
I want to implement [FEATURE] in my DDD system.

Instead of directly implementing it, first ask me clarifying questions about:
1. The business rules involved
2. The state transitions needed
3. External dependencies required
4. Edge cases to handle
5. Integration with existing code

After I answer all questions, propose an implementation plan.
```

### K.4: Comparative Analysis

Cereți AI-ului să compare opțiuni:

```
I need to [IMPLEMENT SOMETHING] and I'm considering two approaches:

Approach A:
[DESCRIPTION]
Pros: [LIST]
Cons: [LIST]

Approach B:
[DESCRIPTION]
Pros: [LIST]
Cons: [LIST]

Analyze both approaches in the context of:
- DDD principles
- Maintainability
- Testability
- Performance
- Complexity

Recommend the better approach with detailed reasoning.
Then implement the recommended approach.
```

---

## ANEXA L: Integrare cu Alte Tool-uri

### L.1: Mermaid Diagrams Generate cu AI

```
Generate a Mermaid diagram for my domain model:

Bounded Contexts:
- [CONTEXT 1]: [aggregates]
- [CONTEXT 2]: [aggregates]

Relationships:
- [CONTEXT 1] → [CONTEXT 2]: [events/commands]

Include:
- Aggregates as boxes
- Value objects as nested boxes
- Events as arrows with labels
- Context boundaries as subgraphs

Format: Mermaid class diagram or flowchart (recommend which is better).
```

### L.2: PlantUML pentru Architecture

```
Generate PlantUML component diagram for:

Components:
- [COMPONENT 1]: [responsibility]
- [COMPONENT 2]: [responsibility]

Dependencies:
- [COMPONENT 1] depends on [COMPONENT 2] for [reason]

Include:
- Packages for bounded contexts
- Components within packages
- Dependency arrows
- Notes for key interfaces
```

### L.3: OpenAPI/Swagger pentru API Design

```
Design REST API for my [BOUNDED CONTEXT]:

Commands:
- [COMMAND 1]: POST /api/[endpoint]
- [COMMAND 2]: PUT /api/[endpoint]

Queries:
- [QUERY 1]: GET /api/[endpoint]

Generate OpenAPI 3.0 specification with:
- Request DTOs matching unvalidated entities
- Response DTOs for success/failure events
- Appropriate HTTP status codes
- Validation annotations
```

### L.4: Database Schema pentru Event Store

```
Design database schema for event sourcing:

Aggregates:
- [AGGREGATE 1]: [events it produces]
- [AGGREGATE 2]: [events it produces]

Requirements:
- Events table with type/data/metadata
- Snapshots table for performance
- Projections for read models
- Proper indexes for queries

Generate:
1. SQL CREATE TABLE statements
2. Explanation of each table/column
3. Example INSERT for each event type
4. Common query patterns
```

---

## ANEXA M: Troubleshooting GitHub Copilot

### M.1: Copilot Nu Sugerează Nimic

**Cauze posibile:**
1. Contextul este insuficient
2. Comentariul este prea vag
3. Fișierul nu are extensia corectă (.cs)
4. Copilot este dezactivat pentru acest fișier

**Soluții:**
```csharp
// BAD - prea vag
// Create a value object

// GOOD - specific cu context
// Create a value object for StudentId following the pattern from 
// StudentRegistrationNumber.cs:
// - Private constructor with CNP validation (13 digits)
// - Static TryParse method
// - Immutable string Value property
// - ToString override
// Reference: StudentRegistrationNumber.cs in same folder
```

### M.2: Copilot Sugerează Cod Greșit Repetat

**Cauză:** Copilot a învățat din cod greșit anterior în fișier

**Soluție:**
1. Ștergeți codul greșit complet
2. Salvați fișierul
3. Adăugați comentariu explicit cu pattern-ul corect
4. Așteptați sugestie nouă

```csharp
// IMPORTANT: This value object must follow these rules EXACTLY:
// 1. Private constructor - NOT public
// 2. Readonly properties - NOT mutable
// 3. TryParse returns bool - NOT throws exceptions
// 4. Validation in constructor - throws specific exception
//
// Example from codebase: StudentRegistrationNumber.cs
```

### M.3: Copilot Generează Cod Neterminat

**Cauză:** Suggestia este prea lungă

**Soluție:**
1. Acceptați parțial (Tab pentru linie curentă)
2. Continuați cu noi comentarii pentru fiecare metodă:

```csharp
public record MyValueObject
{
    public string Value { get; }
    
    // Private constructor with validation
    // Throw InvalidMyValueObjectException if validation fails
    private MyValueObject(string value)
    {
        // Copilot will complete...
    }
    
    // Static TryParse method
    // Returns bool, out parameter for result
    public static bool TryParse(string input, out MyValueObject? result)
    {
        // Copilot will complete...
    }
}
```

### M.4: Copilot Folosește Libraries Inexistente

**Cauză:** Copilot nu știe ce librării sunt disponibile în proiect

**Soluție:**
```csharp
// Available libraries in this project:
// - System
// - System.Collections.Generic
// - System.Linq
// - System.Text.RegularExpressions
// - NO external libraries (no Newtonsoft, no FluentValidation)
//
// Create validation using only built-in .NET features
```

---

## ANEXA N: Evaluation Rubric - Detailed Breakdown

### Category 1: Event Storming (15 points total)

#### Events Identification (5 points)
- **5 pts**: 15+ relevant events covering all major scenarios; events are atomic and well-named
- **4 pts**: 12-14 events; good coverage but missing some scenarios
- **3 pts**: 10-11 events; basic coverage with gaps
- **2 pts**: 7-9 events; significant gaps in coverage
- **1 pt**: 4-6 events; major scenarios missing
- **0 pts**: <4 events or mostly irrelevant

#### Bounded Contexts (5 points)
- **5 pts**: Clear context boundaries; distinct responsibilities; explicit communication patterns
- **4 pts**: Well-defined contexts with minor boundary issues
- **3 pts**: Contexts identified but boundaries unclear
- **2 pts**: Attempted context separation but inconsistent
- **1 pt**: Contexts mentioned but not properly separated
- **0 pts**: No bounded contexts or all in one context

#### Commands & Aggregates (5 points)
- **5 pts**: All events have corresponding commands; aggregates identified with clear invariants
- **4 pts**: Most events have commands; aggregates mostly clear
- **3 pts**: Some commands/aggregates identified
- **2 pts**: Few commands/aggregates identified
- **1 pt**: Minimal identification
- **0 pts**: No commands/aggregates identified

### Category 2: Value Objects (20 points total)

#### Implementation Pattern (8 points)
- **8 pts**: All VOs: private ctor, TryParse, validation, immutable, ToString
- **6-7 pts**: Most VOs follow pattern; 1-2 minor deviations
- **4-5 pts**: Pattern followed but with several issues
- **2-3 pts**: Partial pattern implementation
- **0-1 pts**: Pattern not followed

#### Validation Logic (6 points)
- **6 pts**: All validation in constructor; appropriate exceptions; complete rules
- **4-5 pts**: Most validation correct; minor gaps
- **2-3 pts**: Some validation; several gaps
- **0-1 pts**: Minimal or incorrect validation

#### Domain Appropriateness (6 points)
- **6 pts**: VOs perfectly model domain concepts; names from ubiquitous language
- **4-5 pts**: Good domain modeling; minor naming issues
- **2-3 pts**: Basic domain representation
- **0-1 pts**: Poor domain modeling

### Category 3: Entity States (20 points total)

#### State Pattern (8 points)
- **8 pts**: Interface + records; internal ctors; all states modeled
- **6-7 pts**: Pattern mostly correct; 1-2 issues
- **4-5 pts**: Basic pattern with several issues
- **2-3 pts**: Attempted pattern but significant problems
- **0-1 pts**: Pattern not used or incorrect

#### State Transitions (6 points)
- **6 pts**: All transitions logical; no impossible states
- **4-5 pts**: Most transitions correct
- **2-3 pts**: Some transitions unclear/illogical
- **0-1 pts**: Poor transition design

#### Type Safety (6 points)
- **6 pts**: Impossible states unrepresentable; compiler-enforced correctness
- **4-5 pts**: Mostly type-safe; minor gaps
- **2-3 pts**: Some type safety
- **0-1 pts**: No type safety benefits

### Category 4: Operations (20 points total)

#### Single Responsibility (5 points)
- **5 pts**: Each operation does one thing; clear purpose
- **4 pts**: Mostly focused; 1 operation too complex
- **3 pts**: Several operations mixing concerns
- **2 pts**: Operations too complex
- **0-1 pts**: Poor separation of concerns

#### Pattern Matching (5 points)
- **5 pts**: Complete switch expressions; all states handled
- **4 pts**: Mostly complete; minor gaps
- **3 pts**: Pattern matching used but incomplete
- **2 pts**: Minimal pattern matching
- **0-1 pts**: No pattern matching

#### Dependency Injection (5 points)
- **5 pts**: All dependencies via constructor; Func<> pattern used correctly
- **4 pts**: Most dependencies injected
- **3 pts**: Some dependencies injected
- **2 pts**: Few dependencies injected
- **0-1 pts**: No DI; hardcoded dependencies

#### Code Quality (5 points)
- **5 pts**: Clean, readable, well-commented; follows conventions
- **4 pts**: Good quality; minor issues
- **3 pts**: Acceptable quality
- **2 pts**: Poor quality; hard to read
- **0-1 pts**: Very poor quality

### Category 5: Workflow (15 points total)

#### Composition (6 points)
- **6 pts**: Perfect pipeline; operations chained correctly
- **4-5 pts**: Good composition; minor issues
- **2-3 pts**: Basic composition with problems
- **0-1 pts**: Poor or no composition

#### Error Handling (5 points)
- **5 pts**: All states converted to events; comprehensive error handling
- **4 pts**: Most states handled
- **3 pts**: Basic error handling
- **2 pts**: Minimal error handling
- **0-1 pts**: No error handling

#### Dependency Management (4 points)
- **4 pts**: All dependencies injected via parameters
- **3 pts**: Most dependencies injected
- **2 pts**: Some dependencies hardcoded
- **0-1 pts**: Poor dependency management

### Category 6: AI Tool Usage (10 points total)

#### Prompt Quality (4 points)
- **4 pts**: Clear, specific prompts with context
- **3 pts**: Good prompts; minor improvements possible
- **2 pts**: Basic prompts; often too vague
- **0-1 pts**: Poor prompts

#### Critical Validation (4 points)
- **4 pts**: All AI output validated and corrected
- **3 pts**: Most output validated
- **2 pts**: Some output validated
- **0-1 pts**: Accepted AI output without validation

#### Documentation (2 points)
- **2 pts**: Prompts documented; process explained
- **1 pt**: Minimal documentation
- **0 pts**: No documentation

### Bonus Points (up to 10)

#### Unit Tests (+5)
- Complete test suite with edge cases: +5
- Basic tests: +3
- Minimal tests: +1

#### Documentation (+3)
- Comprehensive README and comments: +3
- Basic documentation: +2
- Minimal documentation: +1

#### Advanced Patterns (+2)
- CQRS, Event Sourcing, or Saga implemented: +2
- Attempted but incomplete: +1

---

## Final Notes for Students

Acest laborator reprezintă o introducere în lumea modernă a dezvoltării software, unde AI devine un tool indispensabil dar nu înlocuiește expertiza umană.

**Cele mai importante lecții:**
1. **AI este un asistent, nu un înlocuitor** - gândirea critică rămâne esențială
2. **Pattern-urile contează** - un cod bun urmează principii clare
3. **Domeniul dictează designul** - nu tehnologia
4. **Imutabilitatea simplifică** - states care nu se schimbă sunt mai ușor de înțeles
5. **Compoziția bate moștenirea** - operations compuse bat clase complexe

**Continuare după laborator:**
- Experimentați cu domenii proprii
- Explorați Event Sourcing pentru audit trails
- Încercați CQRS pentru separarea read/write
- Implementați comunicarea între bounded contexts
- Învățați despre distributed sagas

Mult succes! 🎓

---

## ANEXA O: Quick Reference Cards

### O.1: Value Object Checklist

```
✓ Checklist pentru Value Objects:
□ Record type declarat
□ Constructor PRIVAT
□ Properties cu doar { get; } (readonly)
□ Metodă IsValid(value) privată
□ Constructor aruncă excepție specifică domeniului
□ Metodă statică TryParse implementată
□ TryParse inițializează out parameter cu null
□ ToString() override implementat
□ Validare completă în constructor
□ Fără dependențe externe în constructor
□ Documentație XML pentru parametri
```

### O.2: Entity States Checklist

```
✓ Checklist pentru Entity States:
□ Interface I[Entity] definită (goală)
□ Fiecare stare = record separat
□ Toate records implementează I[Entity]
□ Constructor INTERNAL pentru fiecare record
□ Collections sunt IReadOnlyCollection<T>
□ Properties sunt readonly
□ InvalidState include IEnumerable<string> Reasons
□ Toate stările posibile sunt modelate
□ Tranziții între stări sunt logice
□ Stări imposibile nu pot fi reprezentate
```

### O.3: Operations Checklist

```
✓ Checklist pentru Operations:
□ Extinde [Entity]Operation sau [Entity]Operation<TState>
□ Override doar metodele relevante (OnUnvalidated, OnValid, etc.)
□ Metode neoverride returnează același obiect (identity)
□ Switch expression include TOATE stările
□ Cazul _ → throw exception pentru stări neașteptate
□ Dependencies injectate prin constructor
□ Fără logică de business în Transform()
□ O singură responsabilitate per operație
□ Naming: [Verb][Entity]Operation
□ Metodele helper sunt private
```

### O.4: Workflow Checklist

```
✓ Checklist pentru Workflows:
□ Metodă publică Execute()
□ Primește Command ca parametru
□ Dependencies injectate ca parametri Execute()
□ Creează entitate Unvalidated la început
□ Chainuiește operații: result = op.Transform(result)
□ Fiecare operație transformă rezultatul anterior
□ Nu conține logică de business
□ Returnează Event (Success sau Failure)
□ Conversie la event: entity.ToEvent()
□ Nu face exception handling (se propagă)
```

### O.5: Prompts Quick Reference

**Pentru Value Objects:**
```
// Create value object for [NAME] representing [DESCRIPTION]
// Rules: [LIST CONSTRAINTS]
// Pattern: private ctor, TryParse, validation, immutable
// Example valid: [EXAMPLES]
// Example invalid: [EXAMPLES]
```

**Pentru Entity States:**
```
// Create entity states for [ENTITY]
// Flow: State1 → State2 → State3 → Final
//           ↘ Invalid
// Follow pattern from Exam.cs: interface + internal records
```

**Pentru Operations:**
```
// Create [Verb][Entity]Operation
// Transform: [SourceState] → [TargetState]
// Business rules: [LIST RULES]
// Dependencies: [LIST AS Func<>]
```

**Pentru Workflows:**
```
// Create [Action][Entity]Workflow
// Input: [Command]
// Pipeline: Validate → [Step2] → [Step3] → Convert to Event
// Dependencies: [LIST]
```

---

## ANEXA P: Common Mistakes and Solutions

### P.1: Mistake - Public Constructor în Value Object

❌ **Wrong:**
```csharp
public record StudentId
{
    public string Value { get; }
    
    public StudentId(string value) // PUBLIC!
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidStudentIdException(value);
    }
}
```

✅ **Correct:**
```csharp
public record StudentId
{
    public string Value { get; }
    
    private StudentId(string value) // PRIVATE!
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidStudentIdException(value);
    }
    
    public static bool TryParse(string input, out StudentId? result)
    {
        result = null;
        if (!IsValid(input))
            return false;
            
        try
        {
            result = new StudentId(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool IsValid(string value) => 
        !string.IsNullOrWhiteSpace(value) && value.Length == 13;
}
```

**De ce?** Constructor public permite crearea de obiecte invalide fără a forța validarea.

### P.2: Mistake - Mutable Properties

❌ **Wrong:**
```csharp
public record ExamDate
{
    public DateTime Value { get; set; } // MUTABLE!
}
```

✅ **Correct:**
```csharp
public record ExamDate
{
    public DateTime Value { get; } // IMMUTABLE!
    
    private ExamDate(DateTime value)
    {
        if (IsValid(value))
            Value = value.Date;
        else
            throw new InvalidExamDateException($"{value:yyyy-MM-dd}");
    }
}
```

**De ce?** Value objects trebuie să fie immutable pentru a preveni modificări neașteptate.

### P.3: Mistake - Logică de Business în Workflow

❌ **Wrong:**
```csharp
public IExamPublishedEvent Execute(PublishExamCommand command)
{
    var exam = new UnvalidatedExam(command.InputExamGrades);
    
    // WRONG: Business logic în workflow!
    var validatedGrades = new List<ValidatedStudentGrade>();
    foreach (var grade in exam.GradeList)
    {
        if (StudentRegistrationNumber.TryParse(grade.StudentRegistrationNumber, out var regNumber))
        {
            if (Grade.TryParseGrade(grade.ExamGrade, out var examGrade))
            {
                validatedGrades.Add(new ValidatedStudentGrade(regNumber, examGrade, null));
            }
        }
    }
    
    // More business logic...
}
```

✅ **Correct:**
```csharp
public IExamPublishedEvent Execute(PublishExamCommand command, 
                                   Func<StudentRegistrationNumber, bool> checkStudentExists)
{
    // Workflow doar compune operații
    UnvalidatedExam exam = new(command.InputExamGrades);
    
    IExam result = new ValidateExamOperation(checkStudentExists).Transform(exam);
    result = new CalculateExamOperation().Transform(result);
    result = new PublishExamOperation().Transform(result);
    
    return result.ToEvent();
}
```

**De ce?** Workflow-ul trebuie să fie doar o compoziție de operații, nu să conțină logica de business.

### P.4: Mistake - TryParse Throws Exceptions

❌ **Wrong:**
```csharp
public static bool TryParse(string input, out StudentId? result)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentNullException(nameof(input)); // THROWS!
    
    result = new StudentId(input);
    return true;
}
```

✅ **Correct:**
```csharp
public static bool TryParse(string input, out StudentId? result)
{
    result = null;
    
    if (string.IsNullOrWhiteSpace(input))
        return false; // Returns false, not throws
    
    if (!IsValid(input))
        return false;
    
    try
    {
        result = new StudentId(input);
        return true;
    }
    catch
    {
        result = null;
        return false;
    }
}
```

**De ce?** TryParse nu trebuie să arunce excepții - asta e diferența între Parse și TryParse.

### P.5: Mistake - List<T> în Loc de IReadOnlyCollection<T>

❌ **Wrong:**
```csharp
public record ValidatedExam
{
    internal ValidatedExam(List<ValidatedStudentGrade> gradesList) // List!
    {
        GradeList = gradesList;
    }

    public List<ValidatedStudentGrade> GradeList { get; } // List exposed!
}
```

✅ **Correct:**
```csharp
public record ValidatedExam
{
    internal ValidatedExam(IReadOnlyCollection<ValidatedStudentGrade> gradesList)
    {
        GradeList = gradesList;
    }

    public IReadOnlyCollection<ValidatedStudentGrade> GradeList { get; }
}
```

**De ce?** IReadOnlyCollection previne modificări externe ale listei, menținând immutabilitatea.

### P.6: Mistake - Switch Expression Incomplet

❌ **Wrong:**
```csharp
public IExam Transform(IExam exam) => exam switch
{
    UnvalidatedExam unvalidated => OnUnvalidated(unvalidated),
    ValidatedExam validated => OnValid(validated),
    // Missing: CalculatedExam, PublishedExam, InvalidExam!
};
```

✅ **Correct:**
```csharp
public IExam Transform(IExam exam) => exam switch
{
    UnvalidatedExam unvalidated => OnUnvalidated(unvalidated),
    ValidatedExam validated => OnValid(validated),
    CalculatedExam calculated => OnCalculated(calculated),
    PublishedExam published => OnPublished(published),
    InvalidExam invalid => OnInvalid(invalid),
    _ => throw new InvalidExamStateException(exam.GetType().Name)
};
```

**De ce?** Toate stările posibile trebuie să fie gestionate pentru a preveni runtime errors.

### P.7: Mistake - Validare Externă în Constructor

❌ **Wrong:**
```csharp
public record StudentId
{
    private StudentId(string value, IStudentRepository repository) // External dependency!
    {
        if (!repository.Exists(value)) // Database call in constructor!
            throw new StudentNotFoundException(value);
            
        Value = value;
    }
}
```

✅ **Correct:**
```csharp
// Value object - only format validation
public record StudentId
{
    private StudentId(string value)
    {
        if (IsValid(value)) // Only format validation
            Value = value;
        else
            throw new InvalidStudentIdException(value);
    }
    
    private static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length == 13;
}

// Operation - external validation
internal sealed class ValidateStudentOperation : DomainOperation
{
    private readonly Func<StudentId, bool> checkStudentExists;
    
    internal ValidateStudentOperation(Func<StudentId, bool> checkStudentExists)
    {
        this.checkStudentExists = checkStudentExists;
    }
    
    protected override IEntity OnUnvalidated(UnvalidatedEntity entity)
    {
        if (StudentId.TryParse(entity.StudentId, out var studentId))
        {
            if (!checkStudentExists(studentId)) // External check in operation
            {
                return new InvalidEntity("Student not found");
            }
            // Continue validation...
        }
        // ...
    }
}
```

**De ce?** Value objects trebuie să fie self-contained. Validarea externă se face în operații.

---

## ANEXA Q: Sample Exam Questions

### Q.1: Multiple Choice

**1. Care dintre următoarele este caracteristică unui value object în DDD?**
- A) Are identitate unică
- B) Este mutable
- C) Este definit prin atributele sale ✓
- D) Are ciclu de viață independent

**2. În pattern-ul pe care îl folosim, constructor-ul unui value object trebuie să fie:**
- A) Public
- B) Protected
- C) Private ✓
- D) Internal

**3. Ce tip de metodă folosim pentru conversie sigură din string în value object?**
- A) Parse() care aruncă excepție
- B) TryParse() care returnează bool ✓
- C) Convert() din System
- D) Cast operator

**4. Într-un workflow DDD, ce conține metoda Execute()?**
- A) Toată logica de business
- B) Doar compoziția de operații ✓
- C) Doar validări
- D) Accesul la baza de date

**5. Ce returnează un workflow DDD?**
- A) Entitatea finală
- B) True/false pentru succes
- C) Un event (success sau failure) ✓
- D) void

### Q.2: True/False

**1. Un value object poate fi modificat după creare.**
- [ ] True
- [x] False ✓

**2. Fiecare stare a unei entități DDD trebuie să fie un record separat.**
- [x] True ✓
- [ ] False

**3. O operație poate modifica mai multe tipuri de stări în același timp.**
- [ ] True
- [x] False ✓ (o operație transform o stare în alta)

**4. TryParse trebuie să arunce excepție pentru input invalid.**
- [ ] True
- [x] False ✓

**5. Workflow-ul poate conține validări de business.**
- [ ] True
- [x] False ✓ (validările sunt în operații)

### Q.3: Code Review

**Identificați problemele în următorul cod:**

```csharp
public class Grade
{
    public decimal Value { get; set; }
    
    public Grade(string value)
    {
        Value = decimal.Parse(value);
    }
    
    public void UpdateValue(decimal newValue)
    {
        Value = newValue;
    }
}
```

**Probleme identificate:**
1. ❌ Class în loc de record (ar trebui record pentru immutability)
2. ❌ Property mutable (set în loc de doar get)
3. ❌ Constructor public (ar trebui private)
4. ❌ Fără validare (acceptă orice valoare)
5. ❌ Parse aruncă excepție (ar trebui TryParse)
6. ❌ Metodă UpdateValue (value objects sunt immutable!)
7. ❌ Fără TryParse static method
8. ❌ Fără ToString override

**Versiunea corectă:**
```csharp
public record Grade
{
    public decimal Value { get; }
    
    private Grade(decimal value)
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidGradeException($"{value:0.##}");
    }
    
    private static bool IsValid(decimal value) => 
        value >= 1.00m && value <= 10.00m;
    
    public static bool TryParse(string input, out Grade? result)
    {
        result = null;
        
        if (!decimal.TryParse(input, out decimal value))
            return false;
            
        if (!IsValid(value))
            return false;
        
        try
        {
            result = new Grade(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public override string ToString() => $"{Value:0.##}";
}
```

### Q.4: Implementation Exercise

**Implementați un value object pentru `EmailAddress` care:**
- Validează formatul email (conține @, domeniu valid)
- Are constructor privat
- Are metodă TryParse
- Este immutable
- Aruncă `InvalidEmailAddressException` pentru input invalid

**Cod starter:**
```csharp
public record EmailAddress
{
    // TODO: Implement following the pattern
}
```

**Soluție:**
```csharp
using System;
using System.Text.RegularExpressions;

public record EmailAddress
{
    private static readonly Regex ValidPattern = 
        new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    
    public string Value { get; }
    
    private EmailAddress(string value)
    {
        if (IsValid(value))
        {
            Value = value.ToLowerInvariant(); // Normalize
        }
        else
        {
            throw new InvalidEmailAddressException(
                $"'{value}' is not a valid email address");
        }
    }
    
    private static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && ValidPattern.IsMatch(value);
    
    public static bool TryParse(string input, out EmailAddress? result)
    {
        result = null;
        
        if (string.IsNullOrWhiteSpace(input))
            return false;
        
        if (!IsValid(input))
            return false;
        
        try
        {
            result = new EmailAddress(input);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
    
    public override string ToString() => Value;
}

public class InvalidEmailAddressException : Exception
{
    public InvalidEmailAddressException(string message) : base(message) { }
}
```

---

## ANEXA R: Project Setup Guide

### R.1: Structura de Foldere Recomandată

```
LabDDD/
├── LabDDD.sln
├── src/
│   ├── LabDDD.Domain/
│   │   ├── LabDDD.Domain.csproj
│   │   ├── Models/
│   │   │   ├── ValueObjects/
│   │   │   │   ├── StudentId.cs
│   │   │   │   ├── CourseCode.cs
│   │   │   │   ├── ExamDate.cs
│   │   │   │   └── ...
│   │   │   ├── Entities/
│   │   │   │   ├── ExamScheduling.cs
│   │   │   │   └── ...
│   │   │   ├── Commands/
│   │   │   │   ├── ScheduleExamCommand.cs
│   │   │   │   └── ...
│   │   │   └── Events/
│   │   │       ├── ExamScheduledEvent.cs
│   │   │       └── ...
│   │   ├── Operations/
│   │   │   ├── DomainOperation.cs
│   │   │   ├── ExamSchedulingOperation.cs
│   │   │   ├── ValidateExamProposalOperation.cs
│   │   │   ├── AllocateRoomOperation.cs
│   │   │   └── ...
│   │   ├── Workflows/
│   │   │   ├── ScheduleExamWorkflow.cs
│   │   │   └── ...
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── InvalidExamDateException.cs
│   │       └── ...
│   └── LabDDD.ConsoleApp/
│       ├── LabDDD.ConsoleApp.csproj
│       ├── Program.cs
│       └── TestData/
│           └── SampleData.cs
├── tests/
│   └── LabDDD.Tests/
│       ├── LabDDD.Tests.csproj
│       ├── ValueObjects/
│       │   ├── StudentIdTests.cs
│       │   └── ...
│       ├── Operations/
│       │   └── ValidateExamOperationTests.cs
│       └── Workflows/
│           └── ScheduleExamWorkflowTests.cs
├── docs/
│   ├── EventStorming.md
│   ├── BoundedContexts.md
│   └── adr/
│       └── 0001-<decizie>.md
└── README.md
```

### R.2: .csproj Configuration

**LabDDD.Domain.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```
(`net10.0` implică deja C# 14 — nu mai e nevoie de `<LangVersion>` explicit.)

**LabDDD.ConsoleApp.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\LabDDD.Domain\LabDDD.Domain.csproj" />
  </ItemGroup>
</Project>
```

**LabDDD.Tests.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit.v3" Version="4.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\LabDDD.Domain\LabDDD.Domain.csproj" />
  </ItemGroup>
</Project>
```
(xunit.v3 rulează pe Microsoft.Testing.Platform — nu mai sunt necesare `Microsoft.NET.Test.Sdk` sau `xunit.runner.visualstudio`; proiectul de teste e un executabil, de aici `<OutputType>Exe</OutputType>`. Fără `FluentAssertions` — folosiți `Assert` simplu, vezi `copilot-instructions.md`.)

### R.3: .editorconfig pentru Coding Standards

Creați fișier `.editorconfig` în rădăcina soluției:

```ini
root = true

[*.cs]
# Indentation
indent_style = space
indent_size = 4

# New line preferences
end_of_line = crlf
insert_final_newline = true

# Organize usings
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Naming conventions
dotnet_naming_rule.private_fields_with_underscore.severity = suggestion
dotnet_naming_rule.private_fields_with_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_with_underscore.style = underscore_prefix

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.underscore_prefix.capitalization = camel_case
dotnet_naming_style.underscore_prefix.required_prefix = _

# Code style
csharp_prefer_braces = true:warning
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion

# Null checking
csharp_style_pattern_matching_over_is_with_cast_check = true:suggestion
csharp_style_throw_expression = true:suggestion
csharp_style_conditional_delegate_call = true:suggestion

# Expression preferences
csharp_style_inlined_variable_declaration = true:suggestion
csharp_prefer_simple_default_expression = true:suggestion
csharp_style_pattern_local_over_anonymous_function = true:suggestion

# Record preferences
csharp_style_prefer_record_structs = true:suggestion
```

### R.4: copilot-instructions.md

Nu recreați acest fișier — folosiți direct [`copilot-instructions.md`](copilot-instructions.md) din acest laborator (copiați-l în `.github/copilot-instructions.md` din repository-ul echipei, ca la Sarcina 2.2). Acolo sunt descrise, actualizate pentru stilul funcțional, regulile pentru obiecte-valoare, stări ale entității, operații, workflow-uri, evenimente și erori.

Singura regulă specifică acestui laborator pe care instrucțiunile generale nu o conțin: la acest stadiu folosiți doar biblioteca standard — fără Entity Framework, fără ASP.NET (persistența și API-ul vin în Lucrările 5-6).

