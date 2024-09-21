using Example.Api.Models;
using Example.Dto.Events;
using Example.Dto.Models;
using Example.Events;
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
    private readonly IEventSender eventSender;

    public GradesController(
      ILogger<GradesController> logger,
      PublishExamWorkflow publishGradeWorkflow,
      IEventSender eventSender)
    {
      this.logger = logger;
      this.publishGradeWorkflow = publishGradeWorkflow;
      this.eventSender = eventSender;
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
      await eventSender.SendAsync("grades", new GradesPublishedEvent()
      {
        Grades = successEvent.Grades.Select(grade => new StudentGradeDto
        {
          StudentRegistrationNumber = grade.StudentRegistrationNumber.Value,
          ActivityGrade = grade.ActivityGrade?.Value,
          ExamGrade = grade.ExamGrade?.Value,
          FinalGrade = grade.FinalGrade?.Value
        }).ToList()
      });

      return Ok();
    }

    private static UnvalidatedStudentGrade MapInputGradeToUnvalidatedGrade(InputGrade grade) => new(
        StudentRegistrationNumber: grade.RegistrationNumber,
        ExamGrade: grade.Exam,
        ActivityGrade: grade.Activity);
  }
}
