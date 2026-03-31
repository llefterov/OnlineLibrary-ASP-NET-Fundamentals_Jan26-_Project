using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Web.Areas.Admin.Controllers;
using OnlineLibrary.Web.ViewModels.Admin.UserManagement;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminUserManagementControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;
        private UserManagementController _sut;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStore = new Mock<IRoleStore<IdentityRole<Guid>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
                roleStore.Object, null!, null!, null!, null!);

            _sut = new UserManagementController(_userManagerMock.Object, _roleManagerMock.Object);
            SetUser(Guid.NewGuid());
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        private void SetUser(Guid userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // Index
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Index_ReturnsViewWithOrderedUserViewModels()
        {
            var user1 = new ApplicationUser { Id = Guid.NewGuid(), Email = "zara@test.com", UserName = "zara" };
            var user2 = new ApplicationUser { Id = Guid.NewGuid(), Email = "alan@test.com", UserName = "alan" };
            var users = new List<ApplicationUser> { user1, user2 }.AsQueryable();

            _userManagerMock.Setup(um => um.Users).Returns(users);
            _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());

            var result = await _sut.Index();

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as List<UserViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(2));
            Assert.That(model[0].Email, Is.EqualTo("alan@test.com"));
        }

        [Test]
        public async Task Index_UsersHaveRoles_ViewModelContainsRoles()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, Email = "admin@test.com", UserName = "admin" };
            _userManagerMock.Setup(um => um.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            var result = await _sut.Index();

            var viewResult = result as ViewResult;
            var model = (viewResult!.Model as List<UserViewModel>)!;
            Assert.That(model[0].Roles, Contains.Item("Admin"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // AssignRole GET
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AssignRole_Get_UserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.Users).Returns(new List<ApplicationUser>().AsQueryable());

            var result = await _sut.AssignRole(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AssignRole_Get_ValidUser_ReturnsViewWithModel()
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
            _userManagerMock.Setup(um => um.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
            _roleManagerMock.Setup(rm => rm.Roles)
                .Returns(new List<IdentityRole<Guid>> { new IdentityRole<Guid> { Name = "Admin" } }.AsQueryable());

            var result = await _sut.AssignRole(userId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as UserViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(userId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // AssignRole POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AssignRole_Post_UserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.AssignRole(Guid.NewGuid(), "Admin");

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AssignRole_Post_EmptyRole_RedirectsToIndex()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _sut.AssignRole(user.Id, "");

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task AssignRole_Post_RoleDoesNotExist_RedirectsToIndex()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("NonExistent")).ReturnsAsync(false);

            var result = await _sut.AssignRole(user.Id, "NonExistent");

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task AssignRole_Post_UserAlreadyInRole_DoesNotAddAndRedirects()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin")).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var result = await _sut.AssignRole(user.Id, "Admin");

            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task AssignRole_Post_UserNotInRole_AddsRoleAndRedirects()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin")).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            _userManagerMock.Setup(um => um.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.AssignRole(user.Id, "Admin");

            _userManagerMock.Verify(um => um.AddToRoleAsync(user, "Admin"), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // RemoveRole POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task RemoveRole_UserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.RemoveRole(Guid.NewGuid(), "Admin");

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RemoveRole_EmptyRole_RedirectsToIndex()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _sut.RemoveRole(user.Id, "");

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task RemoveRole_UserNotInRole_RedirectsToIndex()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var result = await _sut.RemoveRole(user.Id, "Admin");

            _userManagerMock.Verify(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task RemoveRole_UserInRole_RemovesRoleAndRedirects()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.RemoveFromRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _sut.RemoveRole(user.Id, "Admin");

            _userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, "Admin"), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteUser POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.DeleteUser(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteUser_ValidUser_DeletesAndRedirectsToIndex()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _sut.DeleteUser(user.Id);

            _userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        }
    }
}
