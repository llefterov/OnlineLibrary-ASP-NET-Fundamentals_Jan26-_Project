using OnlineLibrary.Data.Models;
using static OnlineLibrary.GCommon.ValidationConstants;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherAddDto
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(PublisherNameMinLength)]
        [MaxLength(PublisherNameMaxLength)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
