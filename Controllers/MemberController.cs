using System.Text.Json;
using GymMvc.Services;
using GymMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class MemberController : Controller
    {
        private readonly IAiChatService _aiChatService;

        private const string SessionKey = "AI_CHAT";

        public MemberController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Classes()
        {
            return View();
        }

        public IActionResult Bookings()
        {
            return View();
        }

        public IActionResult FitnessPlan()
        {
            return View();
        }

        public IActionResult Progress()
        {
            return View();
        }

        public IActionResult CheckIn()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Assistant()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Chat(
            [FromBody] AiChatRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    message = "Please enter a message."
                });
            }

            var history = GetChatHistory();

            history.Add(new AiChatMessageViewModel
            {
                Role = "user",
                Text = request.Message.Trim()
            });

            var aiResponse =
                await _aiChatService.GetResponseAsync(
                    request.Message.Trim());

            history.Add(new AiChatMessageViewModel
            {
                Role = "assistant",
                Text = aiResponse
            });

            SaveChatHistory(history);

            return Json(new
            {
                reply = aiResponse
            });
        }

        [HttpPost]
        public IActionResult ClearChat()
        {
            HttpContext.Session.Remove(SessionKey);

            return Json(new
            {
                success = true
            });
        }

        private List<AiChatMessageViewModel> GetChatHistory()
        {
            var json = HttpContext.Session.GetString(SessionKey);

            if (string.IsNullOrWhiteSpace(json))
                return new List<AiChatMessageViewModel>();

            try
            {
                return JsonSerializer.Deserialize<
                    List<AiChatMessageViewModel>>(json)
                    ?? new List<AiChatMessageViewModel>();
            }
            catch
            {
                return new List<AiChatMessageViewModel>();
            }
        }

        private void SaveChatHistory(
            List<AiChatMessageViewModel> history)
        {
            var json = JsonSerializer.Serialize(history);

            HttpContext.Session.SetString(
                SessionKey,
                json);
        }
    }

    public class AiChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}