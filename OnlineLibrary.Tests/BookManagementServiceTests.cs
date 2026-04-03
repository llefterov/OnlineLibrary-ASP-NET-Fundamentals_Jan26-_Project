using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.Services.Core.Admin;
using OnlineLibrary.Services.Models.Book;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BookManagementServiceTests
    {
        private Mock<IBookRepository> _repoMock;
        private BookManagementService _sut;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IBookRepository>();
            _sut = new BookManagementService(_repoMock.Object);
        }

        private static Book MakeBook(Guid id, string title, Guid publisherId, string publisherName, bool isDeleted = false) =>
            new Book
            {
                Id = id,
                Title = title,
                Description = "Desc",
                Genre = BookGenre.Fiction,
                Rating = 5,
                CoverUrl = "http://example.com/cover.jpg",
                DateAdded = DateTime.UtcNow,
                PublisherId = publisherId,
                AddedByUserId = Guid.NewGuid(),
                IsDeleted = isDeleted,
                Publisher = new Publisher { Id = publisherId, Name = publisherName },
                AddedByUser = new ApplicationUser { UserName = "owner" },
                BooksAuthors = new HashSet<BookAuthor>(),
                UsersBooks = new HashSet<UserBook>()
            };

        // ──────────────────────────────────────────────────────────────────────
        // GetBookForAdminEditDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookForAdminEditDtoAsync_BookNotFound_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetBookForAdminEditAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Book?)null);

            var result = await _sut.GetBookForAdminEditDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookForAdminEditDtoAsync_BookFound_ReturnsMappedDto()
        {
            var bookId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var book = MakeBook(bookId, "Test Book", pubId, "Pub");
            _repoMock.Setup(r => r.GetBookForAdminEditAsync(bookId)).ReturnsAsync(book);

            var result = await _sut.GetBookForAdminEditDtoAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Test Book"));
            Assert.That(result.PublisherId, Is.EqualTo(pubId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // EditBookForAdminDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task EditBookForAdminDtoAsync_BookNotFound_ReturnsFalse()
        {
            _repoMock.Setup(r => r.EditBookForAdminAsync(It.IsAny<Book>())).ReturnsAsync(false);

            var dto = new BookEditDto
            {
                Id = Guid.NewGuid(),
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid()
            };

            var result = await _sut.EditBookForAdminDtoAsync(dto);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EditBookForAdminDtoAsync_ValidBook_ReturnsTrue()
        {
            _repoMock.Setup(r => r.EditBookForAdminAsync(It.IsAny<Book>())).ReturnsAsync(true);

            var dto = new BookEditDto
            {
                Id = Guid.NewGuid(),
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid()
            };

            var result = await _sut.EditBookForAdminDtoAsync(dto);

            Assert.That(result, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookAdminDeleteDetailsDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookAdminDeleteDetailsDtoAsync_BookNotFound_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetBookAdminDeleteDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Book?)null);

            var result = await _sut.GetBookAdminDeleteDetailsDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookAdminDeleteDetailsDtoAsync_BookFound_ReturnsMappedDto()
        {
            var bookId = Guid.NewGuid();
            var book = new Book
            {
                Id = bookId,
                Title = "Deletable Book",
                AddedByUserName = "owner",
                CoverUrl = "http://example.com/cover.jpg"
            };
            _repoMock.Setup(r => r.GetBookAdminDeleteDetailsAsync(bookId)).ReturnsAsync(book);

            var result = await _sut.GetBookAdminDeleteDetailsDtoAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Deletable Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteBookForAdminDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteBookForAdminDtoAsync_BookNotFound_ReturnsFalse()
        {
            _repoMock.Setup(r => r.DeleteBookForAdminAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteBookForAdminDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteBookForAdminDtoAsync_ValidBook_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteBookForAdminAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.DeleteBookForAdminDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllBooksForAdminDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllBooksForAdminDtoAsync_EmptyRepo_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetAllBooksForAdminAsync()).ReturnsAsync(new List<Book>());

            var result = await _sut.GetAllBooksForAdminDtoAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllBooksForAdminDtoAsync_BothActiveAndDeletedBooks_ReturnsMappedDtos()
        {
            var pubId = Guid.NewGuid();
            var books = new List<Book>
            {
                MakeBook(Guid.NewGuid(), "Active Book", pubId, "Pub A", isDeleted: false),
                MakeBook(Guid.NewGuid(), "Deleted Book", pubId, "Pub A", isDeleted: true)
            };
            _repoMock.Setup(r => r.GetAllBooksForAdminAsync()).ReturnsAsync(books);

            var result = (await _sut.GetAllBooksForAdminDtoAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(d => d.Title == "Active Book"), Is.True);
            Assert.That(result.Any(d => d.Title == "Deleted Book"), Is.True);
        }

        [Test]
        public async Task GetAllBooksForAdminDtoAsync_MapsIsDeletedFlag()
        {
            var pubId = Guid.NewGuid();
            var deletedBook = MakeBook(Guid.NewGuid(), "Deleted", pubId, "Pub", isDeleted: true);
            _repoMock.Setup(r => r.GetAllBooksForAdminAsync()).ReturnsAsync(new List<Book> { deletedBook });

            var result = (await _sut.GetAllBooksForAdminDtoAsync()).ToList();

            Assert.That(result[0].IsDeleted, Is.True);
        }

        [Test]
        public async Task GetAllBooksForAdminDtoAsync_MapsPublisherName()
        {
            var pubId = Guid.NewGuid();
            var book = MakeBook(Guid.NewGuid(), "Book", pubId, "Acme Press");
            _repoMock.Setup(r => r.GetAllBooksForAdminAsync()).ReturnsAsync(new List<Book> { book });

            var result = (await _sut.GetAllBooksForAdminDtoAsync()).ToList();

            Assert.That(result[0].PublisherName, Is.EqualTo("Acme Press"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // RestoreBookForAdminDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task RestoreBookForAdminDtoAsync_BookNotFound_ReturnsFalse()
        {
            _repoMock.Setup(r => r.RestoreBookForAdminAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.RestoreBookForAdminDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task RestoreBookForAdminDtoAsync_ValidBook_ReturnsTrue()
        {
            _repoMock.Setup(r => r.RestoreBookForAdminAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _sut.RestoreBookForAdminDtoAsync(Guid.NewGuid());

            Assert.That(result, Is.True);
        }
    }
}
