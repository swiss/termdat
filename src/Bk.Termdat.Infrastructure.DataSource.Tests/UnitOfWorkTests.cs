using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests;

[TestFixture]
internal class UnitOfWorkTests
{
    private UnitOfWork uow;
    private DataContext context;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(nameof(UnitOfWorkTests));
        context = new DataContext(builder.Options);

        context.Database.EnsureCreated();
        context.SaveChanges();

        uow = new UnitOfWork(context);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        context.ChangeTracker.Clear();
    }

    [Test]
    public void SaveChanges_ChangesEntriesOnDataContext()
    {
        var entityToAdd = new Business.Domain.Models.Entry()
        {
            Id = 1,
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            AdminInfo = "adminfo",
            Comment = "comment",
            LockedBy = "lockedBy"
        };
        context.Entries.Add(entityToAdd);

        Assert.That(context.Entry(entityToAdd).State, Is.EqualTo(EntityState.Added));

        uow.SaveChanges();

        Assert.That(context.Entry(entityToAdd).State, Is.EqualTo(EntityState.Unchanged));
    }

    [Test]
    public void SaveChanges_ThrowsConcurrencyException_WhenContextHasChanged()
    {
        var remoteEntity = new Business.Domain.Models.Entry()
        {
            Id = 10,
            ConcurrencyToken = 10,
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            AdminInfo = "adminfo",
            Comment = "comment",
            LockedBy = "lockedBy"
        };

        context.Entries.Add(remoteEntity);
        uow.SaveChanges();

        Assert.That(context.Entry(remoteEntity).State, Is.EqualTo(EntityState.Unchanged));

        // this should mock a complete mapping
        var localEntity = new Business.Domain.Models.Entry() { Id = 10, ConcurrencyToken = 9, CreatedBy = "test@test.ch", ModifiedBy = "test@test.ch" };
        remoteEntity.Id = localEntity.Id;
        remoteEntity.ConcurrencyToken = localEntity.ConcurrencyToken;

        Assert.That(context.Entry(remoteEntity).State, Is.EqualTo(EntityState.Modified));

        Assert.Throws<DbUpdateConcurrencyException>(() => uow.SaveChanges());
    }

    [Test]
    public async Task SaveChangesAsync_ChangesEntriesOnDataContext()
    {
        var entityToAdd = new Business.Domain.Models.Entry()
        {
            Id = 2,
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            AdminInfo = "adminfo",
            Comment = "comment",
            LockedBy = "lockedBy"
        };

        context.Entries.Add(entityToAdd);

        Assert.That(context.Entry(entityToAdd).State, Is.EqualTo(EntityState.Added));

        await uow.SaveChangesAsync();

        Assert.That(context.Entry(entityToAdd).State, Is.EqualTo(EntityState.Unchanged));
    }

    [Test]
    public void SaveChangesAsync_ThrowsConcurrencyException_WhenContextHasChanged()
    {
        var remoteEntity = new Business.Domain.Models.Entry()
        {
            Id = 20,
            ConcurrencyToken = 10,
            CreatedBy = "test@test.ch",
            ModifiedBy = "test@test.ch",
            AdminInfo = "adminfo",
            Comment = "comment",
            LockedBy = "lockedBy"
        };

        context.Entries.Add(remoteEntity);
        uow.SaveChanges();

        Assert.That(context.Entry(remoteEntity).State, Is.EqualTo(EntityState.Unchanged));

        // this should mock a complete mapping
        var localEntity = new Business.Domain.Models.Entry() { Id = 20, ConcurrencyToken = 9, CreatedBy = "test@test.ch", ModifiedBy = "test@test.ch" };
        remoteEntity.Id = localEntity.Id;
        remoteEntity.ConcurrencyToken = localEntity.ConcurrencyToken;

        Assert.That(context.Entry(remoteEntity).State, Is.EqualTo(EntityState.Modified));

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await uow.SaveChangesAsync());
    }
}
