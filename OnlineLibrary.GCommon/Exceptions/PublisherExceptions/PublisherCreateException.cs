using System;

namespace OnlineLibrary.GCommon.Exceptions.PublisherExceptions
{
    public class PublisherCreateException : Exception
    {
        public PublisherCreateException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}