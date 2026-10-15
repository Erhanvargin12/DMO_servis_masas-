using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
public class TicketCategoryController : ControllerBase
{
    private readonly ITicketCategoryService _categoryService;

    public TicketCategoryController(ITicketCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("getbranches/{categoryId}")]
    public async Task<IActionResult> GetBranches(int categoryId)
    {
        var result = await _categoryService.GetBranchesByCategoryAsync(categoryId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(TicketCategory category)
    {
        var result = await _categoryService.AddAsync(category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(TicketCategory category)
    {
        var result = await _categoryService.UpdateAsync(category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("updatebranches")]
    public async Task<IActionResult> UpdateBranches(TicketCategoryBranchUpdateDto dto)
    {
        var result = await _categoryService.UpdateBranchesAsync(dto.CategoryId, dto.BranchIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
