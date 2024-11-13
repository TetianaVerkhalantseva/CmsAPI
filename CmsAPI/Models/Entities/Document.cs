using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Document
{
    public int DocumentId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    
    // Navigation properties
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; }
    
    [Required]
    public string FolderId { get; set; } = string.Empty;
    public Folder? Folder { get; set; }
}