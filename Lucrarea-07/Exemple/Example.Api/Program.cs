using Example.Data;
using Example.Data.Repositories;
using Example.Events;
using Exemple.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Example.Events.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;
using Exemple.Domain.Workflows;

namespace Example.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<GradesContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging();
            });

            builder.Services.AddTransient<IGradesRepository, GradesRepository>();
            builder.Services.AddTransient<IStudentsRepository, StudentsRepository>();
            builder.Services.AddTransient<PublishGradeWorkflow>();
            builder.Services.AddSingleton<IEventSender, ServiceBusTopicEventSender>();
            
            builder.Services.AddAzureClients(client =>
            {
                client.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example.Api", Version = "v1" });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Example.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}