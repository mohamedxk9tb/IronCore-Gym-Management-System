using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymMvc.Helpers;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // AI chat is member-only. Change this if public chat is the final decision
    // (and add rate limiting if so).
    [Authorize(Roles = "Member")]
    [Route("AiChat")]
    public class AiChatController : Controller
    {
        private readonly IAiChatService _aiChatService;
        private const int MaxStoredMessages = 20;
        private const int MaxMessageLength = 500;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public class SendMessageRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
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
        public IActionResult ClearChat()
        {
            HttpContext.Session.Remove(SessionExtensions.AiChatHistoryKey);
            return Json(new { success = true });
        }
    }
}