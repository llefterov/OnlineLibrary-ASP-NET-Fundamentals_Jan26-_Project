using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Web.ViewModels.Admin.UserManagement;

namespace OnlineLibrary.Web.Areas.Admin.Controllers
{
    public class UserManagementController : BaseAdminController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole<Guid>> roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = userManager
                .Users
                .OrderBy(u => u.Email)
                .ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(Guid id)
        {
            var user = userManager.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roleManager.Roles
                    .Select(r => r.Name!)
                    .ToList(),
                SelectedRoles = (await userManager.GetRolesAsync(user)).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(Guid userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(role) || !await roleManager.RoleExistsAsync(role))
            {
                return RedirectToAction(nameof(Index));
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(Guid userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(role) || !await userManager.IsInRoleAsync(user, role))
            {
                return RedirectToAction(nameof(Index));
            }

            await userManager.RemoveFromRoleAsync(user, role);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            await userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
