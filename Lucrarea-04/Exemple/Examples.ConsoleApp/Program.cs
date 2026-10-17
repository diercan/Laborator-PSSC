using Examples.Data;
using Examples.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Namespace-ul proiectului este Examples.ConsoleApp (nu Examples.Console): în interiorul unui namespace
// numit Console, identificatorul Console ar fi ambiguu cu System.Console.

// Consola Windows nu pornește implicit pe UTF-8: fără asta, diacriticele (ă, â, î, ș, ț) s-ar afișa greșit.
System.Console.OutputEncoding = System.Text.Encoding.UTF8;

// Host-ul generic configurează logging-ul (consolă + debug) și citește appsettings.json / dotnet user-secrets.
// Comenzile SQL generate de EF Core apar în consolă (vezi categoria Microsoft.EntityFrameworkCore.Database.Command).
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<GradesContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Lipsește ConnectionStrings:DefaultConnection (appsettings.json sau dotnet user-secrets).")));

using IHost host = builder.Build();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
GradesContext gradesContext = scope.ServiceProvider.GetRequiredService<GradesContext>();

StudentEntity? foundStudent = await gradesContext.Students
    .FirstOrDefaultAsync(s => s.RegistrationNumber == "LM34567");

if (foundStudent is null)
{
    // adăugare
    gradesContext.Students.Add(new StudentEntity
    {
        RegistrationNumber = "LM34567",
        Name = "John Doe",
    });
}
else
{
    // actualizare: entitatea este urmărită de context, SaveChangesAsync generează UPDATE
    foundStudent.Name = "John Doe";
}

await gradesContext.SaveChangesAsync();

// listare
List<StudentEntity> allStudents = await gradesContext.Students
    .AsNoTracking()
    .OrderBy(s => s.RegistrationNumber)
    .ToListAsync();

System.Console.WriteLine();
System.Console.WriteLine("============================");
System.Console.WriteLine("Toți studenții:");
System.Console.WriteLine("============================");
foreach (StudentEntity student in allStudents)
{
    System.Console.WriteLine($"{student.RegistrationNumber} {student.Name}");
}
