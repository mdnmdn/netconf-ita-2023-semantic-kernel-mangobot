using MangoBot.Runner.Utils;

namespace MangoBot.Runner;

public class Config
{
    public static Config Instance { get; set; } = new Config();
    
    private Config()
    {
        DotEnv.LoadRecursiveParent();
        var discordToken = Environment.GetEnvironmentVariable("DISCORD_MANGOBOT_TOKEN");
        var openAiToken = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var singSearchToken = Environment.GetEnvironmentVariable("BING_SEARCH_KEY");
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_URL");

        if (discordToken.IsNullOrEmpty())
            throw new Exception("Missing env DISCORD_MANGOBOT_TOKEN");
        if (openAiToken.IsNullOrEmpty())
            throw new Exception("Missing env OPENAI_API_KEY");
        
        DiscordToken = discordToken!;
        OpenAiToken = openAiToken!;
        BingSearchToken = singSearchToken;
        RedisConnectionString = redisConnectionString ?? "localhost:6979";
    }
    
    public string DiscordToken { get; set; }
    public string OpenAiToken { get; set; }
    public string? BingSearchToken { get; set; }
    
    public string RedisConnectionString  { get; set; }

    public string ChatModelId  { get => "gpt-3.5-turbo"; }
    public string EmbeddingsModelId  { get => "text-embedding-ada-002"; }
}