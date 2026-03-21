using OnlineLibrary.Services.Models.Author;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IAuthorService 
    {
        Task<IEnumerable<AuthorsAllDto>> GetAllAuthorsForViewModelAsync();
        Task<AuthorDetailsDto?> GetAuthorDetailsByIdAsync(Guid id);

        //AuthorAddViewModel GetEmtyAuthorFormModelAsync();


        //Task AddAuthorAsync(AuthorAddViewModel model);

        //Task<AuthorEditViewModel> GetAuthorForEditByIdAsync(Guid id);

        //Task UpdateAuthorAsync(Guid id, AuthorEditViewModel model);
        //Task<bool> ExistsAsync(Guid id);

        //Task DeleteAuthorAsync(Guid id);

        //Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(Guid id);
    }
}
