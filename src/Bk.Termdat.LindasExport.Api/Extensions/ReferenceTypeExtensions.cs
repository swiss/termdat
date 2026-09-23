using Bk.Termdat.LindasExport.Api.Constants;
using Bk.Termdat.LindasExport.Api.Models;

namespace Bk.Termdat.LindasExport.Api.Extensions;

public static class ReferenceTypeExtensions
{
    public static string GetUri(this ReferenceType referenceType)
    {
        return referenceType switch
        {
            ReferenceType.General => Uris.ReferenceGeneral,
            ReferenceType.Specific => Uris.ReferenceSpecific,
            ReferenceType.Antonym => Uris.ReferenceAntonym,
            ReferenceType.Whole => Uris.ReferenceWhole,
            ReferenceType.Part => Uris.ReferencePart,
            ReferenceType.None or _ => Uris.ReferenceNone
        };
    }

    public static ReferenceType ToSparqlReferenceType(this Business.Domain.Models.ReferenceType referenceType)
    {
        return referenceType switch
        {
            Business.Domain.Models.ReferenceType.None => ReferenceType.None,
            Business.Domain.Models.ReferenceType.General => ReferenceType.General,
            Business.Domain.Models.ReferenceType.Specific => ReferenceType.Specific,
            Business.Domain.Models.ReferenceType.Antonym => ReferenceType.Antonym,
            Business.Domain.Models.ReferenceType.Whole => ReferenceType.Whole,
            Business.Domain.Models.ReferenceType.Part => ReferenceType.Part,
            _ => ReferenceType.None
        };
    }
}
