using System.Collections.Generic;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Mapping;
using NSubstitute;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Mapping;

public class MappingUtilTests
{
    [Test]
    public void AssertTranslationsLanguageNotNullThrowsException()
    {
        Assert.Throws<AutoMapperMappingException>(() => MappingUtil.AssertTranslationsLanguageNotNull(null));

        var translation1 = Substitute.For<IEntityTranslation>();
        var language = new Language();
        translation1.Language.ReturnsForAnyArgs(language);
        var translation2 = Substitute.For<IEntityTranslation>();
        var translations = new List<IEntityTranslation>
        {
            translation1, translation2
        };

        Assert.Throws<AutoMapperMappingException>(() => MappingUtil.AssertTranslationsLanguageNotNull(translations));

        translation2.Language.ReturnsForAnyArgs(language);

        Assert.DoesNotThrow(() => MappingUtil.AssertTranslationsLanguageNotNull(translations));
    }
}
