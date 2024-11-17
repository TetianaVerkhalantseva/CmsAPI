using CmsAPI.Models;
using CmsAPI.Services.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers
{   
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDocumentById([FromRoute] int id)
        {
            var document = await _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound($"No document found for the document Id {id}.");
            }
            
            return Ok(document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] EditDocumentDto dDto)
        {
            var document = await _documentService.GetDocumentByTitle(dDto.Title);
            if (document != null)
            {
                return Conflict($"There is already a document with the title {dDto.Title}.");
            }
            
            var result = await _documentService.CreateDocument(dDto);
            if (result is null)
            {
                return NotFound("Something went wrong...");
            }
            
            return Ok(result);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDocument([FromRoute] int id, [FromBody] EditDocumentDto eDto)
        {
            var result = await _documentService.UpdateDocument(eDto, id);
            if (result is null)
            {
                return NotFound("Something went wrong...");
            }
            
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDocument([FromRoute] int id)
        {
            var folder = await _documentService.GetDocumentById(id);
            if (folder == null)
            {
                return NotFound($"No document found for the document Id {id}.");
            }
            
            var result = await _documentService.DeleteDocument(id);
            if (result is false)
            {
                return BadRequest($"Either the id {id} does not exist or you do not have permission to delete the document.");
            }

            return NoContent();
        }
    }
}