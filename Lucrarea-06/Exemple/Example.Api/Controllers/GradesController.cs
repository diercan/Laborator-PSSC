using Example.Api.Models;
using Examples.Domain.Models;
using Examples.Domain.Repositories;
using Examples.Domain.Workflows;
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
    private readonly IHttpClientFactory _httpClientFactory;

    public GradesController(ILogger<GradesController> logger, PublishExamWorkflow publishGradeWorkflow, IHttpClientFactory httpClientFactory)
    {
      this.logger = logger;
      this.publishGradeWorkflow = publishGradeWorkflow;
      _httpClientFactory = httpClientFactory;
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
        ExamPublishSucceededEvent @event => Ok(),
        ExamPublishFailedEvent @event => BadRequest(@event.Reasons),
        _ => throw new NotImplementedException()
      };

      return response;
    }

    private static UnvalidatedStudentGrade MapInputGradeToUnvalidatedGrade(InputGrade grade) => new(
        StudentRegistrationNumber: grade.RegistrationNumber,
        ExamGrade: grade.Exam,
        ActivityGrade: grade.Activity);
  }
}
