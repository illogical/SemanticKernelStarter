using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace SemanticKernelStarter
{
    // Custom class to serialize chat messages
    public class ChatMessageJson
    {
        public string Role { get; set; }
        public string Content { get; set; }

        public ChatMessageJson(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    // Extension methods for ChatHistory serialization
    public static class ChatHistoryExtensions
    {
        public static string ToJson(this ChatHistory history)
        {
            var messages = history.Select(m => new ChatMessageJson(m.Role.Label, m.Content));
            return JsonSerializer.Serialize(messages, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static ChatHistory LoadFromJson(string json)
        {
            var history = new ChatHistory();
            var messages = JsonSerializer.Deserialize<List<ChatMessageJson>>(json);

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    history.AddMessage(new AuthorRole(message.Role), message.Content);
                }
            }

            return history;
        }
    }
}
