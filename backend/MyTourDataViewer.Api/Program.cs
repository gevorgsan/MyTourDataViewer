using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// ── MVC / Controllers ──────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyTourDataViewer API",
        Version = "v1"
    });
});

// ── Database ───────────────────────────────────────────────────────────────────
// Switch between SQLite (default) and PostgreSQL via the DBPROVIDER env var.
var dbProvider = builder.Configuration["DbProvider"] ?? "sqlite";
Log.Information("DbProvider = {DbProvider}", dbProvider);

if (dbProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
{
    var rawConnectionString = builder.Configuration.GetConnectionString("Postgres");
    Log.Information("PostgreSQL connection string is {Status}",
        string.IsNullOrWhiteSpace(rawConnectionString) ? "MISSING" : "present");

    var pgConnectionString = NormalizePostgresConnectionString(rawConnectionString);
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(pgConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ── Identity ───────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequireDigit = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireLowercase = true;
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── CORS ───────────────────────────────────────────────────────────────────────
// Set CORS_ORIGINS to a comma-separated list of allowed origins.
// Defaults to localhost for local development.
// Render's fromService host property returns a bare hostname; prepend https:// when no scheme is present.
var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || o.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? o : $"https://{o}")
    .ToArray();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── HttpClient ─────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// ── Application Services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApiSettingsService, ApiSettingsService>();
builder.Services.AddScoped<IExternalApiAuthorizationService, ExternalApiAuthorizationService>();
builder.Services.AddScoped<IDefaultApiAuthorizationProvider, DefaultApiAuthorizationProvider>();
builder.Services.AddScoped<IExternalApiClientService, ExternalApiClientService>();
builder.Services.AddScoped<ISearchRequestService, SearchRequestService>();

// ──────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// Trust X-Forwarded-For and X-Forwarded-Proto from reverse proxies (e.g. Render).
// Must be first in the pipeline so subsequent middleware sees the correct scheme/IP.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MyTourDataViewer API v1");
    opt.RoutePrefix = "swagger";
});

// Apply pending migrations and seed default data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .AllowAnonymous();

app.Run();

// ── Helpers ──────────────────────────────────────────────────────────────────
/// <summary>
/// Converts a PostgreSQL URI (postgres://user:pass@host:port/db) to an
/// ADO.NET connection string that Npgsql understands.
/// Render's managed PostgreSQL injects the URI format; Npgsql requires the
/// key=value format.  If the input is already in ADO.NET format it is
/// returned unchanged.
/// </summary>
static string? NormalizePostgresConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        return connectionString;

    if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return connectionString;

    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user     = Uri.UnescapeDataString(userInfo[0]);
    var password  = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host     = uri.Host;
    var port     = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

