using Examples.Data.Queries;
using Examples.Data.Repositories;
using Examples.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Data;

/// <summary>Înregistrează <see cref="GradesContext"/> și adaptoarele care implementează porturile domeniului.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGradesData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GradesContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IStudentsRepository, StudentsRepository>();
        services.AddScoped<IGradesRepository, GradesRepository>();
        services.AddScoped<IGradesQuery, GradesQuery>();
        return services;
    }
}
