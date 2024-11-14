namespace CmsAPI.Models;

public class EditDocumentDto
{
    public string Title { get; set; }
    
    public string Content { get; set; } = string.Empty;

    public int ContentTypeId { get; set; }

    public string FolderId { get; set; }
}