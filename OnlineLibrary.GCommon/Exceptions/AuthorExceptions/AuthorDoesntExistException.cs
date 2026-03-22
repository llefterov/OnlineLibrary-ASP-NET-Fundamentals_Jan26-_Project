namespace OnlineLibrary.GCommon.Exceptions.AuthorExceptions
{
    [Serializable]
    public class AuthorDoesntExistException : Exception
    {

        public AuthorDoesntExistException(string fullName) : base($"An author with the name '{fullName}' doesn't exists.")
        {

        }
    }
}

