using System.Threading.Tasks;
using Bk.Termdat.Business.Clients;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.Service.Tests;

[TestFixture]
internal class SearchApiStatusServiceTests
{
    private readonly ISearchApiClient searchApiClientMock = Substitute.For<ISearchApiClient>();
    private SearchApiHealthCheck healthCheck;

    [SetUp]
    public void SetUp()
    {
        healthCheck = new SearchApiHealthCheck(searchApiClientMock);
    }

    [TearDown]
    public void TearDown()
    {
        searchApiClientMock.ClearSubstitute();
    }

    [Test]
    public async Task GetStatusShouldCallIsAlive()
    {
        await healthCheck.CheckHealthAsync(null);
        await searchApiClientMock.Received(1).IsAlive();
    }

    [Test]
    public async Task ShouldReturnUpWhenAlive()
    {
        searchApiClientMock.IsAlive().Returns(true);

        var status = await healthCheck.CheckHealthAsync(null);

        await searchApiClientMock.Received(1).IsAlive();
        Assert.That(status.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public async Task ShouldReturnDownWhenNotAlive()
    {
        searchApiClientMock.IsAlive().Returns(false);

        var status = await healthCheck.CheckHealthAsync(null);

        await searchApiClientMock.Received(1).IsAlive();
        Assert.That(status.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }
}
