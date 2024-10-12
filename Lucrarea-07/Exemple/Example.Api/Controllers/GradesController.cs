using Example.Api.Models;
using Examples.Api.Clients;
using Examples.Domain.Models;
using Examples.Domain.Repositories;
using Examples.Domain.Workflows;
using Examples.ReportGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using static Examples.Domain.Models.ExamPublishedEvent;

namespace Example.Api.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class GradesController : ControllerBase
  {
    private readonly ILogger<GradesController> logger;
    private readonly PublishExamWorkflow publishGradeWorkflow;
    private readonly ReportApiClient _reportApiClient;

    public GradesController(ILogger<GradesController> logger, PublishExamWorkflow publishGradeWorkflow, ReportApiClient reportApiClient)
    {
      this.logger = logger;
      this.publishGradeWorkflow = publishGradeWorkflow;
      _reportApiClient = reportApiClient;
    }

    [HttpGet("getAllGrades")]
    public async Task<IActionResult> GetAllGrades([FromServices] IGradesRepository gradesRepository)
    {
      List<CalculatedStudentGrade> grades = await gradesRepository.GetExistingGradesAsync();
      var result = grades.Select(grade => new
      {
        StudentRegistrationNumber = grade.StudentRegistrationNumber.Value,
        ExamGrade = grade.ExamGrade?.Value,
        ActivityGrade = grade.ActivityGrade?.Value,
        FinalGrade = grade.FinalGrade?.Value
      });
      return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PublishGrades([FromBody] InputGrade[] grades)
    {
      ReadOnlyCollection<UnvalidatedStudentGrade> unvalidatedGrades = grades
        .Select(MapInputGradeToUnvalidatedGrade)
        .ToList()
        .AsReadOnly();
      PublishExamCommand command = new(unvalidatedGrades);
      IExamPublishedEvent workflowResult = await publishGradeWorkflow.ExecuteAsync(command);

      IActionResult response = workflowResult switch
      {
        ExamPublishSucceededEvent @event => await PublishEvent(@event),
        ExamPublishFailedEvent @event => BadRequest(@event.Reasons),
        _ => throw new NotImplementedException()
      };

      return response;
    }

    private async Task<IActionResult> PublishEvent(ExamPublishSucceededEvent successEvent)
    {
      ExamPublishedModel dto = new()
      {
        Csv = successEvent.Csv,
        PublishedDate = successEvent.PublishedDate
      };

      Task w1 = _reportApiClient.GenerateReportAsync(dto);
      Task w2 = _reportApiClient.CalculateScholarshipAsync(dto);
      await Task.WhenAll(w1, w2);
      return Ok();
    }

    private static UnvalidatedStudentGrade MapInputGradeToUnvalidatedGrade(InputGrade grade) => new(
        StudentRegistrationNumber: grade.RegistrationNumber,
        ExamGrade: grade.Exam,
        ActivityGrade: grade.Activity);
  }
}
