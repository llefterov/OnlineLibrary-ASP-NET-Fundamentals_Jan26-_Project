using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Books
{
    public class BooksAllViewModel
    {


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
        public BookGenre Genre { get; set; }

        [Required]
        public bool isRead { get; set; }


        public DateTime? DateRead { get; set; }

        [Required]
        public int Rating { get; set; }

        [Required]
        [MinLength(BookCoverUrlMinLength)]
        [MaxLength(BookCoverUrlMaxLength)]
        public string CoverUrl { get; set; } = null!;

        [Required]
        public DateTime DateAdded { get; set; }

        public string? AddedByUserName { get; set; }

        [Required]
        public int PublisherId { get; set; }

        // Extra friendly fields for the view
        public string PublisherName { get; set; } = null!;
        public string AuthorsName { get; set; } = null!;

        // New: string representation of the enum for display
        public string GenreName { get; set; } = null!;

        public ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();
    }
}
