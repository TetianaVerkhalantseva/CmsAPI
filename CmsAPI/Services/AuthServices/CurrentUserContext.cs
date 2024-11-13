using System.Security.Claims;

namespace CmsAPI.Services.AuthServices;

public class CurrentUserContext
{
    private readonly HttpContext _httpContext;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor!.HttpContext!;
    }
    
    public Guid? GetUserId()
    {
        string? value = _httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (value is null)
        {
            return null;
        }

        if (!Guid.TryParse(value, out Guid id))
        {
            return null;
        }

        return id;
    }
}