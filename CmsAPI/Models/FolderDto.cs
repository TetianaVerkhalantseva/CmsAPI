namespace CmsAPI.Models
{
    public class FolderDto
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Nested folders and documents as lists of IDs or summary details
        public List<FolderDto> SubFolders { get; set; } = new List<FolderDto>(); // Summary details for child folders
        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>(); // Summary details for contained documents
    }
}