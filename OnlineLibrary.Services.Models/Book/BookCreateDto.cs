using OnlineLibrary.Data.Models;
using System.ComponentModel.DataAnnotations;
using static OnlineLibrary.GCommon.ValidationConstants; 

namespace OnlineLibrary.Services.Models.Book
{
    public class BookCreateDto
    {
        [Required]
        [MinLength(BookTitleMinLength)]
        [MaxLength(BookTitleMaxLength)]
        public string Title { get; set; } = null!;

        [Required]
        [MinLength(BookDescriptionMinLength)]
        [MaxLength(BookDescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [Required]
        public BookGenre Genre { get; set; }

        [Required]
        public bool IsRead { get; set; }


        public DateTime? DateRead { get; set; }

        [Range(BookRatingMinValue, BookRatingMaxValue)]
        public int Rating { get; set; }



        [Url]
        [MinLength(BookCoverUrlMinLength)]
        [MaxLength(BookCoverUrlMaxLength)]
        public string? CoverUrl { get; set; }

        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid PublisherId { get; set; }

        public Guid? AddedByUserId { get; set; }

        public List<Guid> AuthorIds { get; set; } = new();
    }
}
