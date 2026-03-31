using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Models.Book;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BooksServiceTests
    {
        private Mock<IBookRepository> _repoMock;
        private BooksService _sut;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IBookRepository>();
            _sut = new BooksService(_repoMock.Object);
        }

        private static Book MakeBook(Guid id, string title, Guid publisherId, string publisherName, Guid ownerId, string ownerName) =>
            new Book
            {
                Id = id,
                Title = title,
                Description = "A description",
                Genre = BookGenre.Fiction,
                IsRead = false,
                Rating = 5,
                CoverUrl = "http://example.com/cover.jpg",
                DateAdded = DateTime.UtcNow,
                PublisherId = publisherId,
                AddedByUserId = ownerId,
                IsDeleted = false,
                Publisher = new Publisher { Id = publisherId, Name = publisherName },
                AddedByUser = new ApplicationUser { Id = ownerId, UserName = ownerName },
                UsersBooks = new HashSet<UserBook>(),
                BooksAuthors = new HashSet<BookAuthor>()
            };

        // ──────────────────────────────────────────────────────────────────────
        // GetAllBooksDtoOrderedByTitleThenByGenreAscAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllBooksDtoOrderedByTitleThenByGenreAscAsync_EmptyList_ReturnsEmptyDtos()
        {
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(null))
                .ReturnsAsync(new List<Book>());

            var (dtos, totalPages) = await _sut.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(null);

            Assert.That(dtos, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllBooksDtoOrderedByTitleThenByGenreAscAsync_WithSearchQuery_FiltersResults()
        {
            var pubId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var books = new List<Book>
            {
                MakeBook(Guid.NewGuid(), "Dune", pubId, "Pub A", ownerId, "user1"),
                MakeBook(Guid.NewGuid(), "Foundation", pubId, "Pub A", ownerId, "user1")
            };
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(null))
                .ReturnsAsync(books);

            var (dtos, _) = await _sut.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(null, searchQuery: "dune");

            Assert.That(dtos.Count(), Is.EqualTo(1));
            Assert.That(dtos.First().Title, Is.EqualTo("Dune"));
        }

        [Test]
        public async Task GetAllBooksDtoOrderedByTitleThenByGenreAscAsync_WithPublisherFilter_FiltersResults()
        {
            var pubIdA = Guid.NewGuid();
            var pubIdB = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var books = new List<Book>
            {
                MakeBook(Guid.NewGuid(), "Book A", pubIdA, "Penguin", ownerId, "user1"),
                MakeBook(Guid.NewGuid(), "Book B", pubIdB, "Addison", ownerId, "user1")
            };
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(null))
                .ReturnsAsync(books);

            var (dtos, _) = await _sut.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(null, publisherFilter: "penguin");

            Assert.That(dtos.Count(), Is.EqualTo(1));
            Assert.That(dtos.First().PublisherName, Is.EqualTo("Penguin"));
        }

        [Test]
        public async Task GetAllBooksDtoOrderedByTitleThenByGenreAscAsync_Pagination_ReturnsCorrectPage()
        {
            var pubId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var books = Enumerable.Range(1, 10)
                .Select(i => MakeBook(Guid.NewGuid(), $"Book {i:D2}", pubId, "Publisher", ownerId, "user"))
                .ToList();
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(null))
                .ReturnsAsync(books);

            var (dtos, totalPages) = await _sut.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(null, pageNumber: 2, pageSize: 5);

            Assert.That(dtos.Count(), Is.EqualTo(5));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllBooksDtoOrderedByTitleThenByGenreAscAsync_CalculatesIsAddedByUserFlag()
        {
            var pubId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var book = MakeBook(Guid.NewGuid(), "My Book", pubId, "Publisher", userId, "owner");
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId))
                .ReturnsAsync(new List<Book> { book });

            var (dtos, _) = await _sut.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(userId);

            Assert.That(dtos.First().IsAddedByUser, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBooksDtoCreatedByUserAsync_FiltersToCurrentUserOnly()
        {
            var pubId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var books = new List<Book>
            {
                MakeBook(Guid.NewGuid(), "My Book", pubId, "Publisher", userId, "owner"),
                MakeBook(Guid.NewGuid(), "Other Book", pubId, "Publisher", otherId, "other")
            };
            _repoMock.Setup(r => r.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId))
                .ReturnsAsync(books);

            var (dtos, _) = await _sut.GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(userId);

            Assert.That(dtos.Count(), Is.EqualTo(1));
            Assert.That(dtos.First().Title, Is.EqualTo("My Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookDtoDetailsByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookDtoDetailsByIdAsync_NullFromRepository_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetBookDetailsByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Book?)null);

            var result = await _sut.GetBookDtoDetailsByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookDtoDetailsByIdAsync_ValidBook_ReturnsMappedDto()
        {
            var bookId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var book = MakeBook(bookId, "Test Book", pubId, "Great Publisher", ownerId, "alice");
            _repoMock.Setup(r => r.GetBookDetailsByIdAsync(bookId))
                .ReturnsAsync(book);

            var result = await _sut.GetBookDtoDetailsByIdAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Test Book"));
            Assert.That(result.PublisherName, Is.EqualTo("Great Publisher"));
            Assert.That(result.AddedByUserName, Is.EqualTo("alice"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllAuthorsAndPublishersAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllAuthorsAndPublishersAsync_DelegatesToRepository()
        {
            var publishers = new List<Publisher> { new Publisher { Id = Guid.NewGuid(), Name = "Pub" } };
            var authors = new List<Author> { new Author { Id = Guid.NewGuid(), FullName = "Author" } };
            _repoMock.Setup(r => r.GetAuthorsAndPublishersAsync())
                .ReturnsAsync((publishers, (IEnumerable<Author>)authors));

            var (pubs, auths) = await _sut.GetAllAuthorsAndPublishersAsync();

            Assert.That(pubs.Count(), Is.EqualTo(1));
            Assert.That(auths.Count(), Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────────────
        // IsBookDtoAddedByUserAsync / IsBookDtoAddedToUserCollectionAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task IsBookDtoAddedByUserAsync_DelegatesToRepository()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            _repoMock.Setup(r => r.IsBookAddedByUserAsync(userId, bookId)).ReturnsAsync(true);

            var result = await _sut.IsBookDtoAddedByUserAsync(userId, bookId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsBookDtoAddedToUserCollectionAsync_DelegatesToRepository()
        {
            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            _repoMock.Setup(r => r.IsBookAddedToUserCollectionAsync(userId, bookId)).ReturnsAsync(true);

            var result = await _sut.IsBookDtoAddedToUserCollectionAsync(userId, bookId);

            Assert.That(result, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // CreateDtoBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task CreateDtoBookAsync_MapsInputAndCallsRepository()
        {
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var inputDto = new BookCreateDto
            {
                Title = "New Book",
                Description = "A description",
                Genre = BookGenre.Mystery,
                IsRead = false,
                Rating = 4,
                DateAdded = DateTime.UtcNow,
                PublisherId = pubId,
                AuthorIds = new List<Guid>()
            };
            _repoMock.Setup(r => r.CreateBookAsync(It.IsAny<Book>(), userId))
                .Returns(Task.CompletedTask);

            await _sut.CreateDtoBookAsync(inputDto, userId);

            _repoMock.Verify(r => r.CreateBookAsync(
                It.Is<Book>(b => b.Title == "New Book" && b.AddedByUserId == userId),
                userId), Times.Once);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetFavoriteBooksDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetFavoriteBooksDtoAsync_NoFavorites_ReturnsEmptyWithZeroPages()
        {
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetFavoriteBooksAsync(userId))
                .ReturnsAsync(new List<Book>());

            var (dtos, totalPages) = await _sut.GetFavoriteBooksDtoAsync(userId);

            Assert.That(dtos, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        [Test]
        public async Task GetFavoriteBooksDtoAsync_WithSearchQuery_FiltersResults()
        {
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var favBooks = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Dune", CoverUrl = "http://a.com/img.jpg" },
                new Book { Id = Guid.NewGuid(), Title = "Foundation", CoverUrl = "http://a.com/img2.jpg" }
            };
            _repoMock.Setup(r => r.GetFavoriteBooksAsync(userId)).ReturnsAsync(favBooks);

            var (dtos, _) = await _sut.GetFavoriteBooksDtoAsync(userId, searchQuery: "dune");

            Assert.That(dtos.Count(), Is.EqualTo(1));
            Assert.That(dtos.First().Title, Is.EqualTo("Dune"));
        }

        [Test]
        public async Task GetFavoriteBooksDtoAsync_Pagination_ReturnsCorrectSlice()
        {
            var userId = Guid.NewGuid();
            var favBooks = Enumerable.Range(1, 10)
                .Select(i => new Book { Id = Guid.NewGuid(), Title = $"Book {i:D2}", CoverUrl = "" })
                .ToList();
            _repoMock.Setup(r => r.GetFavoriteBooksAsync(userId)).ReturnsAsync(favBooks);

            var (dtos, totalPages) = await _sut.GetFavoriteBooksDtoAsync(userId, pageNumber: 2, pageSize: 5);

            Assert.That(dtos.Count(), Is.EqualTo(5));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        // ──────────────────────────────────────────────────────────────────────
        // SaveFevBookDtoAsync / RemoveFevBookDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task SaveFevBookDtoAsync_DelegatesToRepository()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.SaveFevBookAsync(id, userId)).Returns(Task.CompletedTask);

            await _sut.SaveFevBookDtoAsync(id, userId);

            _repoMock.Verify(r => r.SaveFevBookAsync(id, userId), Times.Once);
        }

        [Test]
        public async Task RemoveFevBookDtoAsync_DelegatesToRepository()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.RemoveFevBookAsync(id, userId)).Returns(Task.CompletedTask);

            await _sut.RemoveFevBookDtoAsync(id, userId);

            _repoMock.Verify(r => r.RemoveFevBookAsync(id, userId), Times.Once);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookForEditDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookForEditDtoAsync_NullFromRepository_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetBookForEditAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Book?)null);

            var result = await _sut.GetBookForEditDtoAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookForEditDtoAsync_ValidBook_ReturnsMappedDto()
        {
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var book = new Book
            {
                Id = bookId,
                Title = "Editable Book",
                Description = "Desc",
                Genre = BookGenre.History,
                IsRead = true,
                Rating = 8,
                DateAdded = DateTime.UtcNow,
                PublisherId = pubId,
                AddedByUserId = userId,
                BooksAuthors = new HashSet<BookAuthor>()
            };
            _repoMock.Setup(r => r.GetBookForEditAsync(bookId, userId)).ReturnsAsync(book);

            var result = await _sut.GetBookForEditDtoAsync(bookId, userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Editable Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // EditBookDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task EditBookDtoAsync_MapsAndDelegatesToRepository()
        {
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var editDto = new BookEditDto
            {
                Id = bookId,
                Title = "Updated",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                IsRead = false,
                Rating = 5,
                DateAdded = DateTime.UtcNow,
                PublisherId = pubId,
                AuthorIds = new List<Guid>()
            };
            _repoMock.Setup(r => r.EditBookAsync(It.IsAny<Book>(), userId)).ReturnsAsync(true);

            var result = await _sut.EditBookDtoAsync(editDto, userId);

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.EditBookAsync(
                It.Is<Book>(b => b.Id == bookId && b.Title == "Updated"),
                userId), Times.Once);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookDeleteDetailsDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookDeleteDetailsDtoAsync_NullFromRepository_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetBookDeleteDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Book?)null);

            var result = await _sut.GetBookDeleteDetailsDtoAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookDeleteDetailsDtoAsync_ValidBook_ReturnsMappedDto()
        {
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var book = new Book { Id = bookId, Title = "Deletable Book", CoverUrl = "http://a.com/img.jpg" };
            _repoMock.Setup(r => r.GetBookDeleteDetailsAsync(bookId, userId)).ReturnsAsync(book);

            var result = await _sut.GetBookDeleteDetailsDtoAsync(bookId, userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Deletable Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteBookDtoAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteBookDtoAsync_DelegatesToRepository()
        {
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteBookAsync(bookId, userId)).ReturnsAsync(true);

            var result = await _sut.DeleteBookDtoAsync(bookId, userId);

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.DeleteBookAsync(bookId, userId), Times.Once);
        }
    }
}
