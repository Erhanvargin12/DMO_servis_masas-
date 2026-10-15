using Business.Abstract.TicketServices;
using Entities.Concrete.Ticket;
using Entities.DTOs.TicketDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TicketTransferController : ControllerBase
{
    private readonly ITicketTransferService _ticketTransferService;
    private readonly ITicketRoutingService _routingService;

    public TicketTransferController(
        ITicketTransferService ticketTransferService,
        ITicketRoutingService routingService)
    {
        _ticketTransferService = ticketTransferService;
        _routingService = routingService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ticketTransferService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketTransferService.GetByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("getbyticketid/{ticketId}")]
    public async Task<IActionResult> GetByTicketId(int ticketId)
    {
        var result = await _ticketTransferService.GetAllAsync(t => t.TicketId == ticketId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(TicketTransferDto dto)
    {
        var result = await _routingService.TransferToBranchAsync(
            dto.TicketId,
            dto.FromBranchId,
            dto.ToBranchId,
            dto.TransferredById,
            dto.Note);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
