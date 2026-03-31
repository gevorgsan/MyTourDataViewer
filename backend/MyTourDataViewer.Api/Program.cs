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

// ── Startup readiness flag ────────────────────────────────────────────────────
// The server starts listening immediately (before migrations run) so that Render
// can reach the /health endpoint.  All non-health requests receive 503 while
// migrations are pending; once migrations succeed the gate is set and the health
// check returns 200, at which point Render routes real traffic to this service.
// This eliminates the 502 Bad Gateway that nginx returned when the backend was
// not yet listening during the migration window.
//
// ManualResetEventSlim.IsSet is backed by a volatile field, ensuring changes are
// visible across threads without additional synchronisation.
const string HealthPath = "/health";
using var migrationsGate = new System.Threading.ManualResetEventSlim(false);

// Return 503 for every request except /health while migrations are running.
// This prevents requests from hitting controllers before the schema is ready.
app.Use((context, next) =>
{
    if (!migrationsGate.IsSet && !context.Request.Path.StartsWithSegments(HealthPath))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return context.Response.WriteAsJsonAsync(new { message = "Service is starting. Please try again in a moment." });
    }
    return next();
});

app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "MyTourDataViewer API v1");
    opt.RoutePrefix = "swagger";
});

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health endpoint returns 503 while migrations are in progress and 200 once they
// complete.  Render waits for 200 before routing traffic, so no real request
// reaches a controller until the database schema is fully applied.
app.MapGet(HealthPath, () => migrationsGate.IsSet
    ? Results.Ok(new { status = "healthy" })
    : Results.Json(new { status = "starting" }, statusCode: StatusCodes.Status503ServiceUnavailable))
   .AllowAnonymous();

// Start listening immediately so Render health checks succeed and nginx does not
// receive connection-refused errors that produce 502 responses.
await app.StartAsync();
Log.Information("Server started; running database migrations");

// Apply pending migrations with retry for transient PostgreSQL connection
// failures.  On Render free tier the managed PostgreSQL may take several seconds
// to accept connections after a cold start, so we back off exponentially.
const int maxMigrationRetries = 3;
for (var attempt = 1; attempt <= maxMigrationRetries; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
        break;
    }
    catch (Exception ex) when (attempt < maxMigrationRetries && IsTransientDbException(ex))
    {
        var delay = TimeSpan.FromSeconds(5 * attempt); // 5 s, 10 s
        Log.Warning(ex,
            "Migration attempt {Attempt}/{Max} failed (transient). Retrying in {Delay}s...",
            attempt, maxMigrationRetries, (int)delay.TotalSeconds);
        await Task.Delay(delay);
    }
    // Non-transient errors or the final attempt propagate, crashing the process
    // so Render restarts the container and tries again.
}

migrationsGate.Set();
Log.Information("Database migrations complete; service is healthy");

await app.WaitForShutdownAsync();

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
    var host     = uri.Host;
    var port     = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    var user     = "";
    var password  = "";
    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
        var parts = uri.UserInfo.Split(':', 2);
        user     = Uri.UnescapeDataString(parts[0]);
        password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
    }

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Require;TrustServerCertificate=true";
}

/// <summary>
/// Returns true when the exception represents a transient database connectivity
/// failure that is safe to retry (e.g. PostgreSQL not yet accepting connections
/// during a cold start).  Schema errors and other non-transient failures return
/// false so they are not retried unnecessarily.
/// </summary>
static bool IsTransientDbException(Exception ex)
{
    var pg = (ex as Npgsql.NpgsqlException)
          ?? ex.InnerException as Npgsql.NpgsqlException;
    return pg?.IsTransient ?? false;
}
