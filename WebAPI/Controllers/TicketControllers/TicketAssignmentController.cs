using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketAssignmentController : ControllerBase
{
    private readonly ITicketAssignmentService _ticketAssignmentService;
    public TicketAssignmentController(ITicketAssignmentService ticketAssignmentService)
    {
        _ticketAssignmentService = ticketAssignmentService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketAssignmentService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketAssignmentService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(Assignment entity)
    {
        var result = await _ticketAssignmentService.AddAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(TicketAssignmentDto dto)
    {
        var entity = new Assignment();

        var result = await _ticketAssignmentService.UpdateAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(TicketAssignmentDto dto)
    {
        var entity = new Assignment();

        var result = await _ticketAssignmentService.DeleteAsync(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
