namespace CmsAPI.Models;

public class FolderInputDto
{
    public string FolderName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; } // Nullable to allow folders without parents
}