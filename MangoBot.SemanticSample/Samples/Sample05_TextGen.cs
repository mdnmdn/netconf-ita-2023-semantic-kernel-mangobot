using MangoBot.SemanticSample.Utils;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

namespace MangoBot.SemanticSample.Samples;

public class Sample05_TextGen
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Inline Function Definition ========");

        string openAIModelId = Constants.OpenAIChatModel;
        string openAIApiKey = Constants.OpenAIToken;

        if (openAIModelId == null || openAIApiKey == null)
        {
            Console.WriteLine("OpenAI credentials not found. Skipping example.");
            return;
        }

        /*
         * Example: normally you would place prompt templates in a folder to separate
         *          C# code from natural language code, but you can also define a semantic
         *          function inline if you like.
         */

        IKernel kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithOpenAIChatCompletionService(
                modelId: openAIModelId,
                apiKey: openAIApiKey)
            .Build();

        // Function defined using few-shot design pattern
        string promptTemplate = @"
Generate a creative reason or excuse for the given event.
Be creative and be funny. Let your imagination run wild.

Event: I am running late.
Excuse: I was being held ransom by giraffe gangsters.

Event: I haven't been to the gym for a year
Excuse: I've been too busy training my pet dragon.

Event: {{$input}}
";

        var excuseFunction = kernel.CreateSemanticFunction(promptTemplate, new OpenAIRequestSettings() { MaxTokens = 100, Temperature = 0.4, TopP = 1 });

        var result = await kernel.RunAsync("I missed the F1 final race", excuseFunction);
        Console.WriteLine(result.GetValue<string>());

        result = await kernel.RunAsync("sorry I forgot your birthday", excuseFunction);
        Console.WriteLine(result.GetValue<string>());

        var fixedFunction = kernel.CreateSemanticFunction($"Translate this date {DateTimeOffset.Now:f} to French format", new OpenAIRequestSettings() { MaxTokens = 100 });

        result = await kernel.RunAsync(fixedFunction);
        Console.WriteLine(result.GetValue<string>());
    }
}