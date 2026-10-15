using Core.Entities.DTOs;
using Core.Utilities.Results;

namespace Business.Abstract;

public interface ITicketAiService
{
    /// <summary>
    /// Talep başlığı ve açıklamasını Gemini AI ile analiz eder.
    /// </summary>
    Task<IDataResult<AiTicketAnalysisResponseDto>> AnalyzeTicketAsync(string title, string description);
}
