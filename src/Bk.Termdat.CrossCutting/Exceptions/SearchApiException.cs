using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class SearchApiException : Exception
{
    public SearchApiException()
    {
    }

    public SearchApiException(string message) : base(message)
    {
    }

    public SearchApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
