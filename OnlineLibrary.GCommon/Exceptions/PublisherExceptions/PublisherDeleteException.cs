using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.GCommon.Exceptions.PublisherExceptions
{
    public class PublisherDeleteException : Exception
    {

        public PublisherDeleteException(string Name) : base($"Cannot delete publisher '{Name}' because it has associated books.")
        {

        }
    }
}

