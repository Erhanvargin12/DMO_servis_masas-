using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketTransferController : ControllerBase
{
    private readonly ITicketTransferService _ticketTransferService;
    public TicketTransferController(ITicketTransferService ticketTransferService)
    {
        _ticketTransferService = ticketTransferService;
    }


    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketTransferService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketTransferService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = await _ticketTransferService.AddAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = await _ticketTransferService.UpdateAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = await _ticketTransferService.DeleteAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
