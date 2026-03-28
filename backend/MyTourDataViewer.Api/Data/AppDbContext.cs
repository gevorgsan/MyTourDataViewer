using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyTourDataViewer.Api.Entities;

namespace MyTourDataViewer.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApiSettings> ApiSettings => Set<ApiSettings>();
    public DbSet<ApiEndpointSettings> ApiEndpointSettings => Set<ApiEndpointSettings>();
    public DbSet<ApiEndpointHeader> ApiEndpointHeaders => Set<ApiEndpointHeader>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var authorizationTypeConverter = new EnumToStringConverter<AuthorizationType>();

        builder.Entity<ApiSettings>(entity =>
        {
            entity.Property(a => a.Name)
                .HasMaxLength(200);

            entity.Property(a => a.BaseUrl)
                .HasMaxLength(2048);

            entity.Property(a => a.AuthorizationType)
                .HasConversion(authorizationTypeConverter)
                .HasMaxLength(32);

            entity.Property(a => a.TokenUrl)
                .HasMaxLength(2048);

            entity.HasMany(a => a.Endpoints)
                .WithOne(e => e.ApiSettings)
                .HasForeignKey(e => e.ApiSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApiEndpointSettings>(entity =>
        {
            entity.Property(e => e.Name)
                .HasMaxLength(200);

            entity.Property(e => e.Url)
                .HasMaxLength(2048);

            entity.Property(e => e.HttpMethod)
                .HasMaxLength(16);

            entity.Property(e => e.AuthorizationType)
                .HasConversion(authorizationTypeConverter)
                .HasMaxLength(32);

            entity.Property(e => e.TokenEndpointUrl)
                .HasMaxLength(2048);

            entity.Property(e => e.Username)
                .HasMaxLength(256);

            entity.Property(e => e.ClientId)
                .HasMaxLength(256);

            entity.HasMany(e => e.Headers)
                .WithOne(h => h.ApiEndpointSettings)
                .HasForeignKey(h => h.ApiEndpointSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApiEndpointHeader>(entity =>
        {
            entity.Property(h => h.Name)
                .HasMaxLength(256);

            entity.Property(h => h.Value)
                .HasMaxLength(2048);
        });
    }
}
