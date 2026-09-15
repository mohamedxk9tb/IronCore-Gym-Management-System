using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GymMvc.Helpers;
using GymMvc.ViewModels;

namespace GymMvc.ViewComponents
{
    public class AiChatWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var history = HttpContext.Session.GetObject<List<AiChatMessageViewModel>>(GymMvc.Helpers.SessionExtensions.AiChatHistoryKey)
                          ?? new List<AiChatMessageViewModel>();

            return View(new AiChatViewModel { Messages = history });
        }
    }
}