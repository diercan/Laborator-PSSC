using Examples.Functional;

namespace Examples.Domain.Tests;

/// <summary>Ajutoare pentru a extrage direct valoarea de succes sau de eroare dintr-un <see cref="Result{TSuccess, TFailure}"/> în teste.</summary>
internal static class ResultAssert
{
    public static TSuccess Ok<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
        where TSuccess : notnull
        where TFailure : notnull
        => result switch
        {
            Result<TSuccess, TFailure>.Ok ok => ok.Value,
            Result<TSuccess, TFailure>.Error error => throw new Xunit.Sdk.XunitException($"Expected Ok, got Error: {error.Value}"),
            _ => throw new Xunit.Sdk.XunitException("Unreachable result case."),
        };

    public static TFailure Error<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
        where TSuccess : notnull
        where TFailure : notnull
        => result switch
        {
            Result<TSuccess, TFailure>.Error error => error.Value,
            Result<TSuccess, TFailure>.Ok ok => throw new Xunit.Sdk.XunitException($"Expected Error, got Ok: {ok.Value}"),
            _ => throw new Xunit.Sdk.XunitException("Unreachable result case."),
        };
}
