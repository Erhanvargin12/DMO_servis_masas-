using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;


[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getticketdtobyid/{id}")]
    public async Task<IActionResult> GetTicketDtoById(int id)
    {
        var result = await _ticketService.GetTicketDtoByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }


    [HttpPost("add")]
    public async Task<IActionResult> Add(TicketDto ticket)
    {
        ticket.Ticket.TicketNo = $"TKT-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        ticket.Ticket.CreatedAt = DateTime.Now;
        ticket.Ticket.StatusId ??= 1;

        var result = await _ticketService.AddAsync(ticket);
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(TicketDto dto)
    {
        var entity = new Ticket()
        {
            Id = dto.Ticket.Id,
            TicketNo = dto.Ticket.TicketNo,
            Title = dto.Ticket.Title,
            Description = dto.Ticket.Description,
            CreatedById = dto.Ticket.CreatedById,
            CurrentBranchId = dto.Ticket.CurrentBranchId,
            AssignedUserId = dto.Ticket.AssignedUserId,
            StatusId = dto.Ticket.StatusId,
            PriorityId = dto.Ticket.PriorityId,
            CreatedAt = dto.Ticket.CreatedAt,
            UpdatedAt = dto.Ticket.UpdatedAt,
            ClosedAt = dto.Ticket.ClosedAt,
        };
        var result = await _ticketService.UpdateAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _ticketService.DeleteAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
