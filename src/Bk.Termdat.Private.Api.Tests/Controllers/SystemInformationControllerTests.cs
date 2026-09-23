using System;
using Bk.Termdat.Private.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Version = Bk.Termdat.Business.Resource.Models.Version;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class SystemInformationControllerTests
{
    private SystemInformationController controller = null!;

    [SetUp]
    public void SetUp()
    {
        controller = new SystemInformationController();
    }

    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("local", "local")]
    [TestCase("1.2.0_24hff8", "1.2.0 (24hff8)")]
    [TestCase("1.2.0_main_2jk34hj5", "1.2.0_main_2jk34hj5")]
    public void GetApplicationVersion_ShouldReturnExpectedVersion(string? version, string? expectedVersion)
    {
        Environment.SetEnvironmentVariable("APPLICATION_VERSION", version);

        var result = controller.GetApplicationVersion();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        var value = (result as OkObjectResult)!.Value as Version;
        Assert.That(value, Is.Not.Null);
        Assert.That(value.ApplicationVersion, Is.EqualTo(expectedVersion));
    }
}
