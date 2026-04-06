namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<PublisherBookDto> BooksWithAuthorName { get; set; } = new List<PublisherBookDto>();
    }
}
