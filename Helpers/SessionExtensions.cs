using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GymMvc.Helpers
{
    // الـ Session الأساسية بتحفظ Strings بس، فبنسريلايز/ديسريلايز أي Object كـ JSON.
    // GetObject محمي: لو الـ JSON اتلخبط لأي سبب، بيرجع default بدل ما يرمي Exception
    // ويكسر الصفحة (مطلوب صراحة في الـ Audit: invalid session data must not crash the app).
    public static class SessionExtensions
    {
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