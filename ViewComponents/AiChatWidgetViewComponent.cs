using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GymMvc.Helpers;
using GymMvc.ViewModels;

namespace GymMvc.ViewComponents
{
    public class AiChatWidgetViewComponent : ViewComponent
    {
        private const string SessionKey = "AiChatHistory";

        public IViewComponentResult Invoke()
        {
            var history = HttpContext.Session.GetObject<List<AiChatMessageViewModel>>(SessionKey)
                          ?? new List<AiChatMessageViewModel>();

            var model = new AiChatViewModel { Messages = history };

            return View(model);
        }
    }
}