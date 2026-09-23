using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class AdminInfoRepository : IAdminInfoRepository
{
    private readonly DataContext context;

    public AdminInfoRepository(DataContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<AdminInfo>> GetAll()
    {
        return await context.AdminInfos
            .Where(a => a.Code != null)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task<AdminInfo> GetById(int id)
    {
        return await context.AdminInfos
            .SingleOrDefaultAsync(d => d.Id == id);
    }

    public async Task<AdminInfo> GetByCode(string code)
    {
        var allAdminInfos = await GetAll();

        var filteredAdminInfo = allAdminInfos.SingleOrDefault(a => a.Code.ToLowerInvariant().Equals(code.ToLowerInvariant()));

        return filteredAdminInfo;
    }

    public async Task<IEnumerable<AdminInfo>> GetByCodes(IEnumerable<string> codes)
    {
        var adminInfoCodes = codes.ToList();

        return await context.AdminInfos
           .Where(a => adminInfoCodes.Contains(a.Code))
           .ToListAsync();
    }

    public async Task Create(AdminInfo adminInfo)
    {
        await context.AdminInfos.AddAsync(adminInfo);
    }

    public async Task<IEnumerable<AdminInfoWithEntryCount>> GetAllDetails()
    {
        return await context.AdminInfos
            .OrderBy(a => a.Code)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(a => new AdminInfoWithEntryCount
            {
                AdminInfo = a,
                EntryCount = Convert.ToInt32(a.Entries.Count)
            })
            .ToListAsync();
    }

    public void Update(AdminInfo existing, AdminInfo update)
    {
        context.Entry(existing).CurrentValues.SetValues(update);
    }

    public void Delete(AdminInfo adminInfo)
    {
        context.AdminInfos.Remove(adminInfo);
    }
}
