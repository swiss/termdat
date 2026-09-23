using System;
using System.Runtime.Serialization;

namespace Bk.Termdat.CrossCutting.Exceptions;

[Serializable]
public class ConcurrencyException : Exception
{
    public object Local { get; set; }
    public object Remote { get; set; }

    public ConcurrencyException()
    {
    }

    public ConcurrencyException(object local, object remote)
    {
        Local = local;
        Remote = remote;
    }

    protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
