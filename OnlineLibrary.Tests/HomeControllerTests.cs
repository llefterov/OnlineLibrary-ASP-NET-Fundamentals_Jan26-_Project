using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Controllers;
using OnlineLibrary.Models;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new HomeController();
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Index
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Index_ReturnsView()
        {
            var result = _sut.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Privacy
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Privacy_ReturnsView()
        {
            var result = _sut.Privacy();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Error
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Error_StatusCode400_ReturnsBadRequestView()
        {
            var result = _sut.Error(StatusCodes.Status400BadRequest) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("BadRequest"));
        }

        [Test]
        public void Error_StatusCode404_ReturnsNotFoundView()
        {
            var result = _sut.Error(StatusCodes.Status404NotFound) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("NotFound"));
        }

        [Test]
        public void Error_StatusCode500_ReturnsServerErrorView()
        {
            var result = _sut.Error(StatusCodes.Status500InternalServerError) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.EqualTo("ServerError"));
        }

        [Test]
        public void Error_UnhandledStatusCode_ReturnsViewWithErrorViewModel()
        {
            _sut.HttpContext.TraceIdentifier = "test-trace-id";

            var result = _sut.Error(StatusCodes.Status503ServiceUnavailable) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewName, Is.Null);
            var model = result.Model as ErrorViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.RequestId, Is.EqualTo("test-trace-id"));
        }

        [Test]
        public void Error_UnhandledStatusCode_ErrorViewModelShowRequestId_IsTrue()
        {
            _sut.HttpContext.TraceIdentifier = "any-trace-id";

            var result = _sut.Error(0) as ViewResult;
            var model = result!.Model as ErrorViewModel;

            Assert.That(model!.ShowRequestId, Is.True);
        }
    }
}
