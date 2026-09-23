using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Public.Api.Services;

public interface IBackendTextHelperService
{
    string GetReliabilityText(Reliability reliability, string isoCode);
    string GetStatusText(Status status, string isoCode);
    string GetReferenceTypeText(ReferenceType referenceType, string isoCode);
}
