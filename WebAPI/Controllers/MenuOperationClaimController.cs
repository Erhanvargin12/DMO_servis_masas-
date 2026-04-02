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
    public IActionResult GetAll()
    {
        var result = _menuOperationClaimService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbymenuid/{id}")]
    public IActionResult GetByMenuId(int id)
    {
        var result = _menuOperationClaimService.GetByMenuId(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public IActionResult Add(MenuOperationClaim claim)
    {
        var result = _menuOperationClaimService.Add(claim);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(MenuOperationClaimUpdateDto menuOperationClaimUpdateDto)
    {
        var result = _menuOperationClaimService.Update(menuOperationClaimUpdateDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(MenuOperationClaim claim)
    {
        var result = _menuOperationClaimService.Delete(claim);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
