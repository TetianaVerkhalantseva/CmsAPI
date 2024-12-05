using CmsAPI.Data;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Services.FolderServices;

public class FolderService : IFolderService
{
    private readonly CmsContext _db;
    private readonly CurrentUserContext _currentUser;

    public FolderService(CmsContext db,
        CurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser; 
    }

    public async Task<List<FolderDto>> GetFoldersByUserId()
    {
        Guid? ownerId = _currentUser.GetUserId();

        var folders = await _db.Folders
            .Where(f => f.UserId == ownerId.ToString())
            .Include(f => f.Folders)
                .ThenInclude(sf => sf.User)
            .Include(f => f.Documents)
                .ThenInclude(d => d.ContentType)
            .Include(f => f.Documents)
                .ThenInclude(d => d.User)
            .Include(f => f.ParentFolder)
            .Include(f => f.User)
            .ToListAsync();

        return folders.Select(f => new FolderDto
        {
            FolderId = f.FolderId,
            FolderName = f.FolderName,
            ParentFolderId = f.ParentFolderId,
            ParentFolderName = f.ParentFolder?.FolderName,
            UserId = f.UserId,
            UserName = f.User?.UserName,
            SubFolders = f.Folders.Select(sf => new FolderDto
            {
                FolderId = sf.FolderId,
                FolderName = sf.FolderName,
                ParentFolderId = sf.ParentFolderId,
                ParentFolderName = sf.ParentFolder?.FolderName,
                UserId = sf.UserId,
                UserName = sf.User?.UserName
            }).ToList(),
            Documents = f.Documents.Select(d => new DocumentDto
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Content = d.Content,
                CreatedOn = d.CreatedOn,
                ContentTypeId = d.ContentTypeId,
                ContentType = d.ContentType?.Type,
                UserId = d.UserId,
                UserName = d.User?.UserName,
                FolderId = d.FolderId,
                FolderName = f.FolderName
            }).ToList()
        }).ToList();
    }

    public async Task<FolderDto?> GetFolderById(int? id)
    {
        Guid? ownerId = _currentUser.GetUserId();
        Folder? folder = null;
        
        if (id is null)
        {
            folder = new Folder()
            {
                Folders = await _db.Folders
                    .Where(folder => folder.UserId == ownerId.ToString() &&
                            !folder.ParentFolderId.HasValue)
                    .ToListAsync(),
                
                Documents = await _db.Documents
                    .Include(document => document.ContentType)
                    .Where(document => document.UserId == ownerId.ToString() &&
                            string.IsNullOrEmpty(document.FolderId))
                    .ToListAsync()
            };

            return new FolderDto()
            {
                UserId = folder.UserId,
                UserName = folder.User?.UserName,
                SubFolders = folder.Folders.Select(sf => new FolderDto()
                {
                    FolderId = sf.FolderId,
                    FolderName = sf.FolderName,
                    ParentFolderId = sf.ParentFolderId,
                    ParentFolderName = sf.ParentFolder?.FolderName,
                    UserId = sf.UserId,
                    UserName = sf.User?.UserName
                }).ToList(),
                Documents = folder.Documents.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    Content = d.Content,
                    CreatedOn = d.CreatedOn,
                    ContentTypeId = d.ContentTypeId,
                    ContentType = d.ContentType?.Type,
                    UserId = d.UserId,
                    UserName = d.User?.UserName,
                    FolderId = d.FolderId,
                    FolderName = folder.FolderName
                }).ToList()
            };
        }
        else
        {
            folder = await _db.Folders
                .Include(f => f.Folders)
                .ThenInclude(sf => sf.User)
                .Include(f => f.Documents)
                .ThenInclude(d => d.ContentType)
                .Include(f => f.Documents)
                .ThenInclude(d => d.User)
                .Include(f => f.ParentFolder)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FolderId == id);
        }
        

        if (folder == null || folder.UserId != ownerId.ToString())
        {
            return null;
        }
        
        return new FolderDto
        {
            FolderId = folder.FolderId,
            FolderName = folder.FolderName,
            ParentFolderId = folder.ParentFolderId,
            ParentFolderName = folder.ParentFolder?.FolderName,
            UserId = folder.UserId,
            UserName = folder.User?.UserName, 
            SubFolders = folder.Folders.Select(sf => new FolderDto()
            {
                FolderId = sf.FolderId,
                FolderName = sf.FolderName,
                ParentFolderId = sf.ParentFolderId,
                ParentFolderName = sf.ParentFolder?.FolderName,
                UserId = sf.UserId,
                UserName = sf.User?.UserName
            }).ToList(),
            Documents = folder.Documents.Select(d => new DocumentDto
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Content = d.Content,
                CreatedOn = d.CreatedOn,
                ContentTypeId = d.ContentTypeId,
                ContentType = d.ContentType?.Type,
                UserId = d.UserId,
                UserName = d.User?.UserName, 
                FolderId = d.FolderId,
                FolderName = folder.FolderName
            }).ToList()
        };
    }

    public async Task<FolderUpdateResult> CreateFolder(FolderInputDto dto)
    {
        Guid? ownerIdNullable = _currentUser.GetUserId();
        if (ownerIdNullable == null)
        {
            return new FolderUpdateResult
            {
                IsSuccess = false,
                ErrorMessage = "User is not authorized."
            };
        }
        Guid ownerId = ownerIdNullable.Value;
        
        Folder? parentFolder = null;

        if (dto.ParentFolderId.HasValue)
        {
            parentFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == dto.ParentFolderId.Value);

            if (parentFolder == null || parentFolder.UserId != ownerId.ToString())
            {
                return new FolderUpdateResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Parent folder with Id {dto.ParentFolderId.Value} not found."
                };
            }
        }

        Folder dbRecord = new Folder()
        {
            FolderName = dto.FolderName,
            ParentFolderId = parentFolder?.FolderId,
            UserId = ownerId.ToString()
        };

        try
        {
            await _db.Folders.AddAsync(dbRecord);
            await _db.SaveChangesAsync();

            return new FolderUpdateResult
            {
                IsSuccess = true,
                UpdatedFolder = dbRecord
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to create folder: {e.Message}");
            return new FolderUpdateResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while creating the folder."
            };
        }
    }

    public async Task<FolderUpdateResult> UpdateFolder(FolderInputDto dto, int id)
    {
        Guid? ownerId = _currentUser.GetUserId();

        Folder? dbRecord = await _db.Folders
            .Include(folder => folder.Folders)
            .Include(folder => folder.Documents)
            .Include(folder => folder.ParentFolder)
            .FirstOrDefaultAsync(f => f.FolderId == id);

        if (dbRecord == null || dbRecord.UserId != ownerId.ToString())
        {
            return new FolderUpdateResult
            {
                IsSuccess = false,
                ErrorMessage = $"Folder with Id {id} not found."
            };
        }

        dbRecord.FolderName = dto.FolderName;

        if (dto.ParentFolderId.HasValue)
        {
            Folder? newParentFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == dto.ParentFolderId.Value);

            if (newParentFolder == null || newParentFolder.UserId != ownerId.ToString())
            {
                return new FolderUpdateResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Parent folder with Id {dto.ParentFolderId.Value} not found."
                };
            }

            dbRecord.ParentFolderId = newParentFolder.FolderId;
        }
        else
        {
            dbRecord.ParentFolderId = null;
        }

        try
        {
            _db.Folders.Update(dbRecord);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine ($"Failed to update folder: {e.Message}");
            return new FolderUpdateResult
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while updating the folder.",
                ProblematicFolderId = id
            };
        }

        return new FolderUpdateResult
        {
            IsSuccess = true,
            UpdatedFolder = dbRecord
        };
    }

    public async Task<bool> DeleteFolder(int id)
    {   
        Guid? ownerId = _currentUser.GetUserId();
        var folder = await _db.Folders.FindAsync(id);
        
        if (folder is not null && folder.UserId != ownerId.ToString())
        {
            return false;
        }
        
        if (folder != null)
        {
            _db.Folders.Remove(folder);
            await _db.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<RootFolderModel> GetRootFolder()
    {
        Guid? ownerId = _currentUser.GetUserId();

        List<Folder> folderRecords = await _db.Folders
            .Where(folder => folder.UserId == ownerId.ToString() &&
                             !folder.ParentFolderId.HasValue)
            .ToListAsync();
        
        List<Document> documentRecords = await _db.Documents
            .Include(document => document.ContentType)
            .Where(document => document.UserId == ownerId.ToString() &&
                               string.IsNullOrEmpty(document.FolderId))
            .ToListAsync();

        return new RootFolderModel()
        {
            Files = documentRecords,
            Folders = folderRecords
        };
    }
}
