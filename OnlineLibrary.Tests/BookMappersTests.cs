using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.CustomMappers;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.ViewModels.Books;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BookMappersTests
    {
        [Test]
        public void MapBookEditDtoToBook_MapsAllProperties()
        {
            var id = Guid.NewGuid();
            var publisherId = Guid.NewGuid();
            var authorIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var dateAdded = DateTime.UtcNow.AddDays(-10);

            var dto = new BookEditDto
            {
                Id = id,
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Fantasy,
                Rating = 5,
                CoverUrl = "https://example.com/cover.jpg",
                DateAdded = dateAdded,
                PublisherId = publisherId,
                AuthorIds = authorIds
            };

            var result = BookMappers.MapBookEditDtoToBook(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Title, Is.EqualTo(dto.Title));
                Assert.That(result.Description, Is.EqualTo(dto.Description));
                Assert.That(result.Genre, Is.EqualTo(dto.Genre));
                Assert.That(result.Rating, Is.EqualTo(dto.Rating));
                Assert.That(result.CoverUrl, Is.EqualTo(dto.CoverUrl));
                Assert.That(result.DateAdded, Is.EqualTo(dto.DateAdded));
                Assert.That(result.PublisherId, Is.EqualTo(dto.PublisherId));
                Assert.That(result.AuthorIds, Is.EquivalentTo(authorIds));
            });
        }

        [Test]
        public void MapBookToBookEditDto_FiltersDeletedBookAuthors()
        {
            var keptAuthorId = Guid.NewGuid();
            var deletedAuthorId = Guid.NewGuid();

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Mystery,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                BooksAuthors = new List<BookAuthor>
                {
                    new BookAuthor { AuthorId = keptAuthorId, IsDeleted = false },
                    new BookAuthor { AuthorId = deletedAuthorId, IsDeleted = true }
                }
            };

            var result = BookMappers.MapBookToBookEditDto(book);

            Assert.That(result.AuthorIds, Is.EquivalentTo(new[] { keptAuthorId }));
        }

        [Test]
        public void MapBookToBookDeleteDto_WhenAddedByUserIsNull_ReturnsEmptyUserName()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Fiction,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AddedByUser = null!,
                CoverUrl = null
            };

            var result = BookMappers.MapBookToBookDeleteDto(book);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo(book.Title));
                Assert.That(result.AddedByUserName, Is.EqualTo(string.Empty));
                Assert.That(result.CoverUrl, Is.Null);
            });
        }

        [Test]
        public void MapBookToBookDeleteDto_WhenAddedByUserExists_ReturnsUserName()
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Fiction,
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AddedByUser = new ApplicationUser { UserName = "reader1" },
                CoverUrl = "https://example.com/cover.jpg"
            };

            var result = BookMappers.MapBookToBookDeleteDto(book);

            Assert.That(result.AddedByUserName, Is.EqualTo("reader1"));
        }

        [Test]
        public void MapBookAllDtoToBooksAllViewModel_NullCoverUrl_MapsToEmptyAndGenreNameFromEnum()
        {
            var dto = new BookAllDto
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Genre = BookGenre.ScienceFiction,
                Rating = 4,
                CoverUrl = null!,
                AddedByUserName = "reader",
                PublisherId = Guid.NewGuid(),
                PublisherName = "Pub",
                GenreName = "ignored",
                IsAddedByUser = true,
                IsAddedToUserCollection = false,
                IsDeleted = true
            };

            var result = BookMappers.MapBookAllDtoToBooksAllViewModel(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.CoverUrl, Is.EqualTo(string.Empty));
                Assert.That(result.GenreName, Is.EqualTo(BookGenre.ScienceFiction.ToString()));
                Assert.That(result.IsDeleted, Is.True);
                Assert.That(result.IsAddedByUser, Is.True);
            });
        }

        [Test]
        public void MapBookDetailsDtoToBookDetailsViewModel_MapsAllProperties()
        {
            var dto = new BookDetailsDto
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.History,
                GenreName = "History",
                Rating = 3,
                CoverUrl = "https://example.com/cover.jpg",
                DateAdded = "2026-01-01",
                PublisherId = Guid.NewGuid(),
                PublisherName = "Pub",
                AuthorsName = "Author",
                AddedByUserName = "reader",
                IsAddedByUser = true,
                IsAddedToUserCollection = false
            };

            var result = BookMappers.MapBookDetailsDtoToBookDetailsViewModel(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo(dto.Title));
                Assert.That(result.GenreName, Is.EqualTo(dto.GenreName));
                Assert.That(result.Rating, Is.EqualTo(dto.Rating));
                Assert.That(result.IsAddedByUser, Is.True);
                Assert.That(result.CoverUrl, Is.EqualTo(dto.CoverUrl));
            });
        }

        [Test]
        public void MapBookDetailsDtoToBookDetailsViewModel_NullCoverUrl_ReturnsEmptyString()
        {
            var dto = new BookDetailsDto
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.History,
                GenreName = "History",
                Rating = 3,
                CoverUrl = null!,
                DateAdded = "2026-01-01",
                PublisherId = Guid.NewGuid(),
                PublisherName = "Pub",
                AuthorsName = "Author",
                AddedByUserName = "reader",
                IsAddedByUser = true,
                IsAddedToUserCollection = false
            };

            var result = BookMappers.MapBookDetailsDtoToBookDetailsViewModel(dto);

            Assert.That(result.CoverUrl, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MapUserBookToBookFavoritesDto_NullCoverUrl_ReturnsEmptyString()
        {
            var userBook = new UserBook
            {
                UserId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                IsRead = false,
                Book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Title",
                    Description = "Description",
                    Genre = BookGenre.Romance,
                    DateAdded = DateTime.UtcNow,
                    PublisherId = Guid.NewGuid(),
                    CoverUrl = null
                }
            };

            var result = BookMappers.MapUserBookToBookFavoritesDto(userBook);

            Assert.That(result.CoverUrl, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MapUserBookToBookFavoritesDto_MapsIsReadAndDateRead()
        {
            var dateRead = new DateTime(2025, 6, 1);
            var userBook = new UserBook
            {
                UserId = Guid.NewGuid(),
                BookId = Guid.NewGuid(),
                IsRead = true,
                DateRead = dateRead,
                Book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Read Book",
                    Description = "Desc",
                    Genre = BookGenre.Fiction,
                    DateAdded = DateTime.UtcNow,
                    PublisherId = Guid.NewGuid(),
                    CoverUrl = "http://example.com/cover.jpg"
                }
            };

            var result = BookMappers.MapUserBookToBookFavoritesDto(userBook);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsRead, Is.True);
                Assert.That(result.DateRead, Is.EqualTo("2025-06-01"));
                Assert.That(result.Title, Is.EqualTo("Read Book"));
            });
        }

        [Test]
        public void MapBookDeleteDtoToBookDeleteViewModel_NullCoverUrl_ReturnsEmptyString()
        {
            var dto = new BookDeleteDto
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                AddedByUserName = "reader",
                CoverUrl = null
            };

            var result = BookMappers.MapBookDeleteDtoToBookDeleteViewModel(dto);

            Assert.That(result.CoverUrl, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MapBookCreateViewModelToBookCreateDto_MapsCoreProperties()
        {
            var authorIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var vm = new BookCreateViewModel
            {
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Thriller,
                Rating = 5,
                CoverUrl = "https://example.com/cover.jpg",
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AddedByUserId = Guid.NewGuid(),
                AuthorIds = authorIds
            };

            var result = BookMappers.MapBookCreateViewModelToBookCreateDto(vm);

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Is.EqualTo(vm.Title));
                Assert.That(result.Description, Is.EqualTo(vm.Description));
                Assert.That(result.AuthorIds, Is.EquivalentTo(authorIds));
            });
        }

        [Test]
        public void MapBookEditViewModelToBookEditDto_MapsCoreProperties()
        {
            var authorIds = new List<Guid> { Guid.NewGuid() };
            var vm = new BookEditViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Genre = BookGenre.Biography,
                Rating = 4,
                CoverUrl = "https://example.com/cover.jpg",
                DateAdded = DateTime.UtcNow,
                PublisherId = Guid.NewGuid(),
                AuthorIds = authorIds
            };

            var result = BookMappers.MapBookEditViewModelToBookEditDto(vm);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(vm.Id));
                Assert.That(result.Genre, Is.EqualTo(vm.Genre));
                Assert.That(result.AuthorIds, Is.EquivalentTo(authorIds));
            });
        }
    }
}
