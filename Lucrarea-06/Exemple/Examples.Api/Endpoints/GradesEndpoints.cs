using Examples.Api.Mapping;
using Examples.Api.Models;
using Examples.Data.Queries;
using Examples.Domain.Commands;
using Examples.Domain.Events;
using Examples.Domain.Workflows;
using Examples.Functional;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.Api.Endpoints;

/// <summary>Punctele de intrare HTTP pentru catalogul de note.</summary>
public static class GradesEndpoints
{
    public static IEndpointRouteBuilder MapGrades(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/grades").WithTags("Grades");

        group.MapPost("/", PublishAsync)
            .WithName("PublishGrades")
            .WithSummary("Publică notele unui examen.");

        group.MapGet("/", GetAllAsync)
            .WithName("GetGrades")
            .WithSummary("Catalogul curent (model de citire).");

        return app;
    }

    private static async Task<Results<Ok<PublishGradesResponse>, ValidationProblem>> PublishAsync(
        IReadOnlyList<InputGrade> grades,
        PublishExamWorkflow workflow,
        CancellationToken cancellationToken)
    {
        PublishExamCommand command = new([.. grades.Select(grade => grade.ToUnvalidated())]);
        Result<ExamPublishedEvent, Examples.Domain.Errors.PublishExamError> result =
            await workflow.ExecuteAsync(command, cancellationToken);

        return result.Match<Results<Ok<PublishGradesResponse>, ValidationProblem>>(
            published => TypedResults.Ok(new PublishGradesResponse(published.Csv, published.PublishedAt)),
            error => error.ToValidationProblem());
    }

    private static async Task<Ok<IReadOnlyList<StudentGradeRow>>> GetAllAsync(IGradesQuery query, CancellationToken cancellationToken) =>
        TypedResults.Ok(await query.GetAllAsync(cancellationToken));
}

/// <summary>Răspunsul la publicarea cu succes a unui catalog de note.</summary>
public sealed record PublishGradesResponse(string Csv, DateTimeOffset PublishedAt);
