using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.ViewModels.Author;

namespace OnlineLibrary.Services.CustomMappers
{
    public static class AuthorMappers
    {
        public static AuthorDeleteViewModel MapAuthorDeleteDtoToAuthorDeleteViewModel(AuthorDeleteDto authorToDeleteDto)
        {
            return new AuthorDeleteViewModel
            {
                Id = authorToDeleteDto.Id,
                FullName = authorToDeleteDto.FullName,
                BooksAuthors = authorToDeleteDto.BooksAuthors
            };
        }
    }
}
