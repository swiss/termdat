using System;

namespace Bk.Termdat.CrossCutting;

public static class IsoCodeExtensions
{
    public static bool IsEqualTo(this IsoCode isoCode, string isoCodeText)
    {
        return string.Equals(isoCode.ToString(), isoCodeText, StringComparison.OrdinalIgnoreCase);
    }
}
