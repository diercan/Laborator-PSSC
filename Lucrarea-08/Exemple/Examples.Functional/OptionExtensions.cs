namespace Examples.Functional;

/// <summary>Combinatori pentru <see cref="Option{T}"/>, ca membri de extensie C# 14.</summary>
public static class OptionExtensions
{
    extension<T>(Option<T> option)
        where T : notnull
    {
        /// <summary><see langword="true"/> dacă valoarea este prezentă.</summary>
        public bool IsSome => option is Option<T>.Some;

        /// <summary>Transformă valoarea, dacă există.</summary>
        public Option<TOut> Map<TOut>(Func<T, TOut> map)
            where TOut : notnull
            => option.Match(value => Option.Some(map(value)), Option.None<TOut>);

        /// <summary>Valoarea prezentă sau o valoare de rezervă.</summary>
        public T GetValueOrDefault(T fallback)
            => option.Match(value => value, () => fallback);

        /// <summary>Convertește în <c>T?</c> la granițe (de exemplu pentru o coloană nullable sau un DTO).</summary>
        public TOut? ToNullable<TOut>(Func<T, TOut> map)
            where TOut : struct
            => option.Match<TOut?>(value => map(value), () => null);
    }
}
