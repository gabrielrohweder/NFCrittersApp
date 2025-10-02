using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using Npgsql;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Configure database - convert DATABASE_URL to Npgsql connection string
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parse DATABASE_URL (postgresql://user:password@host:port/database?options)
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    var connStrBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Database = databaseUri.AbsolutePath.TrimStart('/'),
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        SslMode = SslMode.Require
    };
    
    connectionString = connStrBuilder.ToString();
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=localhost;Database=animalcollector;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    }));

// Add health checks for Cloud Run with database connectivity check
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: connectionString,
        name: "database",
        timeout: TimeSpan.FromSeconds(5),
        tags: new[] { "ready" });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add session support for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add external authentication (Google and Apple) - only if configured
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/api/auth/external-login";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// Add Google authentication if configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/api/auth/signin-google";
        options.SaveTokens = true;
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });
    Console.WriteLine("Google authentication configured");
}

// Add Apple authentication if configured
var appleClientId = builder.Configuration["Authentication:Apple:ClientId"];
var appleKeyId = builder.Configuration["Authentication:Apple:KeyId"];
var appleTeamId = builder.Configuration["Authentication:Apple:TeamId"];
if (!string.IsNullOrEmpty(appleClientId) && !string.IsNullOrEmpty(appleKeyId) && !string.IsNullOrEmpty(appleTeamId))
{
    authBuilder.AddApple(options =>
    {
        options.ClientId = appleClientId;
        options.KeyId = appleKeyId;
        options.TeamId = appleTeamId;
        options.GenerateClientSecret = true;
        
        var privateKeyContent = builder.Configuration["Authentication:Apple:PrivateKey"];
        if (!string.IsNullOrEmpty(privateKeyContent))
        {
            options.PrivateKey = (keyId, cancellationToken) =>
            {
                return Task.FromResult(privateKeyContent.AsMemory());
            };
        }
        
        options.CallbackPath = "/api/auth/signin-apple";
        options.SaveTokens = true;
    });
    Console.WriteLine("Apple authentication configured");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database asynchronously in background to not block startup
_ = Task.Run(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await db.Database.EnsureCreatedAsync();
            Console.WriteLine("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization error: {ex.Message}");
        }
    }
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure static file options to serve all Blazor WASM files
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".pdb"] = "application/octet-stream";
provider.Mappings[".br"] = "application/octet-stream";
provider.Mappings[".json"] = "application/json";

// Serve the Client's wwwroot files
var clientWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "..", "Client", "wwwroot");
if (Directory.Exists(clientWwwroot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientWwwroot),
        ContentTypeProvider = provider
    });
}

// Serve the Client's build output (_framework folder)
var clientBuildOutput = Path.Combine(Directory.GetCurrentDirectory(), "..", "Client", "bin", "Debug", "net8.0", "wwwroot");
if (Directory.Exists(clientBuildOutput))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientBuildOutput),
        ContentTypeProvider = provider
    });
}

// Fallback to default wwwroot if it exists
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Add health check endpoints for Cloud Run
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapControllers();
app.MapFallbackToFile("index.html");

// Configure to listen on all interfaces on port 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine($"Starting server on port {port}");
app.Run($"http://0.0.0.0:{port}");
