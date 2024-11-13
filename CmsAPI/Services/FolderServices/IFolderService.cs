using CmsAPI.Models;
using CmsAPI.Models.Entities;

namespace CmsAPI.Services.FolderServices;

public interface IFolderService
{
    Task<List<FolderDto>> GetFoldersByUserId();
    Task<FolderDto?> GetFolderById(int id);
    Task<Folder> CreateFolder(CreateFolderDto dto);
    Task<Folder> UpdateFolder(UpdateFolderDto dto, int id);
    Task<bool> DeleteFolder(int id);
}