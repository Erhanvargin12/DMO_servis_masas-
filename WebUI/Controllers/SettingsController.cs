using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;

namespace WebUI.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Kullanıcının mevcut ayarlarını localStorage'dan alacağız (client-side)
            var model = new SettingsViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveSettings([FromBody] SettingsViewModel settings)
        {
            try
            {
                // Ayarlar client-side localStorage'da saklanacak
                // İsteğe bağlı olarak DB'ye de kaydedilebilir
                return Json(new { success = true, message = "Ayarlar başarıyla kaydedildi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayarlar kaydedilirken hata oluştu");
                return Json(new { success = false, message = "Ayarlar kaydedilemedi" });
            }
        }

        [HttpPost]
        public IActionResult ResetSettings()
        {
            try
            {
                return Json(new { success = true, message = "Ayarlar sıfırlandı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayarlar sıfırlanırken hata oluştu");
                return Json(new { success = false, message = "Ayarlar sıfırlanamadı" });
            }
        }

        [HttpGet]
        public IActionResult ExportSettings()
        {
            try
            {
                // Ayarları JSON olarak export et
                var settings = new SettingsViewModel();
                return Json(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayarlar export edilirken hata oluştu");
                return Json(new { success = false, message = "Export başarısız" });
            }
        }
    }
}
