using System;
using System.Collections.Generic;

namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherBookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
    }
    public class PublisherDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<PublisherBookDto> BooksWithAuthorName { get; set; } = new List<PublisherBookDto>();
    }
}
