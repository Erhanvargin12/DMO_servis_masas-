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
    public IActionResult GetAll()
    {
        var result = _ticketService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketService.GetById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getticketdtobyid/{id}")]
    public IActionResult GetTicketDtoById(int id)
    {
        var result = _ticketService.GetTicketDtoById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }


    [HttpPost("add")]
    public IActionResult Add(TicketDto dto)
    {
        var entity = new Ticket() 
        {
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
        var result = _ticketService.Add(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(TicketDto dto)
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
        var result = _ticketService.Update(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(int id)
    {
        var result = _ticketService.Delete(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
