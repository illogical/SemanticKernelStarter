using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelStarter;
using System.Diagnostics;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var ollamaManager = new MultiServerOllamaManager(configuration);

// Create a history to store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;

do
{
    // Collect user input
    Console.Write($"Prompt > ");
    userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.WriteLine("Exiting...");
        break;
    }

    // Add user input
    history.AddUserMessage(userInput);

    Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Assistant > ");

    var (kernel, chatService, settings) = await ollamaManager.GetNextAvailableServerAsync();

    // Create stopwatch to measure response time
    var stopwatch = Stopwatch.StartNew();

    // Stream the response
    string fullResponse = "";
    await foreach (var content in chatService.GetStreamingChatMessageContentsAsync(
                   history,
                   executionSettings: settings,
                   kernel: kernel))
    {
        Console.Write(content.Content);
        fullResponse += content.Content;
    }

    // Stop the stopwatch and calculate elapsed time
    stopwatch.Stop();
    var elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000.0;

    // Print completion time and response duration
    var completionTime = DateTime.Now;
    Console.WriteLine(); // Add newline after response
    Logger.Log($"Response completed in {elapsedSeconds:F2} seconds");

    // Add the complete message to the chat history
    history.AddAssistantMessage(fullResponse);

    // Save the updated chat history to file with timestamps
    try
    {
        new HistoryEntry(history).SerializeJson();
    }
    catch (Exception ex)
    {
        Logger.Log($"Error saving chat history: {ex.Message}");
    }
} while (true);