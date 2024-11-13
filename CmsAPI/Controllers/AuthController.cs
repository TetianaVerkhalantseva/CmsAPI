using CmsAPI.Models;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CmsAPI.Controllers;

[Route("/api/[controller]")]
[ApiController]
  
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, UserManager<User> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterUser(LoginDto user)
    {
        var identityUser = await _userManager.FindByNameAsync(user.Username);
        if (identityUser != null)
        {
            return Conflict(new
            {
                IsSuccess = false,
                Message = "User already exists. Please log in instead."
            });
        }

        if (await _authService.RegisterUser(user))
        {
            return Ok(new
            {
                IsSuccess = true,
                Message = "User successfully registered!"
            });
        }

        return BadRequest(new
        {
            IsSuccess = false,
            Message = "Something went wrong..."
        });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto user)
    {
        var identityUser = await _userManager.FindByNameAsync(user.Username);
        if (identityUser == null)
        {
            return BadRequest(new
            {
                IsSuccess = false,
                Message = "User does not exist..."
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                IsSuccess = false,
                Message = "Invalid ModelState..."
            });
        }
        
        if (!await _authService.Login(user)) 
        {
            return BadRequest(new
            {
                IsSuccess = false,
                Message = "Something went wrong when logging in..."
            });
        }

        return Ok(new
        {
            IsSuccess = true,
            Token = _authService.GenerateTokenString(identityUser),
            Message = "Login Successful!"
        });
    }
}