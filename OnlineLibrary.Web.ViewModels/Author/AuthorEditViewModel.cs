using System.ComponentModel.DataAnnotations;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Author
{
    public class AuthorEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(AuthorFullNameMinLength)]
        [MaxLength(AuthorFullNameMaxLength)]
        public string FullName { get; set; } = null!;
    }
}
