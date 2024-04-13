namespace NugetDepednencies;

public static class CollectionExtensions
{
    public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key)
        where V : class, new()
        where K : notnull
    {
        if (dict.TryGetValue(key, out V? val)) {
            return val!;
        }

        val = new();
        dict[key] = val;
        return val;
    }
}
