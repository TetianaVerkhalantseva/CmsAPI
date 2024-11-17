using System.Text;
using CmsAPI;
using CmsAPI.Data;
using CmsAPI.Models.Entities;
using CmsAPI.Services.AuthServices;
using CmsAPI.Services.DocumentServices;
using CmsAPI.Services.FolderServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure the application to use controllers with JSON options to prevent cyclic references.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Enable Swagger/OpenAPI for API documentation and testing.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the database service with SQLite and DbContextFactory.
builder.Services.AddDbContextFactory<CmsContext>(options =>
    options.UseSqlite($"Data Source={nameof(CmsContext.CmsDb)}.db"));

// Configure Identity service for user management, using Entity Framework and default token providers.
builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<CmsContext>()
    .AddDefaultTokenProviders();

// Configure JWT authentication with token validation parameters and custom responses.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Disable HTTPS metadata for local development.
        
        // Set up token validation parameters to ensure secure JWT handling.
        var byteKey = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = true,
            ValidateIssuer = true, // Validate the token issuer.
            ValidateAudience = true, // Validate the token audience.
            RequireExpirationTime = true, // Ensure tokens expire.
            ValidateIssuerSigningKey = true, // Ensures the token's signing key is valid and trusted.                                     
            ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value, // The server that issues the JWT token.    
            ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value, // The client or application expected to consume the token.
            IssuerSigningKey = new SymmetricSecurityKey(byteKey)
        };
        
        // Provide a custom response for unauthorized access attempts.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Suppress the default response to send a custom one.
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    message = "Authorization required. Please log in to access this resource."
                }));
            }
        };
    });

// Register application services with appropriate lifetimes (Transient or Scoped).
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IFolderService, FolderService>();
builder.Services.AddScoped<CurrentUserContext>();

var app = builder.Build();

// Ensure database creation and seeding with example data if necessary.
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

// Enable Swagger in development mode for API testing and documentation.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware for HTTPS redirection, authentication, and authorization.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints to the corresponding controllers.
app.MapControllers();

// Start the application.
app.Run();
