using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
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

    protected SearchTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
