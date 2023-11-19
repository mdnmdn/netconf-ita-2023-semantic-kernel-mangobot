// See https://aka.ms/new-console-template for more information


using MangoBot.SemanticSample.Utils;
using StackExchange.Redis;

public static class Constants
{
    static Constants()
    {
        DotEnv.LoadRecursiveParent();
        OpenAIToken = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    }

    public static string OpenAIToken { get; private set; }
    //public const string OpenAICompletionModel = "gpt-3.5-turbo";
    public const string OpenAIChatModel = "gpt-4-1106-preview";
    //public const string OpenAIChatModel = "gpt-3.5-turbo";
    public const string OpenAIEmbeddingModel = "text-embedding-ada-002";
    public const string RedisConnectionString = "localhost:6979";
}
