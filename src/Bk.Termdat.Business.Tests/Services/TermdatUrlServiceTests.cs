using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class TermdatUrlServiceTests
{
    private ITermdatUrlService termdatUrlService;
    private readonly IOptions<TermdatClientOptions> termdatClientOptionsMock = Substitute.For<IOptions<TermdatClientOptions>>();

    [SetUp]
    public void Setup()
    {
        termdatClientOptionsMock.Value.Returns(new TermdatClientOptions
        {
            BaseUrl = "http://testtermdat.test/",
            EntryDetailPath = "/search/entry/{0}",
            EntryIdSearchQueryParameter = "s=Entryid:{0}",
        });

        termdatUrlService = new TermdatUrlService(termdatClientOptionsMock);
    }

    [Test]
    public void GetTermdatUrlShouldReturnAnUrl()
    {
        var generatedUrl = termdatUrlService.GetTermdatUrl(100);

        Assert.That(generatedUrl, Is.EqualTo("http://testtermdat.test/search/entry/100?s=Entryid:100"));
    }
}
