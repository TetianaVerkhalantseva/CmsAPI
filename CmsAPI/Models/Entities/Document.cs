using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Document
{
    public int DocumentId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
    [StringLength(100)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    
    // Navigation properties
    [Required]
    [StringLength(40)]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    
    [StringLength(5)]
    public string? FolderId { get; set; } = string.Empty;
    public Folder? Folder { get; set; }
}