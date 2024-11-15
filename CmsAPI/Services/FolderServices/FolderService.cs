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
            .Include(f => f.Documents)
            .ToListAsync();

        return folders.Select(f => new FolderDto
        {
            FolderId = f.FolderId,
            FolderName = f.FolderName,
            ParentFolderId = f.ParentFolderId,
            Documents = f.Documents.Select(d => new DocumentDto
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Content = d.Content,
                CreatedOn = d.CreatedOn,
                ContentTypeId = d.ContentTypeId
            }).ToList(),
        }).ToList();
    }

    public async Task<FolderDto?> GetFolderById(int id)
    {
        Guid? ownerId = _currentUser.GetUserId();
        
        var folder = await _db.Folders
            .Include(f => f.Folders)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.FolderId == id);

        if (folder is null)
        {
            return null;
        }
        
        if (folder.UserId != ownerId.ToString())
        {
            return new FolderDto();
        }
        
        return folder == null ? null : new FolderDto
        {
            FolderId = folder.FolderId,
            FolderName = folder.FolderName,
            ParentFolderId = folder.ParentFolderId,
            SubFolders = folder.Folders.Select(sf => new FolderDto()
            {
                FolderId = sf.FolderId,
                FolderName = sf.FolderName,
                UserId = sf.UserId,
            }).ToList(),
            Documents = folder.Documents.Select(d => new DocumentDto
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Content = d.Content,
                CreatedOn = d.CreatedOn,
                ContentTypeId = d.ContentTypeId
            }).ToList()
        };
    }

    public async Task<Folder?> CreateFolder(CreateFolderDto dto)
    {
        Guid? ownerId = _currentUser.GetUserId();
        if (ownerId == null)
        {
            return null; // User not authenticated
        }

        Folder? parentFolder = null;
    
        // Check if ParentFolderId is provided and validate it
        if (dto.ParentFolderId.HasValue)
        {
            parentFolder = await _db.Folders.FirstOrDefaultAsync(f => f.FolderId == dto.ParentFolderId.Value);
        
            if (parentFolder == null)
            {
                return null; // Parent folder doesn't exist
            }

            if (parentFolder.UserId != ownerId.ToString())
            {
                return null; // Parent folder belongs to another user
            }
        }

        Folder dbRecord = new Folder()
        {
            FolderName = dto.FolderName,
            ParentFolderId = parentFolder?.FolderId, // Assign parent folder ID or null
            UserId = ownerId.ToString()
        };

        try
        {
            await _db.Folders.AddAsync(dbRecord);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null; // Error occurred
        }

        return dbRecord;
    }

    public async Task<Folder> UpdateFolder(UpdateFolderDto dto, int id)
    {
        Folder? dbRecord = await _db.Folders    
            .Include(folder => folder.Folders)
            .Include(folder => folder.Documents)
            .Include(folder => folder.ParentFolder)
            .FirstOrDefaultAsync(f => f.FolderId == id);
        
        if (dbRecord == null)
        {
            return new Folder();
        }
        
        dbRecord.FolderName = dto.FolderName;

        try
        {
            _db.Folders.Update(dbRecord);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Folder();
        }

        return dbRecord;
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
}
