using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _companyService.GetAllAsync();
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _companyService.GetByIdAsync(id);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("getbymail")]
    public async Task<IActionResult> GetByMail([FromQuery] string mail)
    {
        var result = await _companyService.GetByMailAsync(mail);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CompanyLoginDto dto)
    {
        var result = await _companyService.LoginAsync(dto.Mail, dto.Password);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] Company company)
    {
        var result = await _companyService.AddAsync(company);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}

public class CompanyLoginDto
{
    public string Mail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
