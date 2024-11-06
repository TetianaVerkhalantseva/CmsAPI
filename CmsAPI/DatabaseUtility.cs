using CmsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI;

public class DatabaseUtility
{
    // Method to see the database. Should not be used in production: demo purposes only.
    // options: The configured options.
    // count: The number of concerts to seed.
    public static async Task EnsureDbCreatedAndSeedWithCountOfAsync(DbContextOptions<CmsContext> options, int count)
    {
        // Empty to avoid logging while inserting (otherwise will flood console).
        var factory = new LoggerFactory();
        var builder = new DbContextOptionsBuilder<CmsContext>(options)
            .UseLoggerFactory(factory);

        using var context = new CmsContext(builder.Options);
        // Result is true if the database had to be created.
        if (await context.Database.EnsureCreatedAsync())
        {
            var seed = new SeedCms();
            await seed.SeedDatabaseWithItemCountOfAsync(context, count);
        }
    }
}