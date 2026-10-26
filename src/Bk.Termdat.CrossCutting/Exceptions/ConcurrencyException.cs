using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class ConcurrencyException : Exception
{
    public object Local { get; set; }
    public object Remote { get; set; }

    public ConcurrencyException()
    {
    }

    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ConcurrencyException(object local, object remote)
    {
        Local = local;
        Remote = remote;
    }
}
