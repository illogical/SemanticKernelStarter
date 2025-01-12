using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelStarter
{
    public class HistoryEntry
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("history")]
        public ChatHistory History { get; set; }

        public HistoryEntry(ChatHistory history)
        {
            Timestamp = DateTime.Now;
            History = history;
        }

        public void SerializeJson()
        {
            string historyFile = "chat_history.json";

            try
            {
                var history = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.AppendAllText(historyFile, history + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error saving chat history: {ex.Message}");
            }
        }
    }
}