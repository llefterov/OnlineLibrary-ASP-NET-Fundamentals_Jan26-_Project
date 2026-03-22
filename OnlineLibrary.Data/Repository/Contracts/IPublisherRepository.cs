using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Repository.Contracts
{
    public interface IPublisherRepository
    {
            Task<IEnumerable<Publisher>> GetAllPublishersAsync();
    
            Task<Publisher?> GetPublisherByIdAsync(Guid id);
    
            Publisher GetEmptyPublisherFormModelAsync();
    
            Task AddPublisherAsync(Publisher model);
    
            Task<Publisher> GetPublisherForEditByIdAsync(Guid id);
    
            Task UpdatePublisherAsync(Guid id, Publisher model);
    
            Task<bool> ExistsAsync(Guid id);
    
            Task<Publisher> GetPublisherDeleteDetailsAsync(Guid id);

            Task DeletePublisherAsync(Guid id);
    }
}
