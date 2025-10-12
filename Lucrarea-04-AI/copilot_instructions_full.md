# GitHub Copilot Instructions for DDD Project

## Code Style and Patterns

### Value Objects
Use `record` types with immutability and validation:
- Private constructor with validation
- Static `TryParse` method for safe parsing
- Validation logic in private static `IsValid` method
- Override `ToString()` for serialization
- Throw domain-specific exceptions

Example pattern:
```csharp
public record ValueObjectName
{
    private static readonly Regex ValidPattern = new("pattern");
    public TypeName Value { get; }
    
    private ValueObjectName(TypeName value)
    {
        if (IsValid(value))
            Value = value;
        else
            throw new InvalidValueObjectNameException($"message");
    }
    
    private static bool IsValid(TypeName value) => /* validation */;
    
    public static bool TryParse(string input, out ValueObjectName? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (!IsValid(input)) return false;
        try { result = new(input); return true; }
        catch { return false; }
    }
    
    public override string ToString() => Value.ToString();
}
```

### Entity States
Model entity lifecycle as separate immutable states:
- Interface `IEntityName` as base type
- Each state = separate record with `internal` constructor
- Use `IReadOnlyCollection<T>` for collections
- Group all states in static class `EntityName`
- Include `InvalidEntity` state with `IEnumerable<string> Reasons`

Example pattern:
```csharp
public static class EntityName
{
    public interface IEntityName { }
    
    public record UnvalidatedEntity(string Field1, string Field2, ...) : IEntityName;
    
    public record ValidatedEntity : IEntityName
    {
        internal ValidatedEntity(ValueObject1 field1, ValueObject2 field2, ...)
        {
            Field1 = field1;
            Field2 = field2;
        }
        public ValueObject1 Field1 { get; }
        public ValueObject2 Field2 { get; }
    }
    
    public record ProcessedEntity : IEntityName
    {
        internal ProcessedEntity(ValueObject1 field1, ProcessedData data)
        {
            Field1 = field1;
            Data = data;
        }
        public ValueObject1 Field1 { get; }
        public ProcessedData Data { get; }
    }
    
    public record InvalidEntity : IEntityName
    {
        internal InvalidEntity(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        public IEnumerable<string> Reasons { get; }
    }
}
```

### Domain Operations
Transform entities through their lifecycle:
- Inherit from `DomainOperation<TEntity, TState, TResult>` or `EntityOperation`
- Use pattern matching in `Transform` method to handle all states
- Override only relevant `OnXxx` methods (OnUnvalidated, OnValidated, etc.)
- Default implementation returns same entity (identity function)
- Inject dependencies via constructor as `Func<Input, Output>`

Example pattern:
```csharp
// Specific operation implementation
internal sealed class ProcessEntityOperation : EntityOperation
{
    private readonly Func<ValueObject, bool> checkSomething;
    
    internal ProcessEntityOperation(Func<ValueObject, bool> checkSomething)
    {
        this.checkSomething = checkSomething;
    }
    
    protected override IEntity OnValidated(ValidatedEntity entity)
    {
        // Business logic here
        // Transform ValidatedEntity to ProcessedEntity or InvalidEntity
        
        if (!checkSomething(entity.Field1))
        {
            return new InvalidEntity(new[] { "Validation failed for Field1" });
        }
        
        return new ProcessedEntity(entity.Field1, /* computed data */);
    }
}

// Base operation class handles pattern matching
public abstract class EntityOperation : EntityOperation<object>
{
    internal IEntity Transform(IEntity entity) => Transform(entity, null);
    
    public override IEntity Transform(IEntity entity, object? state) => entity switch
    {
        UnvalidatedEntity unvalidated => OnUnvalidated(unvalidated),
        ValidatedEntity validated => OnValidated(validated),
        ProcessedEntity processed => OnProcessed(processed),
        InvalidEntity invalid => OnInvalid(invalid),
        _ => throw new InvalidEntityStateException(entity.GetType().Name)
    };
    
    protected virtual IEntity OnUnvalidated(UnvalidatedEntity entity) => entity;
    protected virtual IEntity OnValidated(ValidatedEntity entity) => entity;
    protected virtual IEntity OnProcessed(ProcessedEntity entity) => entity;
    protected virtual IEntity OnInvalid(InvalidEntity entity) => entity;
}

// Generic base for all operations
public abstract class DomainOperation<TEntity, TState, TResult>
    where TEntity : notnull
    where TState : class
{
    public abstract TResult Transform(TEntity entity, TState? state);
}
```

### Workflows
Compose operations into business process pipelines:
- Public `Execute` method that takes a command and dependencies
- Create initial unvalidated entity from command
- Chain operations: `result = operation.Transform(result)`
- No business logic in workflow (only composition)
- Convert final state to event using pattern matching
- Return event (success or failure)

Example pattern:
```csharp
public class ProcessEntityWorkflow
{
    public IEntityProcessedEvent Execute(
        ProcessEntityCommand command,
        Func<ValueObject, bool> dependency1,
        Func<ValueObject, ResultType> dependency2)
    {
        // Step 1: Create unvalidated entity from command
        var unvalidated = new UnvalidatedEntity(
            command.Field1,
            command.Field2,
            command.Field3
        );
        
        // Step 2: Chain operations to transform entity through states
        IEntity result = new ValidateEntityOperation(dependency1).Transform(unvalidated);
        result = new ProcessEntityOperation(dependency2).Transform(result);
        result = new FinalizeEntityOperation().Transform(result);
        
        // Step 3: Convert final state to event
        return result.ToEvent();
    }
}
```

### Events
Represent outcomes using discriminated unions:
- Interface `IEntityActionEvent` as base type
- Success event: `EntityActionSucceededEvent` with relevant data
- Failure event: `EntityActionFailedEvent` with `IEnumerable<string> Reasons`
- Extension method `ToEvent()` for conversion using pattern matching

Example pattern:
```csharp
public static class EntityProcessedEvent
{
    public interface IEntityProcessedEvent { }
    
    public record EntityProcessSucceededEvent : IEntityProcessedEvent
    {
        internal EntityProcessSucceededEvent(ResultData data, DateTime timestamp)
        {
            Data = data;
            Timestamp = timestamp;
        }
        
        public ResultData Data { get; }
        public DateTime Timestamp { get; }
    }
    
    public record EntityProcessFailedEvent : IEntityProcessedEvent
    {
        internal EntityProcessFailedEvent(IEnumerable<string> reasons)
        {
            Reasons = reasons;
        }
        
        public IEnumerable<string> Reasons { get; }
    }
    
    // Extension method for conversion
    public static IEntityProcessedEvent ToEvent(this IEntity entity) => entity switch
    {
        ProcessedEntity processed => new EntityProcessSucceededEvent(
            processed.Data, 
            DateTime.Now),
        InvalidEntity invalid => new EntityProcessFailedEvent(invalid.Reasons),
        UnvalidatedEntity _ => new EntityProcessFailedEvent(
            new[] { "Unexpected unvalidated state" }),
        ValidatedEntity _ => new EntityProcessFailedEvent(
            new[] { "Unexpected validated state" }),
        _ => new EntityProcessFailedEvent(
            new[] { $"Unexpected state: {entity.GetType().Name}" })
    };
}
```

## Naming Conventions

### Commands
Format: `VerbNounCommand`
- Use imperative verb (what to do)
- Singular noun for entity
- Examples: `ScheduleExamCommand`, `AllocateRoomCommand`, `PlaceOrderCommand`

### Events
Format: `NounVerbedEvent` (past tense)
- Use past participle (what happened)
- Singular noun for entity
- Examples: `ExamScheduledEvent`, `RoomAllocatedEvent`, `OrderPlacedEvent`

### Operations
Format: `VerbEntityOperation`
- Use imperative verb describing the transformation
- Entity name in singular
- Examples: `ValidateExamOperation`, `CalculateScoreOperation`, `PublishResultsOperation`

### Value Objects
Format: Domain-specific nouns
- Clear, unambiguous domain terms
- Avoid generic names like "Value" or "Data"
- Examples: `StudentId`, `ExamDate`, `CourseCode`, `EmailAddress`, `PhoneNumber`

### Entity States
Format: `StateEntity`
- State as adjective or descriptor
- Entity name in singular
- Examples: `UnvalidatedExam`, `ValidatedExam`, `PublishedExam`, `InvalidExam`

## Project Structure
```
Domain/
├── Models/
│   ├── Commands/
│   │   ├── ScheduleExamCommand.cs
│   │   └── AllocateRoomCommand.cs
│   ├── Events/
│   │   ├── ExamScheduledEvent.cs
│   │   └── RoomAllocatedEvent.cs
│   ├── ValueObjects/
│   │   ├── StudentId.cs
│   │   ├── CourseCode.cs
│   │   └── ExamDate.cs
│   └── Entities/
│       ├── ExamScheduling.cs
│       └── DormitoryApplication.cs
├── Operations/
│   ├── DomainOperation.cs
│   ├── ExamSchedulingOperation.cs
│   ├── ValidateExamSchedulingOperation.cs
│   └── AllocateRoomOperation.cs
├── Workflows/
│   ├── ScheduleExamWorkflow.cs
│   └── AllocateDormitoryWorkflow.cs
└── Exceptions/
    ├── DomainException.cs
    ├── InvalidExamDateException.cs
    └── InvalidCourseCodeException.cs
```

## Important Rules

### Value Objects
1. **Always private constructor** - prevents direct instantiation
2. **Always implement TryParse** - for safe parsing from strings
3. **Always immutable** - use `{ get; }` only, never `{ get; set; }`
4. **Always validate in constructor** - throw specific domain exception on invalid input
5. **Never use external dependencies in constructor** - only format/structure validation

### Entity States
1. **Always use interface** - `IEntityName` as base type
2. **Always use internal constructors** - control instantiation from outside domain
3. **Always use IReadOnlyCollection** - for collections, never `List<T>` or arrays
4. **Always include InvalidEntity** - state with `IEnumerable<string> Reasons`
5. **Never allow impossible states** - use separate records for each valid state

### Operations
1. **Always inherit from base** - `EntityOperation` or `DomainOperation<T,S,R>`
2. **Always use pattern matching** - handle all entity states in Transform
3. **Always inject dependencies** - via constructor as `Func<Input, Output>`
4. **Always single responsibility** - one operation does one transformation
5. **Never mix concerns** - keep validation, business logic, and persistence separate

### Workflows
1. **Always only composition** - chain operations, no business logic
2. **Always inject dependencies** - pass them to Execute method
3. **Always start with Unvalidated** - create from command
4. **Always end with Event** - convert final state using ToEvent()
5. **Never catch exceptions** - let them propagate

### Events
1. **Always success and failure** - two event types minimum
2. **Always internal constructors** - control creation
3. **Always use extension method** - ToEvent() for conversion
4. **Always pattern match all states** - in ToEvent() method
5. **Never expose domain internals** - only necessary data in events

## Code Quality Standards

### General
- Enable nullable reference types: `<Nullable>enable</Nullable>`
- Treat warnings as errors: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Use C# 12 features (record types, pattern matching, init properties)
- Follow SOLID principles
- Write self-documenting code with clear names

### Comments
- Use XML documentation comments for public APIs
- Explain WHY, not WHAT (code should be self-explanatory)
- Add comments for complex business rules
- Document assumptions and constraints

### Error Handling
- Use specific domain exceptions, not generic ones
- Include meaningful error messages with context
- Never swallow exceptions silently
- Validate at boundaries (value object constructors, operation inputs)

### Testing
- Value objects: test TryParse with valid/invalid/edge cases
- Operations: test each state transition
- Workflows: test happy path and failure scenarios
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
