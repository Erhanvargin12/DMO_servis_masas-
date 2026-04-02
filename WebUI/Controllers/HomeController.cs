using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;

namespace WebUI.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public HomeController(IApiClient apiClient) : base(apiClient) { }

        public async Task<IActionResult> Index()
        {
            var users = await SendRequest<List<User>>("user/getall");
            var roles = await SendRequest<List<OperationClaim>>("operationclaim/getall");

            var model = new DashboardViewModel
            {
                TotalUsers = users.Data.Count,
                ActiveUsers = users.Data.Count(u => u.Status),
                TotalRoles = roles.Data.Count
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
