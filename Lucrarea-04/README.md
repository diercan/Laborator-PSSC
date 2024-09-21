# Lucrarea 4: EF Core

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: implementarea accesului la baza de date folosind EF Core

## Sarcină 1

Analizați exemplul.

## Sarcină 2

Creați o nouă bază de date SQL care va conține următoarele tabele: 
* Products (Id, Code, Name, Price, QuantityType), 
* Customers(Id, Code, Name), 
* Order(Id, Date, DeliveryAddress, CustomerId), 
* OrderItem(Id, OrderId, ProductId, Quantity)

Implementați un context EF.

Realizați un meniu în linie de comandă ce permite adăugarea de înregistrări in cele 4 tabele și afișarea conținutului fiecărei tabele.

## GitHub Copilot

### EF Core
To set up EF Core DB Context using the SQL first approach, follow these steps:

1. Define your database schema: Create the necessary tables and define their relationships. In your case, you need to create the following tables: Products, Customers, Order, and OrderItem. Define the columns and their data types for each table.

2. Install the necessary NuGet packages: Open your project in Visual Studio and install the following NuGet packages:
    - Microsoft.EntityFrameworkCore.SqlServer: This package provides the SQL Server database provider for EF Core.
    - Microsoft.EntityFrameworkCore.Design (optional): This package provides design-time services for EF Core, including migrations.

3. Create a new class for your DB Context: In your project, create a new class that derives from the DbContext class. This class will represent your database context and will provide access to the tables and their data. For example, you can name this class "ShoppingContext".

4. Define the DbSet properties: Inside the ShoppingContext class, define DbSet properties for each of your tables. This will allow you to query and manipulate the data in those tables. For example:

    ```csharp
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    ```

5. Override OnModelCreating to configure the entities, e.g., map them to SQL tables, specify primary key, etc.

    ```csharp
     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProductDto>()
            .ToTable("Product")
            .HasKey(s => s.ProductId);
    }
    ```

6. Configure the DB Context: create a new DbContextOptionsBuilder and pass it to the `ShoppingContext` when creating it. For example:

    ```csharp
    DbContextOptionsBuilder<ShoppingContext> dbContextBuilder = new DbContextOptionsBuilder<ShoppingContext>()
                                          .UseSqlServer(ConnectionString)
                                          .UseLoggerFactory(loggerFactory);

    ShoppingContext shoppingContext = new(dbContextBuilder.Options);
    ```

7. Use the DB Context in your application: Now you can use the ShoppingContext class in your application to interact with the database. You can add, update, or query data using the DbSet properties defined in the context.

Remember to replace "YourConnectionString" with the actual connection string for your SQL Server database.

For more information and advanced usage of EF Core, refer to the official documentation and other online resources.


## Referințe

[1] https://learn.microsoft.com/en-us/ef/core/
[2] https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup?view=sql-server-ver16

