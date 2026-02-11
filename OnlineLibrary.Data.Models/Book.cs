using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required]
        [MaxLength(BookCoverUrlMaxLength)]
        public string CoverUrl { get; set; } = null!;

        [MaxLength(AddedByUserIdMaxLength)]
        [ForeignKey(nameof(AddedByUser))]
        public string? AddedByUserId { get; set; }

        public IdentityUser AddedByUser { get; set; } = null!;


        [Required]
        public DateTime DateAdded { get; set; }

        [Required]
        public int PublisherId { get; set; }

        public bool IsDeleted { get; set; } = false;

        [Required]
        [ForeignKey(nameof(PublisherId))]
        public Publisher Publisher { get; set; } = null!;

        public virtual ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();

        public virtual ICollection<UserBook> UsersBooks { get; set; } = new HashSet<UserBook>();
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
