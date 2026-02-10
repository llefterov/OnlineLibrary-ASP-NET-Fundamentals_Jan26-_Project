using OnlineLibrary.Services.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Exceptions
{
    public class AuthorDeleteException : Exception
    {

        public AuthorDeleteException(string fullName): base($"Cannot delete author '{fullName}' because they have associated books.")
        {
            
        }
    }
}

