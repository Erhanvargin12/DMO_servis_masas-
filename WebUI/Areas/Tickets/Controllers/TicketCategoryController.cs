using Core.Communication;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Concrete.Ticket;
using Microsoft.AspNetCore.Mvc;
using WebUI.Areas.Tickets.Models;
using WebUI.Models.DTOs;

namespace WebUI.Areas.Tickets.Controllers;

[Area("Tickets")]
public class TicketCategoryController : Controller
{
    private readonly IServiceCommunication _service;

    public TicketCategoryController(IServiceCommunication service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var categoriesResponse = await _service.GetAsync<DataResult<List<TicketCategory>>>("KIP", "ticketcategory/getall");
        var branchesResponse = await _service.GetAsync<DataResult<List<Branch>>>("KIP", "branch/getall");

        var categories = categoriesResponse?.Data ?? new List<TicketCategory>();
        var branches = branchesResponse?.Data ?? new List<Branch>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            categories = categories
                .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            (c.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        var items = new List<TicketCategoryItemViewModel>();
        foreach (var category in categories)
        {
            var branchResponse = await _service.GetAsync<DataResult<List<TicketCategoryBranch>>>(
                "KIP", $"ticketcategory/getbranches/{category.Id}");

            items.Add(new TicketCategoryItemViewModel
            {
                Category = category,
                BranchIds = branchResponse?.Data?
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.Id)
                    .Select(b => b.BranchId)
                    .ToList() ?? new List<int>()
            });
        }

        var model = new TicketCategoryListViewModel
        {
            Categories = items,
            Branches = branches,
            SearchTerm = search
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TicketCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            TempData["ErrorMessage"] = "Kategori adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        if (category.Id == 0)
            await _service.PostAsync<TicketCategory, DataResult<TicketCategory>>("KIP", "ticketcategory/add", category);
        else
            await _service.PostAsync<TicketCategory, DataResult<TicketCategory>>("KIP", "ticketcategory/update", category);

        TempData["SuccessMessage"] = category.Id == 0 ? "Kategori eklendi." : "Kategori güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignBranches(int categoryId, List<int>? selectedBranchIds, string? orderedBranchIds)
    {
        if (categoryId <= 0)
        {
            TempData["ErrorMessage"] = "Geçersiz kategori.";
            return RedirectToAction(nameof(Index));
        }

        List<int> branchIds;
        if (!string.IsNullOrWhiteSpace(orderedBranchIds))
        {
            branchIds = orderedBranchIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();
        }
        else
        {
            branchIds = selectedBranchIds ?? new List<int>();
        }

        await _service.PostAsync<TicketCategoryBranchUpdateDto, Result>(
            "KIP",
            "ticketcategory/updatebranches",
            new TicketCategoryBranchUpdateDto
            {
                CategoryId = categoryId,
                BranchIds = branchIds
            });

        TempData["SuccessMessage"] = "Şube ilişkileri kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            TempData["ErrorMessage"] = "Geçersiz kategori.";
            return RedirectToAction(nameof(Index));
        }

        await _service.DeleteAsync("KIP", $"ticketcategory/delete/{id}");
        TempData["SuccessMessage"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }
}
