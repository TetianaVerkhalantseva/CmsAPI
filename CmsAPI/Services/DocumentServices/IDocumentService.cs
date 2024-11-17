using CmsAPI.Models;
using CmsAPI.Models.Entities;

namespace CmsAPI.Services.DocumentServices;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetDocumentsByUserId();
    
    Task<DocumentDto?> GetDocumentById(int documentId);

    Task<Document?> GetDocumentByTitle(string title);
    
    Task<Document?> CreateDocument(EditDocumentDto eDto);

    Task<Document?> UpdateDocument(EditDocumentDto eDto, int documentId);
    
    Task<bool> DeleteDocument(int documentId);
}