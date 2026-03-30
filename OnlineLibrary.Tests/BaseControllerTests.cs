using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Web.Controllers;
using System.Security.Claims;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BaseControllerTests
    {
        private BaseController _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new BaseController();
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        private void SetUser(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetUserId
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GetUserId_WithValidNameIdentifierClaim_ReturnsParsedGuid()
        {
            var expectedId = Guid.NewGuid();
            SetUser(new[] { new Claim(ClaimTypes.NameIdentifier, expectedId.ToString()) });

            var result = _sut.GetUserId();

            Assert.That(result, Is.EqualTo(expectedId));
        }

        [Test]
        public void GetUserId_WithNoNameIdentifierClaim_ReturnsGuidEmpty()
        {
            SetUser(new[] { new Claim(ClaimTypes.Name, "testuser") });

            var result = _sut.GetUserId();

            Assert.That(result, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void GetUserId_WithNoClaims_ReturnsGuidEmpty()
        {
            SetUser(Array.Empty<Claim>());

            var result = _sut.GetUserId();

            Assert.That(result, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void GetUserId_WithEmptyNameIdentifierClaim_ReturnsGuidEmpty()
        {
            SetUser(new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) });

            var result = _sut.GetUserId();

            Assert.That(result, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void GetUserId_CalledTwiceWithSameClaim_ReturnsSameGuid()
        {
            var expectedId = Guid.NewGuid();
            SetUser(new[] { new Claim(ClaimTypes.NameIdentifier, expectedId.ToString()) });

            var first = _sut.GetUserId();
            var second = _sut.GetUserId();

            Assert.That(first, Is.EqualTo(second));
        }
    }
}
