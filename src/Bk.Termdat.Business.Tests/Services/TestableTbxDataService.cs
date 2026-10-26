using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.Extensions.Options;
using Swiss.FCh.Utils.Services;

namespace Bk.Termdat.Business.Tests.Services;

public class TestableTbxDataService : TbxDataService
{
    public TestableTbxDataService(
        IOptions<FileImportOptions> importOptions,
        ILanguageRepository languageRepository,
        IEntryRepository entryRepository,
        IEntryService entryService,
        IEmailService emailService,
        IOptions<EmailOptions> emailOptions) : base(
            importOptions,
            null,
            languageRepository,
            entryRepository,
            entryService,
            emailService,
            emailOptions)
    {
    }

    public static string AdjustLinkStructureAccessor(string source)
    {
        return AdjustLinkStructure(source);
    }

    public async Task<IList<IEntryModification>> ComposeEntryModificationItemsAccessor(XDocument document,
        int entryOfficeId, int entryCollectionId, Status status,
        Reliability reliability, IList<int> subjectIds, IList<int> domainIds, IList<int> glossaryIds, IList<AdminInfoSummary> adminInfos,
        string comment)
    {
        return await ComposeEntryModificationItems(document, entryOfficeId, entryCollectionId,
            status, reliability, subjectIds, domainIds, glossaryIds, adminInfos, comment);
    }
}
