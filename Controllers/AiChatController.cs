using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GymMvc.Helpers;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // ⚠️ افتراض معماري: الشات هنا Controller مستقل (مش جوه MemberController بتاع محمد).
    // لو قرارك النهائي إنه يتنقل جوه MemberController زي دايجرام البرومبت الجديد،
    // ده لازم تنسيق صريح مع محمد الأول لأنه ملفه.
    [Route("AiChat")]
    public class AiChatController : Controller
    {
        private readonly IAiChatService _aiChatService;
        private const string SessionKey = "AiChatHistory";
        private const int MaxStoredMessages = 20;

        public AiChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public class SendMessageRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        // POST: /AiChat/SendMessage
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "الرسالة فاضية" });
            }

            var history = HttpContext.Session.GetObject<List<AiChatMessageViewModel>>(SessionKey)
                          ?? new List<AiChatMessageViewModel>();

            history.Add(new AiChatMessageViewModel { Role = "user", Text = request.Message.Trim() });

            var reply = await _aiChatService.GetReplyAsync(history, request.Message.Trim());

            history.Add(new AiChatMessageViewModel { Role = "assistant", Text = reply });

            if (history.Count > MaxStoredMessages)
            {
                history = history.Skip(history.Count - MaxStoredMessages).ToList();
            }

            HttpContext.Session.SetObject(SessionKey, history);

            return Json(new { reply });
        }

        // POST: /AiChat/ClearChat
        [HttpPost("ClearChat")]
        public IActionResult ClearChat()
        {
            HttpContext.Session.Remove(SessionKey);
            return Json(new { success = true });
        }
    }
}