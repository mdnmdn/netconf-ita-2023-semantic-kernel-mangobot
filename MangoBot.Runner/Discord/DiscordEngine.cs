using Discord;
using Discord.WebSocket;
using MangoBot.Runner.SK;
using MangoBot.Runner.Utils;

namespace MangoBot.Runner;

public class DiscordEngine(string token)
{
    private DiscordSocketClient _client;

    public event Func<ChatMessage, Task>? OnMessage;


    private List<SocketGuildUser> _users = new();
    private SocketGuild? _lastGuild;
    ISocketMessageChannel? _lastChannel;

    public async Task Start()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.GuildMessages
                             | GatewayIntents.Guilds
                             | GatewayIntents.GuildMessageReactions
                             | GatewayIntents.MessageContent
                             | GatewayIntents.GuildMembers
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

            _lastGuild = _client.Guilds.FirstOrDefault();
            return Task.CompletedTask;
        };

        _client.MessageUpdated += MessageUpdated;
        _client.MessageReceived += MessageReceived;
    }

    private async Task MessageReceived(SocketMessage message)
    {
        _lastChannel = message.Channel;
        var channelType = _lastChannel.GetType();

        var cleanMessage = DereferenceMentions(message);
        if (message.Author.Id == _client.CurrentUser.Id)
        {
            Logger.MangoChat($"\ud83e\udd6d {message.Author.Username}@{message.Channel.Name}> {cleanMessage}");
            return;
        }

        var isMention = message.MentionedUsers.Any(it => it.Id == _client.CurrentUser.Id);
        Logger.Chat($"{message.Author.Username}@{message.Channel.Name}> {cleanMessage}", isMention);

        var msg = new ChatMessage()
        {
            IsMention = message.MentionedUsers.Any(it => it.Id == _client.CurrentUser.Id),
            OriginalMessage = message,
            Channel = message.Channel,
            Message = cleanMessage,
        };

        if (OnMessage != null)
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

    private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after,
        ISocketMessageChannel channel)
    {
        // If the message was not in the cache, downloading it will result in getting a copy of `after`.
        var message = await before.GetOrDownloadAsync();
        Console.WriteLine($"{message} -> {after}");
    }

    public async Task SendMessage(ulong channelId, string msg, ulong? responseToMessageId = null)
    {
        var channel = (IMessageChannel)_client.GetChannel(channelId);

        IUserMessage? messageToReplyTo = null;

        if (responseToMessageId.HasValue)
        {
            var responseToMessage = await channel.GetMessageAsync(responseToMessageId.Value);
            messageToReplyTo = responseToMessage as IUserMessage;
        }

        if (messageToReplyTo != null)
            await messageToReplyTo.ReplyAsync(msg);
        else
            await channel.SendMessageAsync(msg);
    }

    public async Task SetTyping(ISocketMessageChannel originalMessageChannel)

    {
        await originalMessageChannel.TriggerTypingAsync();
    }

    public async Task SendMessageToUser(string username, string message)
    {
        var channel = (IMessageChannel)_lastChannel!;
        var user = GetUser(username.Replace("@", ""));
        var msg = $"{MentionUtils.MentionUser(user.Id)} {message}";
        await channel.SendMessageAsync(msg);
    }

    ///  
    public SocketUser? GetUser(string username)
    {
        return _users.FirstOrDefault(i =>
            i.Username == username || i.Id.ToString() == username || i.GlobalName == username);
    }

    public ChannelInfo[] ListGroups() => _lastGuild.Channels
        .Where(i => i is SocketTextChannel)
        .Cast<SocketTextChannel>()
        .Select(i => new ChannelInfo(i.Id, i.Name, i.Topic)).ToArray();

    public async Task<UserInfo[]> ListUsers()
    {
        await _lastGuild.DownloadUsersAsync();
        _users = _lastGuild.Users.ToList();
        return _users.Select(i => new UserInfo(i.Id, i.DisplayName, i.Username)).ToArray();
    }


    public record ChannelInfo(ulong Id, string Name, string Topic);

    public record UserInfo(ulong Id, string Name, string Username);

    public Task SetBotStatus(string status)
    {
        return _client.SetCustomStatusAsync(status);
    }
}