using Core.Utilities.Results;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Services.Abstract;
using WebUI.Core.Services.Concrete;
using WebUI.Models.ViewModels;

namespace WebUI.Controllers
{
    [Authorize]
    public class RolesController : BaseController
    {
        public RolesController(IApiClient apiClient) : base(apiClient){}
        
        public async Task<IActionResult> Index(string? search)
        {
            var allRoles = await SendRequest<List<OperationClaim>>("operationclaim/getall");
            var roles = allRoles?.Data ?? new List<OperationClaim>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                roles = roles.Where(r => (r.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                        (r.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            var model = new RoleListViewModel
            {
                Roles = roles,
                SearchTerm = search,
                TotalCount = roles.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(", ",ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            IDataResult<OperationClaim> result;

            if (model.Id == 0)
                result = await SendRequest<OperationClaim>("operationclaim/add",model,HttpMethod.Post);
            else
                result = await SendRequest<OperationClaim>("operationclaim/update", model, HttpMethod.Post);

            if (result.Success)
                TempData["SuccessMessage"] = result.Message;
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz rol";
                return RedirectToAction(nameof(Index));
            }

            var entity = await SendRequest<OperationClaim>($"operationclaim/getbyid/{id}");

            var result = await SendRequest<OperationClaim>($"operationclaim/delete", entity.Data, HttpMethod.Post);

            if (result.Success)
                TempData["SuccessMessage"] = result.Message;
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
