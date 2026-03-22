using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IAuthorService 
    {
        Task<IEnumerable<AuthorsAllDto>> GetAllAuthorsForViewModelAsync();
        Task<AuthorDetailsDto?> GetAuthorDetailsByIdAsync(Guid id);

        AuthorsAllDto GetEmptyAuthorViewModelAsync();

        Task AddNewAuthorAsync(AuthorsAllDto model);

        Task<AuthorsAllDto> GetNewAuthorForEditByIdAsync(Guid id);

        Task UpdateNewAuthorAsync(Guid id, AuthorsAllDto model);

        Task DeleteAuthorByIdAsync(Guid id);

        Task<AuthorDeleteDto> GetAuthorNewDeleteDetailsAsync(Guid id);
    }
}
