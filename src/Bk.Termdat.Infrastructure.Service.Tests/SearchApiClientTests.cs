using System;
using System.Collections.Generic;
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
public class SearchApiClientTests
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockResponse, Encoding.UTF8, "application/json")
        });

        var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClientFactoryMock.CreateClient().Returns(httpClient);

        var result = await searchApiClient.IsAlive();

        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri, Is.Not.Null);
        Assert.That(httpMessageHandlerMock.RequestMessage.RequestUri.ToString(), Is.EqualTo("http://fake.io/api/systemstatus"));
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SearchExecutesSearchRequest()
    {
        const string searchResult = "{\"EntryIds\": [1,2,3], \"TotalEntryCount\": 13}";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(searchResult, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);
        var httpContext = new DefaultHttpContext();
        const string expectedAuthorizeValue = "superauthtoken";
        httpContext.Request.Headers.Add("Authorization", expectedAuthorizeValue);
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseString, Encoding.UTF8, "application/json")
        });
        SetupHttpClientFactory(httpMessageHandlerMock);
        var httpContext = new DefaultHttpContext();
        const string expectedAuthorizeValue = "superauthtoken";
        httpContext.Request.Headers.Add("Authorization", expectedAuthorizeValue);
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(new HttpResponseMessage
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
            stringBuilder.Append($"{nameof(SearchFilterParameter.Phrase)}={searchFilterParameter.Phrase}&");
        }

        if (searchFilterParameter.SourceLanguageIds != null)
        {
            foreach (var id in searchFilterParameter.SourceLanguageIds)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.SourceLanguageIds)}={id}&");
            }
        }

        if (searchFilterParameter.Offices != null)
        {
            foreach (var id in searchFilterParameter.Offices)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.Offices)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.OfficesPriority)}={searchFilterParameter.OfficesPriority}&");

        if (searchFilterParameter.LenochClassifications != null)
        {
            foreach (var id in searchFilterParameter.LenochClassifications)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.LenochClassifications)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.LenochClassificationPriority)}={searchFilterParameter.LenochClassificationPriority}&");

        if (searchFilterParameter.Collections != null)
        {
            foreach (var id in searchFilterParameter.Collections)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.Collections)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.CollectionsPriority)}={searchFilterParameter.CollectionsPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.CollectionsExclude)}={searchFilterParameter.CollectionsExclude}&");

        if (searchFilterParameter.SubjectDomains != null)
        {
            foreach (var id in searchFilterParameter.SubjectDomains)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.SubjectDomains)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.SubjectDomainsPriority)}={searchFilterParameter.SubjectDomainsPriority}&");

        if (searchFilterParameter.RelatedCollections != null)
        {
            foreach (var id in searchFilterParameter.RelatedCollections)
            {
                stringBuilder.Append($"{nameof(searchFilterParameter.RelatedCollections)}={id}&");
            }
        }

        if (searchFilterParameter.Glossaries != null)
        {
            foreach (var id in searchFilterParameter.Glossaries)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.Glossaries)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.GlossariesExclude)}={searchFilterParameter.GlossariesExclude}&");

        if (searchFilterParameter.Status != null)
        {
            foreach (var id in searchFilterParameter.Status)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.Status)}={id}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.StatusPriority)}={searchFilterParameter.StatusPriority}&");

        if (searchFilterParameter.AdminInfos != null)
        {
            foreach (var adminInfo in searchFilterParameter.AdminInfos)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.AdminInfos)}={adminInfo}&");
            }
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.AdminInfosPriority)}={searchFilterParameter.AdminInfosPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.AdminInfosExclude)}={searchFilterParameter.AdminInfosExclude}&");

        if (searchFilterParameter.CommentStatus != null)
        {
            foreach (var selectedOption in searchFilterParameter.CommentStatus)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.CommentStatus)}={selectedOption}&");
            }
        }

        if (searchFilterParameter.CreatedBy != null)
        {
            stringBuilder.Append($"{nameof(SearchFilterParameter.CreatedBy)}={searchFilterParameter.CreatedBy}&");
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.CreatedByPriority)}={searchFilterParameter.CreatedByPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.CreatedByExclude)}={searchFilterParameter.CreatedByExclude}&");

        if (searchFilterParameter.ModifiedBy != null)
        {
            stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedBy)}={searchFilterParameter.ModifiedBy}&");
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedByPriority)}={searchFilterParameter.ModifiedByPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedByExclude)}={searchFilterParameter.ModifiedByExclude}&");

        if (searchFilterParameter.ModifiedFrom.HasValue)
        {
            stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedFrom)}={UrlEncoder.Default.Encode(searchFilterParameter.ModifiedFrom.Value.ToString("O"))}&");
        }

        if (searchFilterParameter.ModifiedTo.HasValue)
        {
            stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedTo)}={UrlEncoder.Default.Encode(searchFilterParameter.ModifiedTo.Value.ToString("O"))}&");
        }

        stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedPeriodPriority)}={searchFilterParameter.ModifiedPeriodPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.ModifiedPeriodExclude)}={searchFilterParameter.ModifiedPeriodExclude}&");

        if (searchFilterParameter.Reliability != null)
        {
            foreach (var id in searchFilterParameter.Reliability)
            {
                stringBuilder.Append($"{nameof(SearchFilterParameter.Reliability)}={id}&");
            }
        }
        stringBuilder.Append($"{nameof(SearchFilterParameter.ReliabilityPriority)}={searchFilterParameter.ReliabilityPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.Deleted)}={searchFilterParameter.Deleted}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.DeletedPriority)}={searchFilterParameter.DeletedPriority}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.HasMissingSource)}={searchFilterParameter.HasMissingSource}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.PageIndex)}={searchFilterParameter.PageIndex}&");
        stringBuilder.Append($"{nameof(SearchFilterParameter.PageSize)}={searchFilterParameter.PageSize}&");

        stringBuilder.Append($"{nameof(SearchFields.Terminus)}={searchFields.Terminus}&");
        stringBuilder.Append($"{nameof(SearchFields.Name)}={searchFields.Name}&");
        stringBuilder.Append($"{nameof(SearchFields.Abbreviation)}={searchFields.Abbreviation}&");
        stringBuilder.Append($"{nameof(SearchFields.Phraseology)}={searchFields.Phraseology}&");
        stringBuilder.Append($"{nameof(SearchFields.Definition)}={searchFields.Definition}&");
        stringBuilder.Append($"{nameof(SearchFields.Note)}={searchFields.Note}&");
        stringBuilder.Append($"{nameof(SearchFields.Context)}={searchFields.Context}&");
        stringBuilder.Append($"{nameof(SearchFields.Source)}={searchFields.Source}&");
        stringBuilder.Append($"{nameof(SearchFields.Metadata)}={searchFields.Metadata}&");
        stringBuilder.Append($"{nameof(SearchFields.Country)}={searchFields.Country}&");
        stringBuilder.Append($"{nameof(SearchFields.Comment)}={searchFields.Comment}");
        return stringBuilder.ToString();
    }

    private static string BuildQueryString(string searchPhrase, IEnumerable<int> languageIds = null)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"{nameof(searchPhrase)}={searchPhrase}");
        if (languageIds != null)
        {
            stringBuilder.Append('&');
            stringBuilder.Append(string.Join("&", languageIds.Select(id => $"{nameof(languageIds)}={id}")));
        }

        return stringBuilder.ToString();
    }

    private void SetupHttpClientFactory(HttpMessageHandlerMock httpMessageHandlerMock)
    {
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri(HttpBaseAddress)
        };
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
