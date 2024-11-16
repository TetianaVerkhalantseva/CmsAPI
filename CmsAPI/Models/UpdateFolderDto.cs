namespace CmsAPI.Models;

public class UpdateFolderDto
{
    public string FolderName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
}