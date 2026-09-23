using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
public class BusinessValidationException : Exception
{
    public string ModelKey { get; }

    protected BusinessValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public BusinessValidationException(string modelKey, string message) : base(message)
    {
        ModelKey = modelKey;
    }
}
