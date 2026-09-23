using System.Text;
using Bk.Termdat.Business.Services;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class PictureEntityTagServiceTests
{
    [TestCase("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4", "")]
    [TestCase("294AC0B9648BCF7E775E68D2A943E72C8F745A9C", "bogue")]
    [TestCase("A10B73E0FAB172DA6A5522DEF664A38D136FDCDA", "The quick brown fox jumps over the lazy dog.")]
    public void ComputeEntityTagShouldReturnCorrectHash(string expected, string data)
    {
        var actual = PictureEntityTagService.ComputeEntityTag(Encoding.Unicode.GetBytes(data));

        Assert.That(actual, Is.EqualTo(expected));
    }
}
