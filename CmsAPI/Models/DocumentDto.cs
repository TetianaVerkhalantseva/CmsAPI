using CmsAPI.Models.Entities;

namespace CmsAPI.Models;

public class DocumentDto
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public int ContentTypeId { get; set; }
    public string? ContentType { get; set; } // Content type name can be passed to simplify
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }
}