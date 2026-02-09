using System.ComponentModel.DataAnnotations;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Books
{
    public class BookEditViewModel
    {
        [Required]
        public Guid Id { get; set; }


        [Required]
        [MinLength(BookTitleMinLength)]
        [MaxLength(BookTitleMaxLength)]
        public string Title { get; set; } = null!;

        [Required]
        [MinLength(BookDescriptionMinLength)]
        [MaxLength(BookDescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [Required]
        public string Genre { get; set; } = null!;

        [Required]
        public bool isRead { get; set; }


        public DateTime? DateRead { get; set; }


        public int Rating { get; set; }

        [Required]
        [MinLength(BookCoverUrlMinLength)]
        [MaxLength(BookCoverUrlMaxLength)]
        public string CoverUrl { get; set; } = null!;

        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [Required]
        public int PublisherId { get; set; }


        public List<int> AuthorIds { get; set; } = new();
    }
}