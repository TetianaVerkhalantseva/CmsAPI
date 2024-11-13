using CmsAPI.Models;
using CmsAPI.Models.Entities;

namespace CmsAPI.Services.DocumentServices;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetDocumentsByUserId(string userId);
}