using OnlineLibrary.Web.ViewModels.Author;
using OnlineLibrary.Web.ViewModels.Publisher;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IPublisherService
    {

        Task<IEnumerable<PublisherAllViewModel>> GetPublisherAllAsync();
        Task<PublisherDetailsViewModel?> GetPublisherByIdAsync(Guid id);

        PublisherAddViewModel GetEmtyPublisherFormModelAsync();

        Task AddPublisherAsync(PublisherAddViewModel model);

        Task<PublisherEditViewModel> GetPublisherForEditByIdAsync(Guid id);

        Task UpdatePublisherAsync(Guid id, PublisherEditViewModel model);
        Task<bool> ExistsAsync(Guid id);

        Task DeletePublisherAsync(Guid id);

        Task<PublisherDeleteViewModel> GetPublisherDeleteDetailsAsync(Guid id);
    }
}
