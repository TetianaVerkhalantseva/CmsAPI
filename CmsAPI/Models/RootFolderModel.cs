using CmsAPI.Models.Entities;

namespace CmsAPI.Models;

public class RootFolderModel
{
    public List<Folder> Folders { get; set; } = new List<Folder>();
    public List<Document> Files { get; set; } = new List<Document>();
}