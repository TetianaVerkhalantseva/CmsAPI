using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Folder
{
    [Required]
    public int FolderId { get; set; } 

    [Required] [MaxLength(30)]
    public string FolderName { get; set; } = string.Empty;
    
    // Navigation properties
    public int? ParentFolderId { get; set; }
    public Folder? ParentFolder {get; set; }
    
    [Required]
    public string UserId { get; set; }
    public User User { get; set; }
    
    public List<Folder> Folders { get; set; } = new List<Folder>();
    public List<Document> Documents { get; set; } = new List<Document>();
    
}