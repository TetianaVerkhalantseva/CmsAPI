using CmsAPI.Data;
using CmsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Services.DocumentServices;

public class DocumentService : IDocumentService
{
    private readonly CmsContext _db;
    public DocumentService(CmsContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByUserId(string userId)
    {
        try
        {
            var documents = await _db.Documents
                .Where(document => document.UserId == userId)
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

}