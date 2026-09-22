namespace Examples.Functional;

/// <summary>
/// Variantele asincrone ale combinatorilor, pentru pașii impuri de la marginile workflow-ului
/// (citire/scriere în baza de date, apeluri HTTP, publicare de evenimente).
/// </summary>
public static class ResultAsyncExtensions
{
    extension<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
        where TSuccess : notnull
        where TFailure : notnull
    {
        /// <summary>Execută un efect asincron (de exemplu persistarea) pe ramura de succes; valoarea trece neschimbată.</summary>
        public async Task<Result<TSuccess, TFailure>> TapAsync(Func<TSuccess, Task> effect)
        {
            if (result is Result<TSuccess, TFailure>.Ok ok)
            {
                await effect(ok.Value);
            }

            return result;
        }

        /// <summary><see cref="Result{TSuccess, TFailure}.Match{TResult}"/> cu ramuri asincrone.</summary>
        public Task<TResult> MatchAsync<TResult>(Func<TSuccess, Task<TResult>> ok, Func<TFailure, Task<TResult>> error)
            => result.Match(ok, error);
    }

    extension<TSuccess, TFailure>(Task<Result<TSuccess, TFailure>> resultTask)
        where TSuccess : notnull
        where TFailure : notnull
    {
        /// <summary><c>Map</c> aplicat unui rezultat care încă se calculează.</summary>
        public async Task<Result<TOut, TFailure>> Map<TOut>(Func<TSuccess, TOut> map)
            where TOut : notnull
            => (await resultTask).Map(map);

        /// <summary><c>Bind</c> cu un pas următor asincron.</summary>
        public async Task<Result<TOut, TFailure>> Bind<TOut>(Func<TSuccess, Task<Result<TOut, TFailure>>> bind)
            where TOut : notnull
        {
            Result<TSuccess, TFailure> result = await resultTask;
            return await result.Match(
                bind,
                error => Task.FromResult<Result<TOut, TFailure>>(new Result<TOut, TFailure>.Error(error)));
        }

        /// <summary><c>Match</c> aplicat unui rezultat care încă se calculează.</summary>
        public async Task<TResult> Match<TResult>(Func<TSuccess, TResult> ok, Func<TFailure, TResult> error)
            => (await resultTask).Match(ok, error);
    }
}
