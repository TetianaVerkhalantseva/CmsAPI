using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CmsAPI.Models.Entities;

public class User : IdentityUser
{
    [Required]
    [MaxLength(20)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string LastName { get; set; }
}