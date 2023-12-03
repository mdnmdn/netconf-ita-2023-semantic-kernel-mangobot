using Discord;

namespace MangoBot.Runner.Utils;

public class Logger
{
    public static Task DiscordLog(LogMessage message)
    {
        ColorConsole.WriteEmbeddedColorLine(message.Message, ConsoleColor.Gray);
        return Task.CompletedTask;
    }

    public static void Info(string message)
    {
        ColorConsole.WriteEmbeddedColorLine(message, ConsoleColor.Green);
    }

    public static void Error(Exception exception)
    {
        ColorConsole.WriteEmbeddedColorLine(exception.ToString(), ConsoleColor.Red);
    }

    public static void MangoChat(string message)
    {
        ColorConsole.WriteEmbeddedColorLine(message, ConsoleColor.Yellow);
    }

    public static void Chat(string message, bool isMention = false)
    {
        var color = isMention ? ConsoleColor.Gray : ConsoleColor.DarkGray;
        ColorConsole.WriteEmbeddedColorLine(message, color);
    }
}