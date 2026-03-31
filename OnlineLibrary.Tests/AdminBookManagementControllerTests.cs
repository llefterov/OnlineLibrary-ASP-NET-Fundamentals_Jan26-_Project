using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.Areas.Admin.Controllers;
using OnlineLibrary.Web.ViewModels.Books;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminBookManagementControllerTests
    {
        private Mock<IBookManagementService> _serviceMock;
        private Mock<ILogger<BookManagementController>> _loggerMock;
        private BookManagementController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IBookManagementService>();
            _loggerMock = new Mock<ILogger<BookManagementController>>();
            _sut = new BookManagementController(_serviceMock.Object, _loggerMock.Object);
            SetUser(Guid.NewGuid());

            // Default: dropdown population always returns empty lists
            _serviceMock
                .Setup(s => s.GetAllAuthorsAndPublishersAsync())
                .ReturnsAsync((new List<Publisher>(), (IEnumerable<Author>)new List<Author>()));
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

        private static BookCreateViewModel MakeCreateViewModel(Guid publisherId) => new BookCreateViewModel
        {
            Title = "Test Book",
            Description = "A description for a test book entry",
            Genre = BookGenre.Fiction,
            IsRead = false,
            Rating = 5,
            CoverUrl = "http://example.com/cover.jpg",
            DateAdded = DateTime.UtcNow,
            PublisherId = publisherId
        };

        private static BookEditViewModel MakeEditViewModel(Guid bookId, Guid publisherId) => new BookEditViewModel
        {
            Id = bookId,
            Title = "Test Book",
            Description = "A description for a test book entry",
            Genre = BookGenre.Fiction,
            IsRead = false,
            Rating = 5,
            CoverUrl = "http://example.com/cover.jpg",
            DateAdded = DateTime.UtcNow,
            PublisherId = publisherId
        };

        // ──────────────────────────────────────────────────────────────────────
        // Manage
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Manage_ReturnsViewWithBookList()
        {
            _serviceMock.Setup(s => s.GetAllBooksForAdminDtoAsync())
                .ReturnsAsync(new List<BookAllDto>());

            var result = await _sut.Manage();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Create GET
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Create_Get_ReturnsViewWithModel()
        {
            _serviceMock.Setup(s => s.GetBookDtoCreateViewModelAsync())
                .ReturnsAsync(new BookCreateDto { Title = string.Empty, Description = string.Empty });

            var result = await _sut.Create();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Create POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var model = MakeCreateViewModel(Guid.NewGuid());
            _sut.ModelState.AddModelError("Title", "Required");

            var result = await _sut.Create(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToManage()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = MakeCreateViewModel(Guid.NewGuid());
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), userId))
                .Returns(Task.CompletedTask);

            var result = await _sut.Create(model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Create_Post_PublisherDoesntExist_ReturnsViewWithModelError()
        {
            var model = MakeCreateViewModel(Guid.NewGuid());
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), It.IsAny<Guid>()))
                .ThrowsAsync(new PublisherDoesntExistException("No publisher"));

            var result = await _sut.Create(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Create_Post_AuthorDoesntExist_ReturnsViewWithModelError()
        {
            var model = MakeCreateViewModel(Guid.NewGuid());
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), It.IsAny<Guid>()))
                .ThrowsAsync(new AuthorDoesntExistException("No author"));

            var result = await _sut.Create(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Create_Post_InvalidOperationException_ReturnsViewWithModelError()
        {
            var model = MakeCreateViewModel(Guid.NewGuid());
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), It.IsAny<Guid>()))
                .ThrowsAsync(new InvalidOperationException("Validation error"));

            var result = await _sut.Create(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Create_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var model = MakeCreateViewModel(Guid.NewGuid());
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _sut.Create(model);

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
        public async Task Edit_Get_BookNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetBookForAdminEditDtoAsync(It.IsAny<Guid>()))
                .ReturnsAsync((BookEditDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ValidBook_ReturnsViewWithModel()
        {
            var bookId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetBookForAdminEditDtoAsync(bookId))
                .ReturnsAsync(new BookEditDto
                {
                    Id = bookId,
                    Title = "Test",
                    Description = "Desc",
                    Genre = BookGenre.Fiction,
                    DateAdded = DateTime.UtcNow,
                    PublisherId = Guid.NewGuid()
                });

            var result = await _sut.Edit(bookId);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_EmptyId_ReturnsBadRequest()
        {
            var model = MakeEditViewModel(Guid.NewGuid(), Guid.NewGuid());

            var result = await _sut.Edit(Guid.Empty, model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var model = MakeEditViewModel(Guid.NewGuid(), Guid.NewGuid());

            var result = await _sut.Edit(Guid.NewGuid(), model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _sut.ModelState.AddModelError("Title", "Required");

            var result = await _sut.Edit(bookId, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Edit_Post_ValidData_RedirectsToManage()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _serviceMock.Setup(s => s.EditBookForAdminDtoAsync(It.IsAny<BookEditDto>())).ReturnsAsync(true);

            var result = await _sut.Edit(bookId, model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Edit_Post_EditReturnsFalse_ReturnsNotFound()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _serviceMock.Setup(s => s.EditBookForAdminDtoAsync(It.IsAny<BookEditDto>())).ReturnsAsync(false);

            var result = await _sut.Edit(bookId, model);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_PublisherDoesntExist_ReturnsViewWithModelError()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _serviceMock.Setup(s => s.EditBookForAdminDtoAsync(It.IsAny<BookEditDto>()))
                .ThrowsAsync(new PublisherDoesntExistException("No publisher"));

            var result = await _sut.Edit(bookId, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Edit_Post_AuthorDoesntExist_ReturnsViewWithModelError()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _serviceMock.Setup(s => s.EditBookForAdminDtoAsync(It.IsAny<BookEditDto>()))
                .ThrowsAsync(new AuthorDoesntExistException("No author"));

            var result = await _sut.Edit(bookId, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Edit_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var bookId = Guid.NewGuid();
            var model = MakeEditViewModel(bookId, Guid.NewGuid());
            _serviceMock.Setup(s => s.EditBookForAdminDtoAsync(It.IsAny<BookEditDto>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var result = await _sut.Edit(bookId, model);

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
        public async Task Delete_Get_BookNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetBookAdminDeleteDetailsDtoAsync(It.IsAny<Guid>()))
                .ReturnsAsync((BookDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ValidBook_ReturnsViewWithModel()
        {
            var bookId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetBookAdminDeleteDetailsDtoAsync(bookId))
                .ReturnsAsync(new BookDeleteDto { Id = bookId, Title = "Test Book" });

            var result = await _sut.Delete(bookId);

            Assert.That(result, Is.InstanceOf<ViewResult>());
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
            _serviceMock.Setup(s => s.DeleteBookForAdminDtoAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_ValidId_RedirectsToManage()
        {
            _serviceMock.Setup(s => s.DeleteBookForAdminDtoAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task DeleteConfirmed_Exception_BookFoundAfter_ReturnsView()
        {
            var bookId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteBookForAdminDtoAsync(bookId))
                .ThrowsAsync(new Exception("Error"));
            _serviceMock.Setup(s => s.GetBookAdminDeleteDetailsDtoAsync(bookId))
                .ReturnsAsync(new BookDeleteDto { Id = bookId, Title = "Test Book" });

            var result = await _sut.DeleteConfirmed(bookId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_Exception_BookNotFoundAfter_ReturnsNotFound()
        {
            var bookId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteBookForAdminDtoAsync(bookId))
                .ThrowsAsync(new Exception("Error"));
            _serviceMock.Setup(s => s.GetBookAdminDeleteDetailsDtoAsync(bookId))
                .ReturnsAsync((BookDeleteDto?)null);

            var result = await _sut.DeleteConfirmed(bookId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Restore POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Restore_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Restore(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Restore_BookNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.RestoreBookForAdminDtoAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.Restore(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Restore_ValidId_RedirectsToManage()
        {
            _serviceMock.Setup(s => s.RestoreBookForAdminDtoAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.Restore(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }
    }
}
