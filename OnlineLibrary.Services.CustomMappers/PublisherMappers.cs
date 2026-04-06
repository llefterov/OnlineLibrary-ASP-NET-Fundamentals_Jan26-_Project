
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.ViewModels.Publisher;

namespace OnlineLibrary.Services.CustomMappers
{
    public static class PublisherMappers
    {

        public static PublisherDeleteViewModel MapPublisherDeleteDtoToPublisherDeleteViewModel(PublisherDeleteDto publisherToDeleteDto)
        {
            return new PublisherDeleteViewModel
            {
                Id = publisherToDeleteDto.Id,
                Name = publisherToDeleteDto.Name,
                Books = publisherToDeleteDto.Books
            };
        }
    }
}
