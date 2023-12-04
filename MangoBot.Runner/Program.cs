using MangoBot.Runner.SK;

namespace MangoBot.Runner;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var engine = new DiscordEngine(Config.Instance.DiscordToken);
        
        var bot = new KernelBotOne(engine);
        // var bot = new KernelBotTwoWithMemory(engine);
        // var bot = new KernelBotThreeWithPlanner(engine);

        await engine.Start();
        await bot.Init();

        await Task.Delay(-1);
    }
}