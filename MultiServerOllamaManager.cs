using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SemanticKernelStarter
{
    public class MultiServerOllamaManager
    {
        private readonly ConcurrentDictionary<string, OllamaServerConfig> _servers = new();
        private readonly ConcurrentDictionary<string, Kernel> _kernels = new();
        private readonly SemaphoreSlim _serverSelectionLock = new(1);

        public MultiServerOllamaManager(IConfiguration configuration)
        {
            var serverConfigs = configuration.GetSection("OllamaServers").Get<List<OllamaServerConfig>>();

            if (serverConfigs == null || !serverConfigs.Any())
            {
                throw new InvalidOperationException("No Ollama server configurations found in the configuration.");
            }

            foreach (var config in serverConfigs)
            {
                AddServer(config.ServerId, config.Endpoint, config.ModelId, config.Temperature);
            }
        }

        public void AddServer(string serverId, string endpoint, string modelId, float temperature)
        {
            var config = new OllamaServerConfig
            {
                Endpoint = endpoint,
                ModelId = modelId,
                Temperature = temperature
            };

            _servers.TryAdd(serverId, config);

            // Initialize kernel for this server
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var builder = Kernel.CreateBuilder()
                .AddOllamaChatCompletion(modelId, new Uri(endpoint));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var kernel = builder.Build();
            _kernels.TryAdd(serverId, kernel);
        }

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        public async Task<(Kernel kernel, IChatCompletionService chatService, OllamaPromptExecutionSettings settings)>
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            GetNextAvailableServerAsync()
        {
            await _serverSelectionLock.WaitAsync();
            try
            {
                // Select server based on availability and last used time
                var availableServer = _servers
                    .Where(s => s.Value.IsAvailable)
                    .OrderBy(s => s.Value.LastUsed)
                    .FirstOrDefault();

                if (availableServer.Key == null)
                    throw new InvalidOperationException("No available servers");

                var config = availableServer.Value;
                config.LastUsed = DateTime.UtcNow;

                var kernel = _kernels[availableServer.Key];
                var chatService = kernel.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var settings = new OllamaPromptExecutionSettings
                {
                    Temperature = config.Temperature,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                return (kernel, chatService, settings);
            }
            finally
            {
                _serverSelectionLock.Release();
            }
        }

        public async Task<Dictionary<string, (string ModelId, string Response, TimeSpan Duration)>> SendPromptToAllServersAsync(ChatHistory history, string prompt)
        {
            var tasks = new List<Task<(string ServerId, string ModelId, string Response, TimeSpan Duration)>>();

            foreach (var serverEntry in _servers)
            {
                var serverId = serverEntry.Key;
                var config = serverEntry.Value;

                tasks.Add(SendPromptToServerAsync(serverId, config, history, prompt));
            }

            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(
                r => r.ServerId,
                r => (r.ModelId, r.Response, r.Duration)
            );
        }

        private async Task<(string ServerId, string ModelId, string Response, TimeSpan Duration)> SendPromptToServerAsync(
            string serverId, OllamaServerConfig config, ChatHistory history, string prompt)
        {
            if (!_kernels.TryGetValue(serverId, out var kernel))
            {
                throw new InvalidOperationException($"Kernel not found for server {serverId}");
            }

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var settings = new OllamaPromptExecutionSettings
            {
                Temperature = config.Temperature,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            var localHistory = new ChatHistory(history);
            localHistory.AddUserMessage(prompt);

            var stopwatch = Stopwatch.StartNew();
            string fullResponse = "";

            await foreach (var content in chatService.GetStreamingChatMessageContentsAsync(
                localHistory,
                executionSettings: settings,
                kernel: kernel))
            {
                fullResponse += content.Content;
            }

            stopwatch.Stop();

            return (serverId, config.ModelId, fullResponse, stopwatch.Elapsed);
        }

        public async IAsyncEnumerable<(string ServerId, string ModelId, string Response, TimeSpan Duration)> SendPromptToAllServersStreamingAsync(
    ChatHistory history,
    string prompt,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<(string ServerId, string ModelId, string Response, TimeSpan Duration)>>();

            foreach (var serverEntry in _servers)
            {
                var serverId = serverEntry.Key;
                var config = serverEntry.Value;

                tasks.Add(SendPromptToServerAsync(serverId, config, history, prompt));
            }

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (!cancellationToken.IsCancellationRequested)
                {
                    yield return await completedTask;
                }
                else
                {
                    break;
                }
            }
        }


        public async Task MarkServerStatus(string serverId, bool isAvailable)
        {
            if (_servers.TryGetValue(serverId, out var config))
            {
                config.IsAvailable = isAvailable;
            }
        }
    }
}
