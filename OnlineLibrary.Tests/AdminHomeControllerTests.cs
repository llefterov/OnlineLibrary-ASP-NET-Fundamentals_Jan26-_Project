using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Web.Areas.Admin.Controllers;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminHomeControllerTests
    {
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new HomeController();
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

        [Test]
        public void Index_AuthenticatedUser_ReturnsViewResult()
        {
            var result = _sut.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Index_AnonymousUser_ReturnsViewResult()
        {
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            var result = _sut.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }
}
