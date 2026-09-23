using System;

namespace Bk.Termdat.Common.Utilities;

public static class SizeExtension
{
    public enum SizeUnits
    {
        // ReSharper disable InconsistentNaming
        Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        // ReSharper restore InconsistentNaming
    }

    public static string ToSize(this long value, SizeUnits unit)
    {
        return $"{value / Math.Pow(1024, (long)unit):0.00} {unit}";
    }
}
