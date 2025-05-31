namespace ByteBard.AsyncAPI;

using System.Collections.Generic;

public static class PolyfillExtensions
{
#if NETSTANDARD2_0
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }

    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
    {
        if (source.ContainsKey(key))
        {
            return false;
        }

        source.Add(key, value);
        return true;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
    {
        if (source.ContainsKey(key))
        {
            return source[key];
        }
        
        return default(TValue);
    }
#endif
}