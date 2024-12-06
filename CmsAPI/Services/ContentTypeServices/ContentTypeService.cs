using CmsAPI.Data;
using CmsAPI.Models;
using CmsAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Services.ContentTypeServices;

public class ContentTypeService : IContentTypeService
{
    private readonly CmsContext _db;

    public ContentTypeService(CmsContext db)
    {
        _db = db;
    }
    
    public async Task<IEnumerable<ContentTypeDto>> GetAll()
    {
        List<ContentType> dbRecords = await _db.ContentTypes.ToListAsync();
        return dbRecords.Select(record => new ContentTypeDto()
        {
            Id = record.ContentTypeId,
            Name = record.Type
        });
    }
}