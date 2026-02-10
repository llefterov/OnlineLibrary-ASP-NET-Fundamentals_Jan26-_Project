using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Author
{
    public class AuthorAddViewModel
    {
        public int Id { get; set; }

        [Required]
        [MinLength(AuthorFullNameMinLength)]
        [MaxLength(AuthorFullNameMaxLength)]
        public string FullName { get; set; } = null!;




    }
}
