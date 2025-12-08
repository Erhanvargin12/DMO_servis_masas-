namespace WebUI.Models
{
    public class AccessTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}

