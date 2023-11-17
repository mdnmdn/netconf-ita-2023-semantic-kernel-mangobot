using Discord;
using Discord.WebSocket;
using MangoBot.Runner.SK;
using MangoBot.Runner.Utils;

namespace MangoBot.Runner;

public class DiscordEngine(string token)
{
    private DiscordSocketClient _client;
    
    public event Func<ChatMessage, Task>? OnMessage; 

    public async Task Start()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.GuildMessages 
                             | GatewayIntents.Guilds 
                             | GatewayIntents.GuildMessageReactions
                             | GatewayIntents.MessageContent
            //| GatewayIntents.GuildMembers
            //| GatewayIntents.GuildPresences
            ,
            
        });
        //_commandService = new CommandService(new CommandServiceConfig() { });

        _client.Log += Logger.DiscordLog;

        await _client.LoginAsync(TokenType.Bot, Config.Instance.DiscordToken);
        await _client.StartAsync();
        
        _client.Ready += () => 
        {
            Logger.Info("\ud83e\udd6d MangoBot is online!");
            
            return Task.CompletedTask;
        };
        
        
        
        _client.MessageUpdated += MessageUpdated;
        _client.MessageReceived += MessageReceived;
    }
    
    private async Task MessageReceived(SocketMessage message)
    {

        var cleanMessage = DereferenceMentions(message);
        if (message.Author.Id == _client.CurrentUser.Id)
        {
            Logger.MangoChat($"\ud83e\udd6d {message.Author.Username}@{message.Channel.Name}> {cleanMessage}");
            return;
        }

        var isMention = message.MentionedUsers.Any( it => it.Id == _client.CurrentUser.Id);
        Logger.Chat($"{message.Author.Username}@{message.Channel.Name}> {cleanMessage}", isMention);

        var msg = new ChatMessage()
        {
            IsMention = message.MentionedUsers.Any( it => it.Id == _client.CurrentUser.Id),
            OriginalMessage = message,
            Channel = message.Channel,
            Message = cleanMessage,
        };

        if (OnMessage!= null)
            await OnMessage(msg);
    }

    private string DereferenceMentions(SocketMessage message)
    {
        var result = message.Content;
        foreach (var mentionedUser in message.MentionedUsers)
        {
            result = result.Replace(mentionedUser.Mention, $"@{mentionedUser.Username}");
        }

        return result;
    }

    private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        // If the message was not in the cache, downloading it will result in getting a copy of `after`.
        var message = await before.GetOrDownloadAsync();
        Console.WriteLine($"{message} -> {after}");
    }

    public async Task SendMessage(ulong channelId, string msg)
    {
        var channel = (IMessageChannel)_client.GetChannel(channelId);
        await channel.SendMessageAsync(msg);
    }
}
    