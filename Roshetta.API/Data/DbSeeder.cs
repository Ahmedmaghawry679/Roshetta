using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roshetta.API.Models;

namespace Roshetta.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<AppDbContext>();
            var config = services.GetRequiredService<IConfiguration>();

            // 1. Roles
            string[] roles = { Roles.Admin, Roles.Doctor, Roles.Patient };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    if (!result.Succeeded)
                        throw new InvalidOperationException(
                            $"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 2. Admin user
            var adminEmail = config["AdminSeed:Email"]
                ?? throw new InvalidOperationException("AdminSeed:Email is missing. Set it with user-secrets.");
            var adminPassword = config["AdminSeed:Password"]
                ?? throw new InvalidOperationException("AdminSeed:Password is missing. Set it with user-secrets.");

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Failed to create admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }

            // 3. Specializations
            if (!await context.Specializations.AnyAsync())
            {
                var specializations = new[]
                {
                    "Cardiology", "Dermatology", "Pediatrics", "Orthopedics",
                    "Neurology", "Dentistry", "Ophthalmology", "ENT", "Psychiatry",
                    "Internal Medicine", "Gynecology", "Urology"
                }.Select(name => new Specialization { Name = name });

                await context.Specializations.AddRangeAsync(specializations);
                await context.SaveChangesAsync();
            }
        }
    }
}