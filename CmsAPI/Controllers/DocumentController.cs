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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var document = await _documentService.GetDocumentByTitle(dDto.Title);
            if (document != null)
            {
                return Conflict($"There is already a document with the title {dDto.Title}.");
            }
            
            var result = await _documentService.CreateDocument(dDto);
            if (result is null)
            {
                return BadRequest("Something went wrong...");
            }
            
            return CreatedAtAction("GetDocumentById", new { id = result.DocumentId }, result);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDocument([FromRoute] int id, [FromBody] EditDocumentDto eDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var document = await _documentService.GetDocumentByTitle(eDto.Title);
            if (document != null && document.DocumentId != id)
            {
                return Conflict($"There is already a document with the title {eDto.Title}.");
            }
            
            var existingDocument = await _documentService.GetDocumentById(id);
            if (existingDocument == null)
            {
                return NotFound($"No document found for the document Id {id}.");
            }
            
            var result = await _documentService.UpdateDocument(eDto, id);
            if (result is null)
            {
                return BadRequest("Something went wrong...");
            }
            
            return Ok($"Document with id {result.DocumentId} updated successfully");
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
                return BadRequest($"Either the file with id {id} does not exist or you do not have permission to delete the document.");
            }

            return NoContent();
        }
    }
}