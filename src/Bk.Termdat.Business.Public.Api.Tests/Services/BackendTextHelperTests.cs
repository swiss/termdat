using System;
using System.Linq;
using AutoFixture;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Services;
using NUnit.Framework;

namespace Bk.Termdat.Business.Public.Api.Tests.Services;

[TestFixture]
public class BackendTextHelperTests
{
    private BackendTextHelperService backendTextHelperService;
    private Fixture fixture;

    [SetUp]
    public void SetUp()
    {
        backendTextHelperService = new BackendTextHelperService();

        fixture = new();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [TestCase(Reliability.Approved, "de")]
    [TestCase(Reliability.FirstDraft, "de")]
    [TestCase(Reliability.Tentative, "de")]
    [TestCase(Reliability.Translated, "de")]
    [TestCase(Reliability.Verified, "de")]
    public void GetReliabilityTextReturns(Reliability reliability, string isoCode)
    {
        var result = backendTextHelperService.GetReliabilityText(reliability, isoCode);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetReliabilityText_ForUnknownReferenceType_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => backendTextHelperService.GetReliabilityText((Reliability)1_000, "de"));
    }

    [TestCase(Status.InProgress, "de")]
    [TestCase(Status.Validated, "de")]
    public void GetStatusTextReturns(Status status, string isoCode)
    {
        var result = backendTextHelperService.GetStatusText(status, isoCode);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetStatusText_ForUnknownReferenceType_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => backendTextHelperService.GetStatusText((Status)1_000, "de"));
    }

    [TestCase(ReferenceType.None, "de")]
    [TestCase(ReferenceType.Antonym, "de")]
    [TestCase(ReferenceType.General, "de")]
    [TestCase(ReferenceType.Specific, "de")]
    [TestCase(ReferenceType.Whole, "de")]
    [TestCase(ReferenceType.Part, "de")]
    public void GetReferenceTypeTextReturns(ReferenceType referenceType, string isoCode)
    {
        var result = backendTextHelperService.GetReferenceTypeText(referenceType, isoCode);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetReferenceTypeText_ForUnknownReferenceType_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => backendTextHelperService.GetReferenceTypeText((ReferenceType)1_000, "de"));
    }
}
