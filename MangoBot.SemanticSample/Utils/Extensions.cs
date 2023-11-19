using System.Text.Json;

namespace MangoBot.SemanticSample.Utils;

public static class Extensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptions);
    }
}