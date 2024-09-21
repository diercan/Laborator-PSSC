using Microsoft.AspNetCore.Mvc;
using static Examples.Domain.Models.ExamPublishedEvent;

namespace Example.ReportGenerator.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ReportController : ControllerBase
  {
    private readonly ILogger<ReportController> _logger;

    public ReportController(ILogger<ReportController> logger)
    {
      _logger = logger;
    }

    [HttpPost("semester-report")]
    public IActionResult GenerateReport([FromBody] ExamPublishSucceededEvent examPublished)
    {
      _logger.LogInformation($"Landed on GenerateReport Action {examPublished.Csv}");
      return Ok("Report generated successfully");
    }

    [HttpPost("scholarship")]
    public IActionResult ScholarshipCalculation([FromBody] ExamPublishSucceededEvent examPublished)
    {
      _logger.LogInformation($"Landed on ScholarshipCalculation Action {examPublished.Csv}");

      return Ok("Scholarship calculated successfully");
    }
  }
}