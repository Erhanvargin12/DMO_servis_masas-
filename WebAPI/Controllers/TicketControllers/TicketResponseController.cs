using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketResponseController : ControllerBase
{
    private readonly ITicketResponseService _ticketResponseService;
    public TicketResponseController(ITicketResponseService ticketResponseService)
    {
        _ticketResponseService = ticketResponseService;
    }


    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketResponseService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketResponseService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(TicketResponseDto dto)
    {
        var result = await _ticketResponseService.AddAsync(dto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(TicketResponseDto dto)
    {
        var entity = new Response();

        var result = await _ticketResponseService.UpdateAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(TicketResponseDto dto)
    {
        var entity = new Response();

        var result = await _ticketResponseService.DeleteAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
