using Example.Data;
using Example.Data.Repositories;
using Examples.Domain.Models;
using Examples.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Examples.Domain.WorkflowEvents.ExamGradesPublishedEvent;

namespace Examples
{
  internal class Program
  {
    private static readonly string ConnectionString = "Server=.;Database=Student;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";

    private static async Task Main(string[] args)
    {
      //setup dependencies, e.g., logger and repositories
      using ILoggerFactory loggerFactory = ConfigureLoggerFactory();
      ILogger<PublishGradeWorkflow> logger = loggerFactory.CreateLogger<PublishGradeWorkflow>();
      DbContextOptionsBuilder<GradesContext> dbContextBuilder = new DbContextOptionsBuilder<GradesContext>()
                                                .UseSqlServer(ConnectionString)
                                                .UseLoggerFactory(loggerFactory);
      GradesContext gradesContext = new(dbContextBuilder.Options);
      StudentsRepository studentsRepository = new(gradesContext);
      GradesRepository gradesRepository = new(gradesContext);

      //get user input
      UnvalidatedStudentGrade[] listOfGrades = ReadListOfGrades().ToArray();

      //execute domain workflow
      PublishGradesCommand command = new(listOfGrades);
      PublishGradeWorkflow workflow = new(studentsRepository, gradesRepository, logger);
      IExamGradesPublishedEvent result = await workflow.ExecuteAsync(command);

      string consoleMessage = result switch
      {
        ExamGradesPublishSucceededEvent @event => @event.Csv,
        ExamGradesPublishFailedEvent @event => $"Publish failed: {@event.Reason}",
        _ => throw new NotImplementedException()
      };

      Console.WriteLine(consoleMessage);
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

    private static List<UnvalidatedStudentGrade> ReadListOfGrades()
    {
      List<UnvalidatedStudentGrade> listOfGrades = [];
      do
      {
        //read registration number and grade and create a list of grades
        string? registrationNumber = ReadValue("Registration Number: ");
        if (string.IsNullOrEmpty(registrationNumber))
        {
          break;
        }

        string? examGrade = ReadValue("Exam Grade: ");
        if (string.IsNullOrEmpty(examGrade))
        {
          break;
        }

        string? activityGrade = ReadValue("Activity Grade: ");
        if (string.IsNullOrEmpty(activityGrade))
        {
          break;
        }

        listOfGrades.Add(new(registrationNumber, examGrade, activityGrade));
      } while (true);
      return listOfGrades;
    }

    private static string? ReadValue(string prompt)
    {
      Console.Write(prompt);
      return Console.ReadLine();
    }

  }
}
