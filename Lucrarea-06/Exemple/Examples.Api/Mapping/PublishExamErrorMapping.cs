using Examples.Domain.Errors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.Api.Mapping;

/// <summary>Conversia unui eșec al workflow-ului de domeniu într-un răspuns HTTP 400 (RFC 9457).</summary>
public static class PublishExamErrorMapping
{
    extension(PublishExamError error)
    {
        public ValidationProblem ToValidationProblem() => error switch
        {
            PublishExamError.Validation validation => TypedResults.ValidationProblem(
                validation.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(group => group.Key, group => group.Select(e => e.ToMessage()).ToArray()),
                title: "Catalogul nu a putut fi publicat."),
            _ => throw new System.Diagnostics.UnreachableException(),
        };
    }
}
