using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> keySelector,
        int n,
        Func<T, string> tieBreaker) =>
        source
            .OrderByDescending(keySelector)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n);
}