namespace MangoBot.Runner.SK;

public abstract class BaseKernelBot
{
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
    
    public virtual Task Init() => Task.CompletedTask; 
    
    protected DiscordEngine Discord { get; init; }

    protected abstract Task OnMessage(ChatMessage message);
}