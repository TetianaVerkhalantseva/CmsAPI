using CmsAPI.Models;

namespace CmsAPI.Services.ContentTypeServices;

public interface IContentTypeService
{
    Task<IEnumerable<ContentTypeDto>> GetAll();
}