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
    [StringLength(40)]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    // Nested folders and documents as lists of IDs or summary details
    public List<Folder> Folders { get; set; } = new List<Folder>(); // Summary details for child folders
    public List<Document> Documents { get; set; } = new List<Document>(); // Summary details for contained documents
    
}