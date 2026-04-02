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
    public IActionResult GetAll()
    {
        var result = _ticketResponseService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketResponseService.GetById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public IActionResult Add(TicketResponseDto dto)
    {
        var entity = new Response();

        var result = _ticketResponseService.Add(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(TicketResponseDto dto)
    {
        var entity = new Response();

        var result = _ticketResponseService.Update(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(TicketResponseDto dto)
    {
        var entity = new Response();

        var result = _ticketResponseService.Delete(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
