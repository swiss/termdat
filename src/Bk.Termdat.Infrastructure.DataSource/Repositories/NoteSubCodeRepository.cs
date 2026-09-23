using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class NoteSubCodeRepository : INoteSubCodeRepository
{
    private readonly DataContext context;

    public NoteSubCodeRepository(DataContext context)
    {
        this.context = context;
    }

    public IReadOnlyCollection<NoteSubCodeTranslation> GetTranslations()
    {
        return context.NoteSubCodeTranslations
           .Include(x => x.NoteSubCode)
           .Include(x => x.Language)
           .AsNoTracking()
           .ToList();
    }
}
