namespace CmsAPI.Models;

public class CreateFolderDto
{
    public string FolderName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
}