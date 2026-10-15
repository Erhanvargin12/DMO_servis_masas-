using Business.Abstract.TicketServices;
using Core.Utilities.Results;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
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

    [HttpGet("get-ticket-by-user-id")]
    public async Task<IActionResult> GetTicketByUserId([FromQuery] int id)
    {
        try
        {
            if (id <= 0)
            {
                return Ok(new SuccessDataResult<List<Ticket>>(new List<Ticket>(), "Kullanıcı ID boş."));
            }

            var result = await _ticketService.GetAllAsync(t => t.CreatedById == id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return Ok(new ErrorDataResult<List<Ticket>>("Sorgulama hatası: " + ex.Message));
        }
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
    
    [HttpGet("get-tickets-by-user-id/{id}")]
    public async Task<IActionResult> GetTicketsByUserId(int id)
    {
        var result = await _ticketService.GetAllAsync(t=>t.CreatedById == id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("get-unit-pending/{branchId}")]
    public async Task<IActionResult> GetUnitPending(int branchId)
    {
        var result = await _ticketService.GetByBranchAsync(branchId);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("get-assigned/{userId}")]
    public async Task<IActionResult> GetAssigned(int userId)
    {
        var result = await _ticketService.GetAssignedAsync(userId);
        if (result.Success) return Ok(result);
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
            AiSummary = dto.Ticket.AiSummary,
            AiSuggestedAction = dto.Ticket.AiSuggestedAction,
            AiSentiment = dto.Ticket.AiSentiment
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
