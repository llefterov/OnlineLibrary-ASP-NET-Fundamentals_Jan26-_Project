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
        Task<PublisherDetailsViewModel?> GetPublisherByIdAsync(int id);

        PublisherAddViewModel GetEmtyPublisherFormModelAsync();

        Task AddPublisherAsync(PublisherAddViewModel model);

        Task<PublisherEditViewModel> GetPublisherForEditByIdAsync(int id);

        Task UpdatePublisherAsync(int id, PublisherEditViewModel model);
        Task<bool> ExistsAsync(int id);

        Task DeletePublisherAsync(int id);

        Task<PublisherDeleteViewModel> GetPublisherDeleteDetailsAsync(int id);



    }
}
