using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.Controllers;
using OnlineLibrary.Web.ViewModels.Books;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BooksControllerTests
    {
        private Mock<IBooksService> _serviceMock;
        private Mock<ILogger<BooksController>> _loggerMock;
        private BooksController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IBooksService>();
            _loggerMock = new Mock<ILogger<BooksController>>();
            _sut = new BooksController(_serviceMock.Object, _loggerMock.Object);

            // Default: publishers/authors list returns empty (needed by Create/Edit actions)
            _serviceMock
                .Setup(s => s.GetAllAuthorsAndPublishersAsync())
                .ReturnsAsync((new List<Publisher>(), (IEnumerable<Author>)new List<Author>()));
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        private void SetUser(Guid userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetAnonymousUser()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
        }

        private static BookAllDto MakeBookAllDto(string title = "Test Book") => new BookAllDto
        {
            Id = Guid.NewGuid(),
            Title = title,
            Genre = BookGenre.Fiction,
            GenreName = "Fiction",
            Rating = 5,
            CoverUrl = "http://example.com/cover.jpg",
            AddedByUserName = "alice",
            PublisherId = Guid.NewGuid(),
            PublisherName = "Publisher"
        };

        private static BookDetailsDto MakeBookDetailsDto(Guid id) => new BookDetailsDto
        {
            Id = id,
            Title = "Details Book",
            Description = "Desc",
            Genre = BookGenre.Fiction,
            GenreName = "Fiction",
            IsRead = false,
            Rating = 5,
            CoverUrl = "http://example.com/cover.jpg",
            DateAdded = DateTime.UtcNow.ToString("dd-MM-yyyy"),
            PublisherId = Guid.NewGuid(),
            PublisherName = "Publisher",
            AuthorsName = "Author A",
            AddedByUserName = "alice"
        };

        private static BookEditDto MakeBookEditDto(Guid id) => new BookEditDto
        {
            Id = id,
            Title = "Editable Book",
            Description = "Desc",
            Genre = BookGenre.Fiction,
            IsRead = false,
            Rating = 5,
            DateAdded = DateTime.UtcNow,
            PublisherId = Guid.NewGuid(),
            AuthorIds = new List<Guid>()
        };

        private static BookDeleteDto MakeBookDeleteDto(Guid id) => new BookDeleteDto
        {
            Id = id,
            Title = "Deletable Book",
            CoverUrl = "http://example.com/cover.jpg",
            AddedByUserName = "alice"
        };

        // ──────────────────────────────────────────────────────────────────────
        // All (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task All_ReturnsViewWithMappedBookList()
        {
            SetAnonymousUser();
            var dtos = new List<BookAllDto> { MakeBookAllDto("Alpha"), MakeBookAllDto("Beta") };
            _serviceMock.Setup(s => s.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(It.IsAny<Guid?>(), null, null, null, 1, 5))
                .ReturnsAsync((dtos, 1));

            var result = await _sut.All();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = (view!.Model as IEnumerable<BooksAllViewModel>)!.ToList();
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task All_SetsViewDataCorrectly()
        {
            SetAnonymousUser();
            _serviceMock.Setup(s => s.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(It.IsAny<Guid?>(), "dune", "penguin", "fiction", 3, 5))
                .ReturnsAsync((new List<BookAllDto>(), 5));

            await _sut.All("dune", "penguin", "fiction", 3);

            Assert.That(_sut.ViewData["SearchQuery"], Is.EqualTo("dune"));
            Assert.That(_sut.ViewData["PublisherFilter"], Is.EqualTo("penguin"));
            Assert.That(_sut.ViewData["GenreFilter"], Is.EqualTo("fiction"));
            Assert.That(_sut.ViewData["CurrentPage"], Is.EqualTo(3));
            Assert.That(_sut.ViewData["TotalPages"], Is.EqualTo(5));
        }

        // ──────────────────────────────────────────────────────────────────────
        // MyBooks (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task MyBooks_EmptyUserId_ReturnsViewWithEmptyList()
        {
            SetAnonymousUser();

            var result = await _sut.MyBooks();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = (view!.Model as IEnumerable<BooksAllViewModel>)!.ToList();
            Assert.That(model, Is.Empty);
            Assert.That(_sut.ViewData["TotalPages"], Is.EqualTo(0));
        }

        [Test]
        public async Task MyBooks_ValidUserId_ReturnsViewWithUserBooks()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var dtos = new List<BookAllDto> { MakeBookAllDto("My Book") };
            _serviceMock.Setup(s => s.GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(userId, null, null, null, 1, 5))
                .ReturnsAsync((dtos, 1));

            var result = await _sut.MyBooks();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = (view!.Model as IEnumerable<BooksAllViewModel>)!.ToList();
            Assert.That(model.Count, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Details (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Details_BookNotFound_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookDtoDetailsByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((BookDetailsDto?)null);

            var result = await _sut.Details(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_ValidBook_ReturnsViewWithDetailsAndFlags()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            var dto = MakeBookDetailsDto(bookId);
            _serviceMock.Setup(s => s.GetBookDtoDetailsByIdAsync(bookId)).ReturnsAsync(dto);
            _serviceMock.Setup(s => s.IsBookDtoAddedByUserAsync(userId, bookId)).ReturnsAsync(true);
            _serviceMock.Setup(s => s.IsBookDtoAddedToUserCollectionAsync(userId, bookId)).ReturnsAsync(true);

            var result = await _sut.Details(bookId);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as BookDetailsViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(bookId));
            Assert.That(model.IsAddedByUser, Is.True);
            Assert.That(model.IsAddedToUserCollection, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Create (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Create_Get_ReturnsViewWithCreateModel()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookDtoCreateViewModelAsync())
                .ReturnsAsync(new BookCreateDto { DateAdded = DateTime.UtcNow });

            var result = await _sut.Create();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.InstanceOf<BookCreateViewModel>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Create (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = new BookCreateViewModel();
            _sut.ModelState.AddModelError("Title", "Required");

            var result = await _sut.Create(model);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Create_Post_EmptyUserId_ReturnsView()
        {
            SetAnonymousUser();
            var model = new BookCreateViewModel
            {
                Title = "Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid()
            };

            var result = await _sut.Create(model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Create_Post_Success_RedirectsToAll()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = new BookCreateViewModel
            {
                Title = "New Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), userId))
                .Returns(Task.CompletedTask);

            var result = await _sut.Create(model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
        }

        [Test]
        public async Task Create_Post_PublisherDoesntExistException_AddsModelError()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = new BookCreateViewModel
            {
                Title = "Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), userId))
                .ThrowsAsync(new PublisherDoesntExistException("No publisher."));

            var result = await _sut.Create(model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_sut.ModelState.ErrorCount, Is.GreaterThan(0));
        }

        [Test]
        public async Task Create_Post_AuthorDoesntExistException_AddsModelError()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = new BookCreateViewModel
            {
                Title = "Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), userId))
                .ThrowsAsync(new AuthorDoesntExistException("No author."));

            var result = await _sut.Create(model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_sut.ModelState.ErrorCount, Is.GreaterThan(0));
        }

        [Test]
        public async Task Create_Post_InvalidOperationException_AddsModelError()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var model = new BookCreateViewModel
            {
                Title = "Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.CreateDtoBookAsync(It.IsAny<BookCreateDto>(), userId))
                .ThrowsAsync(new InvalidOperationException("Save failed."));

            var result = await _sut.Create(model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_sut.ModelState.ErrorCount, Is.GreaterThan(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Favorites (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Favorites_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.Favorites();

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Account"));
        }

        [Test]
        public async Task Favorites_ValidUserId_ReturnsViewWithFavorites()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var favDtos = new List<BookFavoritesDto>
            {
                new BookFavoritesDto { Id = Guid.NewGuid(), Title = "Fav Book", CoverUrl = "" }
            };
            _serviceMock.Setup(s => s.GetFavoriteBooksDtoAsync(userId, null, 1, 5))
                .ReturnsAsync((favDtos, 1));

            var result = await _sut.Favorites();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = (view!.Model as IEnumerable<BookFavoritesViewModel>)!.ToList();
            Assert.That(model.Count, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Save (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Save_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.Save(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Save_ValidUserId_CallsServiceAndRedirects()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.SaveFevBookDtoAsync(bookId, userId)).Returns(Task.CompletedTask);

            var result = await _sut.Save(bookId);

            _serviceMock.Verify(s => s.SaveFevBookDtoAsync(bookId, userId), Times.Once);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Remove (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Remove_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.Remove(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Remove_ValidUserId_CallsServiceAndRedirectsToFavorites()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.RemoveFevBookDtoAsync(bookId, userId)).Returns(Task.CompletedTask);

            var result = await _sut.Remove(bookId);

            _serviceMock.Verify(s => s.RemoveFevBookDtoAsync(bookId, userId), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Favorites"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Get_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.Edit(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Edit_Get_BookNotFound_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookForEditDtoAsync(It.IsAny<Guid>(), userId))
                .ReturnsAsync((BookEditDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ValidBook_ReturnsViewWithModel()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookForEditDtoAsync(bookId, userId))
                .ReturnsAsync(MakeBookEditDto(bookId));

            var result = await _sut.Edit(bookId);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as BookEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(bookId));
        }

        [Test]
        public async Task Edit_Get_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookForEditDtoAsync(It.IsAny<Guid>(), userId))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Edit_Get_ArgumentException_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookForEditDtoAsync(It.IsAny<Guid>(), userId))
                .ThrowsAsync(new ArgumentException("Invalid"));

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            SetAnonymousUser();
            var bookId = Guid.NewGuid();
            var model = new BookEditViewModel { Id = Guid.NewGuid() }; // different from route id

            var result = await _sut.Edit(bookId, model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_EmptyRouteId_ReturnsBadRequest()
        {
            SetAnonymousUser();
            var model = new BookEditViewModel { Id = Guid.Empty };

            var result = await _sut.Edit(Guid.Empty, model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();
            var id = Guid.NewGuid();
            var model = new BookEditViewModel { Id = id };

            var result = await _sut.Edit(id, model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsView()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var id = Guid.NewGuid();
            var model = new BookEditViewModel { Id = id };
            _sut.ModelState.AddModelError("Title", "Required");

            var result = await _sut.Edit(id, model);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Edit_Post_NotEdited_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var id = Guid.NewGuid();
            var model = new BookEditViewModel
            {
                Id = id,
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.EditBookDtoAsync(It.IsAny<BookEditDto>(), userId))
                .ReturnsAsync(false);

            var result = await _sut.Edit(id, model);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_Success_RedirectsToDetails()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var id = Guid.NewGuid();
            var model = new BookEditViewModel
            {
                Id = id,
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.EditBookDtoAsync(It.IsAny<BookEditDto>(), userId))
                .ReturnsAsync(true);

            var result = await _sut.Edit(id, model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Details"));
        }

        [Test]
        public async Task Edit_Post_PublisherDoesntExistException_AddsModelError()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var id = Guid.NewGuid();
            var model = new BookEditViewModel
            {
                Id = id,
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.EditBookDtoAsync(It.IsAny<BookEditDto>(), userId))
                .ThrowsAsync(new PublisherDoesntExistException("No pub."));

            var result = await _sut.Edit(id, model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_sut.ModelState.ErrorCount, Is.GreaterThan(0));
        }

        [Test]
        public async Task Edit_Post_AuthorDoesntExistException_AddsModelError()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            var id = Guid.NewGuid();
            var model = new BookEditViewModel
            {
                Id = id,
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = new List<Guid>()
            };
            _serviceMock.Setup(s => s.EditBookDtoAsync(It.IsAny<BookEditDto>(), userId))
                .ThrowsAsync(new AuthorDoesntExistException("No author."));

            var result = await _sut.Edit(id, model);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_sut.ModelState.ErrorCount, Is.GreaterThan(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Delete (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Delete_Get_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.Delete(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Delete_Get_BookNotFound_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookDeleteDetailsDtoAsync(It.IsAny<Guid>(), userId))
                .ReturnsAsync((BookDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ValidBook_ReturnsViewWithDeleteModel()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookDeleteDetailsDtoAsync(bookId, userId))
                .ReturnsAsync(MakeBookDeleteDto(bookId));

            var result = await _sut.Delete(bookId);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as BookDeleteViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(bookId));
        }

        [Test]
        public async Task Delete_Get_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.GetBookDeleteDetailsDtoAsync(It.IsAny<Guid>(), userId))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteConfirmed (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteConfirmed_EmptyUserId_RedirectsToLogin()
        {
            SetAnonymousUser();

            var result = await _sut.DeleteConfirmed(Guid.NewGuid(), null);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task DeleteConfirmed_BookNotDeleted_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.DeleteBookDtoAsync(It.IsAny<Guid>(), userId))
                .ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid(), null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_Success_RedirectsToMyBooks()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.DeleteBookDtoAsync(bookId, userId)).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(bookId, null);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("MyBooks"));
        }

        [Test]
        public async Task DeleteConfirmed_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);
            _serviceMock.Setup(s => s.DeleteBookDtoAsync(It.IsAny<Guid>(), userId))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _sut.DeleteConfirmed(Guid.NewGuid(), null);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }
    }
}
