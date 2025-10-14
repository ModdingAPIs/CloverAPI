using System;
using System.Collections.Generic;

namespace CloverAPI.Utils;

public static class LinqExtensions
{
    public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector) where TKey : IComparable<TKey>
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        T maxElement = enumerator.Current;
        TKey maxKey = selector(maxElement);

        while (enumerator.MoveNext())
        {
            T currentElement = enumerator.Current;
            TKey currentKey = selector(currentElement);

            if (currentKey.CompareTo(maxKey) > 0)
            {
                maxElement = currentElement;
                maxKey = currentKey;
            }
        }

        return maxElement;
    }
    
    public static bool TryGetValueNoCase<TValue>(this IDictionary<string, TValue> dictionary, string key, out TValue value)
    {
        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kvp.Value;
                return true;
            }
        }
        value = default;
        return false;
    }
}