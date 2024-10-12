using Examples.ReportGenerator.Models;
using System.Text;
using System.Text.Json;

namespace Examples.Api.Clients
{
  public class ReportApiClient(HttpClient httpClient)
  {
    public async Task<string> GenerateReportAsync(ExamPublishedModel examPublished)
    {
      StringContent content = new(JsonSerializer.Serialize(examPublished), Encoding.UTF8, "application/json");
      HttpResponseMessage response = await httpClient.PostAsync("report/semester-report", content);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> CalculateScholarshipAsync(ExamPublishedModel examPublished)
    {
      StringContent content = new(JsonSerializer.Serialize(examPublished), Encoding.UTF8, "application/json");
      HttpResponseMessage response = await httpClient.PostAsync("report/scholarship", content);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStringAsync();
    }
  }
}
