using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class ClassificationRepositoryTests
{
    private ClassificationRepository classificationRepository;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        classificationRepository = new ClassificationRepository(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetAllReturns()
    {
        var data = (await classificationRepository.GetAll()).ToList();
        var classification = data.First();

        Assert.That(data, Is.Not.Null);
        Assert.That(classification.Id, Is.Not.Default);

        Assert.That(classification.Code, Is.EqualTo("ABC"));

        Assert.That(classification.Translations.Count, Is.EqualTo(3));
        Assert.That(classification.Translations.All(t => t.Language != null));
        var translation = classification.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.ClassificationId, Is.EqualTo(100));
        Assert.That(translation.Name, Is.EqualTo("BCD"));

        translation = classification.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.ClassificationId, Is.EqualTo(100));
        Assert.That(translation.Name, Is.EqualTo("HIJ"));

        classification = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(classification.Id, Is.Not.Default);

        Assert.That(classification.Code, Is.EqualTo("KLM"));

        Assert.That(classification.Translations.Count, Is.EqualTo(3));
        Assert.That(classification.Translations.All(t => t.Language != null));
        translation = classification.Translations.First();
        Assert.That(translation.LanguageId, Is.EqualTo(1));
        Assert.That(translation.ClassificationId, Is.EqualTo(101));
        Assert.That(translation.Name, Is.EqualTo("NOP"));

        translation = classification.Translations.Last();
        Assert.That(translation.LanguageId, Is.EqualTo(3));
        Assert.That(translation.ClassificationId, Is.EqualTo(101));
        Assert.That(translation.Name, Is.EqualTo("TUV"));
    }
}
