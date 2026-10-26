using System;
using AutoMapper;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Bk.Termdat.Business.Public.Api.Services;

namespace Bk.Termdat.Business.Public.Api.Mapping.Resolvers;

public class EntryDetailReliabilityResolver : IValueResolver<Entry, EntryDetail, CodeAndText>
{
    private readonly IBackendTextHelperService backendTextHelperService;

    public EntryDetailReliabilityResolver(IBackendTextHelperService backendTextHelperService)
    {
        this.backendTextHelperService = backendTextHelperService;
    }

    public CodeAndText Resolve(Entry source, EntryDetail destination, CodeAndText destMember, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue(PublicApiMappingProfile.LanguageIsoCode, out var isoCode))
        {
            return new CodeAndText
            {
                Code = source.Reliability.ToString("D"),
                Text = backendTextHelperService.GetReliabilityText(source.Reliability, isoCode.ToString())
            };
        }

        throw new InvalidOperationException($"Cannot map reliability with provided language isocode '{PublicApiMappingProfile.LanguageIsoCode}'.");
    }
}
