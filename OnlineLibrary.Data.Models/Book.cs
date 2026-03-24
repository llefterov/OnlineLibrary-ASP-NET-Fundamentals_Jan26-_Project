using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Data.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(BookTitleMaxLength)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(BookDescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [Required]
        [MaxLength(BookGenreMaxLength)]
        public BookGenre Genre { get; set; }

        [Required]
        public bool IsRead { get; set; }

        public DateTime? DateRead { get; set; }


        public int Rating { get; set; }

        [MaxLength(BookCoverUrlMaxLength)]
        public string? CoverUrl { get; set; }

        [Required]
        [MaxLength(AddedByUserIdMaxLength)]
        public Guid AddedByUserId { get; set; }

        [ForeignKey(nameof(AddedByUserId))]
        public virtual ApplicationUser AddedByUser { get; set; } = null!;


        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public Guid PublisherId { get; set; }

        [Required]
        [ForeignKey(nameof(PublisherId))]
        public Publisher Publisher { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();

        public virtual ICollection<UserBook> UsersBooks { get; set; } = new HashSet<UserBook>();

        // This property is not mapped to the database and is used to hold the list of AuthorIds when creating or editing a book.
        [NotMapped]
        public List<Guid>? AuthorIds { get; set; }
    }

    public enum BookGenre
    {
        Fiction,
        NonFiction,
        Mystery,
        Fantasy,
        ScienceFiction,
        Biography,
        History,
        Romance,
        Thriller,
        SelfHelp,
        Other
    }
}
