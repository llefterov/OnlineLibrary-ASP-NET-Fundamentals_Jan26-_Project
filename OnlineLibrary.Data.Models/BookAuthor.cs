using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Data.Models
{
    [PrimaryKey(nameof(BookId), nameof(AuthorId))]

    public class BookAuthor
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public Book Book { get; set; } = null!;

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public Author Author { get; set; } = null!;
    }
}