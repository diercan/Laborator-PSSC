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
        public IActionResult GenerateReport()
        {
            _logger.LogInformation("Landed on GenerateReport Action");
            return Ok("Report generated sucessfully");
        }

        [HttpPost("scholarship")]
        public IActionResult ScholarshipCalculation()
        {
            _logger.LogInformation("Landed on ScholarshipCalculation Action");

            return Ok("Sholarship calculated sucessfully");
        }
    }
}