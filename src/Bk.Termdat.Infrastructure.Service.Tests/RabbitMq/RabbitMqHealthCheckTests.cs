using System;
using System.Threading.Tasks;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service.Tests.RabbitMq;

[TestFixture]
internal class RabbitMqHealthCheckTests
{
    private readonly IRabbitMqChannelFactory channelFactory = Substitute.For<IRabbitMqChannelFactory>();

    private RabbitMqHealthCheck healthCheck = null!;

    [SetUp]
    public void SetUp()
    {
        healthCheck = new RabbitMqHealthCheck(channelFactory);
    }

    [TearDown]
    public void TearDown()
    {
        channelFactory.ClearSubstitute();
    }

    [Test]
    public async Task CheckHealthAsync_WithHealthyStatus_ReturnsHealthy()
    {
        var channel = Substitute.For<IChannel>();
        channel.IsOpen.Returns(true);
        channelFactory.CreateChannel().Returns(channel);

        var result = await healthCheck.CheckHealthAsync(null!);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public async Task CheckHealthAsync_WithUnhealthyStatus_ReturnsUnhealthy()
    {
        var channel = Substitute.For<IChannel>();
        channel.IsOpen.Returns(false);
        channelFactory.CreateChannel().Returns(channel);

        var result = await healthCheck.CheckHealthAsync(null!);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }

    [Test]
    public async Task CheckHealthAsync_WithException_ReturnsUnhealthy()
    {
        channelFactory.CreateChannel().Throws(new Exception("SHOULD BE CAUGHT"));

        var result = await healthCheck.CheckHealthAsync(null!);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
    }
}
