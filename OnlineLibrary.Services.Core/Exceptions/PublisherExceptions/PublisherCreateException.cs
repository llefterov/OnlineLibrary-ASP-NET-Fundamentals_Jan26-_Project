using System;

namespace OnlineLibrary.Services.Core.Exceptions.PublisherExceptions
{
    public class PublisherCreateException : Exception
    {
        public PublisherCreateException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}