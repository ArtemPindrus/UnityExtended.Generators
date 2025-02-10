using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public static class SyntaxProviderExtensions {
    public static IncrementalValueProvider<ImmutableArray<T>> ValuesCombine<T>(
        this IncrementalValueProvider<ImmutableArray<T>> left,
        IncrementalValueProvider<ImmutableArray<T>> right) {
        return left.Combine(right).Select(Selector);

        ImmutableArray<T> Selector((ImmutableArray<T> Left, ImmutableArray<T> Right) tuple, CancellationToken cancellationToken) {
            var (i1, i2) = tuple;

            return i1.AddRange(i2);
        }
    }

    public static IncrementalValuesProvider<T> WhereNotNullValues<T>(this IncrementalValuesProvider<T?> provider) {
        return provider.Where(x => x is not null)!;
    }
}