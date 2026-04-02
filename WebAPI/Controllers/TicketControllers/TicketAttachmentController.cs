using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketAttachmentController : ControllerBase
{
    private readonly ITicketAttachmentService _ticketAttachmentService;
    public TicketAttachmentController(ITicketAttachmentService ticketAttachmentService)
    {
        _ticketAttachmentService = ticketAttachmentService;
    }

    [HttpGet("getall")]
    public IActionResult GetAll()
    {
        var result = _ticketAttachmentService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketAttachmentService.GetById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public IActionResult Add(TicketAttachmentDto dto)
    {
        var entity = new Attachment();

        var result = _ticketAttachmentService.Add(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(TicketAttachmentDto dto)
    {
        var entity = new Attachment();

        var result = _ticketAttachmentService.Update(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(TicketAttachmentDto dto)
    {
        var entity = new Attachment();

        var result = _ticketAttachmentService.Delete(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
