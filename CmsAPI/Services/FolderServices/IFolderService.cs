using CmsAPI.Models;

namespace CmsAPI.Services.FolderServices;

public interface IFolderService
{
    Task<List<FolderDto>> GetFoldersByUserId();
    Task<FolderDto?> GetFolderById(int? id);
    Task<FolderUpdateResult> CreateFolder(FolderInputDto dto);
    Task<FolderUpdateResult> UpdateFolder(FolderInputDto dto, int id);
    Task<bool> DeleteFolder(int id);
    Task<RootFolderModel> GetRootFolder();
}