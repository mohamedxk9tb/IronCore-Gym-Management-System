using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GymMvc.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiChatService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetResponseAsync(string message)
        {
            var apiKey = _configuration["AI:ApiKey"];
            var model = _configuration["AI:Model"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "AI API Key is not configured.";
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                return "AI Model is not configured.";
            }

            var requestBody = new
            {
                model = model,

                instructions =
                    "You are IronCore Gym's general fitness assistant. " +
                    "Give general helpful advice about training, nutrition, " +
                    "recovery and fitness. " +
                    "Keep answers clear and helpful. " +
                    "Do not claim access to the user's gym database or private information.",

                input = message
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseText =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"AI API Error: {response.StatusCode} - {responseText}";
                }

                using var document =
                    JsonDocument.Parse(responseText);

                if (document.RootElement.TryGetProperty(
                    "output",
                    out var output))
                {
                    foreach (var item in output.EnumerateArray())
                    {
                        if (!item.TryGetProperty(
                            "content",
                            out var content))
                        {
                            continue;
                        }

                        foreach (var contentItem in content.EnumerateArray())
                        {
                            if (contentItem.TryGetProperty(
                                "text",
                                out var text))
                            {
                                return text.GetString()
                                    ?? "No response was returned.";
                            }
                        }
                    }
                }

                return "No response was returned from the AI.";
            }
            catch (Exception ex)
            {
                return $"AI Connection Error: {ex.Message}";
            }
        }
    }
}