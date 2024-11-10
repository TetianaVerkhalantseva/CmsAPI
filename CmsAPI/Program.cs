using CmsAPI;
using CmsAPI.Data;
using CmsAPI.Models.Entities;
using CmsAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add controller service to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DB service
builder.Services.AddDbContextFactory<CmsContext>(options =>
    options.UseSqlite($"Data Source={nameof(CmsContext.CmsDb)}.db"));

// Configure Identity service
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<CmsContext>()
    .AddDefaultTokenProviders();

// Add transient services
builder.Services.AddTransient<IDocumentService, DocumentService>();

var app = builder.Build();

// Ensure that the database has been created and data inserted (if required)
await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<CmsContext>>();
try
{
    await DatabaseUtility.EnsureDbCreatedAndSeedWithCountOfAsync(options, 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Database seeding error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
