using System;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;

namespace Bk.Termdat.Business.Public.Api.Mapping.Resolvers;

public class EntryDetailStatusResolver : IValueResolver<Entry, EntryDetail, CodeAndText>
{
    private readonly IBackendTextHelperService backendTextHelperService;

    public EntryDetailStatusResolver(IBackendTextHelperService backendTextHelperService)
    {
        this.backendTextHelperService = backendTextHelperService;
    }

    public CodeAndText Resolve(Entry source, EntryDetail destination, CodeAndText destMember, ResolutionContext context)
    {
        if (context.Items.TryGetValue(PublicApiMappingProfile.LanguageIsoCode, out var isoCode))
        {
            return new CodeAndText
            {
                Code = source.Status.ToString("D"),
                Text = backendTextHelperService.GetStatusText(source.Status, isoCode.ToString())
            };
        }

        throw new InvalidOperationException($"Cannot map status with provided language isocode '{PublicApiMappingProfile.LanguageIsoCode}'.");
    }
}
