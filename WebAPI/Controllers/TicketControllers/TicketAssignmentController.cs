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
    public IActionResult GetAll()
    {
        var result = _ticketAssignmentService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketAssignmentService.GetById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public IActionResult Add(TicketAssignmentDto dto)
    {
        var entity = new Assignment();

        var result = _ticketAssignmentService.Add(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(TicketAssignmentDto dto)
    {
        var entity = new Assignment();

        var result = _ticketAssignmentService.Update(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(TicketAssignmentDto dto)
    {
        var entity = new Assignment();

        var result = _ticketAssignmentService.Delete(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
