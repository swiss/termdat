using System;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext dataContext;

    public UnitOfWork(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public int SaveChanges()
    {
        SetChangeTrackingInformation();
        return dataContext.SaveChanges();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // See: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-resilient-entity-framework-core-sql-connections
        var transaction = new ResilientTransaction(dataContext);
        await transaction.ExecuteAsync(action);
    }

    public async Task<int> SaveChangesAsync()
    {
        SetChangeTrackingInformation();
        return await dataContext.SaveChangesAsync();
    }

    private void SetChangeTrackingInformation()
    {
        var entityEntries = dataContext.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged
            && entry.Entity is Entity { ConcurrencyToken: > 0 }).ToList();

        foreach (var entityEntry in entityEntries)
        {
            entityEntry.OriginalValues[nameof(Entity.ConcurrencyToken)] = (entityEntry.Entity as Entity)?.ConcurrencyToken;
        }
    }

    // See: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-resilient-entity-framework-core-sql-connections
    private sealed class ResilientTransaction
    {
        private readonly DbContext context;

        public ResilientTransaction(DbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                await action();
                await transaction.CommitAsync();
            });
        }
    }
}
