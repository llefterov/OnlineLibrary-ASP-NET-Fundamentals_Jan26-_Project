namespace OnlineLibrary.GCommon.Exceptions.PublisherExceptions
{
    [Serializable]
    public class PublisherDoesntExistException : Exception
    {

        public PublisherDoesntExistException(string Name) : base($"A publisher with the name '{Name}' doesn't exists.")
        {

        }
    }
}

