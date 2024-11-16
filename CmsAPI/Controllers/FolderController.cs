using CmsAPI.Models;
using CmsAPI.Models.Exceptions;
using CmsAPI.Services.FolderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers
{   
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }
        
        [HttpGet("user-folders")]
        public async Task<IActionResult> GetUserFolders()
        {
            var folders = await _folderService.GetFoldersByUserId();

            if (!folders.Any())
            {
                return NotFound("No folders found for the user.");
            }

            return Ok(folders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFolderById(int id)
        {
            try
            {
                var folder = await _folderService.GetFolderById(id);
                if (folder == null)
                {
                    return NotFound($"Folder with Id {id} not found.");
                }
                return Ok(folder);
            }
            catch (ForbiddenAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
        {
            try
            {
                var result = await _folderService.CreateFolder(dto);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetFolderById), new { id = result.UpdatedFolder?.FolderId }, result.UpdatedFolder);
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage) && result.ErrorMessage.Contains("permission"))
                {
                    return Forbid(result.ErrorMessage);
                }

                return BadRequest(result.ErrorMessage ?? "An unexpected error occurred.");
            }
            catch (ForbiddenAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while creating the folder.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFolder([FromRoute] int id, [FromBody] UpdateFolderDto dto)
        {
            try
            {
                var result = await _folderService.UpdateFolder(dto, id);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.UpdatedFolder);
            }
            catch (ForbiddenAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _folderService.GetFolderById(id);
            if (folder == null)
            {
                return NotFound($"Folder with Id {id} not found.");
            }

            bool result = await _folderService.DeleteFolder(id);
            if (result)
            {
                return Ok(new { Message = "Folder successfully deleted." }); // 200 OK with a success message
            }

            return BadRequest("An error occurred while deleting the folder."); // 400 Bad Request for failure
        }
    }
}
