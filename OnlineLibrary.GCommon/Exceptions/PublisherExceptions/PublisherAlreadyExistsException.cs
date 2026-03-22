using System;

namespace OnlineLibrary.GCommon.Exceptions.PublisherExceptions
{
    public class PublisherAlreadyExistsException : Exception
    {
        public PublisherAlreadyExistsException(string Name)
            : base($"A Publisher with the name '{Name}' already exists.")
        {
        }
    }
}