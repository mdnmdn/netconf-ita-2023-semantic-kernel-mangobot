using MangoBot.Runner.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Planners;

namespace MangoBot.Runner.SK;

public class KernelBotThreeWithPlanner: BaseKernelBot
{
    private readonly IKernel _kernel;
    const string MessageCollectionName = "mango-messages";
    private bool _init = false;
    private SemanticTextMemory? _memory;
    private FunctionCallingStepwisePlanner _planner;

    protected override string BotVersion { get => "v3"; }
    
    public KernelBotThreeWithPlanner(DiscordEngine engine) : base(engine)
    {
        _kernel = new KernelBuilder()
            
            //.WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithOpenAIChatCompletionService(
                //modelId: Config.Instance.ChatModelId,
                modelId: Config.Instance.ChatModel4Id,
                apiKey: Config.Instance.OpenAiToken)
            .WithOpenAITextEmbeddingGenerationService(Config.Instance.EmbeddingsModelId, Config.Instance.OpenAiToken)
            .Build();

        _kernel.FunctionInvoking += (sender, args) =>
        {
            var function = $"{args.FunctionView.PluginName ?? "@"}.{args.FunctionView.Name}";
            ColorConsole.WriteLine($"Function {function} is being invoked");
            
        };
        
        _kernel.FunctionInvoked += (sender, args) =>
        {
            var function = $"{args.FunctionView.PluginName ?? "@"}.{args.FunctionView.Name}";
            ColorConsole.WriteLine($"Function {function} has been invoked");
        };
    }

    public override async Task Init()
    {
        // Setup memory
        var embeddingGenerator = new OpenAITextEmbeddingGeneration(Config.Instance.EmbeddingsModelId, Config.Instance.OpenAiToken);
        
        var redisStore = await RedisMemoryStoreFactory.CreateSampleRedisMemoryStoreAsync(); 
        _memory = new SemanticTextMemory(redisStore, embeddingGenerator);

        var memoryPlugin = new TextMemoryPlugin(_memory);
        _kernel.ImportFunctions(memoryPlugin);
        
        _kernel.ImportFunctions(new TimePlugin(), "TimePlugin");
        
        // Setup web search
        if (Config.Instance.BingSearchToken.HasValue())
        {
            var bingConnector = new BingConnector(Config.Instance.BingSearchToken);
            var bing = new WebSearchEnginePlugin(bingConnector);
            _kernel.ImportFunctions(bing, "bing");
        }
        
        
        // setup discord plugin
        var discordPlugin = new DiscordPlugin(Discord);
        _kernel.ImportFunctions(discordPlugin, "discord");
        
        // setup planner
        var config = new FunctionCallingStepwisePlannerConfig
        {
            MaxIterations = 15,
            MaxTokens = 4000,
            
        };
        _planner = new FunctionCallingStepwisePlanner(_kernel, config);
        
        _init = true;
        await base.Init();
    }

    protected override async Task OnMessage(ChatMessage message)
    {
        if (!_init) throw new Exception("Init has not been called");
        try
        {
            // save in memory all the messages that are not mentions and are longer than 6 characters
            if (!message.IsMention)
            {
                if (message.Message is { Length: > 6 })
                {
                    var id = $"{DateTime.Now:O}:{message.Sender}";
                    await _memory!.SaveInformationAsync(MessageCollectionName,
                        $"{message.Sender} at {DateTime.Now:O} said {message.Message}", id);
                }
            }
            else
            {
                await Discord.SetTyping(message.OriginalMessage.Channel);

                var result = await _planner.ExecuteAsync(message.Message);
                var response = result.FinalAnswer;
                if (response.HasValue())
                    await Discord.SendMessage(message.ChannelId, response!, message.OriginalMessage.Id);
                
                
            }
        }catch(Exception e)
        {
            await Discord.SendMessage(message.ChannelId, e.ToString());
            Logger.Error(e);
        }


    }
}
