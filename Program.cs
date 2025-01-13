﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using SemanticKernelStarter;
using System.Diagnostics;

var modelId = "codestral";
var endpoint = "http://localhost:11434"; // default Ollama endpoint

// Create a kernel with Ollama chat completion
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var builder = Kernel.CreateBuilder()
    .AddOllamaChatCompletion(modelId, new Uri(endpoint));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create prompt execution settings for Ollama
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var ollamaPromptExecutionSettings = new OllamaPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0.5f // 0.8 by default. Lower is more accurate, higher is more "creative".
};
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

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

    // Create stopwatch to measure response time
    var stopwatch = Stopwatch.StartNew();

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