using System.Collections.Generic;
using System.Threading.Tasks;
using GymMvc.ViewModels;

namespace GymMvc.Services
{
    public interface IAiChatService
    {
        Task<string> GetReplyAsync(List<AiChatMessageViewModel> history, string userMessage);
    }
}