using CmsAPI.Models.Entities;

namespace CmsAPI.Services.DocumentServices;

public interface IDocumentService
{
    Task<IEnumerable<Document>> GetDocumentsByUserId(string userId);
}