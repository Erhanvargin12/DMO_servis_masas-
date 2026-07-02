using Business.Abstract.IntegrationServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DmoApiController : ControllerBase
{
    private readonly IDmoMainService _dmoMainService;

    public DmoApiController(IDmoMainService dmoMainService)
    {
        _dmoMainService = dmoMainService;
    }

    [HttpGet("get-proxy-user/{id}")]
    public async Task<IActionResult> GetProxyUserByEbysId(string id)
    {
        var result = await _dmoMainService.GetDmoProxyUsersUserByEbysId(id);
        return Ok(result);
    }
}
