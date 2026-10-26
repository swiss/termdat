using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

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
}
