
namespace OnlineLibrary.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using static OnlineLibrary.GCommon.ValidationConstants;

    public class Author
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(AuthorFullNameMaxLength)]
        public string FullName { get; set; } = null!;
        public virtual ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();
    }
}
