using CmsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class LibraryController : ControllerBase
{
    private readonly IDocumentService _service;

    public LibraryController(IDocumentService service)
    {
        _service = service;
    }
    
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetDocumentsByUserId([FromRoute] string userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var documents = await _service.GetDocumentsByUserId(userId);
        if (documents == null || !documents.Any())
        {
            return NotFound($"No document with user Id {userId} was found.");
        }

        return Ok(documents);
    }

}