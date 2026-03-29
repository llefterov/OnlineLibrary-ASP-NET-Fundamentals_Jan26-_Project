using OnlineLibrary.Web.Infrastructure.Utilities.Contracts;
using System.Text.RegularExpressions;

namespace OnlineLibrary.Web.Infrastructure.Utilities
{
    public class SlugGenerator : ISlugGenerator
    {
        public string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "book";
            }

            string slug = input.ToLowerInvariant();

            // Replace whitespace with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Keep only url-safe characters: letters, digits and hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);

            // Collapse multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from both ends
            slug = slug.Trim('-');

            return string.IsNullOrWhiteSpace(slug) ? "book" : slug;
        }
    }
}