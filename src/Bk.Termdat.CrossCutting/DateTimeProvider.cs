using System;

namespace Bk.Termdat.CrossCutting;

// Use this class instead of direct access to DateTime.Now / DateTime.Today if you want to mock them during unit-testing
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetNow() => DateTime.Now;
}
