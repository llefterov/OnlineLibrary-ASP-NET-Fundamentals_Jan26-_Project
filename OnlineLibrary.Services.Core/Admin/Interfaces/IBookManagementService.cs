using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Book;

namespace OnlineLibrary.Services.Core.Admin.Interfaces
{
    public interface IBookManagementService : IBooksService
    {
        // Admin-specific: bypass per-user ownership checks
        Task<BookEditDto?> GetBookForAdminEditDtoAsync(Guid id);
        Task<bool> EditBookForAdminDtoAsync(BookEditDto model);

        Task<BookDeleteDto?> GetBookAdminDeleteDetailsDtoAsync(Guid id);
        Task<bool> DeleteBookForAdminDtoAsync(Guid id);

        Task<IEnumerable<BookAllDto>> GetAllBooksForAdminDtoAsync();
        Task<bool> RestoreBookForAdminDtoAsync(Guid id);
    }
}
