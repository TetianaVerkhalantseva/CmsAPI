using CmsAPI.Models.Entities;
using Microsoft.CodeAnalysis;
using Document = CmsAPI.Models.Entities.Document;

namespace CmsAPI.Data;

public class SeedCms
{
    private readonly string[] _contentTypes =
    {
        "Text",
        "Image",
        "Video",
        "Audio",
        "Link",
        "Contact"
    };

    private readonly string[] _documentTitles =
    {
        "Test file 1",
        "Test file 2",
        "Test file 3",
        "Secret file 1",
        "Secret file 2",
        "Secret file 3"
    };

    private readonly string[] _folderNames =
    {
        "Test folder 1",
        "Test folder 2",
        "Test folder 3",
        "Secret folder 1",
        "Secret folder 2",
        "Secret folder 3"
    };

    private readonly string[] _userFirstNames =
    {
        "Adam",
        "Bob",
        "Charles",
        "Dave",
        "Davis",
        "Emily",
    };

    private readonly string[] _userLastNames =
    {
        "Ericsson",
        "Johnson",
        "James",
        "Jones",
        "Mayer",
        "Mary",
    };

    // Generate a random value
    private string RandomOne(string[] list)
    {
        var idx = Random.Shared.Next(list.Length);
        return list[idx];
    }

    // Generate a random ContentType
    private ContentType MakeContentType(int id)
    {
        return new ContentType
        {
            ContentTypeId = id,
            Type = RandomOne(_contentTypes),
        };
    }

    // Generate a random User
    private User MakeUser(int Id)
    {
        var random = new Random();
        var userName = "user" + random.Next(100, 9999);
        var email = userName + "@example.com";
        
        var user = new User
        {
            Id = Id.ToString(),
            UserName = userName,
            Email = email,
            FirstName = RandomOne(_userFirstNames),
            LastName = RandomOne(_userLastNames),
        };

        return user;
    }

    // Generate a random Folder
    private Folder MakeFolder(int id, string userId)
    {
        return new Folder
        {
            FolderId = id,
            FolderName = RandomOne(_folderNames),
            UserId = userId
        };
    }

    // Generate a random Document
    private Document MakeDocument(int id, string userId, int contentTypeId)
    {
        return new Document
        {
            DocumentId = id,
            Title = $"Document {id}",
            Content = $"Content for document {id}",
            CreatedOn = DateTime.Now.AddDays(-Random.Shared.Next(0, 365))
                .AddHours(Random.Shared.Next(0, 24))
                .AddMinutes(Random.Shared.Next(0, 60))
                .AddSeconds(Random.Shared.Next(0, 60)),
            UserId = userId,
            ContentTypeId = contentTypeId
        };
    }

    // Generate with a counter
    public async Task SeedDatabaseWithItemCountOfAsync(CmsContext context, int totalCount)
    {
        var count = 0;
        var currentCycle = 0;

        while (count < totalCount)
        {
            var users = new List<User>();
            var contentTypes = new List<ContentType>();
            var folders = new List<Folder>();
            var documents = new List<Document>();
            
            while (currentCycle++ < 100 && count++ < totalCount)
            {
                var user = MakeUser(count);
                users.Add(user);

                var contentType = MakeContentType(count);
                contentTypes.Add(contentType);

                var folder = MakeFolder(count, user.Id);
                folders.Add(folder);

                var document = MakeDocument(count, user.Id, contentType.ContentTypeId);
                documents.Add(document);
            }
        
            if (users.Count > 0)
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();
            }
            if (contentTypes.Count > 0)
            {
                context.ContentTypes.AddRange(contentTypes);
                await context.SaveChangesAsync();
            }
            if (folders.Count > 0)
            {
                context.Folders.AddRange(folders);
                await context.SaveChangesAsync();
            }
            if (documents.Count > 0)
            {
                context.Documents.AddRange(documents);
                await context.SaveChangesAsync();
            }

            currentCycle = 0;
        }
    }
}