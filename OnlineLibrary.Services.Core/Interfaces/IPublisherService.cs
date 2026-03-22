using OnlineLibrary.Services.Models.Publisher;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IPublisherService
    {
        Task<IEnumerable<PublisherAllDto>> GetAllPublishersAsync();
        Task<PublisherDetailsDto?> GetPublisherDetailsByIdAsync(Guid id);

        PublisherAddDto GetEmptyPublisherViewModelAsync();

        Task AddNewPublisherAsync(PublisherAddDto model);

        Task<PublisherAllDto> GetNewPublisherForEditByIdAsync(Guid id);

        Task UpdateNewPublisherAsync(Guid id, PublisherAllDto model);

        Task<PublisherDeleteDto> GetPublisherNewDeleteDetailsAsync(Guid id);
        Task DeletePublisherByIdAsync(Guid id);
    }
}
