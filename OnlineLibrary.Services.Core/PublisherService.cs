using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using static OnlineLibrary.GCommon.ApplicationConstants;
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

        public async Task<IEnumerable<PublisherAllDto>> GetAllPublishersAsync(string? searchQuery = null)
        {
            var publishers = await publisherRepository.GetAllPublishersAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                publishers = publishers.Where(p => p.Name.ToLower().Contains(searchQuery));
            }

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
                return null;
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
                        DateAdded = b.DateAdded.ToString(DateTimeFormat),
                        GenreName = b.Genre.ToString(),
                        AuthorsName = string.Join(", ", b.BooksAuthors.Select(ba => ba.Author.FullName)),
                        Description = b.Description
                    })
                    .ToList()
            };

            return publisherDto;
        }

        public PublisherAddDto GetEmptyPublisherViewModel()
        {
            var emptyAuthorFormModel = new PublisherAddDto();
            return emptyAuthorFormModel;
        }

        public async Task AddNewPublisherAsync(PublisherAddDto inputModel)
        {
            var normalizedName = inputModel.Name.Trim();

            var publisher = new Publisher
            {
                Name = normalizedName
            };

            try
            {
                await publisherRepository.AddPublisherAsync(publisher);
            }
            catch (DbUpdateException dbEx)
            {
                throw new PublisherCreateException("Unable to save the publisher to the database.", dbEx);
            }
        }

        public async Task<PublisherAllDto?> GetNewPublisherForEditByIdAsync(Guid id)
        {
            var publisher = await publisherRepository.GetPublisherForEditByIdAsync(id);

            if (publisher == null)
            {
                return null;
            }

            var inputModel = new PublisherAllDto
            {
                Id = publisher.Id,
                Name = publisher.Name
            };
            return inputModel;
        }

        public async Task<bool> UpdateNewPublisherAsync(Guid id, PublisherAllDto model)
        {
            var publisher = await publisherRepository.GetPublisherForEditByIdAsync(id);

            if (publisher == null)
            {
                return false;
            }

            publisher.Name = model.Name;

            return await publisherRepository.UpdatePublisherAsync(id, publisher);
        }



        public async Task<PublisherDeleteDto?> GetPublisherNewDeleteDetailsAsync(Guid id)
        {
            var publisherService = await publisherRepository.GetPublisherDeleteDetailsAsync(id);

            if (publisherService == null)
            {
                return null;
            }

            var publisherToDelete = new PublisherDeleteDto
            {
                Id = publisherService.Id,
                Name = publisherService.Name,
                Books = publisherService.Books
            };

            return publisherToDelete;
        }

        public async Task<bool> DeletePublisherByIdAsync(Guid id)
        {
            Publisher? publisher = await publisherRepository.GetPublisherDeleteDetailsAsync(id);

            if (publisher == null)
            {
                return false;
            }

            if (publisher.Books.Any())
            {
                throw new PublisherDeleteException("Cannot delete publisher with associated books.");
            }

            return await publisherRepository.DeletePublisherAsync(id);
        }
    }
}
