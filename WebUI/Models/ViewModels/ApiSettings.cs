namespace WebUI.Models.ViewModels
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "https://localhost:5001/api/";
        public int Timeout { get; set; } = 30;
    }
}

