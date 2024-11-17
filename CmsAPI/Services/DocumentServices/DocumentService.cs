using CmsAPI.Data;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Services.DocumentServices;

public class DocumentService : IDocumentService
{   
    private readonly CurrentUserContext _currentUser;
    private readonly CmsContext _db;
    
    public DocumentService(CmsContext db,
        CurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByUserId()
    {
        Guid? ownerId = _currentUser.GetUserId();
        
        try
        {
            var documents = await _db.Documents
                .Where(document => document.UserId == ownerId.ToString())
                .Include(d => d.ContentType) // Load ContentType
                .Select(document => new DocumentDto
                {
                    DocumentId = document.DocumentId,
                    Title = document.Title,
                    Content = document.Content,
                    CreatedOn = document.CreatedOn,
                    ContentTypeId = document.ContentTypeId,
                    ContentType = document.ContentType != null ? document.ContentType.Type : null, // Secure Access
                    UserId = document.UserId,
                    FolderId = document.FolderId
                })
                .ToListAsync();

            return documents;
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            return new List<DocumentDto>();
        }
    }

    public async Task<DocumentDto?> GetDocumentById(int documentId)
    {
        Guid? ownerId = _currentUser.GetUserId();

        var document = await _db.Documents
            .Where(document => document.UserId == ownerId.ToString())
            .Include(document => document.ContentType)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        
        if (document is null)
        {
            return null;
        }

        if (document.UserId != ownerId.ToString())
        {
            return new DocumentDto();
        }

        return new DocumentDto
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            Content = document.Content,
            CreatedOn = document.CreatedOn,
            ContentTypeId = document.ContentTypeId,
            ContentType = document.ContentType?.Type,
            UserId = document.UserId,
            FolderId = document.FolderId
        };
    }

    public async Task<Document?> GetDocumentByTitle(string title)
    {
        Guid? ownerId = _currentUser.GetUserId();

        var document = await _db.Documents
            .Where(document => document.Title == title)
            .FirstOrDefaultAsync();

        return document;
    }
    
    public async Task<Document?> CreateDocument(EditDocumentDto eDto)
    {
        Guid? ownerIdNullable = _currentUser.GetUserId();
        if (ownerIdNullable == null)
        {
            throw new InvalidOperationException("User is not authorized.");
        }
        
        Guid ownerId = ownerIdNullable.Value;

        var document = new Document
        {
            Title = eDto.Title,
            Content = eDto.Content,
            ContentTypeId = eDto.ContentTypeId,
            UserId = ownerId.ToString(),
            FolderId = eDto.FolderId != "" ? eDto.FolderId : null
        };
        
        try
        {
            await _db.Documents.AddAsync(document);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Document();
        }
        
        return document;
    }

    public async Task<Document?> UpdateDocument(EditDocumentDto eDto, int documentId)
    {
        Guid? ownerId = _currentUser.GetUserId();
        
        var document = await _db.Documents
            .Where(document => document.UserId == ownerId.ToString())
            .Include(document => document.ContentType)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document is null)
        {
            return null;
        }
        
        document.Title = eDto.Title;
        document.Content = eDto.Content;
        document.ContentTypeId = eDto.ContentTypeId;
        document.FolderId = eDto.FolderId != "" ? eDto.FolderId : null;
        
        try
        {
            _db.Documents.Update(document);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        return document;
    }

    public async Task<bool> DeleteDocument(int documentId)
    {
        Guid? ownerId = _currentUser.GetUserId();

        var document = await _db.Documents.FindAsync(documentId);
        if (document != null && ownerId != null && ownerId.ToString() == document.UserId)
        {
            _db.Remove(document);
            await _db.SaveChangesAsync();
            return true;
        }
        return false;
    }
}