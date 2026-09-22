using System.Diagnostics;

namespace Examples.Functional;

/// <summary>
/// Rezultatul unei operații care poate eșua: fie <see cref="Ok"/> cu o valoare, fie <see cref="Error"/> cu o eroare
/// (în F#: <c>Result&lt;'Success, 'Failure&gt;</c>, capitolul 10 din <i>Domain Modeling Made Functional</i>).
/// Ierarhia este închisă: constructorul este privat, deci doar cele două tipuri imbricate pot deriva din ea.
/// </summary>
/// <typeparam name="TSuccess">Tipul valorii de pe ramura de succes.</typeparam>
/// <typeparam name="TFailure">Tipul erorii de pe ramura de eșec.</typeparam>
public abstract record Result<TSuccess, TFailure>
    where TSuccess : notnull
    where TFailure : notnull
{
    private Result()
    {
    }

    /// <summary>Ramura de succes.</summary>
    public sealed record Ok(TSuccess Value) : Result<TSuccess, TFailure>;

    /// <summary>Ramura de eșec.</summary>
    public sealed record Error(TFailure Value) : Result<TSuccess, TFailure>;

    /// <summary>
    /// Reduce rezultatul la o singură valoare tratând ambele ramuri. Compilatorul obligă apelantul
    /// să trateze și succesul și eșecul; este singurul loc din nucleu cu un <c>throw</c>, și acela imposibil de atins.
    /// </summary>
    public TResult Match<TResult>(Func<TSuccess, TResult> ok, Func<TFailure, TResult> error) => this switch
    {
        Ok success => ok(success.Value),
        Error failure => error(failure.Value),
        _ => throw new UnreachableException(),
    };

    /// <summary>Permite <c>return valoare;</c> într-o metodă care întoarce <see cref="Result{TSuccess, TFailure}"/>.</summary>
    public static implicit operator Result<TSuccess, TFailure>(TSuccess value) => new Ok(value);

    /// <summary>Permite <c>return eroare;</c> într-o metodă care întoarce <see cref="Result{TSuccess, TFailure}"/>.</summary>
    public static implicit operator Result<TSuccess, TFailure>(TFailure error) => new Error(error);
}

/// <summary>
/// Funcții de construcție și combinatori care nu pot fi membri de extensie (nu au un receptor).
/// </summary>
public static class Result
{
    /// <summary>Construiește explicit ramura de succes (necesar când <c>TSuccess</c> și <c>TFailure</c> coincid).</summary>
    public static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
        where TSuccess : notnull
        where TFailure : notnull
        => new Result<TSuccess, TFailure>.Ok(value);

    /// <summary>Construiește explicit ramura de eșec.</summary>
    public static Result<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure error)
        where TSuccess : notnull
        where TFailure : notnull
        => new Result<TSuccess, TFailure>.Error(error);

    /// <summary>
    /// Combină două rezultate independente în stil aplicativ: dacă ambele sunt <c>Ok</c> aplică <paramref name="project"/>,
    /// altfel întoarce <b>toate</b> erorile întâlnite (spre deosebire de <c>Bind</c>, care se oprește la prima).
    /// </summary>
    public static Result<TResult, IReadOnlyList<TFailure>> Combine<T1, T2, TFailure, TResult>(
        Result<T1, TFailure> first,
        Result<T2, TFailure> second,
        Func<T1, T2, TResult> project)
        where T1 : notnull
        where T2 : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        if (first is Result<T1, TFailure>.Ok ok1 && second is Result<T2, TFailure>.Ok ok2)
        {
            return new Result<TResult, IReadOnlyList<TFailure>>.Ok(project(ok1.Value, ok2.Value));
        }

        List<TFailure> errors = [];
        CollectError(first, errors);
        CollectError(second, errors);
        return new Result<TResult, IReadOnlyList<TFailure>>.Error(errors);
    }

    /// <summary>Varianta cu trei rezultate independente a lui <see cref="Combine{T1, T2, TFailure, TResult}"/>.</summary>
    public static Result<TResult, IReadOnlyList<TFailure>> Combine<T1, T2, T3, TFailure, TResult>(
        Result<T1, TFailure> first,
        Result<T2, TFailure> second,
        Result<T3, TFailure> third,
        Func<T1, T2, T3, TResult> project)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where TFailure : notnull
        where TResult : notnull
    {
        if (first is Result<T1, TFailure>.Ok ok1
            && second is Result<T2, TFailure>.Ok ok2
            && third is Result<T3, TFailure>.Ok ok3)
        {
            return new Result<TResult, IReadOnlyList<TFailure>>.Ok(project(ok1.Value, ok2.Value, ok3.Value));
        }

        List<TFailure> errors = [];
        CollectError(first, errors);
        CollectError(second, errors);
        CollectError(third, errors);
        return new Result<TResult, IReadOnlyList<TFailure>>.Error(errors);
    }

    private static void CollectError<T, TFailure>(Result<T, TFailure> result, List<TFailure> errors)
        where T : notnull
        where TFailure : notnull
    {
        if (result is Result<T, TFailure>.Error error)
        {
            errors.Add(error.Value);
        }
    }
}
