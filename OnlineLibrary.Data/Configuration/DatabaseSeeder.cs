using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Configuration
{
    public static class DatabaseSeeder
    {
        private static bool _rolesSeeded = false;
        private static bool _adminSeeded = false;
        private static readonly object _lock = new object();

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // Fast path: if already seeded, skip immediately
            if (_rolesSeeded) return;

            lock (_lock)
            {
                // Double-check after acquiring lock
                if (_rolesSeeded) return;

                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                string[] roles = { "Admin", "Manager", "User" };

                // Batch check: Query all roles at once to avoid N+1
                var existingRoles = roleManager.Roles
                    .Select(r => r.Name)
                    .ToHashSet();

                foreach (var role in roles)
                {
                    if (!existingRoles.Contains(role))
                    {
                        var result = roleManager.CreateAsync(new IdentityRole<Guid> { Name = role }).GetAwaiter().GetResult();
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to create role: {role}");
                        }
                    }
                }

                _rolesSeeded = true;
            }
        }

        public static async Task AssignAdminRoleAsync(IServiceProvider serviceProvider)
        {
            // Fast path: if already seeded, skip immediately
            if (_adminSeeded) return;

            lock (_lock)
            {
                // Double-check after acquiring lock
                if (_adminSeeded) return;

                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string adminEmail = "admin@onlinelibrary.com";
                string adminPassword = "Admin123!";

                var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail
                    };
                    var createUserResult = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
                    if (!createUserResult.Succeeded)
                    {
                        throw new Exception($"Failed to create admin user: {adminEmail}");
                    }
                }

                var isInRole = userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                if (!isInRole)
                {
                    var addRoleResult = userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    if (!addRoleResult.Succeeded)
                    {
                        throw new Exception($"Failed to assign admin role to user: {adminEmail}");
                    }
                }

                _adminSeeded = true;
            }
        }

        // Backward compatibility: synchronous wrappers
        public static void SeedRoles(IServiceProvider serviceProvider)
        {
            SeedRolesAsync(serviceProvider).GetAwaiter().GetResult();
        }

        public static void AssignAdminRole(IServiceProvider serviceProvider)
        {
            AssignAdminRoleAsync(serviceProvider).GetAwaiter().GetResult();
        }
    }
}

