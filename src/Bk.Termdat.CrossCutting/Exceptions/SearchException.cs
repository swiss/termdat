using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
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

    protected SearchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
