using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.Areas.Admin.Controllers;
using OnlineLibrary.Web.ViewModels.Publisher;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminPublisherManagementControllerTests
    {
        private Mock<IPublisherManagementService> _serviceMock;
        private Mock<ILogger<PublisherManagementController>> _loggerMock;
        private PublisherManagementController _sut;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IPublisherManagementService>();
            _loggerMock = new Mock<ILogger<PublisherManagementController>>();
            _sut = new PublisherManagementController(_serviceMock.Object, _loggerMock.Object);
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
        public async Task Manage_ReturnsViewWithPublisherList()
        {
            var tuple = (new List<PublisherAllDto> { new PublisherAllDto { Name = "Acme Press" } }.AsEnumerable(), 1);
            _serviceMock.Setup(s => s.GetAllPublishersAsync(null, 1, 20)).ReturnsAsync(tuple);

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
            Assert.That(viewResult!.Model, Is.InstanceOf<PublisherAddViewModel>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Add POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Add_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var model = new PublisherAddViewModel { Name = "X" };
            _sut.ModelState.AddModelError("Name", "Too short");

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Add_Post_ValidModel_RedirectsToManage()
        {
            var model = new PublisherAddViewModel { Name = "Acme Press" };
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>())).Returns(Task.CompletedTask);

            var result = await _sut.Add(model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Add_Post_PublisherAlreadyExists_ReturnsViewWithModelError()
        {
            var model = new PublisherAddViewModel { Name = "Acme Press" };
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
                .ThrowsAsync(new PublisherAlreadyExistsException("Already exists"));

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_PublisherCreateException_ReturnsViewWithModelError()
        {
            var model = new PublisherAddViewModel { Name = "Acme Press" };
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
                .ThrowsAsync(new PublisherCreateException("Create failed"));

            var result = await _sut.Add(model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Add_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var model = new PublisherAddViewModel { Name = "Acme Press" };
            _serviceMock.Setup(s => s.AddNewPublisherAsync(It.IsAny<PublisherAddDto>()))
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
        public async Task Edit_Get_PublisherNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetNewPublisherForEditByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((PublisherAllDto?)null);

            var result = await _sut.Edit(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_ValidPublisher_ReturnsViewWithModel()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetNewPublisherForEditByIdAsync(publisherId))
                .ReturnsAsync(new PublisherAllDto { Id = publisherId, Name = "Acme Press" });

            var result = await _sut.Edit(publisherId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as PublisherEditViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Id, Is.EqualTo(publisherId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edit POST
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task Edit_Post_EmptyId_ReturnsBadRequest()
        {
            var model = new PublisherEditViewModel { Id = Guid.NewGuid(), Name = "Acme Press" };

            var result = await _sut.Edit(Guid.Empty, model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var model = new PublisherEditViewModel { Id = Guid.NewGuid(), Name = "Acme Press" };

            var result = await _sut.Edit(Guid.NewGuid(), model);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var id = Guid.NewGuid();
            var model = new PublisherEditViewModel { Id = id, Name = "X" };
            _sut.ModelState.AddModelError("Name", "Too short");

            var result = await _sut.Edit(id, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(model));
        }

        [Test]
        public async Task Edit_Post_ValidData_RedirectsToManage()
        {
            var id = Guid.NewGuid();
            var model = new PublisherEditViewModel { Id = id, Name = "Acme Press" };
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>())).ReturnsAsync(true);

            var result = await _sut.Edit(id, model);

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task Edit_Post_UpdateReturnsFalse_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var model = new PublisherEditViewModel { Id = id, Name = "Acme Press" };
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>())).ReturnsAsync(false);

            var result = await _sut.Edit(id, model);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_PublisherUpdateException_ReturnsViewWithModelError()
        {
            var id = Guid.NewGuid();
            var model = new PublisherEditViewModel { Id = id, Name = "Acme Press" };
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>()))
                .ThrowsAsync(new PublisherUpdateExeption("Update failed"));

            var result = await _sut.Edit(id, model);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Edit_Post_UnexpectedException_ReturnsViewWithGenericError()
        {
            var id = Guid.NewGuid();
            var model = new PublisherEditViewModel { Id = id, Name = "Acme Press" };
            _serviceMock.Setup(s => s.UpdateNewPublisherAsync(id, It.IsAny<PublisherAllDto>()))
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
        public async Task Delete_Get_PublisherNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((PublisherDeleteDto?)null);

            var result = await _sut.Delete(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_ValidPublisher_ReturnsViewWithModel()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(publisherId))
                .ReturnsAsync(new PublisherDeleteDto { Id = publisherId, Name = "Acme Press" });

            var result = await _sut.Delete(publisherId);

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
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_ValidId_RedirectsToManage()
        {
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("Manage"));
        }

        [Test]
        public async Task DeleteConfirmed_PublisherDeleteException_PublisherFoundAfter_ReturnsView()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(publisherId))
                .ThrowsAsync(new PublisherDeleteException("Has books"));
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(publisherId))
                .ReturnsAsync(new PublisherDeleteDto { Id = publisherId, Name = "Acme Press" });

            var result = await _sut.DeleteConfirmed(publisherId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_PublisherDeleteException_PublisherNotFoundAfter_ReturnsNotFound()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(publisherId))
                .ThrowsAsync(new PublisherDeleteException("Has books"));
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(publisherId))
                .ReturnsAsync((PublisherDeleteDto?)null);

            var result = await _sut.DeleteConfirmed(publisherId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_UnexpectedException_PublisherFoundAfter_ReturnsView()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(publisherId))
                .ThrowsAsync(new Exception("Unexpected"));
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(publisherId))
                .ReturnsAsync(new PublisherDeleteDto { Id = publisherId, Name = "Acme Press" });

            var result = await _sut.DeleteConfirmed(publisherId);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_sut.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteConfirmed_UnexpectedException_PublisherNotFoundAfter_ReturnsNotFound()
        {
            var publisherId = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletePublisherByIdAsync(publisherId))
                .ThrowsAsync(new Exception("Unexpected"));
            _serviceMock.Setup(s => s.GetPublisherNewDeleteDetailsAsync(publisherId))
                .ReturnsAsync((PublisherDeleteDto?)null);

            var result = await _sut.DeleteConfirmed(publisherId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
