using System.Collections.Generic;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class ClassificationSummaryExamples : IExamplesProvider<IEnumerable<ClassificationSummary>>
{
    public IEnumerable<ClassificationSummary> GetExamples()
    {
        return new[]
        {
            new ClassificationSummary
            {
                Id = 12000,
                Code = "BAUW",
                Text = "BAUWESEN"
            },
            new ClassificationSummary
            {
                Id = 12001,
                Text = "BILDUNG",
                Code = "BILD"
            },
            new ClassificationSummary
            {
                Id = 12002,
                Text = "FINANZWESEN",
                Code = "FINA"
            },
            new ClassificationSummary
            {
                Id = 12003,
                Text = "INFORMATIONSTECHNOLOGIE",
                Code = "INFO"
            },
            new ClassificationSummary
            {
                Id = 12004,
                Text = "INTERNATIONALE ORGANISATIONEN",
                Code = "INTE"
            },
            new ClassificationSummary
            {
                Id = 12005,
                Text = "KOMMUNIKATION",
                Code = "KOMM"
            },
            new ClassificationSummary
            {
                Id = 12006,
                Text = "LANDWIRTSCHAFT",
                Code = "LAND"
            },
            new ClassificationSummary
            {
                Id = 12007,
                Text = "MEDIZIN",
                Code = "MEDI"
            },
            new ClassificationSummary
            {
                Id = 12008,
                Text = "NATIONALE ORGANISATIONEN",
                Code = "NATI"
            },
            new ClassificationSummary
            {
                Id = 12009,
                Text = "ÖFFENTLICHE SICHERHEIT",
                Code = "OFFE"
            },
            new ClassificationSummary
            {
                Id = 12010,
                Text = "POLITIK",
                Code = "POLI"
            },
            new ClassificationSummary
            {
                Id = 12011,
                Text = "RECHT",
                Code = "RECH"
            },
            new ClassificationSummary
            {
                Id = 12012,
                Text = "SOZIALE SICHERHEIT",
                Code = "SOZI"
            },
            new ClassificationSummary
            {
                Id = 12013,
                Text = "SPORT",
                Code = "SPOR"
            },
        };
    }
}
