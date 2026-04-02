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
    public IActionResult GetAll()
    {
        var result = _ticketTransferService.GetAll();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketTransferService.GetById(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("add")]
    public IActionResult Add(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = _ticketTransferService.Add(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("update")]
    public IActionResult Update(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = _ticketTransferService.Update(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(TicketTransferDto dto)
    {
        var entity = new Transfer();

        var result = _ticketTransferService.Delete(entity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
