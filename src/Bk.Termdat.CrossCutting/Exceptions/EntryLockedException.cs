using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class EntryLockedException : Exception
{
    public EntryLockedException()
    {
    }

    public EntryLockedException(string message) : base(message)
    {
    }

    public EntryLockedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
