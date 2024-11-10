using CmsAPI.Data;
using CmsAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Services;

public class DocumentService : IDocumentService
{
    private readonly CmsContext _db;
    public DocumentService(CmsContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Document>> GetDocumentsByUserId(string userId)
    {
        try
        {
            var documents = await _db.Documents
                .Where(document => document.UserId == userId)
                .Include(c => c.ContentType)
                .ToListAsync();
            
            return documents;
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            return new List<Document>();
        }
    }
}