namespace GymMvc.Services
{
    public interface IAiChatService
    {
        Task<string> GetResponseAsync(string message);
    }
}