using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class Document
{
    public int DocumentId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Title { get; set; } = string.Empty;
    
    public string? Content { get; set; }
    
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    
    // Navigation properties
    public string UserId { get; set; }
    public User? User { get; set; }
    
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
}