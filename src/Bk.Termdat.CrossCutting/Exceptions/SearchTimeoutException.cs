using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class SearchTimeoutException : Exception
{
    public SearchTimeoutException()
    {
    }

    public SearchTimeoutException(string message) : base(message)
    {
    }

    public SearchTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
