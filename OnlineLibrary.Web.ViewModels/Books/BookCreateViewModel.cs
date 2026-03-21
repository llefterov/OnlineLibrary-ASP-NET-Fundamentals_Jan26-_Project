using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using OnlineLibrary.Data.Models;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Books
{
    public class BookCreateViewModel
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
