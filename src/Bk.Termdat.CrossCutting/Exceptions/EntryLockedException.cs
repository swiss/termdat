using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
public class EntryLockedException : Exception
{
    protected EntryLockedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public EntryLockedException(string message) : base(message)
    {
    }

    public EntryLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
