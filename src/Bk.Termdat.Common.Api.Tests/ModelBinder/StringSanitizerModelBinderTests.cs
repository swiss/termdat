using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Common.Api.ModelBinder;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bk.Termdat.Common.Api.Tests.ModelBinder;

[TestFixture]
internal class StringSanitizerModelBinderTests
{
    [TestCase(null, null)]
    [TestCase("", null)]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Test", "Test")]
    [TestCase("Overkill", "Overkill")]
    public async Task BindModelAsync_WithInput_ShouldReturnSanitizedString(string? value, string? expected)
    {
        var context = new DefaultModelBindingContext
        {
            ModelName = "test_model_name",
            ValueProvider = new SimpleValueProvider(new Dictionary<string, string> { { "test_model_name", value } }),
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string))
        };

        var binder = new StringSanitizerModelBinder();

        await binder.BindModelAsync(context);
        var result = context.Result;

        Assert.That(result.Model, Is.EqualTo(expected));
    }

    private class SimpleValueProvider : IValueProvider
    {
        private readonly Dictionary<string, string> _values;

        public SimpleValueProvider(Dictionary<string, string> values)
        {
            _values = values;
        }

        public bool ContainsPrefix(string prefix)
        {
            return _values.ContainsKey(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return new ValueProviderResult(value);
            }

            return ValueProviderResult.None;
        }
    }
}
