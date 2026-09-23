using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class ConceptSystemPositionRepositoryTests
{
    private ConceptSystemPositionRepository conceptSystemPositionRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        conceptSystemPositionRepository = new ConceptSystemPositionRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetByConceptSystemIdReturns()
    {
        var data = (await conceptSystemPositionRepository.GetByCollectionId(1)).ToList();

        AssertConceptSystem(data);
    }

    [TestCase]
    public async Task GetByIdsReturns()
    {
        var data = await conceptSystemPositionRepository.GetById(1);

        Assert.That(data.Id, Is.EqualTo(1));
    }

    private static void AssertConceptSystem(List<ConceptSystemPosition> data)
    {
        Assert.That(data, Is.Not.Null);

        var conceptSystemPosition = data.First();
        Assert.That(conceptSystemPosition.Id, Is.EqualTo(2));

        conceptSystemPosition = data.Last();
        Assert.That(conceptSystemPosition.Id, Is.EqualTo(1));
    }

}
