using CmsAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CmsAPI.Data;

public class SeedCms
{
    private readonly string[] _contentTypes = { "Text", "Image", "Video", "Audio", "Link", "Contact" };
    private readonly string[] _folderNames = { "Test folder 1", "Test folder 2", "Test folder 3", "Secret folder 1", "Secret folder 2", "Secret folder 3" };
    private readonly List<ContentType> _uniqueContentTypes;

    private readonly Random _random = new Random();
    private readonly UserManager<User> _userManager;

    private int _folderIdCounter = 1;
    private int _documentIdCounter = 1;

    public SeedCms(UserManager<User> userManager)
    {
        _userManager = userManager;
        _uniqueContentTypes = Enumerable.Range(1, _contentTypes.Length)
            .Select(i => new ContentType { ContentTypeId = i, Type = _contentTypes[i - 1] })
            .ToList();
    }

    private string RandomOne(string[] list)
    {
        var idx = _random.Next(list.Length);
        return list[idx];
    }

    private async Task<User> MakeUserAsync()
    {
        var userName = "user" + _random.Next(100, 9999);
        var email = userName + "@example.com";
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            NormalizedUserName = userName.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper()
        };

        var result = await _userManager.CreateAsync(user, "UserPassword123!");
        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user");
        }

        return user;
    }

    private Folder MakeFolder(string userId, List<int> existingFolderIds, List<Folder> userFolders, bool hasParent = false)
    {
        int folderId = _folderIdCounter++;
        Folder? parentFolder = hasParent && userFolders.Any() ? userFolders[_random.Next(userFolders.Count)] : null;

        var newFolder = new Folder
        {
            FolderId = folderId,
            FolderName = RandomOne(_folderNames),
            ParentFolderId = parentFolder?.FolderId,
            ParentFolder = parentFolder,
            UserId = userId,
            User = parentFolder?.User
        };

        parentFolder?.Folders.Add(newFolder);
        existingFolderIds.Add(folderId);
        return newFolder;
    }

    private Document MakeDocument(string userId, int contentTypeId, Folder? folder = null)
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
            ContentTypeId = contentTypeId,
            FolderId = folder?.FolderId.ToString() ?? string.Empty,
            Folder = folder
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
            var documents = new List<Document>();

            while (currentCycle++ < 100 && count++ < totalCount)
            {
                var user = await MakeUserAsync();
                var userFolders = new List<Folder>();
                var existingFolderIds = new List<int>();

                // Create one folder without parent and three folders with parent for user
                var rootFolder = MakeFolder(user.Id, existingFolderIds, userFolders, false);
                userFolders.Add(rootFolder);

                for (int i = 0; i < 3; i++)
                {
                    var childFolder = MakeFolder(user.Id, existingFolderIds, userFolders, true);
                    userFolders.Add(childFolder);
                }

                var randomContentType = _uniqueContentTypes[_random.Next(_uniqueContentTypes.Count)];

                // Create documents
                documents.Add(MakeDocument(user.Id, randomContentType.ContentTypeId)); // No folder
                documents.Add(MakeDocument(user.Id, randomContentType.ContentTypeId, rootFolder)); // In the root folder
                documents.Add(MakeDocument(user.Id, randomContentType.ContentTypeId, userFolders[1])); // In the child folder
                documents.Add(MakeDocument(user.Id, randomContentType.ContentTypeId, userFolders[2])); // In another child folder

                if (userFolders.Count > 0) context.Folders.AddRange(userFolders);
            }

            if (documents.Count > 0) context.Documents.AddRange(documents);

            await context.SaveChangesAsync();
            currentCycle = 0;
        }
    }
}
