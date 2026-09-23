# [Numele Proiectului] - DDD Lab

## Echipa
- [Nume Student 1]
- [Nume Student 2]
- [Nume Student 3]

## Domeniul Ales
[Numele domeniului: Exam Scheduling / Dormitory Allocation / Study Space Reservation]

## Descriere
[Scurtă descriere a sistemului implementat]

## Bounded Contexts Identificate
1. **[Context 1]**: [Responsabilități]
2. **[Context 2]**: [Responsabilități]

## Event Storming Results
[Link la diagram sau imagine]

## Implementare

### Value Objects
- `[ValueObject1]`: [Descriere]
- `[ValueObject2]`: [Descriere]

### Entity States
- `[Entity].Unvalidated`: [When]
- `[Entity].Validated`: [When]
- ...

### Erori
- `[Action][Entity]Error`: [cazurile din ierarhia închisă și ce reprezintă fiecare]

### Operații (funcții)
1. `[Entity]Validation.Validate`: [Ce face]
2. `[Entity][Verb].[Verb]`: [Ce face]

### Workflow
`[Action][Entity]Workflow.Publish`: [Descriere pipeline]

## Rulare

```bash
# Compile
dotnet build

# Run console app
dotnet run --project src/LabDDD.ConsoleApp

# Run tests
dotnet test
```

## Lecții Învățate

### Ce a funcționat bine cu AI
- [Punct 1]
- [Punct 2]

### Limitări ale AI identificate
- [Limitare 1]
- [Limitare 2]

### Prompturi Utile
```
[Prompt 1 care a generat cod bun]
```

## Decizii de arhitectură
[Link la docs/adr/ – câte un ADR (Architecture Decision Record) pentru fiecare decizie importantă: context, opțiuni considerate, decizie, consecințe. Folosiți ca model ADR-urile laboratoarelor, de exemplu [ADR-0002](../Lucrarea-03/docs/adr/0002-operatii-pure-compuse-in-workflow.md)]