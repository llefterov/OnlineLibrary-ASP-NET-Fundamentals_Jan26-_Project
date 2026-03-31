using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.Controllers;
using OnlineLibrary.Web.ViewModels.Author;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AuthorControllerTests
    {
        private Mock<IAuthorService> _serviceMock;
        private Mock<ILogger<AuthorController>> _loggerMock;
        private AuthorController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IAuthorService>();
            _loggerMock = new Mock<ILogger<AuthorController>>();
            _sut = new AuthorController(_serviceMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        // ──────────────────────────────────────────────────────────────────────
        // All (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task All_ReturnsViewWithMappedList()
        {
            var dtos = new List<AuthorsAllDto>
            {
                new AuthorsAllDto { Id = Guid.NewGuid(), FullName = "Alan Poe" }
            };
            _serviceMock
                .Setup(s => s.GetAllAuthorsForViewModelAsync(null, 1, 20))
                .ReturnsAsync((dtos, 1));

            var result = await _sut.All();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as List<AuthorAllViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(1));
            Assert.That(model[0].FullName, Is.EqualTo("Alan Poe"));
        }

        [Test]
        public async Task All_SetsViewDataCorrectly()
        {
            _serviceMock
                .Setup(s => s.GetAllAuthorsForViewModelAsync("test", 2, 20))
                .ReturnsAsync((new List<AuthorsAllDto>(), 5));

            await _sut.All("test", 2);

            Assert.That(_sut.ViewData["SearchQuery"], Is.EqualTo("test"));
            Assert.That(_sut.ViewData["CurrentPage"], Is.EqualTo(2));
            Assert.That(_sut.ViewData["TotalPages"], Is.EqualTo(5));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Details (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetAuthorDetailsByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AuthorDetailsDto?)null);

            var result = await _sut.Details(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            var dto = new AuthorDetailsDto
            {
                Id = id,
                FullName = "Alan Poe",
                BooksWithPublisherName = new List<AuthorBookDto>
                {
                    new AuthorBookDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test Book",
                        CoverUrl = string.Empty,
                        Rating = 4,
                        DateAdded = "01 Jan 2024",
                        GenreName = "Fiction",
                        PublisherName = "Publisher",
                        Description = "Desc"
                    }
                }
            };
            _serviceMock.Setup(s => s.GetAuthorDetailsByIdAsync(id)).ReturnsAsync(dto);

            var result = await _sut.Details(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as AuthorDetailsViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.FullName, Is.EqualTo("Alan Poe"));
            Assert.That(model.BooksWithPublisherName.Count, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Add_Get_ReturnsViewWithEmptyModel()
        {
            var result = _sut.Add();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.InstanceOf<AuthorAddViewModel>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Add_Post_InvalidModelState_ReturnsViewWithModel()
        {
            _sut.ModelState.AddModelError("FullName", "Required");
            var inputModel = new AuthorAddViewModel { FullName = "" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(inputModel));
        }

        [Test]
        public async Task Add_Post_ValidModel_RedirectsToAll()
        {
            _serviceMock.Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>())).Returns(Task.CompletedTask);
            var inputModel = new AuthorAddViewModel { FullName = "New Author" };

            var result = await _sut.Add(inputModel);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Author"));
        }

        [Test]
        public async Task Add_Post_AuthorAlreadyExistsException_ReturnsViewWithModelError()
        {
            _serviceMock
                .Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorAlreadyExistsException("Alan Poe"));
            var inputModel = new AuthorAddViewModel { FullName = "Alan Poe" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
            Assert.That(_sut.ModelState.ContainsKey(nameof(AuthorAddViewModel.FullName)), Is.True);
        }

        [Test]
        public async Task Add_Post_AuthorCreateException_ReturnsViewWithModelError()
        {
            _serviceMock
                .Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorCreateException("DB error"));
            var inputModel = new AuthorAddViewModel { FullName = "New Author" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_UnexpectedException_ReturnsViewWithModelError()
        {
            _serviceMock
                .Setup(s => s.AddNewAuthorAsync(It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new Exception("unexpected"));
            var inputModel = new AuthorAddViewModel { FullName = "New Author" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Get_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Edit(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetNewAuthorForEditByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AuthorsAllDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.GetNewAuthorForEditByIdAsync(id))
                .ReturnsAsync(new AuthorsAllDto { Id = id, FullName = "Edit Me" });

            var result = await _sut.Edit(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as AuthorEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(id));
            Assert.That(model.FullName, Is.EqualTo("Edit Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Edit(Guid.Empty, new AuthorEditViewModel { Id = Guid.Empty, FullName = "X" });

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            _sut.ModelState.AddModelError("FullName", "Required");
            var inputModel = new AuthorEditViewModel { Id = id, FullName = "" };

            var result = await _sut.Edit(id, inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(inputModel));
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var id = Guid.NewGuid();
            var inputModel = new AuthorEditViewModel { Id = Guid.NewGuid(), FullName = "X" };

            var result = await _sut.Edit(id, inputModel);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_NonExistingId_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>())).ReturnsAsync(false);
            var inputModel = new AuthorEditViewModel { Id = id, FullName = "X" };

            var result = await _sut.Edit(id, inputModel);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_ValidModel_RedirectsToAll()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>())).ReturnsAsync(true);
            var inputModel = new AuthorEditViewModel { Id = id, FullName = "Updated Name" };

            var result = await _sut.Edit(id, inputModel);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Author"));
        }

        [Test]
        public async Task Edit_Post_AuthorUpdateException_ReturnsViewWithModelError()
        {
            var id = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.UpdateNewAuthorAsync(id, It.IsAny<AuthorsAllDto>()))
                .ThrowsAsync(new AuthorUpdateExeption("DB error"));
            var inputModel = new AuthorEditViewModel { Id = id, FullName = "X" };

            var result = await _sut.Edit(id, inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Delete (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Delete_Get_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Delete(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Delete_Get_NonExistingId_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetAuthorNewDeleteDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((AuthorDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.GetAuthorNewDeleteDetailsAsync(id))
                .ReturnsAsync(new AuthorDeleteDto { Id = id, FullName = "Delete Me" });

            var result = await _sut.Delete(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as AuthorDeleteViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(id));
            Assert.That(model.FullName, Is.EqualTo("Delete Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteConfirmed (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteConfirmed_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.DeleteConfirmed(Guid.Empty);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task DeleteConfirmed_NonExistingId_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_ExistingId_RedirectsToAll()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAuthorByIdAsync(id)).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(id);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Author"));
        }

        [Test]
        public async Task DeleteConfirmed_AuthorDeleteException_ReturnsDeleteViewWithModelError()
        {
            var id = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.DeleteAuthorByIdAsync(id))
                .ThrowsAsync(new AuthorDeleteException("Has books"));
            _serviceMock
                .Setup(s => s.GetAuthorNewDeleteDetailsAsync(id))
                .ReturnsAsync(new AuthorDeleteDto { Id = id, FullName = "Has Books" });

            var result = await _sut.DeleteConfirmed(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.ViewName, Is.EqualTo("Delete"));
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }
    }
}
