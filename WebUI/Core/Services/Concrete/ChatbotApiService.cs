using System.Net.Http.Json;
using System.Text.Json;
using WebUI.Core.Services.Abstract;
using WebUI.Models.DTOs;

namespace WebUI.Core.Services.Concrete
{
    public class ChatbotApiService : IChatbotApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatbotApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<ChatbotResponseDto> GetPreTicketSolutionAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return new ChatbotResponseDto { Success = false, Solution = "" };
            }

            try
            {
                var baseUrl = _configuration["DmoChatbot:BaseUrl"] ?? _configuration["ChatbotService:BaseUrl"] ?? "http://127.0.0.1:8000";
                var apiKey = _configuration["DmoChatbot:ApiKey"] ?? _configuration["ChatbotService:ApiKey"] ?? "";

                var client = _httpClientFactory.CreateClient("ChatbotClient");
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
                client.Timeout = TimeSpan.FromSeconds(180);

                var requestId = $"ticket-ui-{Guid.NewGuid():N}";
                using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/chat");

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("X-API-Key", apiKey);
                }
                request.Headers.Add("X-Trace-ID", Guid.NewGuid().ToString("N"));

                var payload = new
                {
                    question = prompt,
                    client_request_id = requestId
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
                request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = await response.Content.ReadFromJsonAsync<ChatbotResponseDto>(options);

                    if (result != null && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        result.Success = true;
                        
                        var formattedSolution = result.Answer;
                        if (result.Citations != null && result.Citations.Count > 0)
                        {
                            formattedSolution += "<div style='margin-top:8px; font-size:11.5px; color:#6b7280; border-top:1px dashed #e5e7eb; padding-top:6px;'><strong>📚 Kaynaklar:</strong><ul style='margin:4px 0 0 16px; padding:0;'>";
                            foreach (var cite in result.Citations)
                            {
                                var label = !string.IsNullOrWhiteSpace(cite.CitationLabel) ? cite.CitationLabel : cite.SourceFile;
                                if (!string.IsNullOrWhiteSpace(cite.SourceUrl) && (cite.SourceUrl.StartsWith("http://") || cite.SourceUrl.StartsWith("https://")))
                                {
                                    formattedSolution += $"<li><a href='{cite.SourceUrl}' target='_blank' style='color:#4f46e5; text-decoration:underline;'>{label}</a></li>";
                                }
                                else
                                {
                                    formattedSolution += $"<li>{label}</li>";
                                }
                            }
                            formattedSolution += "</ul></div>";
                        }

                        result.Solution = formattedSolution;
                        return result;
                    }
                }
                else
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    return new ChatbotResponseDto 
                    { 
                        Success = false, 
                        SufficientEvidence = false,
                        Solution = $"<span style='color:#dc2626;'>⚠️ <strong>Chatbot API Uyarısı (HTTP Status {(int)response.StatusCode}):</strong> Sunucu yanıtı: <code>{errBody}</code></span>" 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ChatbotResponseDto 
                { 
                    Success = false, 
                    SufficientEvidence = false,
                    Solution = $"<span style='color:#dc2626;'>⚠️ <strong>Ağ Bağlantı Hatası:</strong> <code>{ex.Message}</code><br/>Furkan'ın bilgisayarına (<code>10.170.222.102:8000</code>) ulaşılamadı. Lütfen Furkan'ın bilgisayarında Güvenlik Duvarı (Firewall) izninin açık olduğunu ve Chatbot'un <code>0.0.0.0</code> üzerinde çalıştığını kontrol ediniz.</span>" 
                };
            }

            return new ChatbotResponseDto { Success = false, Solution = "" };
        }
    }
}
