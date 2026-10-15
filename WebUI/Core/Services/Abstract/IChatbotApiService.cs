using WebUI.Models.DTOs;

namespace WebUI.Core.Services.Abstract
{
    public interface IChatbotApiService
    {
        Task<ChatbotResponseDto> GetPreTicketSolutionAsync(string prompt);
    }
}
