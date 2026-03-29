using OnlineLibrary.Services.Models.Publisher;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IPublisherService
    {
        Task<(IEnumerable<PublisherAllDto> PublisherAllDtos, int TotalPages)> GetAllPublishersAsync(string? searchQuery = null, int pageNumber = 1, int pageSize = 20);
        Task<PublisherDetailsDto?> GetPublisherDetailsByIdAsync(Guid id);

        PublisherAddDto GetEmptyPublisherViewModel();

        Task AddNewPublisherAsync(PublisherAddDto model);

        Task<PublisherAllDto?> GetNewPublisherForEditByIdAsync(Guid id);

        Task<bool> UpdateNewPublisherAsync(Guid id, PublisherAllDto model);

        Task<PublisherDeleteDto?> GetPublisherNewDeleteDetailsAsync(Guid id);
        Task<bool> DeletePublisherByIdAsync(Guid id);
    }
}
