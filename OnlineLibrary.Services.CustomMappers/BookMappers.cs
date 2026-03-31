using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.ViewModels.Books;

namespace OnlineLibrary.Services.CustomMappers
{
    public static class BookMappers
    {
        public static Book MapBookEditDtoToBook(BookEditDto bookEditDto)
        {
            return new Book
            {
                Id = bookEditDto.Id,
                Title = bookEditDto.Title,
                Description = bookEditDto.Description,
                Genre = bookEditDto.Genre,
                IsRead = bookEditDto.IsRead,
                DateRead = bookEditDto.DateRead,
                Rating = bookEditDto.Rating,
                CoverUrl = bookEditDto.CoverUrl,
                DateAdded = bookEditDto.DateAdded,
                PublisherId = bookEditDto.PublisherId,
                AuthorIds = bookEditDto.AuthorIds
            };
        }

        public static BookEditDto MapBookToBookEditDto(Book book)
        {
            return new BookEditDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Genre = book.Genre,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rating = book.Rating,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherId = book.PublisherId,
                AuthorIds = book.BooksAuthors
                    .Where(ba => !ba.IsDeleted)
                    .Select(ba => ba.AuthorId)
                    .ToList()
            };
        }

        public static BookDeleteDto MapBookToBookDeleteDto(Book book)
        {
            return new BookDeleteDto
            {
                Id = book.Id,
                Title = book.Title,
                AddedByUserName = book.AddedByUser?.UserName ?? string.Empty, // safe access
                CoverUrl = book.CoverUrl
            };

        }

        public static BooksAllViewModel MapBookAllDtoToBooksAllViewModel(BookAllDto booksAllDto)
        {
            return new BooksAllViewModel
            {
                Id = booksAllDto.Id,
                Title = booksAllDto.Title,
                Genre = booksAllDto.Genre,
                GenreName = booksAllDto.Genre.ToString(),
                Rating = booksAllDto.Rating,
                CoverUrl = booksAllDto.CoverUrl ?? string.Empty,
                AddedByUserName = booksAllDto.AddedByUserName, // null-safe
                PublisherId = booksAllDto.PublisherId,
                PublisherName = booksAllDto.PublisherName,
                IsAddedByUser = booksAllDto.IsAddedByUser, // Assuming this is already set correctly in the DTO
                IsAddedToUserCollection = booksAllDto.IsAddedToUserCollection, // Assuming this is already set correctly in the DTO
                IsDeleted = booksAllDto.IsDeleted
            };
        }

        public static BookDetailsViewModel MapBookDetailsDtoToBookDetailsViewModel(BookDetailsDto bookDetailsDto)
        {
            var booksDetailsViewModel = new BookDetailsViewModel
            {
                Id = bookDetailsDto.Id,
                Title = bookDetailsDto.Title,
                Description = bookDetailsDto.Description,
                Genre = bookDetailsDto.Genre,
                GenreName = bookDetailsDto.GenreName,
                IsRead = bookDetailsDto.IsRead,
                DateRead = bookDetailsDto.DateRead,
                Rating = bookDetailsDto.Rating,
                CoverUrl = bookDetailsDto.CoverUrl ?? string.Empty,
                DateAdded = bookDetailsDto.DateAdded,
                PublisherId = bookDetailsDto.PublisherId,
                PublisherName = bookDetailsDto.PublisherName,
                AuthorsName = bookDetailsDto.AuthorsName,
                AddedByUserName = bookDetailsDto.AddedByUserName, // safe access
                IsAddedByUser = bookDetailsDto.IsAddedByUser,
                IsAddedToUserCollection = bookDetailsDto.IsAddedToUserCollection
            };

            if (bookDetailsDto.IsRead == false)
            {
                booksDetailsViewModel.DateRead = null;
            }

            return booksDetailsViewModel;
        }

        public static BookCreateViewModel MapBookCreateDtoToBookCreateViewModel(BookCreateDto bookCreateDto)
        {
            return new BookCreateViewModel
            {
                Title = bookCreateDto.Title,
                Description = bookCreateDto.Description,
                Genre = bookCreateDto.Genre,
                IsRead = bookCreateDto.IsRead,
                DateRead = bookCreateDto.DateRead,
                Rating = bookCreateDto.Rating,
                CoverUrl = bookCreateDto.CoverUrl,
                DateAdded = bookCreateDto.DateAdded,
                PublisherId = bookCreateDto.PublisherId,
                AddedByUserId = bookCreateDto.AddedByUserId,
                AuthorIds = bookCreateDto.AuthorIds
            };
        }

        public static BookCreateDto MapBookCreateViewModelToBookCreateDto(BookCreateViewModel bookCreateViewModel)
        {
            return new BookCreateDto
            {
                Title = bookCreateViewModel.Title,
                Description = bookCreateViewModel.Description,
                Genre = bookCreateViewModel.Genre,
                IsRead = bookCreateViewModel.IsRead,
                DateRead = bookCreateViewModel.DateRead,
                Rating = bookCreateViewModel.Rating,
                CoverUrl = bookCreateViewModel.CoverUrl,
                DateAdded = bookCreateViewModel.DateAdded,
                PublisherId = bookCreateViewModel.PublisherId,
                AddedByUserId = bookCreateViewModel.AddedByUserId,
                AuthorIds = bookCreateViewModel.AuthorIds
            };
        }

        public static BookFavoritesDto MapBookToBookFavoritesDto(Book book)
        {
            return new BookFavoritesDto
            {
                Id = book.Id,
                Title = book.Title,
                CoverUrl = book.CoverUrl ?? string.Empty
            };
        }


        public static BookFavoritesViewModel MapBookFavoritesDtoToBookFavoritesViewModel(BookFavoritesDto favBookDto)
        {
            return new BookFavoritesViewModel
            {
                Id = favBookDto.Id,
                Title = favBookDto.Title,
                CoverUrl = favBookDto.CoverUrl ?? string.Empty
            };
        }

        public static BookEditViewModel MapBookEditDtoToBookEditViewModel(BookEditDto bookEditDto)
        {
            return new BookEditViewModel
            {
                Id = bookEditDto.Id,
                Title = bookEditDto.Title,
                Description = bookEditDto.Description,
                Genre = bookEditDto.Genre,
                IsRead = bookEditDto.IsRead,
                DateRead = bookEditDto.DateRead,
                Rating = bookEditDto.Rating,
                CoverUrl = bookEditDto.CoverUrl,
                DateAdded = bookEditDto.DateAdded,
                PublisherId = bookEditDto.PublisherId,
                AuthorIds = bookEditDto.AuthorIds
            };
        }

        public static BookEditDto MapBookEditViewModelToBookEditDto(BookEditViewModel bookEditViewModel)
        {
            return new BookEditDto
            {
                Id = bookEditViewModel.Id,
                Title = bookEditViewModel.Title,
                Description = bookEditViewModel.Description,
                Genre = bookEditViewModel.Genre,
                IsRead = bookEditViewModel.IsRead,
                DateRead = bookEditViewModel.DateRead,
                Rating = bookEditViewModel.Rating,
                CoverUrl = bookEditViewModel.CoverUrl,
                DateAdded = bookEditViewModel.DateAdded,
                PublisherId = bookEditViewModel.PublisherId,
                AuthorIds = bookEditViewModel.AuthorIds
            };
        }

        public static BookDeleteViewModel MapBookDeleteDtoToBookDeleteViewModel(BookDeleteDto bookDeleteDto)
        {
            return new BookDeleteViewModel
            {
                Id = bookDeleteDto.Id,
                Title = bookDeleteDto.Title,
                CoverUrl = bookDeleteDto.CoverUrl ?? string.Empty,
                AddedByUserName = bookDeleteDto.AddedByUserName
            };
        }
    }
}
