using MangoBot.Runner.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;

namespace MangoBot.Runner.SK;

public class KernelBotOne: BaseKernelBot
{
    private readonly IKernel _kernel;
    private readonly ISKFunction _mainFunction;

    public KernelBotOne(DiscordEngine engine) : base(engine)
    {
        _kernel = new KernelBuilder()
            
            //.WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithOpenAIChatCompletionService(
                modelId: Config.Instance.ChatModelId,
                apiKey: Config.Instance.OpenAiToken)
            .Build();

        var promptTemplate = 
            """
            Your name is MangoBot and you are discord server bot, be helpful and answer questions about the server and the users.
            
            {{$input}}
            """;

        _mainFunction = _kernel.CreateSemanticFunction(promptTemplate, 
            new OpenAIRequestSettings() {
                MaxTokens = 100, Temperature = 1, TopP = 1
            });


    }

    protected override async Task OnMessage(ChatMessage message)
    {
        try
        {
            if (message.IsMention)
            {
                var result = await _kernel.RunAsync(message.Message, _mainFunction);
                var response = result.GetValue<string>();
                if (response.HasValue())
                    await Discord.SendMessage(message.ChannelId, response!);
                
                //await Discord.SendMessage(message.ChannelId, $"Hi {message.Sender} ");
            }
        }catch(Exception e)
        {
            await Discord.SendMessage(message.ChannelId, e.ToString());
            Logger.Error(e);
        }


    }
}