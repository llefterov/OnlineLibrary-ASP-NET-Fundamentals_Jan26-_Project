using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IAuthorService 
    {
        Task<IEnumerable<AuthorAllViewModel>> GetAllAuthorsAsync();
        Task<AuthorDetailsViewModel?> GetAuthorByIdAsync(int id);

        AuthorAddViewModel GetEmtyAuthorFormModelAsync();


        Task AddAuthorAsync(AuthorAddViewModel model);

        Task<AuthorEditViewModel> GetAuthorForEditByIdAsync(int id);

        Task UpdateAuthorAsync(int id, AuthorEditViewModel model);
        Task<bool> ExistsAsync(int id);

        Task DeleteAuthorAsync(int id);

        Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(int id);




    }
}
