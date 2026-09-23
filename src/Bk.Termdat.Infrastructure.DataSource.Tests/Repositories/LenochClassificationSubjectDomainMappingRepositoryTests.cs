using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

internal class LenochClassificationSubjectDomainMappingRepositoryTests
{
    private LenochClassificationSubjectDomainMappingRepository lenochClassificationSubjectDomainMappingRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        lenochClassificationSubjectDomainMappingRepository = new LenochClassificationSubjectDomainMappingRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [Test]
    public async Task ShouldGetAll()
    {
        var data = (await lenochClassificationSubjectDomainMappingRepository.GetAll()).ToList();

        Assert.That(data, Is.Not.Empty);
        Assert.That(data[0].LenochClassificationId, Is.EqualTo(1));
        Assert.That(data[0].SubjectId, Is.EqualTo(1));
        Assert.That(data[0].DomainId, Is.EqualTo(1));
        Assert.That(data[0].Subject, Is.Not.Null);
        Assert.That(data[0].Domain, Is.Not.Null);

        Assert.That(data[1].LenochClassificationId, Is.EqualTo(1));
        Assert.That(data[1].SubjectId, Is.EqualTo(1));
        Assert.That(data[1].DomainId, Is.EqualTo(2));
        Assert.That(data[1].Subject, Is.Not.Null);
        Assert.That(data[1].Domain, Is.Not.Null);

        Assert.That(data[2].LenochClassificationId, Is.EqualTo(1));
        Assert.That(data[2].SubjectId, Is.EqualTo(3));
        Assert.That(data[2].DomainId, Is.Null);
        Assert.That(data[2].Subject, Is.Not.Null);
        Assert.That(data[2].Domain, Is.Null);

        Assert.That(data[3].LenochClassificationId, Is.EqualTo(2));
        Assert.That(data[4].LenochClassificationId, Is.EqualTo(2));
        Assert.That(data[5].LenochClassificationId, Is.EqualTo(3));
    }
}
