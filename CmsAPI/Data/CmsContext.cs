using System.Diagnostics;
using CmsAPI.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CmsAPI.Data;

public class CmsContext : IdentityDbContext<User>
{
    public static readonly string RowVersion = nameof(RowVersion);
    
    public static readonly string CmsDb = nameof(CmsDb).ToLower();

    // Inject options
    public CmsContext(DbContextOptions<CmsContext> options) : base(options)
    {
        Debug.WriteLine($"{ContextId} context created");
    }
    
    // No need to define User table
    public DbSet<Document> Documents { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<ContentType> ContentTypes { get; set; }
    
    // modelBuilder and configure RowVersion
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Document>()
            .Property<byte[]>(RowVersion)
            .IsRowVersion();

        builder.Entity<Folder>()
            .Property<byte[]>(RowVersion)
            .IsRowVersion();
    
        builder.Entity<ContentType>()
            .Property<byte[]>(RowVersion)
            .IsRowVersion();
        
        base.OnModelCreating(builder);
    }

    // Dispose pattern
    public override void Dispose()
    {
        Debug.WriteLine($"{ContextId} context disposed");
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        Debug.WriteLine($"{ContextId} context disposed async.");
        return base.DisposeAsync();
    }
}