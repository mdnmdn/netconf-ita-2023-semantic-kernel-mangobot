using MangoBot.SemanticSample.Plugins;
using MangoBot.SemanticSample.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.Redis;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Basic;
using TimePlugin = MangoBot.SemanticSample.Plugins.TimePlugin;

namespace MangoBot.SemanticSample.Samples;

public class Sample66_StepwisePlanner
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Action Planner ========");
        string openAIModelId = Constants.OpenAIChatModel;
        string openAIApiKey = Constants.OpenAIToken;

        IKernel kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithOpenAIChatCompletionService(
                modelId: openAIModelId,
                apiKey: openAIApiKey)
            .Build();

        kernel.ImportFunctions(new EmailPlugin(), "EmailPlugin");
        kernel.ImportFunctions(new MathPlugin(), "MathPlugin");
        kernel.ImportFunctions(new TimePlugin(), "TimePlugin");


        var config = new FunctionCallingStepwisePlannerConfig
        {
            MaxIterations = 15,
            MaxTokens = 4000,
        };
        var planner = new FunctionCallingStepwisePlanner(kernel, config);

        string[] questions = new string[]
        {
            "What is the current hour number, plus 5?",
            "What is 387 minus 22? Email the solution to John and Mary.",
            "Write a limerick, translate it to Spanish, and send it to Jane",
        };

        foreach (var question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(question);
            Console.WriteLine($"Q: {question}\nA: {result.FinalAnswer}");

            // You can uncomment the line below to see the planner's process for completing the request.
            Console.WriteLine($"Chat history:\n{result.ChatHistory?.AsJson()}");
        }
    }
}