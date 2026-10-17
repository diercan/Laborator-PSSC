# Lucrarea 4: EF Core

**Context**: Coșul de cumpărături pentru un magazin virtual. 

**Obiective**: implementarea accesului la baza de date folosind EF Core 10 (abordare *SQL-first*): `DbContext`, `DbSet`, maparea tabelelor în `OnModelCreating`, adăugarea, actualizarea și interogarea înregistrărilor.

## Sarcină 1

Creați baza de date cu [SQL/create-db.sql](SQL/create-db.sql), fie pe un SQL Server local, fie în Docker (`docker compose up -d sqlserver sqlserver-init` — vedeți [compose.yaml](../compose.yaml)). Dacă folosiți Docker, setați connection string-ul o singură dată cu `dotnet user-secrets`:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Student;User Id=sa;Password=<MSSQL_SA_PASSWORD din .env>;TrustServerCertificate=true" --project Exemple/Examples.ConsoleApp
```

Analizați și rulați exemplul din directorul [Exemple](Exemple/) (`Exemple/Examples.slnx`): `dotnet run --project Exemple/Examples.ConsoleApp`. Urmăriți în consolă comenzile SQL generate de EF Core pentru căutare, adăugare/actualizare și listare.

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

2. Install the necessary NuGet packages. The examples use Central Package Management, so versions live in `Exemple/Directory.Packages.props` and the `.csproj` files contain only `<PackageReference Include="..." />`:
    - Microsoft.EntityFrameworkCore.SqlServer: This package provides the SQL Server database provider for EF Core.
    - Microsoft.Extensions.Hosting: This package provides the generic host (configuration, dependency injection, logging) used by the console application.

3. Create a new class for your DB Context: In your project, create a new class that derives from the DbContext class. This class will represent your database context and will provide access to the tables and their data. For example, you can name this class "ShoppingContext".

4. Define the DbSet properties: Inside the ShoppingContext class, define DbSet properties for each of your tables. This will allow you to query and manipulate the data in those tables. For example:

    ```csharp
    public sealed class ShoppingContext(DbContextOptions<ShoppingContext> options) : DbContext(options)
    {
        public DbSet<ProductEntity> Products => Set<ProductEntity>();
        public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
        public DbSet<OrderEntity> Orders => Set<OrderEntity>();
        public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    }
    ```

5. Override OnModelCreating to configure the entities, e.g., map them to SQL tables, specify primary key, column precision, etc.

    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(product =>
        {
            product.ToTable("Products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Price).HasPrecision(10, 2);
        });
    }
    ```

6. Configure the DB Context: register it in the generic host's service collection and read the connection string from configuration (`appsettings.json` or `dotnet user-secrets`). For example:

    ```csharp
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddDbContext<ShoppingContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.")));

    using IHost host = builder.Build();
    await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
    ShoppingContext shoppingContext = scope.ServiceProvider.GetRequiredService<ShoppingContext>();
    ```

7. Use the DB Context in your application: Now you can use the ShoppingContext class in your application to interact with the database. You can add, update, or query data using the DbSet properties defined in the context.

Remember to set `ConnectionStrings:DefaultConnection` to the actual connection string for your SQL Server database.

For more information and advanced usage of EF Core, refer to the official documentation and other online resources.


## Referințe

[1] Microsoft Documentation, [EF Core](https://learn.microsoft.com/ef/core/)

[2] Microsoft Documentation, [Rulați SQL Server în Docker](https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker)

[3] Microsoft Documentation, [Instalarea SQL Server pe Linux](https://learn.microsoft.com/sql/linux/sql-server-linux-setup)
