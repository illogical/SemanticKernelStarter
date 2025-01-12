// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

// Populate values for Ollama
var modelId = "phi4"; // or your preferred Ollama model
var endpoint = "http://localhost:11434"; // default Ollama endpoint

// Create a kernel with Ollama chat completion
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var builder = Kernel.CreateBuilder()
    .AddOllamaChatCompletion(modelId, new Uri(endpoint));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Add enterprise components
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


// Create prompt execution settings for Ollama
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var ollamaPromptExecutionSettings = new OllamaPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Create a history to store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (string.IsNullOrEmpty(userInput))
        break;

    // Add user input
    history.AddUserMessage(userInput);

    // Print "Assistant > " without newline
    Console.Write("Assistant > ");

    // Stream the response
    string fullResponse = "";
    await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: ollamaPromptExecutionSettings,
        kernel: kernel))
    {
        // Print each chunk of the response as it arrives
        Console.Write(content.Content);
        fullResponse += content.Content;
    }
    Console.WriteLine(); // Add newline after response is complete

    // Add the complete message to the chat history
    history.AddAssistantMessage(fullResponse);
} while (true);