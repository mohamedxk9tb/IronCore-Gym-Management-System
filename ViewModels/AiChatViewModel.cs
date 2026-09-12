namespace GymMvc.ViewModels
{
    public class AiChatViewModel
    {
        public List<AiChatMessageViewModel> Messages { get; set; } = new();
    }

    public class AiChatMessageViewModel
    {
        public string Role { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}