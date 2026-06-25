using Business.Abstract;
using Core.Entities.DTOs;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuOperationClaimController : ControllerBase
{
    private readonly IMenuOperationClaimService _menuOperationClaimService;

    public MenuOperationClaimController(IMenuService menuService, IMenuOperationClaimService menuOperationClaimService)
    {
        _menuOperationClaimService = menuOperationClaimService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _menuOperationClaimService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbymenuid/{id}")]
    public async Task<IActionResult> GetByMenuId(int id)
    {
        var result = await _menuOperationClaimService.GetByMenuIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(MenuOperationClaim claim)
    {
        var result = await _menuOperationClaimService.AddAsync(claim);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(MenuOperationClaimUpdateDto menuOperationClaimUpdateDto)
    {
        var result = await _menuOperationClaimService.UpdateAsync(menuOperationClaimUpdateDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(MenuOperationClaim claim)
    {
        var result = await _menuOperationClaimService.DeleteAsync(claim);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
