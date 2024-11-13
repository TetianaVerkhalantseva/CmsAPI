using CmsAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Document = CmsAPI.Models.Entities.Document;

namespace CmsAPI.Data;

public class SeedCms
{
    private readonly string[] _contentTypes = { "Text", "Image", "Video", "Audio", "Link", "Contact" };
    private readonly string[] _folderNames = { "Test folder 1", "Test folder 2", "Test folder 3", "Secret folder 1", "Secret folder 2", "Secret folder 3" };
    private readonly string[] _userFirstNames = { "Adam", "Bob", "Charles", "Dave", "Davis", "Emily" };
    private readonly string[] _userLastNames = { "Ericsson", "Johnson", "James", "Jones", "Mayer", "Mary" };
    private List<ContentType> _uniqueContentTypes;

    private readonly Random _random = new Random();
    private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

    private int _folderIdCounter = 1;
    private int _documentIdCounter = 1;

    public SeedCms()
    {
        _uniqueContentTypes = Enumerable.Range(1, _contentTypes.Length)
            .Select(i => new ContentType { ContentTypeId = i, Type = _contentTypes[i - 1] })
            .ToList();
    }

    private string RandomOne(string[] list)
    {
        var idx = _random.Next(list.Length);
        return list[idx];
    }

    private User MakeUser(int id)
    {
        var userName = "user" + _random.Next(100, 9999);
        var email = userName + "@example.com";
        
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            NormalizedUserName = userName.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            FirstName = RandomOne(_userFirstNames),
            LastName = RandomOne(_userLastNames),
            PasswordHash = _hasher.HashPassword(null, "UserPassword123!"),
            SecurityStamp = string.Empty
        };
    }

    private Folder MakeFolder(string userId, List<int> existingFolderIds)
    {
        int folderId = _folderIdCounter++;
        int? parentFolderId = null;

        if (existingFolderIds.Any())
        {
            do
            {
                parentFolderId = existingFolderIds[_random.Next(existingFolderIds.Count)];
            } while (parentFolderId == folderId);
        }

        existingFolderIds.Add(folderId);

        return new Folder
        {
            FolderId = folderId,
            FolderName = RandomOne(_folderNames),
            ParentFolderId = parentFolderId,
            UserId = userId
        };
    }

    private Document MakeDocument(string userId, int contentTypeId)
    {
        int documentId = _documentIdCounter++;

        return new Document
        {
            DocumentId = documentId,
            Title = $"Document {documentId}",
            Content = $"Content for document {documentId}",
            CreatedOn = DateTime.Now.AddDays(-_random.Next(0, 365))
                .AddHours(_random.Next(0, 24))
                .AddMinutes(_random.Next(0, 60))
                .AddSeconds(_random.Next(0, 60)),
            UserId = userId,
            ContentTypeId = contentTypeId
        };
    }

    public async Task SeedDatabaseWithItemCountOfAsync(CmsContext context, int totalCount)
    {
        var count = 0;
        var currentCycle = 0;

        if (!context.ContentTypes.Any())
        {
            context.ContentTypes.AddRange(_uniqueContentTypes);
            await context.SaveChangesAsync();
        }

        while (count < totalCount)
        {
            var users = new List<User>();
            var folders = new List<Folder>();
            var documents = new List<Document>();
            var existingFolderIds = new List<int>();

            while (currentCycle++ < 100 && count++ < totalCount)
            {
                var user = MakeUser(count);
                users.Add(user);

                for (int i = 0; i < 4; i++)
                {
                    var folder = MakeFolder(user.Id, existingFolderIds);
                    folders.Add(folder);

                    var randomContentType = _uniqueContentTypes[_random.Next(_uniqueContentTypes.Count)];
                    var document = MakeDocument(user.Id, randomContentType.ContentTypeId);
                    documents.Add(document);
                }
            }

            if (users.Count > 0) context.Users.AddRange(users);
            if (folders.Count > 0) context.Folders.AddRange(folders);
            if (documents.Count > 0) context.Documents.AddRange(documents);

            await context.SaveChangesAsync();
            currentCycle = 0;
        }
    }
}
