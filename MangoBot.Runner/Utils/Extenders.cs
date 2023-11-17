namespace MangoBot.Runner.Utils;

public static class Extenders
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var val in collection)
            action(val);
    }

    public static bool IsNullOrEmpty(this string? str) => String.IsNullOrWhiteSpace(str);
    public static bool HasValue(this string? str) => !str.IsNullOrEmpty();
}