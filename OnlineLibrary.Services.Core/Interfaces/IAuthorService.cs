using OnlineLibrary.Services.Models.Author;
using System;
using System.Collections.Generic;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IAuthorService 
    {
        Task<IEnumerable<AuthorsAllDto>> GetAllAuthorsForViewModelAsync();
        Task<AuthorDetailsDto?> GetAuthorDetailsByIdAsync(Guid id);

        AuthorsAllDto GetEmptyAuthorViewModelAsync();

        Task AddNewAuthorAsync(AuthorsAllDto model);

        Task<AuthorsAllDto?> GetNewAuthorForEditByIdAsync(Guid id);

        Task<bool> UpdateNewAuthorAsync(Guid id, AuthorsAllDto model);

        Task<AuthorDeleteDto?> GetAuthorNewDeleteDetailsAsync(Guid id);
        Task<bool> DeleteAuthorByIdAsync(Guid id);
    }
}
