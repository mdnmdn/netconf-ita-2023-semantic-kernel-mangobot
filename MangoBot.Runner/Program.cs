using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MangoBot.Runner.SK;
using MangoBot.Runner.Utils;
using Microsoft.Extensions.Configuration;

namespace MangoBot.Runner;

public class Program
{
    public static async Task Main(string[] args) 
    {
        
        var engine = new DiscordEngine(Config.Instance.DiscordToken);
        //var bot = new KernelBotOne(engine);
        //var bot = new KernelBotTwoWithMemory(engine);
        var bot = new KernelBotThreeWithPlanner(engine);

        await bot.Init();
        await engine.Start();
        
            
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
   
}