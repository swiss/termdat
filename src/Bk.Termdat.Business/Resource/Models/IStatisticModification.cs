using System;

namespace Bk.Termdat.Business.Resource.Models;

public interface IStatisticModification
{
    string SearchText { get; set; }
    string Languages { get; set; }
    string SearchFields { get; set; }
    DateTime DateLastSearch { get; set; }
}
