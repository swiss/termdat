using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Bk.Termdat.Infrastructure.DataSource.Tests.Repositories;

[TestFixture]
internal class AdminInfoRepositoryTests
{
    private AdminInfoRepository adminInfoRepository;
    private DataContext context;

    [SetUp]
    public void SetUp()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        adminInfoRepository = new AdminInfoRepository(context);
    }

    [TearDown]
    public void TearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetAll_WhenCalled_ShouldReturnData()
    {
        var data = (await adminInfoRepository.GetAll()).ToList();
        var adminInfo = data.First();

        Assert.That(data, Is.Not.Null);
        Assert.That(adminInfo.Code, Is.Not.Empty);

        adminInfo = data.Last();

        Assert.That(data, Is.Not.Null);
        Assert.That(adminInfo.Code, Is.Not.Empty);
    }

    [TestCase]
    public async Task GetByCode_WhenCalled_ShouldReturnAdminInfo()
    {
        var data = await adminInfoRepository.GetByCode("Code111");

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Id.Equals(111));
        Assert.That(data.ModifiedBy.Equals("pp"));
    }

    [TestCase]
    public async Task GetByCode_WhenCalledWithDifferentSpelling_ShouldReturnAdminInfo()
    {
        var data = await adminInfoRepository.GetByCode("coDE111");

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Id.Equals(111));
        Assert.That(data.ModifiedBy.Equals("pp"));
    }

    [TestCase]
    public async Task GetByCode_WhenCalledWithNonExistingCode_ShouldReturnNull()
    {
        var data = await adminInfoRepository.GetByCode("CODE777");

        Assert.That(data, Is.Null);
    }

    [TestCase]
    public async Task GetById_WhenCalled_ShouldReturnAdminInfo()
    {
        var data = await adminInfoRepository.GetById(222);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Code.Equals("Code222"));
        Assert.That(data.CreatedBy.Equals("test@test.ch"));
    }

    [TestCase]
    public async Task GetByCodes_When_Called_ShouldReturnData()
    {
        var data = await adminInfoRepository.GetByCodes(new List<string> { "Code333", "Code444" });

        Assert.That(data, Is.Not.Null);
        Assert.That(data.First().Id.Equals(333));
        Assert.That(data.Last().Id.Equals(444));
    }

    [Test]
    public async Task GetAllDetails_WhenCalled_ShouldReturnAdminInfos()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        adminInfoRepository = new AdminInfoRepository(context);

        var data = (await adminInfoRepository.GetAllDetails()).ToList();

        Assert.That(data.Count, Is.EqualTo(9));

        Assert.That(data.First().AdminInfo.Id.Equals(5));
        Assert.That(data.First().EntryCount, Is.EqualTo(1));

        Assert.That(data.Last().AdminInfo.Id.Equals(2));
        Assert.That(data.Last().EntryCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Create_WhenCalled_ShouldAddAdminInfo()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        var createContext = new DataContext(builder.Options);
        var repository = new AdminInfoRepository(createContext);

        var newAdminInfo = new AdminInfo
        {
            Code = "code",
            Description = "description",
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@update.com",
        };

        await repository.Create(newAdminInfo);

        await createContext.SaveChangesAsync();

        Assert.That(newAdminInfo.Id, Is.Not.Default);
    }

    [Test]
    public async Task Update_WhenCalled_ShouldUpdateAdminInfo()
    {
        var databaseAdminInfo = await adminInfoRepository.GetById(111);
        var updatedAdminInfo = new AdminInfo
        {
            Code = "qwe12",
            ConcurrencyToken = databaseAdminInfo.ConcurrencyToken,
            Created = DateTime.UtcNow,
            CreatedBy = "me@me.com",
            Description = "description2",
            Id = databaseAdminInfo.Id,
            Modified = DateTime.UtcNow,
            ModifiedBy = "me@me.kong",
        };

        adminInfoRepository.Update(databaseAdminInfo, updatedAdminInfo);

        await context.SaveChangesAsync();

        var result = await adminInfoRepository.GetById(111);
        Assert.That(result.Code, Is.EqualTo(updatedAdminInfo.Code));
        Assert.That(result.Description, Is.EqualTo(updatedAdminInfo.Description));
        Assert.That(result.Created, Is.EqualTo(updatedAdminInfo.Created));
        Assert.That(result.CreatedBy, Is.EqualTo(updatedAdminInfo.CreatedBy));
        Assert.That(result.Id, Is.EqualTo(updatedAdminInfo.Id));
        Assert.That(result.Modified, Is.EqualTo(updatedAdminInfo.Modified));
        Assert.That(result.ModifiedBy, Is.EqualTo(updatedAdminInfo.ModifiedBy));
    }

    [Test]
    public void Delete_WhenCalled_ShouldRemoveAdminInfo()
    {
        var adminInfoCount = context.AdminInfos.Count();
        var adminInfo = context.AdminInfos.First();

        adminInfoRepository.Delete(adminInfo);
        context.SaveChanges();

        Assert.That(context.AdminInfos.Count(), Is.EqualTo(adminInfoCount - 1));
        Assert.That(context.AdminInfos, Is.Not.Contains(adminInfo));
    }
}
