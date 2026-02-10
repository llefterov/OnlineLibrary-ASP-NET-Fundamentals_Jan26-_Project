using OnlineLibrary.Web.ViewModels.Author;
using OnlineLibrary.Web.ViewModels.Publisher;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IPublisherService
    {

        Task<IEnumerable<PublisherAllViewModel>> GetAllAsync();
        Task<PublisherDetailsViewModel?> GetByIdAsync(int id);
        //Task<TKey> CreateAsync(TCreateModel model, string? userId = null);
        //Task UpdateAsync(TKey id, TEditModel model, string? userId = null);
        //Task DeleteAsync(TKey id, string? userId = null);
        //Task<bool> ExistsAsync(TKey id);



    }
}
