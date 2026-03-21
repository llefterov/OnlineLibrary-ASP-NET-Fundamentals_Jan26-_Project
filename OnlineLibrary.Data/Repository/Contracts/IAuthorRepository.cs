using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Repository.Contracts
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync();

        Task<Author?> GetAuthorByIdAsync(Guid id);

        Author GetEmptyAuthorFormModelAsync();

        Task AddAuthorAsync(Author model);

        //Task<AuthorEditViewModel> GetAuthorForEditByIdAsync(Guid id);

        //Task UpdateAuthorAsync(Guid id, AuthorEditViewModel model);

        //Task<bool> ExistsAsync(Guid id);

        //Task DeleteAuthorAsync(Guid id);

        //Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(Guid id);

    }
}
