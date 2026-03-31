using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.Controllers;
using OnlineLibrary.Web.ViewModels.Publisher;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class PublisherControllerTests
    {
        private Mock<IPublisherService> _serviceMock;
        private Mock<ILogger<PublisherController>> _loggerMock;
        private PublisherController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IPublisherService>();
            _loggerMock = new Mock<ILogger<PublisherController>>();
            _sut = new PublisherController(_serviceMock.Object, _loggerMock.Object);
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
            var dtos = new List<PublisherAllDto>
            {
                new PublisherAllDto { Id = Guid.NewGuid(), Name = "Penguin Books" }
            };
            _serviceMock
                .Setup(s => s.GetAllPublishersAsync(null, 1, 20))
                .ReturnsAsync((dtos, 1));

            var result = await _sut.All();

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as List<PublisherAllViewModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Count, Is.EqualTo(1));
            Assert.That(model[0].Name, Is.EqualTo("Penguin Books"));
        }

        [Test]
        public async Task All_SetsViewDataCorrectly()
        {
            _serviceMock
                .Setup(s => s.GetAllPublishersAsync("test", 3, 20))
                .ReturnsAsync((new List<PublisherAllDto>(), 7));

            await _sut.All("test", 3);

            Assert.That(_sut.ViewData["SearchQuery"], Is.EqualTo("test"));
            Assert.That(_sut.ViewData["CurrentPage"], Is.EqualTo(3));
            Assert.That(_sut.ViewData["TotalPages"], Is.EqualTo(7));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Details (GET)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPublisherDetailsByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((PublisherDetailsDto?)null);

            var result = await _sut.Details(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            var dto = new PublisherDetailsDto
            {
                Id = id,
                Name = "Penguin Books",
                BooksWithAuthorName = new List<PublisherBookDto>
                {
                    new PublisherBookDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test Book",
                        CoverUrl = string.Empty,
                        Rating = 4,
                        DateAdded = "01 Jan 2024",
                        GenreName = "Fiction",
                        AuthorsName = "Test Author",
                        Description = "Desc"
                    }
                }
            };
            _serviceMock.Setup(s => s.GetPublisherDetailsByIdAsync(id)).ReturnsAsync(dto);

            var result = await _sut.Details(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as PublisherDetailsViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Name, Is.EqualTo("Penguin Books"));
            Assert.That(model.BooksWithAuthorName.Count, Is.EqualTo(1));
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
            Assert.That(view!.Model, Is.InstanceOf<PublisherAddViewModel>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Add_Post_InvalidModelState_ReturnsViewWithModel()
        {
            _serviceMock.Setup(s => s.GetEmptyPublisherViewModel()).Returns(new PublisherAddDto());
            _sut.ModelState.AddModelError("Name", "Required");
            var inputModel = new PublisherAddViewModel { Name = "" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(inputModel));
        }

        [Test]
        public async Task Add_Post_ValidModel_RedirectsToAll()
        {
            _serviceMock.Setup(s => s.GetEmptyPublisherViewModel()).Returns(new PublisherAddDto());
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>())).Returns(Task.CompletedTask);
            var inputModel = new PublisherAddViewModel { Name = "New Publisher" };

            var result = await _sut.Add(inputModel);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Publisher"));
        }

        [Test]
        public async Task Add_Post_PublisherAlreadyExistsException_ReturnsViewWithModelError()
        {
            _serviceMock.Setup(s => s.GetEmptyPublisherViewModel()).Returns(new PublisherAddDto());
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
                        .ThrowsAsync(new PublisherAlreadyExistsException("Penguin Books"));
            var inputModel = new PublisherAddViewModel { Name = "Penguin Books" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
            Assert.That(_sut.ModelState.ContainsKey(nameof(PublisherAddViewModel.Name)), Is.True);
        }

        [Test]
        public async Task Add_Post_PublisherCreateException_ReturnsViewWithModelError()
        {
            _serviceMock.Setup(s => s.GetEmptyPublisherViewModel()).Returns(new PublisherAddDto());
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
                        .ThrowsAsync(new PublisherCreateException("DB error"));
            var inputModel = new PublisherAddViewModel { Name = "New Publisher" };

            var result = await _sut.Add(inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_UnexpectedException_ReturnsViewWithModelError()
        {
            _serviceMock.Setup(s => s.GetEmptyPublisherViewModel()).Returns(new PublisherAddDto());
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
                        .ThrowsAsync(new Exception("unexpected"));
            var inputModel = new PublisherAddViewModel { Name = "New Publisher" };

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
            _serviceMock.Setup(s => s.GetNewPublisherForEditByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((PublisherAllDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetNewPublisherForEditByIdAsync(id))
                        .ReturnsAsync(new PublisherAllDto { Id = id, Name = "Edit Me" });

            var result = await _sut.Edit(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as PublisherEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(id));
            Assert.That(model.Name, Is.EqualTo("Edit Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit (POST)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_EmptyId_ReturnsBadRequest()
        {
            var result = await _sut.Edit(Guid.Empty, new PublisherEditViewModel { Id = Guid.Empty, Name = "X" });

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            _sut.ModelState.AddModelError("Name", "Required");
            var inputModel = new PublisherEditViewModel { Id = id, Name = "" };

            var result = await _sut.Edit(id, inputModel);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.Model, Is.SameAs(inputModel));
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var id = Guid.NewGuid();
            var inputModel = new PublisherEditViewModel { Id = Guid.NewGuid(), Name = "X" };

            var result = await _sut.Edit(id, inputModel);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_NonExistingId_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>())).ReturnsAsync(false);
            var inputModel = new PublisherEditViewModel { Id = id, Name = "X" };

            var result = await _sut.Edit(id, inputModel);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_ValidModel_RedirectsToAll()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>())).ReturnsAsync(true);
            var inputModel = new PublisherEditViewModel { Id = id, Name = "Updated Name" };

            var result = await _sut.Edit(id, inputModel);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Publisher"));
        }

        [Test]
        public async Task Edit_Post_PublisherUpdateException_ReturnsViewWithModelError()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>()))
                        .ThrowsAsync(new PublisherUpdateExeption("DB error"));
            var inputModel = new PublisherEditViewModel { Id = id, Name = "X" };

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
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((PublisherDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ExistingId_ReturnsViewWithMappedModel()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(id))
                        .ReturnsAsync(new PublisherDeleteDto { Id = id, Name = "Delete Me" });

            var result = await _sut.Delete(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            var model = view!.Model as PublisherDeleteViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(id));
            Assert.That(model.Name, Is.EqualTo("Delete Me"));
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
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_ExistingId_RedirectsToAll()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(id)).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(id);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("All"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Publisher"));
        }

        [Test]
        public async Task DeleteConfirmed_PublisherDeleteException_ReturnsDeleteViewWithModelError()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(id))
                        .ThrowsAsync(new PublisherDeleteException("Has books"));
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(id))
                        .ReturnsAsync(new PublisherDeleteDto { Id = id, Name = "Has Books" });

            var result = await _sut.DeleteConfirmed(id);

            var view = result as ViewResult;
            Assert.That(view, Is.Not.Null);
            Assert.That(view!.ViewName, Is.EqualTo("Delete"));
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }
    }
}
