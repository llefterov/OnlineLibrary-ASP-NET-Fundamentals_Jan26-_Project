using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.Areas.Admin.Controllers;
using OnlineLibrary.Web.ViewModels.Author;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminAuthorManagementControllerTests
    {
        private Mock<IAuthorManagementService> _serviceMock;
        private Mock<ILogger<AuthorManagementController>> _loggerMock;
        private AuthorManagementController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IAuthorManagementService>();
            _loggerMock = new Mock<ILogger<AuthorManagementController>>();
            _sut = new AuthorManagementController(_serviceMock.Object, _loggerMock.Object);
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
        // Manage
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Manage_ReturnsViewWithAuthorList()
        {
            var tuple = (new List<AuthorsAllDto> { new AuthorsAllDto { FullName = "Alan Poe" } }.AsEnumerable(), 1);
            _serviceMock.Setup(s => s.GetAllAuthorsForViewModelAsync(null, 1, 20)).ReturnsAsync(tuple);

            var result = await _sut.Manage();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add GET
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Add_Get_ReturnsViewWithEmptyModel()
        {
            var result = _sut.Add();

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.InstanceOf<AuthorAddViewModel>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Add_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var model = new AuthorAddViewModel { FullName = "X" };
            _sut.ModelState.AddModelError("FullName", "Too short");

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Add_Post_ValidModel_RedirectsToManage()
        {
            var model = new AuthorAddViewModel { FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>())).Returns(Task.CompletedTask);

            var result = await _sut.Add(model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Add_Post_AuthorAlreadyExists_ReturnsViewWithModelError()
        {
            var model = new AuthorAddViewModel { FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorAlreadyExistsException("Already exists"));

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_AuthorCreateException_ReturnsViewWithModelError()
        {
            var model = new AuthorAddViewModel { FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorCreateException("Create failed"));

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var model = new AuthorAddViewModel { FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit GET
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Get_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Edit(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Get_AuthorNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetNewAuthorForEditByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AuthorsAllDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ValidAuthor_ReturnsViewWithModel()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetNewAuthorForEditByIdAsync(authorId))
                .ReturnsAsync(new AuthorsAllDto { Id = authorId, FullName = "Alan Poe" });

            var result = await _sut.Edit(authorId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as AuthorEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(authorId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_EmptyId_ReturnsBadRequest()
        {
            var model = new AuthorEditViewModel { Id = Guid.NewGuid(), FullName = "Alan Poe" };

            var result = await _sut.Edit(Guid.Empty, model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var model = new AuthorEditViewModel { Id = Guid.NewGuid(), FullName = "Alan Poe" };

            var result = await _sut.Edit(Guid.NewGuid(), model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            var model = new AuthorEditViewModel { Id = id, FullName = "X" };
            _sut.ModelState.AddModelError("FullName", "Too short");

            var result = await _sut.Edit(id, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Edit_Post_ValidData_RedirectsToManage()
        {
            var id = Guid.NewGuid();
            var model = new AuthorEditViewModel { Id = id, FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>())).ReturnsAsync(true);

            var result = await _sut.Edit(id, model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Edit_Post_UpdateReturnsFalse_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var model = new AuthorEditViewModel { Id = id, FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>())).ReturnsAsync(false);

            var result = await _sut.Edit(id, model);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_AuthorUpdateException_ReturnsViewWithModelError()
        {
            var id = Guid.NewGuid();
            var model = new AuthorEditViewModel { Id = id, FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorUpdateExeption("Update failed"));

            var result = await _sut.Edit(id, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Edit_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var id = Guid.NewGuid();
            var model = new AuthorEditViewModel { Id = id, FullName = "Alan Poe" };
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _sut.Edit(id, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Delete GET
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Delete_Get_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Delete(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Delete_Get_AuthorNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AuthorDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ValidAuthor_ReturnsViewWithModel()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(authorId))
                .ReturnsAsync(new AuthorDeleteDto { Id = authorId, FullName = "Alan Poe" });

            var result = await _sut.Delete(authorId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteConfirmed POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteConfirmed_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.DeleteConfirmed(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task DeleteConfirmed_DeleteReturnsFalse_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_ValidId_RedirectsToManage()
        {
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task DeleteConfirmed_AuthorDeleteException_AuthorFoundAfter_ReturnsView()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(authorId))
                .ThrowsAsync(new AuthorDeleteException("Has books"));
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(authorId))
                .ReturnsAsync(new AuthorDeleteDto { Id = authorId, FullName = "Alan Poe" });

            var result = await _sut.DeleteConfirmed(authorId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_AuthorDeleteException_AuthorNotFoundAfter_ReturnsNotFound()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(authorId))
                .ThrowsAsync(new AuthorDeleteException("Has books"));
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(authorId))
                .ReturnsAsync((AuthorDeleteDto?)null);

            var result = await _sut.DeleteConfirmed(authorId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_UnexpectedException_AuthorFoundAfter_ReturnsView()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(authorId))
                .ThrowsAsync(new Exception("Unexpected"));
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(authorId))
                .ReturnsAsync(new AuthorDeleteDto { Id = authorId, FullName = "Alan Poe" });

            var result = await _sut.DeleteConfirmed(authorId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_UnexpectedException_AuthorNotFoundAfter_ReturnsNotFound()
        {
            var authorId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(authorId))
                .ThrowsAsync(new Exception("Unexpected"));
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(authorId))
                .ReturnsAsync((AuthorDeleteDto?)null);

            var result = await _sut.DeleteConfirmed(authorId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
