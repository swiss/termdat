using System.Collections.Generic;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class LenochClassificationSubjectDomainMappingRepository : ILenochClassificationSubjectDomainMappingRepository
{
    private readonly DataContext context;

    public LenochClassificationSubjectDomainMappingRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<LenochClassificationSubjectDomainMapping>> GetAll()
    {
        return await context.LenochClassificationSubjectDomainMappings
            .Include(m => m.Subject)
                .ThenInclude(s => s.Translations)
                    .ThenInclude(st => st.Language)
            .Include(m => m.Domain)
                .ThenInclude(d => d.Translations)
                    .ThenInclude(dt => dt.Language)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }
}
