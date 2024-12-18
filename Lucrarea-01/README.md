# Lucrarea 1: Familiarizarea cu mediul de lucru Rider și dezvoltarea aplicațiilor consolă in C#.

## Obiective

* tipuri de date imutabilă folosind construcții `record`
* potrivirea tiparului folosind `switch expressions`

## Sarcină

Creați o aplicație consolă care prin intermediul unui meniu în linie de comandă să permită utilizatorului crearea unui coș de cumpărături gol, adăugarea de produse în coș, eliminarea de produse și afișarea conținutului coșului.

Pentru a modela coșul de cumpărături se va folosi tipul de date `record`. Pentru a reprezenta cantitatea se va defini o interfață `IQuantity`, ce va avea implementările `UnitQuantity` și `KilogramQuantity`. Coșul de cumpărături va implementa o metodă care va calcula prețul total. Pentru calcularea prețului în funcție de cantitate se va folosi *switch expressions*.

## GitHub CoPilot

### Record type

In C#, a record type is a lightweight alternative to a class that is primarily used for immutable data. It provides a concise syntax for defining types with read-only properties and automatically generates useful methods such as equality checks and hash code calculations.

To define a record type with init-only properties, you can use the `record` keyword followed by the type name and a list of property declarations. Here's an example:

```csharp
public record Person
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Age { get; init; }
}
```

In the above example, the `Person` record type has three init-only properties: `FirstName`, `LastName`, and `Age`. The `init` keyword ensures that these properties can only be set during object initialization and cannot be modified afterwards.

To create an instance of a record type, you can use the object initializer syntax:

```csharp
var person = new Person
{
    FirstName = "John",
    LastName = "Doe",
    Age = 30
};
```

Once created, the properties of a record type cannot be modified directly. Instead, you can create a new instance of the record type with updated values using the `with` keyword:

```csharp
var updatedPerson = person with { Age = 31 };
```

The `with` keyword creates a new instance of the record type with the specified property updated, while keeping the other properties unchanged.

Record types provide a convenient way to work with immutable data in C#, making it easier to reason about and preventing accidental modifications.


### C# Switch Expression

The C# switch expression is a powerful feature introduced in C# 8.0 that allows for concise and expressive pattern matching. It provides a compact syntax for performing different actions based on the value of an expression.

One interesting use case of the switch expression is performing pattern matching for types. Here's an example:

```csharp
public static string GetVariableType(object variable)
{
    return variable switch
    {
        int i => $"Variable {i} is of type Integer",
        string s => $"Variable {s} is of type String",
        float f => $"Variable {f} is of type Float",
        _ => "Unknown"
    };
}
```

In the above example, the switch expression matches the value of the variable parameter and returns a string representation that includes the variable value and its type. If the variable is of type int, it returns a string like "Variable 42 is of type Integer". If the variable is of type string, it returns a string like "Variable hello is of type String". If the variable is of type float, it returns a string like "Variable 3.14 is of type Float". For any other type, it returns "Unknown".

This allows you to easily include variable names in the switch expression cases and provide more meaningful output based on the variable value. This concise syntax makes it easy to handle different types in a clean and readable way.

### Lambda Functions in C#

Lambda functions, also known as anonymous functions, are a powerful feature in C# that allow you to define and use functions without explicitly declaring a separate method. They provide a concise and flexible way to write code, especially when working with delegates or functional programming concepts.

The syntax for a lambda function in C# is as follows:

```csharp
(parameters) => expression
```

Here, `parameters` represents the input parameters of the function, and `expression` represents the code to be executed. The lambda function can have zero or more parameters, and the expression can be a single statement or a block of statements enclosed in curly braces.

Lambda functions are commonly used in scenarios where you need to pass a function as an argument to another method or define a short and simple function inline. They are particularly useful when working with LINQ queries, event handlers, and functional programming constructs like `Func` and `Action` delegates.

For example, consider the following code snippet that uses a lambda function to filter a list of numbers:

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

List<int> evenNumbers = numbers.Where(x => x % 2 == 0).ToList();
```

In this example, the lambda function `x => x % 2 == 0` is used as a predicate to filter out the even numbers from the `numbers` list. The resulting `evenNumbers` list will contain only the even numbers `[2, 4]`.

Lambda functions can also capture variables from their surrounding scope, making them useful for creating closures. This allows you to access and modify variables outside the lambda function's body.

In summary, lambda functions in C# provide a concise and flexible way to define and use functions without explicitly declaring separate methods. They are commonly used in scenarios where you need to pass functions as arguments or define short and simple functions inline. Lambda functions are a powerful tool in C# that can greatly enhance your code readability and maintainability.

## Referințe

[1] https://www.jetbrains.com/rider/

[2] https://learn.microsoft.com/en-us/training/paths/get-started-c-sharp-part-1/

[3] https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-console?view=vs-2022

[4] https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression

