using CmsAPI.Models.Entities;

namespace CmsAPI.Models;

public class FolderUpdateResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ProblematicFolderId { get; set; }
    public Folder? UpdatedFolder { get; set; }
}