namespace WebUI.Models
{
    public class SettingsViewModel
    {
        // Tema Ayarları
        public string Theme { get; set; } = "light";
        public string AccentColor { get; set; } = "#0891b2";
        public string SidebarStyle { get; set; } = "dark";
        public bool CompactMode { get; set; } = false;
        
        // Font Ayarları
        public int FontSize { get; set; } = 14;
        public string FontFamily { get; set; } = "system";
        
        // Layout Ayarları
        public int SidebarWidth { get; set; } = 240;
        public bool SidebarCollapsed { get; set; } = false;
        public string MenuStyle { get; set; } = "default";
        
        // Görünüm Ayarları
        public int BorderRadius { get; set; } = 6;
        public bool ShowAnimations { get; set; } = true;
        public string ShadowIntensity { get; set; } = "medium";
        
        // Tablo Ayarları
        public int PageSize { get; set; } = 8;
        public bool CompactTables { get; set; } = false;
        public bool ShowTableBorders { get; set; } = true;
        
        // Bildirim Ayarları
        public string ToastPosition { get; set; } = "top-right";
        public int ToastDuration { get; set; } = 3000;
        public bool EnableSounds { get; set; } = false;
        
        // Dil ve Bölge
        public string Language { get; set; } = "tr";
        public string DateFormat { get; set; } = "DD.MM.YYYY";
        public string TimeFormat { get; set; } = "24h";
        
        // Erişilebilirlik
        public bool HighContrast { get; set; } = false;
        public bool ReducedMotion { get; set; } = false;
        public bool LargeText { get; set; } = false;
    }

    public class ThemePreset
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, string> Colors { get; set; } = new();
    }
}
