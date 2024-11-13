using CmsAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CmsAPI.Services.AuthServices;

public interface IAuthService
{
    Task<bool> RegisterUser(LoginDto user);

    Task<bool> Login(LoginDto user);

    string GenerateTokenString(IdentityUser user);
}