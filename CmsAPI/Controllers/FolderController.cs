using CmsAPI.Models;
using CmsAPI.Services.FolderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> GetFolderById(int? id)
        {
            var folder = await _folderService.GetFolderById(id);
            if (folder == null)
            {
                return NotFound($"Folder with Id {id} not found.");
            }
            return Ok(JsonConvert.SerializeObject(folder, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            }));
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderInputDto dto)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var folder = await _folderService.GetFolderByFolderName(dto.FolderName);
            if (folder != null)
            {
                return Conflict($"There is already a folder with the name {dto.FolderName}.");
            }
            
            var result = await _folderService.CreateFolder(dto);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetFolderById), new { id = result.UpdatedFolder?.FolderId }, result.UpdatedFolder);
            }
            
            if (!string.IsNullOrEmpty(result.ErrorMessage) && result.ErrorMessage == "Parent folder not found.")
            {
                return NotFound(new { Message = "Parent folder not found.", ParentFolderId = result.ProblematicFolderId });
            }
            
            return BadRequest(result.ErrorMessage ?? "An unexpected error occurred.");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFolder([FromRoute] int id, [FromBody] FolderInputDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var folder = await _folderService.GetFolderByFolderName(dto.FolderName);
            if (folder != null)
            {
                return Conflict($"There is already a folder with the name {dto.FolderName}.");
            }

            var result = await _folderService.UpdateFolder(dto, id);
            
            if (result.IsSuccess)
            {
                return Ok(new { Message = "Folder updated successfully.", result.UpdatedFolder });
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage) && result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.ErrorMessage);
            }

            return BadRequest(result.ErrorMessage ?? "An unexpected error occurred while updating the folder.");
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
                return Ok(new { Message = "Folder successfully deleted." }); 
            }

            return BadRequest("An error occurred while deleting the folder.");
        }

        [HttpGet("root")]
        public async Task<IActionResult> GetRootFolder()
        {
            RootFolderModel model = await _folderService.GetRootFolder();
            return Ok(model);
        }
    }
}
