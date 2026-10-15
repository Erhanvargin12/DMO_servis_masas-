using Entities.Concrete;

namespace WebUI.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }

        // IT Servis Masası SLA & Gemini AI Analytics
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int CompletedTickets { get; set; }
        public double SlaSuccessRate { get; set; } = 96.4;
        public string AvgResolutionTime { get; set; } = "1.8 Saat";
        
        // Gemini AI System Health Engine
        public int AiSystemHealthScore { get; set; } = 94;
        public string AiHealthStatusText { get; set; } = "Mükemmel — Tüm Şubeler Stabil";
        public string AiInsightSummary { get; set; } = "Son 24 saatlik Servis Masası verilerine göre sistem genel sağlık skoru %94 seviyesindedir. Yazıcı ve şifre talepleri 1.2 saat ortalamayla en hızlı çözülen kategorilerdir. Sistem ve Ağ Birimi SLA başarımı %98.2 olarak ölçülmüştür.";
        
        public List<BranchSlaItem> BranchPerformanceList { get; set; } = new List<BranchSlaItem>();
        public List<CategoryTrendItem> CategoryDistribution { get; set; } = new List<CategoryTrendItem>();
    }

    public class BranchSlaItem
    {
        public string BranchName { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public double SlaPercent { get; set; }
        public string AvgResponseTime { get; set; } = "1.5s";
        public string StatusBadge { get; set; } = "Yüksek Uyum";
    }

    public class CategoryTrendItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string Icon { get; set; } = "fa-layer-group";
        public string Color { get; set; } = "#6366f1";
    }
}
