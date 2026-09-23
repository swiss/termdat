using System;
using System.Globalization;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Common.Resources;

namespace Bk.Termdat.Business.Public.Api.Services;

public class BackendTextHelperService : IBackendTextHelperService
{

    public string GetReliabilityText(Reliability reliability, string isoCode)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(isoCode);

        return reliability switch
        {
            Reliability.Tentative => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReliabilityTentative), cultureInfo),
            Reliability.Translated => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReliabilityTranslated), cultureInfo),
            Reliability.Verified => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReliabilityVerified), cultureInfo),
            Reliability.Approved => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReliabilityApproved), cultureInfo),
            Reliability.FirstDraft => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReliabilityFirstDraft), cultureInfo),
            _ => throw new ArgumentOutOfRangeException(nameof(reliability), reliability, null)
        };
    }

    public string GetStatusText(Status status, string isoCode)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(isoCode);

        return status switch
        {
            Status.Validated => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.StatusValidated), cultureInfo),
            Status.InProgress => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.StatusInProgress), cultureInfo),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public string GetReferenceTypeText(ReferenceType referenceType, string isoCode)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(isoCode);

        return referenceType switch
        {
            ReferenceType.Specific => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypeSpecific), cultureInfo),
            ReferenceType.General => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypeGeneral), cultureInfo),
            ReferenceType.Antonym => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypeAntonym), cultureInfo),
            ReferenceType.Whole => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypeWhole), cultureInfo),
            ReferenceType.Part => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypePart), cultureInfo),
            ReferenceType.None => BackendTexts.ResourceManager.GetString(nameof(BackendTexts.ReferenceTypeNone), cultureInfo),
            _ => throw new ArgumentOutOfRangeException(nameof(referenceType), referenceType, null)
        };
    }
}
