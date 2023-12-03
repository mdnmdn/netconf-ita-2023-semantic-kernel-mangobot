using Discord.WebSocket;

namespace MangoBot.Runner;

public class ChatMessage
{
    public bool IsMention { get; set; }

    public string Message { get; set; }

    public string Sender => OriginalMessage.Author.Username;

    public required ISocketMessageChannel Channel { get; set; }
    public required SocketMessage OriginalMessage { get; set; }

    public ulong ChannelId
    {
        get => OriginalMessage.Channel.Id;
    }
}