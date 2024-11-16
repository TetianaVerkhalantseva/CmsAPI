using CmsAPI.Models;
using CmsAPI.Models.Entities;

namespace CmsAPI.Services.FolderServices;

public interface IFolderService
{
    Task<List<FolderDto>> GetFoldersByUserId();
    Task<FolderDto?> GetFolderById(int id);
    Task<FolderUpdateResult> CreateFolder(CreateFolderDto dto);
    Task<FolderUpdateResult> UpdateFolder(UpdateFolderDto dto, int id);
    Task<bool> DeleteFolder(int id);
}