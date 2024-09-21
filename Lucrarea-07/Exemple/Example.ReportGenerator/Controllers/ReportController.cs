using Examples.ReportGenerator.Models;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GenerateReport([FromBody] ExamPublishedModel examPublished)
    {
      _logger.LogInformation($"Landed on GenerateReport Action {examPublished.Csv}");
      return Ok("Report generated successfully");
    }

    [HttpPost("scholarship")]
    public IActionResult ScholarshipCalculation([FromBody] ExamPublishedModel examPublished)
    {
      _logger.LogInformation($"Landed on ScholarshipCalculation Action {examPublished.Csv}");

      return Ok("Scholarship calculated successfully");
    }
  }
}