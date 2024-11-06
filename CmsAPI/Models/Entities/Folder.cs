using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Folder
{
    public int FolderId { get; set; }

    [Required] [MaxLength(30)]
    public string FolderName { get; set; } = "Default";
    
    // Navigation properties
    public string? ParentFolderId { get; set; }
    public Folder ParentFolder {get; set; }
    
    [Required]
    public string UserId { get; set; }
    public User? User { get; set; }
}