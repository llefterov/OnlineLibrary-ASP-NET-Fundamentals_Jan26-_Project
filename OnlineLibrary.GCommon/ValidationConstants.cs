namespace OnlineLibrary.GCommon
{
    public static class ValidationConstants
    {
        /* Author */
        public const int AuthorFullNameMinLength = 2;
        public const int AuthorFullNameMaxLength = 150;

        public const int AddedByUserIdMinLength = 1;
        public const int AddedByUserIdMaxLength = 450; // Assuming a GUID or similar identifier

        /* Publisher */
        public const int PublisherNameMinLength = 2;
        public const int PublisherNameMaxLength = 200;

        /* Book */
        public const int BookTitleMinLength = 2;
        public const int BookTitleMaxLength = 250;

        public const int BookDescriptionMinLength = 2;
        public const int BookDescriptionMaxLength = 1000;

        public const int BookRatingMinValue = 0;
        public const int BookRatingMaxValue = 5;


        public const int BookCoverUrlMinLength = 7;
        public const int BookCoverUrlMaxLength = 2083; // Maximum URL length in Internet Explorer

        public const int BookGenreMinLength = 1;
        public const int BookGenreMaxLength = 100;
    }
}
