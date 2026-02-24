using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebUI.Models;
using WebUI.Services.Abstract;

namespace WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserApiService _userApiService;
        private readonly IRoleApiService _roleApiService;

        public HomeController(ILogger<HomeController> logger, IUserApiService userApiService, IRoleApiService roleApiService)
        {
            _logger = logger;
            _userApiService = userApiService;
            _roleApiService = roleApiService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userApiService.GetAllAsync();
            var roles = await _roleApiService.GetAllAsync();

            var model = new DashboardViewModel
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.Status),
                TotalRoles = roles.Count
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
