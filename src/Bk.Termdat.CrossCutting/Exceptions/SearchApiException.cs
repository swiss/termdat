using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
public class SearchApiException : Exception
{
    protected SearchApiException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public SearchApiException(string message) : base(message)
    {
    }

    public SearchApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
