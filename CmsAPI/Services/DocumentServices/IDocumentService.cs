using CmsAPI.Models;

namespace CmsAPI.Services.DocumentServices;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetDocumentsByUserId();
}