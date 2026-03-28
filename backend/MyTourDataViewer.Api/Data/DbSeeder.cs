using Microsoft.AspNetCore.Identity;
using MyTourDataViewer.Api.Entities;

namespace MyTourDataViewer.Api.Data;

/// <summary>Seeds default roles and the initial admin account.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        // Seed roles
        foreach (var role in new[] { "Administrator", "Viewer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }

        // Seed default admin (credentials should be changed after first login)
        const string adminUser = "admin";
        var admin = await userManager.FindByNameAsync(adminUser);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUser,
                Email = "admin@mytourviewer.local",
                FullName = "System Administrator",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
            {
                logger.LogInformation("Default admin account created");
            }
        }

        if (admin != null && !await userManager.IsInRoleAsync(admin, "Administrator"))
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
            logger.LogInformation("Granted Administrator role to default admin account");
        }
    }
}
