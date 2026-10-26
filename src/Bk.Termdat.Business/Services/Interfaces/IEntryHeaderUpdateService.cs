using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;

namespace Bk.Termdat.Business.Services.Interfaces;

public interface IEntryHeaderUpdateService
{
    Task<EntryMultipleUpdateResult> UpdateMandatoryHeaderFields(EntryHeaderUpdate update);
    Task<EntryMultipleUpdateResult> UpdateGlossaries(EntryChipFieldUpdate update);
    Task<EntryMultipleUpdateResult> UpdateRelatedCollections(EntryChipFieldUpdate update);
    Task<EntryMultipleUpdateResult> UpdateAdminInfos(EntryChipFieldInputUpdate update);
    Task<EntryMultipleUpdateResult> UpdateComment(EntryCommentUpdate update);
    Task<EntryMultipleUpdateResult> UpdateConceptSystemPosition(EntryConceptSystemPositionUpdate update);
    Task<EntrySourceUpdateResult> UpdateSource(EntrySourceUpdate update);
    Task<EntryContentMoveResult> MoveContent(EntryContentMove update);
}
