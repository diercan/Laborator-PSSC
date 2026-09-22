namespace Examples.Functional;

/// <summary>
/// Combinatorii <i>railway-oriented programming</i> pentru <see cref="Result{TSuccess, TFailure}"/>,
/// scriși ca membri de extensie C# 14 (<c>extension(...)</c>): tipul rămâne o simplă structură de date,
/// iar funcțiile care lucrează cu el stau într-un modul separat, ca în F#.
/// </summary>
public static class ResultExtensions
{
    extension<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
        where TSuccess : notnull
        where TFailure : notnull
    {
        /// <summary><see langword="true"/> pe ramura de succes.</summary>
        public bool IsOk => result is Result<TSuccess, TFailure>.Ok;

        /// <summary>Transformă valoarea de succes; eroarea trece neschimbată (functor).</summary>
        public Result<TOut, TFailure> Map<TOut>(Func<TSuccess, TOut> map)
            where TOut : notnull
            => result.Match<Result<TOut, TFailure>>(
                value => new Result<TOut, TFailure>.Ok(map(value)),
                error => new Result<TOut, TFailure>.Error(error));

        /// <summary>Înlănțuie o funcție care poate eșua la rândul ei; prima eroare oprește lanțul (monadă).</summary>
        public Result<TOut, TFailure> Bind<TOut>(Func<TSuccess, Result<TOut, TFailure>> bind)
            where TOut : notnull
            => result.Match<Result<TOut, TFailure>>(
                bind,
                error => new Result<TOut, TFailure>.Error(error));

        /// <summary>Transformă eroarea (de exemplu dintr-o eroare specifică într-una a workflow-ului); valoarea trece neschimbată.</summary>
        public Result<TSuccess, TOut> MapError<TOut>(Func<TFailure, TOut> map)
            where TOut : notnull
            => result.Match<Result<TSuccess, TOut>>(
                value => new Result<TSuccess, TOut>.Ok(value),
                error => new Result<TSuccess, TOut>.Error(map(error)));

        /// <summary>Verifică o condiție suplimentară pe ramura de succes și o transformă în eroare dacă nu este îndeplinită.</summary>
        public Result<TSuccess, TFailure> Ensure(Func<TSuccess, bool> predicate, Func<TSuccess, TFailure> error)
            => result.Bind(value => predicate(value)
                ? result
                : new Result<TSuccess, TFailure>.Error(error(value)));

        /// <summary>Execută un efect (de exemplu logare) pe ramura de succes și întoarce rezultatul neschimbat.</summary>
        public Result<TSuccess, TFailure> Tap(Action<TSuccess> effect)
        {
            if (result is Result<TSuccess, TFailure>.Ok ok)
            {
                effect(ok.Value);
            }

            return result;
        }

        /// <summary>
        /// Extrage valoarea sau aruncă excepția construită din eroare. De folosit doar la granițe unde eșecul
        /// este imposibil prin construcție (de exemplu date citite din propria bază de date): acolo o eroare
        /// înseamnă date corupte, nu o greșeală a utilizatorului.
        /// </summary>
        public TSuccess GetValueOrThrow(Func<TFailure, Exception> toException)
            => result.Match(value => value, error => throw toException(error));
    }
}
