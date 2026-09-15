using System.Collections.Generic;

namespace GymMvc.ViewModels
{
    // Level 1 - محفوظة في الـ Session بس
    public class AiChatViewModel
    {
        public List<AiChatMessageViewModel> Messages { get; set; } = new();

        // بيتعرض في الـ UI كتنبيه ثابت: المساعد ده معلومات عامة مش نصيحة طبية
        public string Disclaimer { get; set; } =
            "هذا المساعد يقدّم معلومات عامة عن اللياقة البدنية فقط، وليس بديلاً عن استشارة طبية أو استشارة مدرب مختص.";
    }

    public class AiChatMessageViewModel
    {
        public string Role { get; set; } = string.Empty; // "user" أو "assistant"
        public string Text { get; set; } = string.Empty;
    }
}