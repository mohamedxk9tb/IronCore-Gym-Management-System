using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymMvc.Helpers;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    [Authorize(Roles = "Member")]
    [Route("AiChat")]
    public class AiChatController : Controller
    {
        private readonly IAiChatService _aiChatService;
        private readonly IAntiforgery _antiforgery;
        private const int MaxStoredMessages = 20;
        private const int MaxMessageLength = 500;

        public AiChatController(IAiChatService aiChatService, IAntiforgery antiforgery)
        {
            _aiChatService = aiChatService;
            _antiforgery = antiforgery;
        }

        public class SendMessageRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (!await IsValidCsrfAsync())
            {
                return BadRequest(new { error = "Invalid request token" });
            }

            var message = request?.Message?.Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { error = "الرسالة فاضية" });
            }

            if (message.Length > MaxMessageLength)
            {
                return BadRequest(new { error = $"الرسالة طويلة أكتر من {MaxMessageLength} حرف" });
            }

            var history = HttpContext.Session.GetObject<List<AiChatMessageViewModel>>(SessionExtensions.AiChatHistoryKey)
                          ?? new List<AiChatMessageViewModel>();

            history.Add(new AiChatMessageViewModel { Role = "user", Text = message });

            var reply = await _aiChatService.GetReplyAsync(history, message);

            history.Add(new AiChatMessageViewModel { Role = "assistant", Text = reply });

            if (history.Count > MaxStoredMessages)
            {
                history = history.Skip(history.Count - MaxStoredMessages).ToList();
            }

            HttpContext.Session.SetObject(SessionExtensions.AiChatHistoryKey, history);

            return Json(new { reply });
        }

        [HttpPost("ClearChat")]
        public async Task<IActionResult> ClearChat()
        {
            if (!await IsValidCsrfAsync())
            {
                return BadRequest(new { error = "Invalid request token" });
            }

            HttpContext.Session.Remove(SessionExtensions.AiChatHistoryKey);
            return Json(new { success = true });
        }

        // Validates the antiforgery token from the request header. This works for
        // AJAX/JSON POSTs (unlike [ValidateAntiForgeryToken], which expects a form field).
        // Requires Program.cs to configure a header name, e.g.:
        // services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
        // The View's JS must send that header with the token from @Html.AntiForgeryToken().
        private async Task<bool> IsValidCsrfAsync()
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
                return true;
            }
            catch (AntiforgeryValidationException)
            {
                return false;
            }
        }
    }
}