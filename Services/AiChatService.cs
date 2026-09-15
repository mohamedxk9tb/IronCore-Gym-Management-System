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
    // Talks to the OpenAI Chat Completions API. Register as a typed client:
    // builder.Services.AddHttpClient<IAiChatService, AiChatService>();
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiChatService> _logger;

        private const string SystemPrompt =
            "You are a general fitness assistant for a gym app. Give general fitness/nutrition info only. " +
            "You are not a doctor and this is not medical advice. " +
            "You have no access to real member/gym data - never claim otherwise.";

        public AiChatService(HttpClient httpClient, IConfiguration configuration, ILogger<AiChatService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(List<AiChatMessageViewModel> history, string userMessage)
        {
            var endpoint = _configuration["AiSettings:Endpoint"];
            var apiKey = _configuration["AiSettings:ApiKey"];
            var model = _configuration["AiSettings:Model"];

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("AiSettings not configured");
                return "المساعد مش متاح دلوقتي.";
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var messages = new List<object> { new { role = "system", content = SystemPrompt } };
            foreach (var msg in history.TakeLast(10))
            {
                messages.Add(new { role = msg.Role, content = msg.Text });
            }

            var requestBody = new { model, messages, max_tokens = 300 };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI API error {StatusCode}: {Body}", response.StatusCode, responseText);
                    return "معلش، حصلت مشكلة، جرب تاني.";
                }

                using var doc = JsonDocument.Parse(responseText);
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return string.IsNullOrWhiteSpace(reply) ? "معلش، ممكن تعيد صياغة السؤال؟" : reply.Trim();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "AI API timeout");
                return "المساعد بياخد وقت أكتر من المتوقع، جرب تاني.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI API network error");
                return "معلش، مش قادر أتواصل مع المساعد دلوقتي.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "AI API returned malformed JSON");
                return "معلش، حصلت مشكلة في الرد، جرب تاني.";
            }
        }
    }
}