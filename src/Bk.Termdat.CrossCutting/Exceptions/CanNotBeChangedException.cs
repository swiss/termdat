using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
public class CanNotBeChangedException : Exception
{
    public CanNotBeChangedException()
    {
    }

    public CanNotBeChangedException(string message) : base(message)
    {
    }

    public CanNotBeChangedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected CanNotBeChangedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
