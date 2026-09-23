using System;
using System.Security.Cryptography;

namespace Bk.Termdat.Business.Services;

public static class PictureEntityTagService
{
    public static string ComputeEntityTag(byte[] data)
    {
        var hash = SHA256.HashData(data);

        return Convert.ToHexString(hash, 0, 20);
    }
}
