using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Data.Models
{
    [PrimaryKey(nameof(BookId), nameof(AuthorId))]

    public class BookAuthor
    {
        [Required]
        [ForeignKey(nameof(Book))]
        public Guid BookId { get; set; }

        [Required]
        public Book Book { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Author))]
        public Guid AuthorId { get; set; }

        [Required]
        public Author Author { get; set; } = null!;
    }
}