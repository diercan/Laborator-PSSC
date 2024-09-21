using Example.Data;
using Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Examples
{
  internal class Program
  {
    private static readonly string ConnectionString = "Server=.;Database=Student;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";

    private static async Task Main(string[] args)
    {
      //setup dependencies, e.g., logger and repositories
      using ILoggerFactory loggerFactory = ConfigureLoggerFactory();
      DbContextOptionsBuilder<GradesContext> dbContextBuilder = new DbContextOptionsBuilder<GradesContext>()
                                                .UseSqlServer(ConnectionString)
                                                .UseLoggerFactory(loggerFactory);
      GradesContext gradesContext = new(dbContextBuilder.Options);

      StudentDto? foundStudent = await gradesContext.Students
        .Where(s => s.RegistrationNumber == "LM34567")
        .FirstOrDefaultAsync();

      if (foundStudent is null)
      {
        //add
        StudentDto student = new()
        {
          RegistrationNumber = "LM34567",
          Name = "John Doe"
        };
        gradesContext.Add(student);
      }
      else
      {
        //update
        foundStudent.Name = "John Doe";
      }

      await gradesContext.SaveChangesAsync();

      //list
      List<StudentDto> allStudents = await gradesContext.Students.ToListAsync();

      Console.WriteLine();
      Console.WriteLine("============================");
      Console.WriteLine("All students:");
      Console.WriteLine("============================");
      allStudents.ForEach(s => System.Console.WriteLine($"{s.RegistrationNumber} {s.Name}"));
    }
    private static ILoggerFactory ConfigureLoggerFactory()
    {
      return LoggerFactory.Create(builder =>
                          builder.AddSimpleConsole(options =>
                          {
                            options.IncludeScopes = true;
                            options.SingleLine = true;
                            options.TimestampFormat = "hh:mm:ss ";
                          })
                          .AddProvider(new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()));
    }
  }
}
