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

    [HttpPost("add")]
    public IActionResult Add(TicketDto dto)
    {
        var entity = new Ticket() 
        {
            TicketNo = dto.TicketNo,
            Title = dto.Title,
            Description = dto.Description,
            CreatedById = dto.CreatedById,
            CurrentBranchId = dto.CurrentBranchId,
            AssignedUserId = dto.AssignedUserId,
            StatusId = dto.StatusId,
            PriorityId = dto.PriorityId,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            ClosedAt = dto.ClosedAt,
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
            Id = dto.Id,
            TicketNo = dto.TicketNo,
            Title = dto.Title,
            Description = dto.Description,
            CreatedById = dto.CreatedById,
            CurrentBranchId = dto.CurrentBranchId,
            AssignedUserId = dto.AssignedUserId,
            StatusId = dto.StatusId,
            PriorityId = dto.PriorityId,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            ClosedAt = dto.ClosedAt,
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
