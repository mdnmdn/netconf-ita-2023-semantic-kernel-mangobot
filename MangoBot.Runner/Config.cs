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
        
        var azureOpenAIToken = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var azureOpenAIModel = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL");
        var azureOpenAIEmbedModel = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBED_MODEL");
        var azureOpenAIUrl= Environment.GetEnvironmentVariable("AZURE_OPENAI_URL");

        if (discordToken.IsNullOrEmpty())
            throw new Exception("Missing env DISCORD_MANGOBOT_TOKEN");
        if (openAiToken.IsNullOrEmpty())
            throw new Exception("Missing env OPENAI_API_KEY");

        DiscordToken = discordToken!;
        OpenAiToken = openAiToken!;
        BingSearchToken = singSearchToken;
        RedisConnectionString = redisConnectionString ?? "localhost:6979";

        AzureOpenAIToken = azureOpenAIToken;
        AzureOpenAIModel = azureOpenAIModel;
        AzureOpenAIEmbedModel = azureOpenAIEmbedModel;
        AzureOpenAIUrl = azureOpenAIUrl;
    }

    public string? AzureOpenAIEmbedModel { get; set; }
    public string? AzureOpenAIUrl { get; set; }
    public string? AzureOpenAIModel { get; set; }
    public string? AzureOpenAIToken { get; set; }

    public string DiscordToken { get; set; }
    public string OpenAiToken { get; set; }
    public string? BingSearchToken { get; set; }

    public string RedisConnectionString { get; set; }

    public string ChatModelId
    {
        get => "gpt-3.5-turbo";
    }

    public string ChatModel4Id
    {
        get => "gpt-4-1106-preview";
    }

    public string EmbeddingsModelId
    {
        get => "text-embedding-ada-002";
    }
}