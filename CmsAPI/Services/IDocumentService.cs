using CmsAPI.Models.Entities;

namespace CmsAPI.Services;

public interface IDocumentService
{
    Task<IEnumerable<Document>> GetDocumentsByUserId(string userId);
}