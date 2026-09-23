using Bk.Termdat.LindasExport.Api.Extensions;
using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Tests.Extensions;

internal class ReferenceTypeExtensionsTests
{
    [TestCase(Business.Domain.Models.ReferenceType.None, ReferenceType.None)]
    [TestCase(Business.Domain.Models.ReferenceType.General, ReferenceType.General)]
    [TestCase(Business.Domain.Models.ReferenceType.Specific, ReferenceType.Specific)]
    [TestCase(Business.Domain.Models.ReferenceType.Antonym, ReferenceType.Antonym)]
    public void ToSparqlReferenceType_WhenCalled_ReturnsSparqlReferenceType(Business.Domain.Models.ReferenceType referenceType, ReferenceType expectedReferenceType)
    {
        Assert.That(referenceType.ToSparqlReferenceType(), Is.EqualTo(expectedReferenceType));
    }

    [Test]
    public void ToSparqlReferenceType_WhenCalledWithUnknownEnumValue_ReturnsReferenceTypeNone()
    {
        const Business.Domain.Models.ReferenceType unknownReferenceType = (Business.Domain.Models.ReferenceType)99;

        Assert.That(unknownReferenceType.ToSparqlReferenceType(), Is.EqualTo(ReferenceType.None));
    }
}
