namespace MangoBot.Runner.Utils;

public static class DotEnv
{
    public static void LoadRecursiveParent(int levels = 5)
    {
        var path = Directory.GetCurrentDirectory();

        for (var level = 0; level < levels; level++)
        {
            var filePath = Path.Combine(path, ".env");
            if (File.Exists(filePath))
            {
                Load(filePath);
                break;
            }

            path = Path.Combine(path, "..");
        }
    }

    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}