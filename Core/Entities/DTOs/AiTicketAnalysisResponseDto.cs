using Core.Entities;
using Newtonsoft.Json;

namespace Core.Entities.DTOs;

public class AiTicketAnalysisResponseDto : IDto
{
    [JsonProperty("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonProperty("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [JsonProperty("suggestedAction")]
    public string SuggestedAction { get; set; } = string.Empty;

    [JsonProperty("keywords")]
    public List<string> Keywords { get; set; } = new();
}
