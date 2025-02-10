using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace UnityExtended.Generator;

public static class SyntaxProviderExtensions {
    public static IncrementalValueProvider<IEnumerable<T>> ValuesCombine<T>(this IncrementalValueProvider<IEnumerable<T>> left,
        IncrementalValueProvider<IEnumerable<T>> right) {
        var provider = left.Combine(right).Select(Selector);

        return provider;

        IEnumerable<T> Selector((IEnumerable<T>, IEnumerable<T>) tuple, CancellationToken _) {
            var (i1, i2) = tuple;

            foreach (var t in i1) {
                yield return t;
            }

            foreach (var t in i2) {
                yield return t;
            }
        }
    }

    public static IncrementalValueProvider<IEnumerable<T>> ValuesCombine<T>(
        this IncrementalValueProvider<ImmutableArray<T>> left,
        IncrementalValueProvider<ImmutableArray<T>> right) {
        return left.AsEnumerable().ValuesCombine(right.AsEnumerable());
    }
    
    public static IncrementalValueProvider<IEnumerable<T>> ValuesCombine<T>(
        this IncrementalValueProvider<IEnumerable<T>> left,
        IncrementalValueProvider<ImmutableArray<T>> right) {
        return left.ValuesCombine(right.AsEnumerable());
    }

    public static IncrementalValueProvider<IEnumerable<T>> AsEnumerable<T>(
        this IncrementalValueProvider<ImmutableArray<T>> provider) {
        return provider.Select((x, _) => x.AsEnumerable());
    }

    public static IncrementalValuesProvider<T> WhereNotNullValues<T>(this IncrementalValuesProvider<T?> provider) {
        return provider.Where(x => x is not null)!;
    }
}