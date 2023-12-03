using MangoBot.Runner.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;

namespace MangoBot.Runner.SK;

/// <summary>
/// Simple bot 
/// </summary>
public class KernelBotOne : BaseKernelBot
{
    private readonly IKernel _kernel;
    private readonly ISKFunction _mainFunction;

    protected override string BotVersion => "v1";

    public KernelBotOne(DiscordEngine engine) : base(engine)
    {
        _kernel = new KernelBuilder()
            .WithLoggerFactory(ColorConsole.LoggerFactory())
            .WithOpenAIChatCompletionService(
                //modelId: Config.Instance.ChatModelId,
                modelId: Config.Instance.ChatModel4Id,
                apiKey: Config.Instance.OpenAiToken)
            .Build();

        var promptTemplate =
            """
            Your name is MangoBot and you are discord server bot, 
            be helpful and answer questions about the server and the users.

            {{$input}}
            """;

        _mainFunction = _kernel.CreateSemanticFunction(promptTemplate,
            new OpenAIRequestSettings()
            {
                MaxTokens = 100, Temperature = 1, TopP = 1
            });
    }


    protected override async Task OnMessage(ChatMessage message)
    {
        try
        {
            if (message.IsMention)
            {
                await Discord.SetTyping(message.OriginalMessage.Channel);

                var result = await _kernel.RunAsync(message.Message, _mainFunction);
                var response = result.GetValue<string>();
                
                if (response.HasValue())
                    await Discord.SendMessage(message.ChannelId, response!, message.OriginalMessage.Id);
            }
        }
        catch (Exception e)
        {
            await Discord.SendMessage(message.ChannelId, e.ToString());
            Logger.Error(e);
        }
    }
}