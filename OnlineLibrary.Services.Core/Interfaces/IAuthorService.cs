using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IAuthorService 
    {
        Task<IEnumerable<AuthorAllViewModel>> GetAllAuthorsAsync();
        Task<AuthorDetailsViewModel?> GetAuthorByIdAsync(Guid id);

        AuthorAddViewModel GetEmtyAuthorFormModelAsync();


        Task AddAuthorAsync(AuthorAddViewModel model);

        Task<AuthorEditViewModel> GetAuthorForEditByIdAsync(Guid id);

        Task UpdateAuthorAsync(Guid id, AuthorEditViewModel model);
        Task<bool> ExistsAsync(Guid id);

        Task DeleteAuthorAsync(Guid id);

        Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(Guid id);
    }
}
