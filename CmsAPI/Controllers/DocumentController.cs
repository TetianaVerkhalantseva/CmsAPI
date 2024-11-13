using System.Security.Claims;
using CmsAPI.Services.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [Authorize]
        [HttpGet("user-documents")]
        public async Task<IActionResult> GetUserDocuments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authorized.");
            }
            
            var documents = await _documentService.GetDocumentsByUserId(userId);

            if (!documents.Any())
            {
                return NotFound("No documents found for the user.");
            }

            return Ok(documents);
        }
    }
}