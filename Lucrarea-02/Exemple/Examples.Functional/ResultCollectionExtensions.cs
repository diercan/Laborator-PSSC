namespace Examples.Functional;

/// <summary>
/// Lucrul cu liste de rezultate (F#: <c>Result.sequence</c> / <c>Result.traverse</c>), în varianta
/// de validare: se evaluează <b>toate</b> elementele și se adună <b>toate</b> erorile.
/// </summary>
public static class ResultCollectionExtensions
{
    extension<T, TFailure>(IEnumerable<Result<T, IReadOnlyList<TFailure>>> results)
        where T : notnull
        where TFailure : notnull
    {
        /// <summary>
        /// Transformă o listă de rezultate într-un rezultat de listă: <c>Ok</c> cu toate valorile dacă niciun element
        /// nu a eșuat, altfel <c>Error</c> cu erorile tuturor elementelor, în ordinea lor.
        /// </summary>
        public Result<IReadOnlyList<T>, IReadOnlyList<TFailure>> Sequence()
        {
            List<T> values = [];
            List<TFailure> errors = [];

            foreach (Result<T, IReadOnlyList<TFailure>> result in results)
            {
                switch (result)
                {
                    case Result<T, IReadOnlyList<TFailure>>.Ok ok:
                        values.Add(ok.Value);
                        break;
                    case Result<T, IReadOnlyList<TFailure>>.Error error:
                        errors.AddRange(error.Value);
                        break;
                    default:
                        break;
                }
            }

            return errors.Count == 0
                ? new Result<IReadOnlyList<T>, IReadOnlyList<TFailure>>.Ok(values)
                : new Result<IReadOnlyList<T>, IReadOnlyList<TFailure>>.Error(errors);
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        /// <summary>Aplică o validare fiecărui element și adună rezultatele cu <see cref="Sequence"/> (<c>traverse f = map f &gt;&gt; sequence</c>).</summary>
        public Result<IReadOnlyList<TOut>, IReadOnlyList<TFailure>> Traverse<TOut, TFailure>(
            Func<T, Result<TOut, IReadOnlyList<TFailure>>> validate)
            where TOut : notnull
            where TFailure : notnull
            => source.Select(validate).Sequence();
    }
}
