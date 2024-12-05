namespace CmsAPI.Models;

public class EditDocumentDto
{
    public string Title { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;

    public int ContentTypeId { get; set; }

    public int? FolderId { get; set; }
}