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
            var documents = await _documentService.GetDocumentsByUserId();

            if (!documents.Any())
            {
                return NotFound("No documents found for the user.");
            }

            return Ok(documents);
        }
    }
}