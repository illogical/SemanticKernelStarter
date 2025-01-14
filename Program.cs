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

var responseTimes = new List<string>();

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

    Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Assistant(s) > ");

    // Sample of dispatching a prompt to multiple Ollama servers at the same time.
    // It will wait to get back multiple responses so it cannot stream the results to the console.
    //var results = await ollamaManager.SendPromptToAllServersAsync(history, userInput);
    //foreach (var (serverId, (response, duration)) in results)
    //{
    //    history.AddAssistantMessage(response);
    //    Console.WriteLine($"Server {serverId} responded in {duration.TotalSeconds:F2} seconds:");
    //    Console.WriteLine(response);
    //    Console.WriteLine();
    //}


    // Sample of dispatching a prompt to multiple Ollama servers at the same time.
    // It will return back each response as they return rather than awaiting all servers to reply.
    await foreach (var (serverId, modelId, response, duration) in ollamaManager.SendPromptToAllServersStreamingAsync(history, userInput))
    {
        string responseTime = $"Server {serverId} with model {modelId} responded in {duration.TotalSeconds:F2} seconds";
        responseTimes.Add(responseTime);
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Logger.Log(responseTime);
        Console.ResetColor(); // Reset color back to default
        Console.WriteLine(response);
        Console.WriteLine();

        history.AddAssistantMessage(response);
    }

    for (int i = 0; i < responseTimes.Count; i++)
    {
        Console.WriteLine($"{i+1}. {responseTimes[i]}");
    }

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