using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GymMvc.ViewModels;

namespace GymMvc.Services
{
    // Talks to the Groq Chat Completions API.
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiChatService> _logger;

        private const string SystemPrompt =
    "You are a general fitness assistant for a gym app. " +
    "Give general fitness and nutrition information only. " +
    "You are not a doctor and this is not medical advice. " +
    "You have no access to real member or gym data. " +
    "Never claim to have access to member data. " +
    "Answer clearly and helpfully. " +
    "Do not use Markdown formatting. " +
    "Do not use asterisks for bold text. " +
    "Do not use Markdown tables or pipe characters for tables. " +
    "Use simple plain text with clear headings and numbered lists.";

        public AiChatService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(
            List<AiChatMessageViewModel> history,
            string userMessage)
        {
            var endpoint = _configuration["AiSettings:Endpoint"];
            var apiKey = _configuration["AiSettings:ApiKey"];
            var model = _configuration["AiSettings:Model"];

            if (string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(model))
            {
                _logger.LogWarning("Groq settings not configured");
                return "المساعد مش متاح دلوقتي.";
            }

            var messages = new List<object>
            {
                new
                {
                    role = "system",
                    content = SystemPrompt
                }
            };

            foreach (var msg in history.TakeLast(10))
            {
                var role = msg.Role?.ToLowerInvariant() == "assistant"
                    ? "assistant"
                    : "user";

                messages.Add(new
                {
                    role = role,
                    content = msg.Text ?? string.Empty
                });
            }

            // Add the current user message
            messages.Add(new
            {
                role = "user",
                content = userMessage
            });

            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7,
                max_completion_tokens = 500
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

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
                    _logger.LogError(
                        "Groq API error {StatusCode}: {Body}",
                        response.StatusCode,
                        responseText);

                    return "معلش، حصلت مشكلة في المساعد، جرب تاني.";
                }

                using var doc = JsonDocument.Parse(responseText);

                if (!doc.RootElement.TryGetProperty(
                        "choices",
                        out var choices) ||
                    choices.GetArrayLength() == 0)
                {
                    _logger.LogError(
                        "Groq API returned no choices. Body: {Body}",
                        responseText);

                    return "معلش، المساعد رجع رد غير متوقع.";
                }

                var reply = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return string.IsNullOrWhiteSpace(reply)
                    ? "معلش، ممكن تعيد صياغة السؤال؟"
                    : reply.Trim();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Groq API timeout");
                return "المساعد بياخد وقت أكتر من المتوقع، جرب تاني.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Groq API network error");
                return "معلش، مش قادر أتواصل مع المساعد دلوقتي.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Groq API returned malformed JSON");

                return "معلش، حصلت مشكلة في الرد، جرب تاني.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Groq AI service");
                return "معلش، حصلت مشكلة غير متوقعة، جرب تاني.";
            }
        }
    }
}