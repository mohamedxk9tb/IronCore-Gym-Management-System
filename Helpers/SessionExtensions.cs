using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GymMvc.Helpers
{
    public static class SessionExtensions
    {
        // Single source of truth for the AI chat session key.
        public const string AiChatHistoryKey = "AiChatHistory";

        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null)
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (JsonException)
            {
                session.Remove(key);
                return default;
            }
        }
    }
}