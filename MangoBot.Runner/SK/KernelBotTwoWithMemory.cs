using MangoBot.Runner.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.VisualBasic;

namespace MangoBot.Runner.SK;

public class KernelBotTwoWithMemory: BaseKernelBot
{
    private readonly IKernel _kernel;
    private readonly ISKFunction _mainFunction;
    const string MessageCollectionName = "mango-messages";
    private bool _init = false;
    private SemanticTextMemory? _memory;
    private IDictionary<string, ISKFunction> _memoryFunctions;
    
    protected override string BotVersion { get => "v2"; }

    public KernelBotTwoWithMemory(DiscordEngine engine) : base(engine)
    {
        _kernel = new KernelBuilder()
            
            //.WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithOpenAIChatCompletionService(
                modelId: Config.Instance.ChatModelId,
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
        
        
        var promptTemplate = 
            """
            Your name is MangoBot and you are discord server bot, be helpful and answer questions about the server and the users.
            
            If possible use the relevant messages to answer the question.
            
            if you mention a user prefix the name with @, as example for user MangoBot use @MangoBot.
            
            
            -------  Relevant messages   -----------------
            {{Recall}}
            --------------------------------------
            
            User input:
            
            {{$input}}
            """;

        _mainFunction = _kernel.CreateSemanticFunction(promptTemplate, 
            new OpenAIRequestSettings() {
                MaxTokens = 100, Temperature = 1, TopP = 1
            });
    }

    public override async Task Init()
    {
        // Setup memory
        var embeddingGenerator = new OpenAITextEmbeddingGeneration(Config.Instance.EmbeddingsModelId, Config.Instance.OpenAiToken);
        
        var redisStore = await RedisMemoryStoreFactory.CreateSampleRedisMemoryStoreAsync(); 
        _memory = new SemanticTextMemory(redisStore, embeddingGenerator);
        
        var memoryPlugin = new TextMemoryPlugin(_memory);
        _memoryFunctions = _kernel.ImportFunctions(memoryPlugin);
        
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
                
                
                var result = await _kernel.RunAsync(
                    new ContextVariables()
                    {
                        [TextMemoryPlugin.CollectionParam] = MessageCollectionName,
                        [TextMemoryPlugin.LimitParam] = "2",
                        [TextMemoryPlugin.RelevanceParam] = "0.79",
                        ["input"] = message.Message
                    },
                    new ISKFunction[] {
                       // _memoryFunctions["Recall"],
                        _mainFunction,
                    }) ;
                var response = result.GetValue<string>();
                if (response.HasValue())
                    await Discord.SendMessage(message.ChannelId, response!, message.OriginalMessage.Id);
                
                //await Discord.SendMessage(message.ChannelId, $"Hi {message.Sender} ");
            }
        }catch(Exception e)
        {
            await Discord.SendMessage(message.ChannelId, e.ToString());
            Logger.Error(e);
        }


    }
}