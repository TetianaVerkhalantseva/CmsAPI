using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CmsAPI.Models.Entities;

public class User : IdentityUser
{
    [MaxLength(20)]
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string LastName { get; set; } = string.Empty;
}