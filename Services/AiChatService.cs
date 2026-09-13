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
    // الكلاس الوحيد في المشروع اللي بيكلم الـ AI API.
    // ⚠️ ملحوظة مهمة: الكود ده مبني على شكل استجابة OpenAI "Chat Completions"
    // (choices[0].message.content). لو المشروع فعلاً هيستخدم "Responses API"
    // (endpoint: /v1/responses)، شكل الـ JSON بتاع الرد مختلف، ولازم تتأكد
    // من الشكل الفعلي وتقولي عشان أظبط الـ parsing بالظبط - النقطة دي
    // معلقة من الـ Audit ولسه محتاجة تأكيد منك.
    public class AiChatService : IAiChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiChatService> _logger;

        private const string SystemPrompt =
            "إنت مساعد لياقة بدنية عام جوه موقع جيم. قدّم معلومات عامة عن التمارين والتغذية بس. " +
            "وضّح دايمًا إنك مش بديل عن استشارة طبية أو استشارة مدرب مختص. " +
            "معندكش أي وصول لبيانات المشروع أو الداتابيز (لا أعضاء ولا اشتراكات ولا مدفوعات ولا خطط حقيقية)، " +
            "فمتحاولش تجاوب على أسئلة عن بيانات حقيقية جوه النظام أو تدّعي إنك عارفها.";

        public AiChatService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AiChatService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(List<AiChatMessageViewModel> history, string userMessage)
        {
            // القيم دي بتيجي من appsettings.json / User Secrets / Environment Variables فقط.
            // مفيش أي مفتاح API متكتوب هنا في الكود أبدًا.
            var endpoint = _configuration["AiSettings:Endpoint"];
            var apiKey = _configuration["AiSettings:ApiKey"];
            var model = _configuration["AiSettings:Model"];

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("AiSettings مش متظبطة (Endpoint/ApiKey مفقودين)");
                return "المساعد مش متاح دلوقتي، حاول تاني بعدين.";
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "من فضلك اكتب رسالة.";
            }

            var client = _httpClientFactory.CreateClient("AiApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var messages = new List<object>
            {
                new { role = "system", content = SystemPrompt }
            };

            foreach (var msg in history.TakeLast(10))
            {
                messages.Add(new { role = msg.Role, content = msg.Text });
            }

            var requestBody = new
            {
                model,
                messages,
                max_tokens = 300
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // بنسجل التفاصيل في الـ Log بس، ومنكشفهاش للمستخدم
                    _logger.LogError("AI API error {StatusCode}: {Body}", response.StatusCode, responseText);
                    return "معلش، حصلت مشكلة وأنا بحاول أرد، جرب تاني.";
                }

                using var doc = JsonDocument.Parse(responseText);
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return string.IsNullOrWhiteSpace(reply)
                    ? "معلش، مقدرتش أفهم قصدك، ممكن تعيد صياغة السؤال؟"
                    : reply.Trim();
            }
            catch (Exception ex)
            {
                // مفيش تفاصيل الـ Exception بترجع للمستخدم أبدًا
                _logger.LogError(ex, "فشل الاتصال بالـ AI API");
                return "معلش، مش قادر أتواصل مع المساعد دلوقتي.";
            }
        }
    }
}