using CmsAPI;
using CmsAPI.Data;
using CmsAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database service
builder.Services.AddDbContextFactory<CmsContext>(options =>
    options.UseSqlite($"Data Source={nameof(CmsContext.CmsDb)}.db"));

// Configure Identity 
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<CmsContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Ensure that the database has been created and data inserted (if required)
await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<CmsContext>>();
await DatabaseUtility.EnsureDbCreatedAndSeedWithCountOfAsync(options, 10);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();