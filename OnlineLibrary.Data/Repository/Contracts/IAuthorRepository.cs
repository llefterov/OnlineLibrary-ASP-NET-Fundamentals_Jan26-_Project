using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Repository.Contracts
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync();

        Task<Author?> GetAuthorByIdAsync(Guid id);

        Author GetEmptyAuthorFormModelAsync();

        Task AddAuthorAsync(Author model);

        Task<Author?> GetAuthorForEditByIdAsync(Guid id);

        Task<bool> UpdateAuthorAsync(Guid id, Author model);

        Task<bool> ExistsAsync(Guid id);

        Task<bool> DeleteAuthorAsync(Guid id);

        Task<Author?> GetAuthorDeleteDetailsAsync(Guid id);
    }
}
