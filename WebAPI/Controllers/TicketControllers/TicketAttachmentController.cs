using Business.Abstract.TicketServices;
using Business.Constants;
using Entities.Concrete.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.TicketControllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TicketAttachmentController : ControllerBase
{
    private readonly ITicketAttachmentService _ticketAttachmentService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt", ".zip"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public TicketAttachmentController(ITicketAttachmentService ticketAttachmentService,IConfiguration configuration,IWebHostEnvironment env)
    {
        _ticketAttachmentService = ticketAttachmentService;
        _configuration = configuration;
        _env = env;
    }

    [HttpGet("getall")]
    public IActionResult GetAll()
    {
        var result = _ticketAttachmentService.GetAll();
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("getbyid/{id}")]
    public IActionResult GetById(int id)
    {
        var result = _ticketAttachmentService.GetById(id);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("getbyticketid/{ticketId}")]
    public IActionResult GetByTicketId(int ticketId)
    {
        var result = _ticketAttachmentService.GetAll(a => a.TicketId == ticketId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] int ticketId, [FromForm] int uploadedById, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "Dosya seçilmedi." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { Success = false, Message = Messages.AttachmentFileTooLarge });

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { Success = false, Message = Messages.AttachmentInvalidType });

        var uploadBasePath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads/tickets";
        var ticketFolder = Path.Combine(_env.ContentRootPath, uploadBasePath, ticketId.ToString());
        Directory.CreateDirectory(ticketFolder);

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(ticketFolder, uniqueFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/tickets/{ticketId}/{uniqueFileName}";

        var attachment = new Attachment
        {
            TicketId = ticketId,
            ResponseId = 0,
            FileName = file.FileName,
            FilePath = relativePath,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };

        var result = _ticketAttachmentService.Add(attachment);
        if (result.Success)
            return Ok(new { Success = true, Message = result.Message, FilePath = relativePath });

        return BadRequest(result);
    }

    [HttpPost("delete")]
    public IActionResult Delete(int id)
    {
        var attachment = _ticketAttachmentService.GetById(id);
        if (!attachment.Success || attachment.Data == null)
            return BadRequest(new { Success = false, Message = Messages.AttachmentNotFound });

        var result = _ticketAttachmentService.Delete(attachment.Data);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}
