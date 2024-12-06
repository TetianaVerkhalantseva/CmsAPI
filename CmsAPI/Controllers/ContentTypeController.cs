using CmsAPI.Services.ContentTypeServices;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentTypeController : ControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public ContentTypeController(IContentTypeService contentTypeService)
    {
        _contentTypeService = contentTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetContentTypes() => Ok(await _contentTypeService.GetAll());
}