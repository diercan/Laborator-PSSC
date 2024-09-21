using Microsoft.OpenApi.Models;

namespace Example.ReportGenerator
{
  public class Program
  {
    public static void Main(string[] args)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example.Api", Version = "v1" });
      });

      WebApplication app = builder.Build();

      // Configure the HTTP request pipeline.

      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();

      app.MapControllers();

      app.Run();
    }
  }
}