using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _branchService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("resolve/{departmentId}")]
    public async Task<IActionResult> Resolve(string departmentId)
    {
        var result = await _branchService.GetByDepartmentIdAsync(departmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
