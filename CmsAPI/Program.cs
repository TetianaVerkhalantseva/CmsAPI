using System.Text;
using CmsAPI;
using CmsAPI.Data;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using CmsAPI.Services.DocumentServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<CmsContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        var byteKey = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,                                            // In the file appsettings.json:
            ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,     // Change to the location of the server issuing the token
            ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value, // Change to the location of the client
            IssuerSigningKey = new SymmetricSecurityKey(byteKey)
        };
    });

// Add transient services
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddTransient<IAuthService, AuthService>();

var app = builder.Build();

// Ensure that the database has been created and data inserted (if required)
await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<CmsContext>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(); // Get UserManager<User>
try
{
    await DatabaseUtility.EnsureDbCreatedAndSeedWithCountOfAsync(options, userManager, 10); // Pass userManager and count
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
