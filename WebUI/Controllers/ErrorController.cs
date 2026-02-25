using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    /// <summary>
    /// HTTP hata sayfalarını yönetir.
    /// Program.cs'de UseStatusCodePagesWithReExecute ve UseExceptionHandler ile birlikte çalışır.
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// HTTP durum kodu tabanlı hata sayfaları (403, 404 vb.)
        /// </summary>
        [Route("/Error/{statusCode:int}")]
        public new IActionResult StatusCode(int statusCode)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var originalPath = feature?.OriginalPath ?? "";

            _logger.LogWarning("HTTP {StatusCode} — Yol: {Path}", statusCode, originalPath);

            return statusCode switch
            {
                403 => View("403"),
                404 => View("404"),
                _   => View("500")
            };
        }

        /// <summary>
        /// Beklenmeyen sunucu hataları (500)
        /// </summary>
        [Route("/Error")]
        public IActionResult ServerError()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (feature?.Error != null)
            {
                _logger.LogError(feature.Error,
                    "İşlenmeyen sunucu hatası — Yol: {Path}", feature.Path);
            }

            return View("500");
        }
    }
}

