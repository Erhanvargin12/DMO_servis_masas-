using Newtonsoft.Json;
using System.Text;
using WebUI.Areas.Talepler.Models;
using WebUI.Services.Abstract;

namespace WebUI.Services.Concrete;

public class TalepApiService : ITalepApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TalepApiService> _logger;

    public TalepApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<TalepApiService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, uri);
        var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (content != null)
            req.Content = content;
        return req;
    }

    private async Task<(bool Success, string? Message, T? Data)> SendAsync<T>(HttpRequestMessage req)
    {
        try
        {
            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiDataResponse<T>>(body);
                return (true, result?.Message, result != null ? result.Data : default);
            }

            var err = JsonConvert.DeserializeObject<ApiResponse>(body);
            return (false, err?.Message ?? "Bir hata oluştu", default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API isteği başarısız");
            return (false, "Bağlantı hatası oluştu", default);
        }
    }

    private StringContent Json(object obj) =>
        new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

    // ── Talep ─────────────────────────────────────────────────────────────

    public async Task<List<TalepViewModel>> GetAllAsync(int? olusturanId = null, int? atananId = null, int? durum = null)
    {
        var query = new List<string>();
        if (olusturanId.HasValue) query.Add($"olusturanId={olusturanId}");
        if (atananId.HasValue) query.Add($"atananId={atananId}");
        if (durum.HasValue) query.Add($"durum={durum}");
        var url = "talep/getall" + (query.Any() ? "?" + string.Join("&", query) : "");

        var req = CreateRequest(HttpMethod.Get, url);
        var (_, _, data) = await SendAsync<List<TalepViewModel>>(req);
        return data ?? new List<TalepViewModel>();
    }

    public async Task<TalepViewModel?> GetByIdAsync(int id)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/getbyid/{id}");
        var (_, _, data) = await SendAsync<TalepViewModel>(req);
        return data;
    }

    public async Task<(bool Success, string? Message)> AddAsync(TalepOlusturViewModel model, int kullaniciId)
    {
        var dto = new
        {
            Baslik = model.Baslik,
            Aciklama = model.Aciklama,
            KategoriId = model.KategoriId,
            Oncelik = model.Oncelik,
            OlusturanKullaniciId = kullaniciId
        };
        var req = CreateRequest(HttpMethod.Post, "talep/add", Json(dto));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, string? Message)> UpdateAsync(TalepViewModel model)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/update", Json(model));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, string? Message)> DeleteAsync(int id)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/delete", Json(new { Id = id }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, string? Message)> KapatAsync(int talepId, int kullaniciId)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/kapat", Json(new { TalepId = talepId, KullaniciId = kullaniciId }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, string? Message)> IptalAsync(int talepId, int kullaniciId)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/iptal", Json(new { TalepId = talepId, KullaniciId = kullaniciId }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, string? Message)> DurumGuncelleAsync(int talepId, int durum, int kullaniciId)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/durum", Json(new { TalepId = talepId, Durum = durum, KullaniciId = kullaniciId }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    // ── Yanıt ─────────────────────────────────────────────────────────────

    public async Task<List<TalepYanitViewModel>> GetYanitlarAsync(int talepId)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/yanitlar/{talepId}");
        var (_, _, data) = await SendAsync<List<TalepYanitViewModel>>(req);
        return data ?? new List<TalepYanitViewModel>();
    }

    public async Task<(bool Success, string? Message)> YanitEkleAsync(int talepId, int kullaniciId, string icerik)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/yanitekle",
            Json(new { TalepId = talepId, KullaniciId = kullaniciId, Icerik = icerik }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    // ── Chat ──────────────────────────────────────────────────────────────

    public async Task<List<TalepChatViewModel>> GetChatAsync(int talepId)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/chat/{talepId}");
        var (_, _, data) = await SendAsync<List<TalepChatViewModel>>(req);
        return data ?? new List<TalepChatViewModel>();
    }

    public async Task<(bool Success, string? Message)> ChatEkleAsync(int talepId, int kullaniciId, string mesaj)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/chatekle",
            Json(new { TalepId = talepId, GonderenKullaniciId = kullaniciId, Mesaj = mesaj }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    // ── Atama ─────────────────────────────────────────────────────────────

    public async Task<List<TalepAtamaViewModel>> GetAtamalarAsync(int talepId)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/atamalar/{talepId}");
        var (_, _, data) = await SendAsync<List<TalepAtamaViewModel>>(req);
        return data ?? new List<TalepAtamaViewModel>();
    }

    public async Task<(bool Success, string? Message)> KullaniciAtaAsync(int talepId, int atayanId, int atananId, string? aciklama)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/ata",
            Json(new { TalepId = talepId, AtayanKullaniciId = atayanId, AtananKullaniciId = atananId, Aciklama = aciklama }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    // ── Sevk ──────────────────────────────────────────────────────────────

    public async Task<List<TalepSevkViewModel>> GetSevklerAsync(int talepId)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/sevkler/{talepId}");
        var (_, _, data) = await SendAsync<List<TalepSevkViewModel>>(req);
        return data ?? new List<TalepSevkViewModel>();
    }

    public async Task<(bool Success, string? Message)> SevketAsync(int talepId, int sevkEdenId, int? hedefKullaniciId, int? hedefRolId, string? aciklama)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/sevket",
            Json(new
            {
                TalepId = talepId,
                SevkEdenKullaniciId = sevkEdenId,
                HedefKullaniciId = hedefKullaniciId,
                HedefRolId = hedefRolId,
                Aciklama = aciklama
            }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    // ── Ek ────────────────────────────────────────────────────────────────

    public async Task<List<TalepEkViewModel>> GetEklerAsync(int talepId)
    {
        var req = CreateRequest(HttpMethod.Get, $"talep/ekler/{talepId}");
        var (_, _, data) = await SendAsync<List<TalepEkViewModel>>(req);
        return data ?? new List<TalepEkViewModel>();
    }

    public async Task<(bool Success, string? Message)> EkEkleAsync(int talepId, int? yanitId, int kullaniciId, IFormFile dosya)
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(talepId.ToString()), "TalepId");
            form.Add(new StringContent(kullaniciId.ToString()), "KullaniciId");
            if (yanitId.HasValue)
                form.Add(new StringContent(yanitId.Value.ToString()), "YanitId");

            var stream = dosya.OpenReadStream();
            form.Add(new StreamContent(stream), "Dosya", dosya.FileName);

            var req = new HttpRequestMessage(HttpMethod.Post, "talep/ekekle") { Content = form };
            if (!string.IsNullOrEmpty(token))
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return (true, "Dosya yüklendi");

            var err = JsonConvert.DeserializeObject<ApiResponse>(body);
            return (false, err?.Message ?? "Dosya yüklenemedi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya yüklenirken hata");
            return (false, "Bağlantı hatası");
        }
    }

    public async Task<(bool Success, string? Message)> EkSilAsync(int ekId, int kullaniciId)
    {
        var req = CreateRequest(HttpMethod.Post, "talep/eksil", Json(new { EkId = ekId, KullaniciId = kullaniciId }));
        var (success, message, _) = await SendAsync<object>(req);
        return (success, message);
    }

    public async Task<(bool Success, byte[]? Data, string? DosyaAdi, string? ContentType)> EkIndirAsync(int ekId)
    {
        try
        {
            var req = CreateRequest(HttpMethod.Get, $"talep/ekindir/{ekId}");
            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode)
                return (false, null, null, null);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var dosyaAdi = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "dosya";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            return (true, bytes, dosyaAdi, contentType);
        }
        catch
        {
            return (false, null, null, null);
        }
    }

    // ── Kategori ──────────────────────────────────────────────────────────

    public async Task<List<TalepKategoriViewModel>> GetKategorilerAsync()
    {
        var req = CreateRequest(HttpMethod.Get, "talep/kategoriler");
        var (_, _, data) = await SendAsync<List<TalepKategoriViewModel>>(req);
        return data ?? new List<TalepKategoriViewModel>();
    }
}

// ── Shared Response Models ────────────────────────────────────────────────

file class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

file class ApiDataResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

