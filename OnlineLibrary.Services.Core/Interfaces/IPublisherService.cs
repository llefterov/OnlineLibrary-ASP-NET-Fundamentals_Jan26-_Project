using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.ViewModels.Author;
using OnlineLibrary.Web.ViewModels.Publisher;
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

        //Task<PublisherEditViewModel> GetPublisherForEditByIdAsync(Guid id);

        //Task UpdatePublisherAsync(Guid id, PublisherEditViewModel model);
        //Task<bool> ExistsAsync(Guid id);

        //Task DeletePublisherAsync(Guid id);

        //Task<PublisherDeleteViewModel> GetPublisherDeleteDetailsAsync(Guid id);
    }
}
