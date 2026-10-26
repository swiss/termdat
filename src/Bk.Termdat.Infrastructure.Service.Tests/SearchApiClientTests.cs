using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.Service.Tests;

[TestFixture]
internal class SearchApiClientTests
{
    private const string HttpBaseAddress = "http://fake.io/api/";
    private SearchApiClient searchApiClient;
    private readonly IHttpClientFactory httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
    private readonly IHttpContextAccessor httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
    private readonly IOptions<SearchApiOptions> searchApiOptionsMock = Substitute.For<IOptions<SearchApiOptions>>();

    private readonly Fixture fixture = new();
    private SearchApiOptions searchApiOptions;

    [SetUp]
    public void SetUp()
    {
        searchApiOptions = fixture.Create<SearchApiOptions>();
        searchApiOptionsMock.Value.Returns(searchApiOptions);

        searchApiClient = new SearchApiClient(Substitute.For<ILogger<SearchApiClient>>(), httpClientFactoryMock, httpContextAccessorMock, searchApiOptionsMock);
    }

    [TearDown]
    public void TearDown()
    {
        httpClientFactoryMock.ClearSubstitute();
        httpContextAccessorMock.ClearSubstitute();
        searchApiOptionsMock.ClearSubstitute();
    }

    [Test]
    public async Task IsAliveExecutesRequest()
    {
        searchApiOptions.BaseUrl = HttpBaseAddress;

        const string mockResponse = "[{\"status\":\"UP\",\"serviceName\":\"Result\",\"serviceStatusTime\":\"2022-07-06T14:44:58.30204Z\"}]";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockResponse, Encoding.UTF8, "application/json")
        });

        using var httpClient = new HttpClient(httpMessageHandlerMock);
#pragma warning disable CA2000
        httpClientFactoryMock.CreateClient().Returns(httpClient);
#pragma warning restore CA2000

        var result = await searchApiClient.IsAlive();

        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo("http://fake.io/api/systemstatus"));
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SearchExecutesSearchRequest()
    {
        const string searchResult = "{\"EntryIds\": [1,2,3], \"TotalEntryCount\": 13}";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(searchResult, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = fixture.Create<SearchFilterParameter>();
        searchFilterParameter.EntryIds = null;

        var searchFields = fixture.Create<SearchFields>();
        var result = await searchApiClient.Search(searchFilterParameter, searchFields);

        var expectedQueryString = BuildQueryString(searchFilterParameter, searchFields);

        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnSearch}?{expectedQueryString}"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(result.TotalEntryCount, Is.EqualTo(13));
    }

    [Test]
    public async Task SearchExecutesSearchRequestWithAuthorizationHeader()
    {
        const string searchResult = "{\"EntryIds\": [1], \"TotalEntryCount\": 1}";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(searchResult, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);
        var httpContext = new DefaultHttpContext();
        const string expectedAuthorizeValue = "superauthtoken";
        httpContext.Request.Headers["Authorization"] = expectedAuthorizeValue;
        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var searchFilterParameter = fixture.Create<SearchFilterParameter>();
        searchFilterParameter.EntryIds = null;

        var searchFields = fixture.Create<SearchFields>();
        var result = await searchApiClient.Search(searchFilterParameter, searchFields);

        var expectedQueryString = BuildQueryString(searchFilterParameter, searchFields);

        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnSearch}?{expectedQueryString}"));
        if (httpMessageHandlerMock.RequestMessage.Headers.TryGetValues("Authorization", out var authorizeValue))
        {
            Assert.That(authorizeValue.First(), Is.EqualTo(expectedAuthorizeValue));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.EntryIds, Is.EquivalentTo(new[] { 1 }));
            Assert.That(result.TotalEntryCount, Is.EqualTo(1));
        }
        else
        {
            Assert.Fail("Authorization header not present");
        }
    }

    [Test]
    public async Task SearchExecutesEmptySearchRequest()
    {
        const string responseString = "{\"EntryIds\": [], \"TotalEntryCount\": 0}";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = new SearchFilterParameter();
        var searchFields = new SearchFields();
        var result = await searchApiClient.Search(searchFilterParameter, searchFields);

        var expectedQueryString = BuildQueryString(searchFilterParameter, searchFields);
        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnSearch}?{expectedQueryString}"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.EntryIds, Is.Empty);
        Assert.That(result.TotalEntryCount, Is.Zero);
    }

    [TestCase("makes no sense")]
    [TestCase("")]
    public void SearchWithUnexpectedResponseThrows(string responseString)
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = new SearchFilterParameter();
        var searchFields = new SearchFields();
        Assert.That(() => searchApiClient.Search(searchFilterParameter, searchFields), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void SearchWithEmptyResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = new SearchFilterParameter();
        var searchFields = new SearchFields();
        Assert.That(() => searchApiClient.Search(searchFilterParameter, searchFields), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void SearchWithErrorResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = new SearchFilterParameter();
        var searchFields = new SearchFields();
        Assert.That(() => searchApiClient.Search(searchFilterParameter, searchFields), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void SearchWithBadRequestResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchFilterParameter = new SearchFilterParameter();
        var searchFields = new SearchFields();
        Assert.That(() => searchApiClient.Search(searchFilterParameter, searchFields), Throws.InstanceOf<SearchException>());
    }

    [Test]
    public async Task RecommendExecutesRecommendRequest()
    {
        const string responseString = "[\"one\",\"two\",\"three\"]";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        var languageIds = fixture.CreateMany<int>().ToArray();
        var result = (await searchApiClient.Recommend(searchPhrase, languageIds)).ToArray();

        var expectedQueryString = BuildQueryString(searchPhrase, languageIds);
        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnRecommend}?{expectedQueryString}"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(new[] { "one", "two", "three" }));
    }

    [Test]
    public async Task RecommendExecutesRecommendRequestWithPhrase()
    {
        const string responseString = "[\"one\",\"two\",\"three\"]";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        var result = (await searchApiClient.Recommend(searchPhrase)).ToArray();

        var expectedQueryString = BuildQueryString(searchPhrase);
        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnRecommend}?{expectedQueryString}"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(new[] { "one", "two", "three" }));
    }

    [Test]
    public async Task RecommendExecutesRecommendRequestWithAuthorizationHeader()
    {
        const string responseString = "[]";
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);
        var httpContext = new DefaultHttpContext();
        const string expectedAuthorizeValue = "superauthtoken";
        httpContext.Request.Headers["Authorization"] = expectedAuthorizeValue;
        httpContextAccessorMock.HttpContext.Returns(httpContext);

        var searchPhrase = fixture.Create<string>();
        var result = (await searchApiClient.Recommend(searchPhrase)).ToArray();

        var expectedQueryString = BuildQueryString(searchPhrase);
        Assert.That(httpMessageHandlerMock.RequestMessage, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo($"{HttpBaseAddress}{searchApiOptions.UrnRecommend}?{expectedQueryString}"));
        if (httpMessageHandlerMock.RequestMessage.Headers.TryGetValues("Authorization", out var authorizeValue))
        {
            Assert.That(authorizeValue.First(), Is.EqualTo(expectedAuthorizeValue));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EquivalentTo(Array.Empty<string>()));
        }
        else
        {
            Assert.Fail("Authorization header not present");
        }
    }

    [TestCase("makes no sense")]
    [TestCase("")]
    public void RecommendWithUnexpectedResponseThrows(string responseString)
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        Assert.That(() => searchApiClient.Recommend(searchPhrase), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void RecommendWithEmptyResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        Assert.That(() => searchApiClient.Recommend(searchPhrase), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void RecommendWithErrorResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        Assert.That(() => searchApiClient.Recommend(searchPhrase), Throws.InstanceOf<SearchApiException>());
    }

    [Test]
    public void RecommendWithBadRequestResponseThrows()
    {
        using var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        });
        SetupHttpClientFactory(httpMessageHandlerMock);

        var searchPhrase = fixture.Create<string>();
        Assert.That(() => searchApiClient.Recommend(searchPhrase), Throws.InstanceOf<SearchException>());
    }

    private static string BuildQueryString(SearchFilterParameter searchFilterParameter, SearchFields searchFields)
    {
        var stringBuilder = new StringBuilder();

        if (searchFilterParameter.Phrase != null)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Phrase)}={searchFilterParameter.Phrase}&");
        }

        if (searchFilterParameter.SourceLanguageIds != null)
        {
            foreach (var id in searchFilterParameter.SourceLanguageIds)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.SourceLanguageIds)}={id}&");
            }
        }

        if (searchFilterParameter.Offices != null)
        {
            foreach (var id in searchFilterParameter.Offices)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Offices)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.OfficesPriority)}={searchFilterParameter.OfficesPriority}&");

        if (searchFilterParameter.LenochClassifications != null)
        {
            foreach (var id in searchFilterParameter.LenochClassifications)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.LenochClassifications)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.LenochClassificationPriority)}={searchFilterParameter.LenochClassificationPriority}&");

        if (searchFilterParameter.Collections != null)
        {
            foreach (var id in searchFilterParameter.Collections)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Collections)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CollectionsPriority)}={searchFilterParameter.CollectionsPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CollectionsExclude)}={searchFilterParameter.CollectionsExclude}&");

        if (searchFilterParameter.SubjectDomains != null)
        {
            foreach (var id in searchFilterParameter.SubjectDomains)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.SubjectDomains)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.SubjectDomainsPriority)}={searchFilterParameter.SubjectDomainsPriority}&");

        if (searchFilterParameter.RelatedCollections != null)
        {
            foreach (var id in searchFilterParameter.RelatedCollections)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(searchFilterParameter.RelatedCollections)}={id}&");
            }
        }

        if (searchFilterParameter.Glossaries != null)
        {
            foreach (var id in searchFilterParameter.Glossaries)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Glossaries)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.GlossariesExclude)}={searchFilterParameter.GlossariesExclude}&");

        if (searchFilterParameter.Status != null)
        {
            foreach (var id in searchFilterParameter.Status)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Status)}={id}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.StatusPriority)}={searchFilterParameter.StatusPriority}&");

        if (searchFilterParameter.AdminInfos != null)
        {
            foreach (var adminInfo in searchFilterParameter.AdminInfos)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.AdminInfos)}={adminInfo}&");
            }
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.AdminInfosPriority)}={searchFilterParameter.AdminInfosPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.AdminInfosExclude)}={searchFilterParameter.AdminInfosExclude}&");

        if (searchFilterParameter.CommentStatus != null)
        {
            foreach (var selectedOption in searchFilterParameter.CommentStatus)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CommentStatus)}={selectedOption}&");
            }
        }

        if (searchFilterParameter.CreatedBy != null)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CreatedBy)}={searchFilterParameter.CreatedBy}&");
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CreatedByPriority)}={searchFilterParameter.CreatedByPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.CreatedByExclude)}={searchFilterParameter.CreatedByExclude}&");

        if (searchFilterParameter.ModifiedBy != null)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedBy)}={searchFilterParameter.ModifiedBy}&");
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedByPriority)}={searchFilterParameter.ModifiedByPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedByExclude)}={searchFilterParameter.ModifiedByExclude}&");

        if (searchFilterParameter.ModifiedFrom.HasValue)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedFrom)}={UrlEncoder.Default.Encode(searchFilterParameter.ModifiedFrom.Value.ToString("O"))}&");
        }

        if (searchFilterParameter.ModifiedTo.HasValue)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedTo)}={UrlEncoder.Default.Encode(searchFilterParameter.ModifiedTo.Value.ToString("O"))}&");
        }

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedPeriodPriority)}={searchFilterParameter.ModifiedPeriodPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ModifiedPeriodExclude)}={searchFilterParameter.ModifiedPeriodExclude}&");

        if (searchFilterParameter.Reliability != null)
        {
            foreach (var id in searchFilterParameter.Reliability)
            {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Reliability)}={id}&");
            }
        }
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.ReliabilityPriority)}={searchFilterParameter.ReliabilityPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.Deleted)}={searchFilterParameter.Deleted}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.DeletedPriority)}={searchFilterParameter.DeletedPriority}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.HasMissingSource)}={searchFilterParameter.HasMissingSource}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.PageIndex)}={searchFilterParameter.PageIndex}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFilterParameter.PageSize)}={searchFilterParameter.PageSize}&");

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Terminus)}={searchFields.Terminus}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Name)}={searchFields.Name}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Abbreviation)}={searchFields.Abbreviation}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Phraseology)}={searchFields.Phraseology}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Definition)}={searchFields.Definition}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Note)}={searchFields.Note}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Context)}={searchFields.Context}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Source)}={searchFields.Source}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Metadata)}={searchFields.Metadata}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Country)}={searchFields.Country}&");
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(SearchFields.Comment)}={searchFields.Comment}");
        return stringBuilder.ToString();
    }

    private static string BuildQueryString(string searchPhrase, IEnumerable<int> languageIds = null)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(CultureInfo.InvariantCulture, $"{nameof(searchPhrase)}={searchPhrase}");
        if (languageIds != null)
        {
            stringBuilder.Append('&');
            stringBuilder.Append(string.Join("&", languageIds.Select(id => $"{nameof(languageIds)}={id}")));
        }

        return stringBuilder.ToString();
    }

    private void SetupHttpClientFactory(HttpMessageHandlerMock httpMessageHandlerMock)
    {
        #pragma warning disable CA2000
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri(HttpBaseAddress)
        };
#pragma warning restore CA2000
        httpClientFactoryMock.CreateClient(SearchApiOptions.HttpClientName).Returns(httpClient);
    }

    private class HttpMessageHandlerMock : DelegatingHandler
    {
        private readonly HttpResponseMessage response;

        public HttpRequestMessage RequestMessage { get; private set; }

        public HttpMessageHandlerMock(HttpResponseMessage responseMessage)
        {
            response = responseMessage;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            return await Task.FromResult(response);
        }
    }
}
