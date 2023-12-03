using MangoBot.Runner.Utils;

namespace MangoBot.Runner.SK;

public abstract class BaseKernelBot
{
    protected abstract string BotVersion { get; }

    protected BaseKernelBot(DiscordEngine discord)
    {
        Discord = discord;
        //discord.OnMessage += OnMessage;
        discord.OnMessage += (msg) =>
        {
            Task.Run(() => OnMessage(msg));
            return Task.CompletedTask;
        };
    }

    public virtual Task Init()
    {
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            await Discord.SetBotStatus($"mango-bot-{BotVersion}");
            ColorConsole.WriteSuccess($"\ud83e\udd16 Bot version: mango-bot-{BotVersion}");
        });
        return Task.CompletedTask;
    }

    protected DiscordEngine Discord { get; init; }

    protected abstract Task OnMessage(ChatMessage message);
}