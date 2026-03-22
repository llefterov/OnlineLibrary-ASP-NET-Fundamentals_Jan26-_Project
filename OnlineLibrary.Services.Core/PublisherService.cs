using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Data.Repository.Contracts;

namespace OnlineLibrary.Services.Core
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository publisherRepository;

        public PublisherService(IPublisherRepository publisherRepository)
        {
            this.publisherRepository = publisherRepository;
        }

        public async Task<IEnumerable<PublisherAllDto>> GetAllPublishersAsync()
        {
            var publishers = await publisherRepository.GetAllPublishersAsync();

            var publishersDto = publishers
            .OrderBy(p => p.Name)
            .Select(p => new PublisherAllDto
            {
                Id = p.Id,
                Name = p.Name
            })
         .ToList();

            return publishersDto;
        }

        public async Task<PublisherDetailsDto?> GetPublisherDetailsByIdAsync(Guid id)
        {
            var publisher = await publisherRepository.GetPublisherByIdAsync(id);
             
            if (publisher == null)
            {
                throw new PublisherDoesntExistException("Publisher not found.");
            }

            PublisherDetailsDto? publisherDto = new PublisherDetailsDto
            {
                Id = publisher.Id,
                Name = publisher.Name,
                BooksWithAuthorName = publisher.Books
                    .Where(b => !b.IsDeleted)
                    .OrderBy(b => b.Title)
                    .Select(b => new PublisherBookDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        CoverUrl = b.CoverUrl ?? string.Empty,
                        Rating = b.Rating,
                        DateAdded = b.DateAdded.ToString("dd/MM/yyyy"),
                        GenreName = b.Genre.ToString(),
                        AuthorsName = string.Join(", ", b.BooksAuthors.Select(ba => ba.Author.FullName)),
                        Description = b.Description
                    })
                    .ToList()
            };

            return publisherDto;
        }

        public PublisherAddDto GetEmptyPublisherViewModelAsync()
        {
            var emptyAuthorFormModel = new PublisherAddDto();
            return emptyAuthorFormModel;
        }

        public async Task AddNewPublisherAsync(PublisherAddDto inputModel)
        {
            var publisher = new Publisher
            {
                Name = inputModel.Name
            };

            var publishers = await publisherRepository
                .GetAllPublishersAsync();
                


            if ((publishers.Any(p => p.Name == publisher.Name)))
            {
                throw new PublisherAlreadyExistsException(publisher.Name);
            }

            try
            {
            await publisherRepository.AddPublisherAsync(publisher);
            }
            catch (DbUpdateException dbEx)
            {
                throw new PublisherCreateException("Unable to save the author to the database.", dbEx);
            }
        }

        public async Task<PublisherAllDto> GetNewPublisherForEditByIdAsync(Guid id)
        {

            var publisher = await publisherRepository.GetPublisherForEditByIdAsync(id);

            if (publisher == null)
            {

                throw new PublisherDoesntExistException("Publisher does not exist");

            }

            var inputModel = new PublisherAllDto
            {
                Id = publisher.Id,
                Name = publisher.Name
            };
            return inputModel;
        }

        public async Task UpdateNewPublisherAsync(Guid id, PublisherAllDto model)
        {
            var publisher = await publisherRepository.GetPublisherForEditByIdAsync(id);

            if (publisher == null)
            {
                throw new PublisherDoesntExistException("Publisher does not exist");
            }

            publisher.Name = model.Name;

            try
            {
                await publisherRepository.UpdatePublisherAsync(id, publisher);
            }
            catch (DbUpdateException)
            {
                throw new PublisherUpdateExeption("Unable to update the publisher in the database.");
            }
        }



        public async Task<PublisherDeleteDto> GetPublisherNewDeleteDetailsAsync(Guid id)
        {

           var publisherService = await publisherRepository.GetPublisherDeleteDetailsAsync(id);

            if (publisherService == null)
            {
                throw new PublisherDoesntExistException("Publisher does not exist");
            }

            var publisherToDelete = new PublisherDeleteDto
            {
                Id = publisherService.Id,
                Name = publisherService.Name,
                Books = publisherService.Books
            };

           return publisherToDelete;
        }

        public async Task DeletePublisherByIdAsync(Guid id)
        {

           Publisher? publisher = await publisherRepository.GetPublisherDeleteDetailsAsync(id);    

            if (publisher == null)
            {
                throw new PublisherDoesntExistException("Publisher not found.");
            }

            if (publisher.Books.Any())
            {
                throw new PublisherDeleteException("Cannot delete publisher with associated books.");
            }

            try
            {
                await publisherRepository.DeletePublisherAsync(id);
            }
            catch (DbUpdateException)
            {
                throw new PublisherDeleteException("Unable to delete the publisher from the database.");
            }
        }
    }
}
