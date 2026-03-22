using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Publisher;
using static OnlineLibrary.GCommon.ApplicationConstants;
using System.Globalization;
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

        //public async Task<PublisherDetailsViewModel?> GetPublisherByIdAsync(Guid id)
        //{
        //    var publisher = await dbContext.Publishers
        //       .Include(p => p.Books)
        //       .ThenInclude(b => b.BooksAuthors)
        //       .ThenInclude(ba => ba.Author)
        //       .AsNoTracking()
        //       .Where(p => p.Id == id)
        //       .Select(p => new PublisherDetailsViewModel
        //       {
        //           Id = p.Id,
        //           Name = p.Name,
        //           BooksWithAuthorName = p.Books
        //           .Where(b => !b.IsDeleted)
        //           .OrderBy(b => b.Title)
        //             .Select(b => new PublisherBookViewModel
        //             {
        //                 Id = b.Id,
        //                 Title = b.Title,
        //                 CoverUrl = b.CoverUrl ?? string.Empty,
        //                 Rating = b.Rating,
        //                 DateAdded = b.DateAdded.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
        //                 GenreName = b.Genre.ToString(),
        //                 AuthorsName = string.Join(", ", b.BooksAuthors.Select(ba => ba.Author.FullName)),
        //                 Description = b.Description
        //             })
        //             .ToList()
        //       })
        //       .FirstOrDefaultAsync();

        //    return publisher;
        //}


        //public PublisherAddViewModel GetEmtyPublisherFormModelAsync()
        //{
        //    PublisherAddViewModel emptyAuthorFormModel = new PublisherAddViewModel();
        //    return emptyAuthorFormModel;
        //}

        //public async Task AddPublisherAsync(PublisherAddViewModel inputModel)
        //{
        //    var publisher = new Publisher
        //    {
        //        Name = inputModel.Name
        //    };

        //    if (await dbContext.Publishers.AnyAsync(p => p.Name == publisher.Name))
        //    {
        //        throw new PublisherAlreadyExistsException(publisher.Name);
        //    }

        //    await dbContext.Publishers.AddAsync(publisher);

        //    try
        //    {
        //        await dbContext.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        throw new PublisherCreateException("Unable to save the author to the database.", dbEx);
        //    }
        //}

        //public async Task<PublisherEditViewModel> GetPublisherForEditByIdAsync(Guid id)
        //{


        //    if (!(await ExistsAsync(id)))
        //    {
        //        throw new PublisherDoesntExistException("Publisher not found.");
        //    }

        //    var publisher = await dbContext.Publishers.FirstOrDefaultAsync(p => p.Id == id);

        //    if (publisher == null)
        //    {

        //        throw new PublisherDoesntExistException("Publisher does not exist");

        //    }

        //    var inputModel = new PublisherEditViewModel
        //    {
        //        Id = publisher.Id,
        //        Name = publisher.Name
        //    };
        //    return inputModel;
        //}

        //public Task UpdatePublisherAsync(Guid id, PublisherEditViewModel model)
        //{
        //    var publisher = dbContext.Publishers
        //       .FirstOrDefault(p => p.Id == model.Id);

        //    if (publisher == null)
        //    {
        //        throw new PublisherDoesntExistException("Publisher does not exist");
        //    }

        //    publisher.Name = model.Name;

        //    try
        //    {
        //        dbContext.Publishers.Update(publisher);
        //        return dbContext.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        throw new PublisherUpdateExeption("Unable to update the publisher in the database.");
        //    }
        //}

        //public async Task<bool> ExistsAsync(Guid id)
        //{
        //    bool publisherExist = await dbContext
        //        .Publishers
        //        .AnyAsync(a => a.Id == id);

        //    return publisherExist;
        //}

        //public async Task<PublisherDeleteViewModel> GetPublisherDeleteDetailsAsync(Guid id)
        //{
        //    var publisherToDelete = await dbContext.Publishers
        //        .Include(b => b.Books)
        //        .Select(b => new PublisherDeleteViewModel
        //        {
        //            Id = b.Id,
        //            Name = b.Name,
        //            Books = b.Books
        //        })
        //        .FirstOrDefaultAsync(b => b.Id == id);

        //    if (publisherToDelete == null)
        //    {
        //        throw new PublisherDoesntExistException("Publisher does not exist");
        //    }

        //    return publisherToDelete;
        //}

        //public async Task DeletePublisherAsync(Guid id)
        //{
        //    var publisher = await dbContext.Publishers
        //        .Include(b => b.Books)
        //        .FirstOrDefaultAsync(b => b.Id == id);

        //    if (publisher == null)
        //    {
        //        throw new PublisherDoesntExistException("Publisher not found.");
        //    }

        //    if (publisher.Books.Any())
        //    {

        //        throw new PublisherDeleteException("Cannot delete publisher with associated books.");

        //    }

        //    dbContext.Publishers.Remove(publisher);

        //    try
        //    {
        //        await dbContext.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        throw new PublisherDeleteException("Unable to delete the publisher from the database.");
        //    }
        //}
    }
}
