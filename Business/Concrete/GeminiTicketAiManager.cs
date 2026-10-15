using Business.Abstract;
using Core.Entities.DTOs;
using Core.Utilities.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Business.Concrete;

public class GeminiTicketAiManager : ITicketAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiTicketAiManager> _logger;

    public GeminiTicketAiManager(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiTicketAiManager> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IDataResult<AiTicketAnalysisResponseDto>> AnalyzeTicketAsync(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description))
        {
            _logger.LogWarning("AI Analizi iptal edildi: Başlık ve açıklama boş.");
            return new ErrorDataResult<AiTicketAnalysisResponseDto>("Analiz edilecek talep içeriği bulunamadı.");
        }

        try
        {
            var apiKey = _configuration["GeminiApi:ApiKey"] ?? _configuration["Gemini:ApiKey"];
            var model = _configuration["GeminiApi:Model"] ?? _configuration["Gemini:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Gemini API Key yapılandırmada bulunamadı (GeminiApi:ApiKey / Gemini:ApiKey).");
                return new ErrorDataResult<AiTicketAnalysisResponseDto>("AI Yapılandırma hatası: ApiKey bulunamadı.");
            }

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var promptText = $$"""
                Sen bir kurumsal destek talebi analiz asistanısın. Aşağıdaki talebi incele ve SADECE saf JSON formatında yanıt dön. Başka açıklama veya markdown ekleme.

                JSON Şeması:
                {
                  "categoryName": "Yazılım | Donanım | Ağ | Erişim | Diğer",
                  "priority": "Düşük | Orta | Yüksek | Kritik",
                  "summary": "1-2 cümlelik kısa özet",
                  "sentiment": "Olumlu | Nötr | Olumsuz | Öfkeli",
                  "suggestedAction": "Çözüm için önerilen ilk adım",
                  "keywords": ["anahtar1", "anahtar2"]
                }

                Talep Başlığı: {{title}}
                Talep Açıklaması: {{description}}
                """;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptText }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var client = _httpClientFactory.CreateClient("GeminiClient");
            _logger.LogInformation("Gemini API isteği gönderiliyor. Talep Başlığı: {Title}", title);

            var response = await client.PostAsync(endpoint, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API HTTP Hatası: {StatusCode} - {Response}", response.StatusCode, errorResponse);
                return new ErrorDataResult<AiTicketAnalysisResponseDto>("AI servisine erişilirken bir hata oluştu.");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(responseString);

            var rawAiJson = jObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrWhiteSpace(rawAiJson))
            {
                _logger.LogWarning("Gemini API yanıtı boş veya geçersiz formatta geldi.");
                return new ErrorDataResult<AiTicketAnalysisResponseDto>("AI yanıt üretemedi.");
            }

            var analysisResult = JsonConvert.DeserializeObject<AiTicketAnalysisResponseDto>(rawAiJson);

            if (analysisResult == null)
            {
                _logger.LogError("Gemini API JSON yanıtı AiTicketAnalysisResponseDto nesnesine ayrıştırılamadı.");
                return new ErrorDataResult<AiTicketAnalysisResponseDto>("AI yanıtı işlenemedi.");
            }

            _logger.LogInformation("Gemini API analizi başarıyla tamamlandı. Tahmin Edilen Kategori: {Category}, Öncelik: {Priority}",
                analysisResult.CategoryName, analysisResult.Priority);

            return new SuccessDataResult<AiTicketAnalysisResponseDto>(analysisResult, "Talep analizi başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API talep analizi sırasında beklenmeyen hata oluştu. Başlık: {Title}", title);
            return new ErrorDataResult<AiTicketAnalysisResponseDto>("AI analizi sırasında bir hata oluştu: " + ex.Message);
        }
    }
}
