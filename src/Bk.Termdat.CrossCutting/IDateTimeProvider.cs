using System;

namespace Bk.Termdat.CrossCutting;

public interface IDateTimeProvider
{
    DateTime GetNow();
}
