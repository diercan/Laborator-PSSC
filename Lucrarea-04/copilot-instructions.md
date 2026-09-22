# GitHub Copilot Instructions for a Functional DDD Project

These rules describe the style used throughout the PSSC labs (see `../Lucrarea-03/Exemple/Examples.Domain/` for the reference implementation). The project targets **.NET 10 / C# 14**; several rules below rely on C# 14 syntax (`extension` blocks, the `field` keyword) that does not compile on older SDKs.

## Code Style and Patterns

### Value Objects
Use `sealed record` types with a private constructor and a smart constructor that returns a `Result` instead of throwing:
- Private constructor that only assigns — no validation logic inside it
- Static `Create` (for typed input) and/or `Parse` (for text input) methods that return `Result<T, TError>`
- A dedicated error type per value object: a single `sealed record` if there is one failure mode, or a closed `abstract record` hierarchy if there are several
- Get-only properties (`{ get; }`), never `{ get; set; }`
- Normalisation (rounding, trimming, canonical casing) happens in the `init` accessor using the C# 14 `field` keyword, so every write path goes through it
- Override `ToString()`, culture-invariant

Example pattern:
```csharp
public sealed record Grade
{
    public const decimal Minimum = 0m;
    public const decimal Maximum = 10m;
    public const int Scale = 2;

    // C# 14 `field`: the single write path, so normalization applies everywhere, including `with`.
    public decimal Value { get; private init => field = Normalize(value); }

    private Grade(decimal value) => Value = value;

    public static Result<Grade, GradeError> Create(decimal value) =>
        Normalize(value) is > Minimum and <= Maximum ? new Grade(value) : new GradeError.OutOfRange(value);

    public static Result<Grade, GradeError> Parse(string? raw) =>
        decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value)
            ? Create(value)
            : new GradeError.NotANumber(raw);

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);

    private static decimal Normalize(decimal value) => decimal.Round(value, Scale, MidpointRounding.AwayFromZero);
}

public abstract record GradeError
{
    private GradeError() { }
    public sealed record NotANumber(string? Raw) : GradeError;
    public sealed record OutOfRange(decimal Value) : GradeError;
}
```

### Entity States
Model entity lifecycle as a **closed choice type**: an `abstract record` with a private constructor and nested `sealed record` states.
- No marker interface, no open hierarchy — the base type itself is the closed union
- Each state is a nested `sealed record` (positional where that stays readable)
- Use `IReadOnlyList<T>`/`IReadOnlySet<T>` for collections, never `List<T>` or arrays
- **No `InvalidEntity` state.** A failed transition is not a state of the entity — it is a `Result` failure (see "Errors" below). An entity's states describe things that *are*, not reasons something didn't happen.
- Provide a `Match<TResult>` fold on the base type for compiler-checked exhaustiveness

Example pattern:
```csharp
public abstract record Entity
{
    private Entity() { }

    public sealed record Unvalidated(string RawField1, string RawField2) : Entity;
    public sealed record Validated(Field1 Field1, Field2 Field2) : Entity;
    public sealed record Processed(Field1 Field1, ProcessedData Data) : Entity;

    public TResult Match<TResult>(
        Func<Unvalidated, TResult> unvalidated,
        Func<Validated, TResult> validated,
        Func<Processed, TResult> processed) => this switch
        {
            Unvalidated e => unvalidated(e),
            Validated e => validated(e),
            Processed e => processed(e),
            _ => throw new UnreachableException(),
        };
}
```

### Operations
Transform an entity from one state to the next as **pure static functions**, not a class hierarchy.
- No `DomainOperation<TEntity, TState, TResult>` base class, no `OnXxx` virtual hooks
- Each function takes **exactly the state it transforms** as its receiver — calling it on the wrong state is a compile error, not a silent no-op
- Write them as C# 14 extension blocks (`extension(Entity.Validated entity) { ... }`) on the state type, grouped into a module (`static class`) per transition
- A function that cannot fail is **total**: it returns the next state directly. A function that can fail returns `Result<NextState, IReadOnlyList<TError>>`
- Validation combines several independent checks with `Result.Combine` (or `Traverse`/`Sequence` across a list) so **all** errors are reported, not just the first
- Dependencies (existence checks, the clock) are plain function parameters, never `Func<Input, Output>` fields on a class

Example pattern:
```csharp
public static class EntityValidation
{
    extension(Entity.Unvalidated entity)
    {
        public Result<Entity.Validated, IReadOnlyList<ValidationError>> Validate(IReadOnlySet<Id> known) =>
            Result.Combine(
                Field1.Parse(entity.RawField1).MapError(e => (ValidationError)new ValidationError.InvalidField1(e)),
                Field2.Parse(entity.RawField2).MapError(e => (ValidationError)new ValidationError.InvalidField2(e)),
                (field1, field2) => new Entity.Validated(field1, field2));
    }
}

public static class EntityProcessing
{
    extension(Entity.Validated entity)
    {
        // Total: every Validated entity can be processed, so no Result is needed.
        public Entity.Processed Process() => new(entity.Field1, Compute(entity));
    }
}
```
> **C# 14 gotcha**: an extension member with its own generic parameter (like `MapError<TOut>`) does not resolve when you pass that type argument explicitly (`.MapError<ValidationError>(...)`). Cast the lambda's return expression to the target type instead — `.MapError(e => (ValidationError)new ValidationError.InvalidField1(e))` — and let ordinary inference pick it up.

### Workflows
Compose operations into a business process, following **impure → pure → impure** (load state, run pure logic, persist).
- A `public static Result<FinalState, WorkflowError> Publish(command, dependencies, now)` pure core, built by `Map`/`Bind`-chaining the operations above
- An instance `ExecuteAsync(command, CancellationToken)` on a class that loads what the pure core needs through injected ports, calls the pure core, persists on the success branch (`TapAsync`), and maps the result to the event
- No business logic outside the pure core — the workflow only composes
- The clock is a parameter (`DateTimeOffset now` in the pure core, `TimeProvider` injected in the impure shell) — **never `DateTime.Now`** inside domain code
- **Never catch exceptions** in the workflow: an exception here is an infrastructure failure, not a validation failure, and must propagate to the edge (API, console)

Example pattern:
```csharp
public sealed class ProcessEntityWorkflow(IEntityRepository repository, TimeProvider clock)
{
    public async Task<Result<EntityProcessedEvent, WorkflowError>> ExecuteAsync(ProcessEntityCommand command, CancellationToken ct)
    {
        IReadOnlySet<Id> known = await repository.GetKnownAsync(command.Ids, ct);
        Result<Entity.Processed, WorkflowError> processed = Publish(command, known, clock.GetUtcNow());
        return await processed.TapAsync(entity => repository.SaveAsync(entity, ct)).Map(entity => entity.ToEvent());
    }

    public static Result<Entity.Processed, WorkflowError> Publish(ProcessEntityCommand command, IReadOnlySet<Id> known, DateTimeOffset now) =>
        new Entity.Unvalidated(command.RawField1, command.RawField2)
            .Validate(known)
            .MapError(errors => (WorkflowError)new WorkflowError.Validation(errors))
            .Map(validated => validated.Process());
}
```

### Events
Represent the workflow's outcome using a plain event record — **not** a discriminated union of succeeded/failed.
- One `sealed record NounVerbedEvent` per successful outcome, past tense, only the data other contexts need
- **No failure event.** Failure is a `Result.Error` value (see "Errors"), not something that happened — only success is worth announcing to other contexts
- Produced from the final state via a `ToEvent()` extension, e.g. `extension(Entity.Processed entity) { public EntityProcessedEvent ToEvent() => new(...); }`

Example pattern:
```csharp
public sealed record EntityProcessedEvent(ProcessedData Data, DateTimeOffset ProcessedAt);
```

### Errors
Represent a workflow's failure as a **closed error type**, not an exception and not an entity state.
- `abstract record WorkflowError` (private constructor, nested `sealed record` cases) for the workflow, and a similar closed hierarchy per value object (`GradeError`, etc.) or per validation step (`ValidationError`)
- A `ToMessage()`/`ToMessages()` method on the error type renders it to text — used by the console or mapped to an HTTP `ValidationProblemDetails` at the API edge
- Exceptions are reserved for things that are actually exceptional: infrastructure failures (a dropped database connection) or corrupted data (a row in your own database that fails a smart constructor) — never for expected validation failures

## Naming Conventions

### Commands
Format: `VerbNounCommand`
- Examples: `ScheduleExamCommand`, `AllocateRoomCommand`, `PlaceOrderCommand`

### Events
Format: `NounVerbedEvent` (past tense)
- Examples: `ExamScheduledEvent`, `RoomAllocatedEvent`, `OrderPlacedEvent`

### Errors
Format: `VerbNounError` for the workflow error, `NounError` for a value object's error
- Examples: `PublishExamError`, `GradeError`, `ValidationError`

### Operations (modules)
Format: `NounVerb` module exposing verb-named functions, e.g. `ExamValidation.Validate`, `ExamCalculation.Calculate`
- Examples: `ValidateExamOperation` → `ExamValidation.Validate`, `CalculateScoreOperation` → `ScoreCalculation.Calculate`

### Value Objects
Format: Domain-specific nouns
- Clear, unambiguous domain terms; avoid generic names like "Value" or "Data"
- Examples: `StudentId`, `ExamDate`, `CourseCode`, `EmailAddress`, `PhoneNumber`

### Entity States
Format: `Entity.State` (state nested inside the entity's closed type)
- Examples: `Exam.Unvalidated`, `Exam.Validated`, `Exam.Published`

## Project Structure
```
Domain/
├── ValueObjects/
│   ├── Grade.cs
│   ├── GradeError.cs
│   └── StudentRegistrationNumber.cs
├── States/
│   ├── Exam.cs
│   ├── UnvalidatedStudentGrade.cs
│   └── ValidatedStudentGrade.cs
├── Commands/
│   └── PublishExamCommand.cs
├── Events/
│   └── ExamPublishedEvent.cs
├── Errors/
│   ├── ValidationError.cs
│   └── PublishExamError.cs
├── Operations/
│   ├── ExamValidation.cs
│   ├── ExamCalculation.cs
│   └── ExamPublishing.cs
├── Workflows/
│   └── PublishExamWorkflow.cs
└── Repositories/
    ├── IStudentsRepository.cs
    └── IGradesRepository.cs
```

## Important Rules

### Value Objects
1. **Always private constructor** — prevents direct instantiation
2. **Always implement `Create` and/or `Parse`** — returning `Result<T, TError>`, for safe construction
3. **Always immutable** — use `{ get; }` only, never `{ get; set; }`
4. **Never throw from a smart constructor** — invalid input is a `Result.Error`, not an exception
5. **Never use external dependencies in a smart constructor** — only format/structure validation; existence checks belong in an operation

### Entity States
1. **Always a closed choice type** — `abstract record` with a private constructor, nested `sealed record` states
2. **Always `IReadOnlyList`/`IReadOnlySet`** for collections — never `List<T>` or arrays
3. **Never an `InvalidEntity` state** — a failed transition is a `Result` failure, not a state
4. **Always provide `Match`** for compiler-checked exhaustive folding
5. **Never allow impossible states** — a field that cannot legitimately vary independently belongs inside the state that needs it, not as a nullable on every state

### Operations
1. **Always a pure static function** — as a C# 14 extension member on the exact state it transforms
2. **Never a base class hierarchy** — no `DomainOperation`, no `OnXxx` virtual hooks
3. **Always inject dependencies as parameters** — not as constructor-captured `Func<,>` fields
4. **Always accumulate all validation errors** — `Result.Combine`/`Traverse`, never stop at the first
5. **Never mix concerns** — keep validation, calculation, and persistence in separate operations

### Workflows
1. **Always impure → pure → impure** — load, then a pure core, then persist
2. **Always inject dependencies** through the constructor of the impure shell; the pure core takes them as parameters
3. **Always start with `Unvalidated`** — built from the command
4. **Always end with an event on success** — via `ToEvent()`
5. **Never catch exceptions** — let infrastructure failures propagate

### Events
1. **Always success only** — no failure event; failure is `Result.Error`
2. **Always produced from the final state** — via a `ToEvent()` extension
3. **Never expose domain internals** — only the data another context needs

### Errors
1. **Always a closed `abstract record` hierarchy** — one case per distinct failure reason
2. **Always render with `ToMessage()`/`ToMessages()`** — not by relying on the default `record` `ToString()`
3. **If you add a custom `ToString()` override on the base type, mark it `sealed override`** — otherwise every derived record silently synthesizes its own and your override is never called
4. **Never use an error type for infrastructure failures** — those are exceptions

## Code Quality Standards

### General
- Target `net10.0`, enable nullable reference types (`<Nullable>enable</Nullable>`)
- Treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- Use C# 14 features where they fit naturally: extension members, the `field` keyword, collection expressions, primary constructors — do not force them where a plain method/property reads better
- Follow SOLID principles; write self-documenting code with clear names

### Comments
- Use XML documentation comments for public APIs
- Explain WHY, not WHAT (code should be self-explanatory)
- Add comments for non-obvious business rules and for C# 14 gotchas future readers will hit (see the extension-member note above)

### Error Handling
- Model expected failures as `Result`, not exceptions
- Reserve exceptions for infrastructure failures and corrupted data
- Never swallow exceptions silently — a domain workflow has no `catch` at all

### Testing
- Use **xunit.v3** with plain `Assert` (no FluentAssertions — the license changed with v8)
- Value objects: test `Create`/`Parse` with valid, invalid and boundary cases
- Operations: test each transition, including that **all** validation errors are reported, not just the first
- Workflows: test the happy path and the failure path with fake ports (in-memory implementations of the repository interfaces)
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
