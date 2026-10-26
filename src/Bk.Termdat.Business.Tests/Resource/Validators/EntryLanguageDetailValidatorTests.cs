using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Resource.Validators;
using Bogus;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Resource.Validators;

[TestFixture]
internal class EntryLanguageDetailValidatorTests
{
    private EntryLanguageDetailValidator validator;

    [SetUp]
    public void SetUp()
    {
        validator = new EntryLanguageDetailValidator();
    }

    [Test]
    public void ShouldHaveErrorWhenMaximumLengthExceeded()
    {
        var faker = new Faker();
        var model = new EntryModificationLanguageDetail
        {
            Terminus = faker.Random.String(4001),
            TerminusSource = faker.Random.String(4001),
            Name = faker.Random.String(4001),
            NameSource = faker.Random.String(4001),
            Abbreviation = faker.Random.String(4001),
            AbbreviationSource = faker.Random.String(4001),
            Phraseology = faker.Random.String(4001),
            PhraseologySource = faker.Random.String(4001),
            Definition = faker.Random.String(4001),
            DefinitionSource = faker.Random.String(4001),
            Note = faker.Random.String(4001),
            NoteSource = faker.Random.String(4001),
            Context = faker.Random.String(4001),
            ContextSource = faker.Random.String(4001),
            Country = faker.Random.String(4001),
            Metadata = faker.Random.String(4001),
            MetadataSource = faker.Random.String(4001),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Terminus);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.TerminusSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Name);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.NameSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Abbreviation);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.AbbreviationSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Phraseology);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.PhraseologySource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Definition);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.DefinitionSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Note);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.NoteSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Context);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.ContextSource);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Country);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Metadata);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.MetadataSource);
    }

    [Test]
    public void ShouldHaveErrorWhenLanguageIdEmpty()
    {
        var model = new EntryLanguageDetail
        {
            LanguageId = default
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.LanguageId);
    }

    [Test]
    public void ShouldNotHaveValidationErrorWhenOnlyTerminusIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            LanguageId = 1,
            Terminus = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldNotHaveValidationErrorWhenOnlyNameIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            LanguageId = 1,
            Name = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldNotHaveValidationErrorWhenOnlyAbbreviationIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            LanguageId = 1,
            Abbreviation = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldNotHaveValidationErrorWhenOnlyPhraseologyIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            LanguageId = 1,
            Phraseology = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldNotHaveValidationErrorWhenTerminusNameAbbreviationAndPhraseologyIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            LanguageId = 1,
            Terminus = faker.Hacker.Noun(),
            Name = faker.Hacker.Noun(),
            Abbreviation = faker.Hacker.Noun(),
            Phraseology = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenNoTerminusNameAbbreviationAndPhraseologyIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            Definition = faker.Hacker.Noun(),
            Note = faker.Hacker.Noun(),
            Context = faker.Hacker.Noun(),
            Country = faker.Hacker.Noun(),
            Metadata = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlySourcesAreSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            Sequence = 1,
            TerminusSource = faker.Hacker.Noun(),
            NameSource = faker.Hacker.Noun(),
            AbbreviationSource = faker.Hacker.Noun(),
            PhraseologySource = faker.Hacker.Noun(),
            DefinitionSource = faker.Hacker.Noun(),
            NoteSource = faker.Hacker.Noun(),
            ContextSource = faker.Hacker.Noun(),
            MetadataSource = faker.Hacker.Noun()
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Terminus);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Name);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Abbreviation);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Phraseology);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Definition);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Note);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Context);
        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Metadata);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyTerminusSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            TerminusSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Terminus);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyNameSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            NameSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Name);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyAbbreviationSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            AbbreviationSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Abbreviation);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyPhraseologySourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            PhraseologySource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Phraseology);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyDefinitionSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            DefinitionSource = faker.Hacker.Noun(),
            Sequence = 1,
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Definition);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenDefinitionIsSetOnSynonym2()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            Definition = faker.Hacker.Noun(),
            Sequence = 2,
            LanguageId = 6,
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Definition);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyNoteSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            NoteSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Note);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyContextSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            ContextSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Context);
    }

    [Test]
    public void ShouldHaveValidationErrorWhenOnlyMetadataSourceIsSet()
    {
        var faker = new Faker();
        var model = new EntryLanguageDetail
        {
            MetadataSource = faker.Hacker.Noun(),
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(languageDetail => languageDetail.Metadata);
    }
}
