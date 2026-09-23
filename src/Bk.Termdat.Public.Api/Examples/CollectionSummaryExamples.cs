using System.Collections.Generic;
using Bk.Termdat.Business.Public.Api.Resource.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Bk.Termdat.Public.Api.Examples;

public class CollectionSummaryExamples : IExamplesProvider<IEnumerable<CollectionSummary>>
{
    public IEnumerable<CollectionSummary> GetExamples()
    {
        return new[]
        {
            new CollectionSummary
            {
                Id = 10438,
                Code = "ABR18v",
                Text = "Abkürzungen der Titel der Bundeserlasse (aufgehoben)"
            },
            new CollectionSummary
            {
                Id = 90,
                Code = "ABR19",
                Text = "Abkürzungen der Titel der Bundeserlasse"
            },
            new CollectionSummary
            {
                Id = 102,
                Code = "ADF15",
                Text = "Bezeichnungen der Organisationseinheiten der Bundesverwaltung"
            },
            new CollectionSummary
            {
                Id = 10403,
                Code = "AHV18",
                Text = "Terminologie der AHV"
            },
            new CollectionSummary
            {
                Id = 122,
                Code = "ALL09",
                Text = "Terminologie der Allianz Suisse"
            },
            new CollectionSummary
            {
                Id = 123,
                Code = "ALLG14",
                Text = "Punktuelle Einträge"
            },
            new CollectionSummary
            {
                Id = 10381,
                Code = "ANG17",
                Text = "Anglizismen"
            },
            new CollectionSummary
            {
                Id = 11447,
                Code = "APPU19",
                Text = "Terminologie des öffentlichen Beschaffungswesens"
            },
            new CollectionSummary
            {
                Id = 10399,
                Code = "ARB17",
                Text = "Terminologie des schweizerischen Arbeitsrechts"
            }
        };
    }
}
