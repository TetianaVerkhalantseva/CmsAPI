using System.ComponentModel.DataAnnotations;

namespace CmsAPI.Models.Entities;

public class ContentType
{
    public int ContentTypeId { get; set; }

    [Required]
    public string Type { get; set; }  = string.Empty;

    public List<Document> Documents { get; set; } = new List<Document>();
}