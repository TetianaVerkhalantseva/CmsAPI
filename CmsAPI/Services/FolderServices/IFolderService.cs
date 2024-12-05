using CmsAPI.Models;
using CmsAPI.Models.Entities;

namespace CmsAPI.Services.FolderServices;

public interface IFolderService
{
    Task<List<FolderDto>> GetFoldersByUserId();
    Task<FolderDto?> GetFolderById(int? id);
    Task<Folder?> GetFolderByFolderName(string folderName);
    Task<FolderUpdateResult> CreateFolder(FolderInputDto dto);
    Task<FolderUpdateResult> UpdateFolder(FolderInputDto dto, int id);
    Task<bool> DeleteFolder(int id);
    Task<RootFolderModel> GetRootFolder();
}