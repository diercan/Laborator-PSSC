# Lucrarea 3: Implementarea unui „workflow” DDD 

**Context**: coșul de cumpărături pentru un magazin virtual. 

**Obiective**: descompunerea comportamentului specific unui domeniu în operațiuni (funcții) și combinarea (compunerea) lor pentru a obține un workflow

## Sarcina 1

Analizați și rulați soluția din directorul exemple. Identificați elementele noi vis-a-vis de modul în care este scris și organizat codul sursă.

## Sarcina 2

Implementați workflow-ul pentru plasarea unei comenzi și realizați o aplicații consolă care să apeleze workflow-ul creat. 
Workflow-ul trebuie să conțină următoarele operații:
* validarea datelor de intrare si conversia la tipurile de date care reprezintă modelului DDD
* verificarea existenței produsului pe baza codului de produs
* verificarea stocului
* verificarea adresei de livrare
* calcularea prețului
* workflow-ul se va finaliza cu generarea unui eveniment care va indica fie faptul că formularul de comanda este valid și comanda a fost preluată cu succes fie faptul că formularul de comanda este invalid și comanda a fost respinsă.

## GitHub Copilot

### Domain Workflow

A domain workflow refers to the sequence of steps or activities that are involved in accomplishing a specific task or process within a particular domain or field. It outlines the logical flow of actions and decisions that need to be taken to achieve a desired outcome.

In the context of software development, a domain workflow typically represents the high-level overview of how different components, modules, or services interact with each other to fulfill a specific business requirement or user need. It helps developers understand the overall structure and logic of a system, and guides them in implementing the necessary functionality.

For example, in a web application for an e-commerce domain, the domain workflow might include steps such as user registration, product browsing, placing an order, and making a payment. Each step in the workflow may involve multiple sub-tasks and interactions with various components of the system.

By documenting and understanding the domain workflow, developers can effectively design and implement the necessary code and logic to ensure that the application functions correctly and meets the desired requirements. It serves as a blueprint for the development process and helps in identifying potential issues or bottlenecks early on.

In summary, a domain workflow provides a high-level overview of the sequence of actions and interactions required to accomplish a specific task or process within a particular domain or field, guiding developers in implementing the necessary functionality in their software applications.

### Domain Operation

A domain operation refers to a specific action or task that is performed within a domain workflow. It represents a distinct behavior or functionality that contributes to the overall process of achieving a desired outcome.

In the context of software development, a domain operation can be thought of as a single step or operation within the larger domain workflow. It encapsulates the logic and behavior required to perform a specific task or manipulate data within the domain.

For example, in the domain workflow of an e-commerce application, a domain operation could be adding an item to a shopping cart, updating the quantity of a product, or calculating the total price of an order. Each of these operations represents a specific action that contributes to the overall process of placing an order.

By identifying and defining domain operations, developers can break down complex workflows into smaller, manageable tasks. This allows for better organization and modularization of code, making it easier to understand, maintain, and test.

In summary, a domain operation is a specific action or task within a domain workflow that represents a distinct behavior or functionality. It helps in breaking down complex processes into smaller, manageable tasks, improving code organization and maintainability.

### Domain Event

A domain event is the outcome or result of a domain workflow. It represents a significant occurrence or state change within a specific domain or field. A domain workflow can generate multiple events of different types, each capturing a specific aspect of the overall process.

In the context of software development, domain events are used to communicate and propagate changes or updates across different components or services within a system. They serve as a means of decoupling and synchronizing the behavior of various parts of the system, ensuring consistency and maintaining a clear separation of concerns.

For example, in an e-commerce application, a domain event could be triggered when a new order is placed, indicating that the order has been successfully processed and needs to be fulfilled. This event can then be consumed by other components, such as inventory management or shipping services, to update their respective states and perform any necessary actions.

By capturing and propagating domain events, developers can ensure that the system remains in a consistent and up-to-date state, even when multiple components or services are involved. It allows for better scalability, flexibility, and maintainability of the overall system architecture.

In summary, a domain event represents the outcome or result of a domain workflow and can be generated by different steps or actions within that workflow. It serves as a means of communication and synchronization between different components or services, ensuring consistency and maintaining a clear separation of concerns in the system.

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

### Performing transformations on lists using LINQ

LINQ (Language-Integrated Query) is a powerful feature in C# that allows you to perform various operations on lists, including transformations. With LINQ, you can easily manipulate and transform data within a list using a declarative and concise syntax.

One of the key operations that LINQ enables is the ability to transform a list by applying a set of operations or functions to each element. This is achieved using the `Select` method, which takes a lambda function as an argument and applies it to each element in the list, returning a new list with the transformed elements.

For example, let's say we have a list of numbers:

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
```

We can use LINQ to transform this list by squaring each number:

```csharp
List<int> squaredNumbers = numbers.Select(x => x * x).ToList();
```

In this example, the lambda function `x => x * x` is applied to each element in the `numbers` list, resulting in a new list `squaredNumbers` that contains the squared values `[1, 4, 9, 16, 25]`.

LINQ also provides other transformation operations, such as `Where` for filtering elements based on a condition, `OrderBy` for sorting elements, and `GroupBy` for grouping elements based on a key. These operations can be combined and chained together to perform complex transformations on lists.

By using LINQ to perform transformations on lists, you can write more expressive and readable code, as the declarative syntax allows you to focus on the what rather than the how of the transformation. Additionally, LINQ takes care of the underlying iteration and manipulation of the list, making your code more efficient and less error-prone.

In summary, LINQ provides a powerful and expressive way to perform transformations on lists in C#. By using operations like `Select`, `Where`, `OrderBy`, and `GroupBy`, you can easily manipulate and transform data within a list, resulting in more readable and efficient code.

## LINQ Aggregate

LINQ's `Aggregate` method is a powerful tool for performing iterative calculations or aggregations on a collection of elements. It allows you to apply a specified function to each element in the collection, accumulating a result as you iterate through the elements.

The syntax for using `Aggregate` is as follows:

```csharp
collection.Aggregate(seed, (accumulator, element) => expression)
```

Here, `collection` represents the input collection, `seed` is the initial value of the accumulator, and `expression` is the function that defines the aggregation logic. The `accumulator` parameter represents the intermediate result of the aggregation, and `element` represents the current element being processed.

For example, let's say we have a list of numbers:

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
```

We can use `Aggregate` to calculate the sum of all the numbers in the list:

```csharp
int sum = numbers.Aggregate(0, (acc, num) => acc + num);
```

In this example, the `seed` value is `0`, and the `expression` `(acc, num) => acc + num` defines the addition operation. The `Aggregate` method iterates through each element in the `numbers` list, adding it to the accumulator `acc`. The final result is stored in the `sum` variable.

The `Aggregate` method can also be used to perform other types of aggregations, such as finding the maximum or minimum value, concatenating strings, or even building complex objects. The key is to provide the appropriate seed value and define the aggregation logic in the expression.

It's important to note that the `Aggregate` method throws an exception if the collection is empty. To handle this scenario, you can use the `Aggregate` method overload that takes a seed value and a result selector function, which allows you to provide a default value in case the collection is empty.

In summary, the `Aggregate` method in LINQ provides a powerful way to perform iterative calculations or aggregations on a collection of elements. By specifying a seed value and an aggregation function, you can accumulate a result as you iterate through the elements. It's a versatile tool that can be used for various types of aggregations and calculations in your code.

## Referințe

[1] Scott Wlaschin, [Domain Modeling Made Functional](https://www.amazon.com/Domain-Modeling-Made-Functional-Domain-Driven-ebook/dp/B07B44BPFB/ref=sr_1_1?dchild=1&keywords=Domain+Modeling+Made+Functional&qid=1632338254&sr=8-1), Pragmatic Bookshelf, 2018  

[2] Microsoft Documentation, [Enumerable.Aggregate Method](https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate?view=net-5.0)
