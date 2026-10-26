using System;

namespace Bk.Termdat.CrossCutting.Exceptions;

public class BusinessValidationException : Exception
{
    public string ModelKey { get; }

    public BusinessValidationException()
    {
    }

    public BusinessValidationException(string message) : base(message)
    {
    }

    public BusinessValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BusinessValidationException(string modelKey, string message) : base(message)
    {
        ModelKey = modelKey;
    }
}
