using NUnit.Framework;
using OnlineLibrary.Web.Infrastructure.Utilities;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class SlugGeneratorTests
    {
        private SlugGenerator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new SlugGenerator();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Null / empty / whitespace inputs  →  fallback "book"
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_NullInput_ReturnsFallback()
        {
            var result = _sut.GenerateSlug(null!);

            Assert.That(result, Is.EqualTo("book"));
        }

        [Test]
        public void GenerateSlug_EmptyString_ReturnsFallback()
        {
            var result = _sut.GenerateSlug(string.Empty);

            Assert.That(result, Is.EqualTo("book"));
        }

        [Test]
        public void GenerateSlug_WhitespaceOnly_ReturnsFallback()
        {
            var result = _sut.GenerateSlug("   ");

            Assert.That(result, Is.EqualTo("book"));
        }

        [Test]
        public void GenerateSlug_OnlySpecialChars_ReturnsFallback()
        {
            var result = _sut.GenerateSlug("!!!@@@###");

            Assert.That(result, Is.EqualTo("book"));
        }

        [Test]
        public void GenerateSlug_OnlyHyphens_ReturnsFallback()
        {
            var result = _sut.GenerateSlug("---");

            Assert.That(result, Is.EqualTo("book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Lowercase conversion
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_UppercaseInput_ReturnsLowercase()
        {
            var result = _sut.GenerateSlug("UPPERCASE");

            Assert.That(result, Is.EqualTo("uppercase"));
        }

        [Test]
        public void GenerateSlug_MixedCase_ReturnsLowercase()
        {
            var result = _sut.GenerateSlug("TheGreatGatsby");

            Assert.That(result, Is.EqualTo("thegreatgatsby"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Whitespace → hyphens
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_SingleSpaceBetweenWords_ReplacesWithHyphen()
        {
            var result = _sut.GenerateSlug("Hello World");

            Assert.That(result, Is.EqualTo("hello-world"));
        }

        [Test]
        public void GenerateSlug_MultipleSpacesBetweenWords_CollapsesToSingleHyphen()
        {
            var result = _sut.GenerateSlug("Hello   World");

            Assert.That(result, Is.EqualTo("hello-world"));
        }

        [Test]
        public void GenerateSlug_LeadingAndTrailingSpaces_TrimsHyphens()
        {
            var result = _sut.GenerateSlug("  spaces around  ");

            Assert.That(result, Is.EqualTo("spaces-around"));
        }

        [Test]
        public void GenerateSlug_TabAndNewlineWhitespace_ReplacesWithHyphen()
        {
            var result = _sut.GenerateSlug("hello\tworld\nnew");

            Assert.That(result, Is.EqualTo("hello-world-new"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Special character removal
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_SpecialCharsStripped_ReturnsCleanSlug()
        {
            var result = _sut.GenerateSlug("C# Programming");

            Assert.That(result, Is.EqualTo("c-programming"));
        }

        [Test]
        public void GenerateSlug_PunctuationRemoved_ReturnsSlugWithoutPunctuation()
        {
            var result = _sut.GenerateSlug("It's a great book!");

            Assert.That(result, Is.EqualTo("its-a-great-book"));
        }

        [Test]
        public void GenerateSlug_AccentedCharsRemoved_ReturnsAsciiOnly()
        {
            var result = _sut.GenerateSlug("Héllo Wörld");

            Assert.That(result, Is.EqualTo("hllo-wrld"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Hyphen collapsing / trimming
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_MultipleHyphensInInput_CollapsedToOne()
        {
            var result = _sut.GenerateSlug("title---multiple---hyphens");

            Assert.That(result, Is.EqualTo("title-multiple-hyphens"));
        }

        [Test]
        public void GenerateSlug_LeadingHyphen_Trimmed()
        {
            var result = _sut.GenerateSlug("-leading");

            Assert.That(result, Is.EqualTo("leading"));
        }

        [Test]
        public void GenerateSlug_TrailingHyphen_Trimmed()
        {
            var result = _sut.GenerateSlug("trailing-");

            Assert.That(result, Is.EqualTo("trailing"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Digits preserved
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_InputWithDigits_PreservesDigits()
        {
            var result = _sut.GenerateSlug("book123");

            Assert.That(result, Is.EqualTo("book123"));
        }

        [Test]
        public void GenerateSlug_TitleWithYear_PreservesYear()
        {
            var result = _sut.GenerateSlug("War and Peace 1869");

            Assert.That(result, Is.EqualTo("war-and-peace-1869"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Already-valid slug input
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GenerateSlug_AlreadyValidSlug_ReturnedUnchanged()
        {
            var result = _sut.GenerateSlug("the-great-gatsby");

            Assert.That(result, Is.EqualTo("the-great-gatsby"));
        }
    }
}
