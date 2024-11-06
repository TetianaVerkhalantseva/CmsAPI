using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Folder
{
    public int FolderId { get; set; }

    [Required] [MaxLength(30)]
    public string FolderName { get; set; } = "Default";
    
    public string? ParentFolderId { get; set; }
    
    // Navigation properties
    public string UserId { get; set; }
    public User? User { get; set; }
}