using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.FolderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [Authorize]
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
            var folder = await _folderService.GetFolderById(id);
            if (folder == null)
            {
                return NotFound($"Folder with Id {id} not found.");
            }

            return Ok(folder);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
        {
            Folder? result = await _folderService.CreateFolder(dto);
            return result is null ? BadRequest() : Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFolder([FromRoute] int id, [FromBody] UpdateFolderDto folderDto)
        {
            Folder? result = await _folderService.UpdateFolder(folderDto, id);
            return result is null ? BadRequest() : Ok(result);
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
            return result ? NoContent() : BadRequest();
        }
    }
}
