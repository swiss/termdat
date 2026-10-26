using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class SearchException : Exception
{
    public SearchException()
    {
    }

    public SearchException(string message) : base(message)
    {
    }

    public SearchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
