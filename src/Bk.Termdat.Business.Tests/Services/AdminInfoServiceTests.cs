using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Models;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
internal class AdminInfoServiceTests
{
    private AdminInfoService adminInfoService;
    private IMapper mapperMock;
    private IAdminInfoRepository adminInfoRepositoryMock;
    private IEntryRepository entryRepositoryMock;
    private ISearchQueueClient searchQueueClientMock;
    private AdminInfo adminInfo;
    private AdminInfoSummary adminInfoSummary;
    private AdminInfoCreate adminInfoCreate;
    private AdminInfoUpdate adminInfoUpdate;
    private AdminInfoDetail adminInfoDetail;
    private IEnumerable<AdminInfo> adminInfos;
    private IEnumerable<AdminInfoSummary> adminInfoSummaries;
    private IEnumerable<AdminInfoWithEntryCount> adminInfosWithEntryCount;
    private readonly IUnitOfWork unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityService identityServiceMock = Substitute.For<IIdentityService>();
    private const string CurrentEmail = "test@test.ch";
    private const string Code = "AdminInfo 1";

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        adminInfoRepositoryMock = Substitute.For<IAdminInfoRepository>();
        entryRepositoryMock = Substitute.For<IEntryRepository>();
        searchQueueClientMock = Substitute.For<ISearchQueueClient>();
        identityServiceMock.GetCurrentEmail().ReturnsForAnyArgs(CurrentEmail);

        adminInfo = new AdminInfo
        {
            Id = 1,
            Code = "AdminInfo 1",
            Created = DateTime.UtcNow,
            CreatedBy = "pp",
            Modified = DateTime.UtcNow,
            ModifiedBy = "pp",
        };

        adminInfoSummary = new AdminInfoSummary
        {
            Id = 1,
            Code = "AdminInfo 1"
        };

        adminInfoCreate = new AdminInfoCreate
        {
            Code = "AdminInfo 1",
            Description = "Description 1",
        };

        adminInfoUpdate = new AdminInfoUpdate
        {
            Id = 1,
            Code = "AdminInfo 1",
            Description = "Description 1",
        };

        adminInfoDetail = new AdminInfoDetail
        {
            Id = 1,
            Code = "AdminInfo 1",
            Created = DateTime.UtcNow,
            CreatedBy = "pp",
            Modified = DateTime.UtcNow,
            ModifiedBy = "pp",
            EntryCount = 2
        };

        adminInfos = new List<AdminInfo>
        {
            adminInfo,
            new() {
                Id = 2,
                Code = "AdminInfo 2",
                Description = "Rosanna",
                Created = DateTime.UtcNow,
                CreatedBy = "pp",
                Modified = DateTime.UtcNow,
                ModifiedBy = "pp",
            }
        };

        adminInfoSummaries = new List<AdminInfoSummary>
        {
            adminInfoSummary,
            new() {
                Id = 2,
                Code = "AdminInfo 2"
            }
        };

        adminInfosWithEntryCount = adminInfos.Select(o => new AdminInfoWithEntryCount
        {
            AdminInfo = o,
            EntryCount = o.Id == 1 ? 12 : 0
        });

        adminInfoRepositoryMock.GetByCode("AdminInfo 1").Returns(adminInfo);
        adminInfoRepositoryMock.GetById(1).Returns(adminInfo);
        adminInfoRepositoryMock.GetAll().Returns(adminInfos);
        adminInfoRepositoryMock.GetAllDetails().Returns(adminInfosWithEntryCount);

        mapperMock.Map<AdminInfo>(adminInfoSummary).Returns(adminInfo);
        mapperMock.Map<AdminInfoDetail>(adminInfo).Returns(adminInfoDetail);
        mapperMock.Map<IEnumerable<AdminInfoSummary>>(adminInfos).Returns(adminInfoSummaries);
        mapperMock.Map<AdminInfo>(adminInfoCreate).Returns(adminInfo);

        adminInfoService = new AdminInfoService(mapperMock, adminInfoRepositoryMock, entryRepositoryMock, searchQueueClientMock, identityServiceMock, unitOfWorkMock, Substitute.For<ILogger<AdminInfoService>>());
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        adminInfoRepositoryMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
        entryRepositoryMock.ClearSubstitute();
        searchQueueClientMock.ClearSubstitute();
    }

    [Test]
    public async Task GetAll_WhenCalled_ShouldReturnAdminInfos()
    {
        var allAdminInfos = await adminInfoService.GetAll();

        await adminInfoRepositoryMock.Received(1).GetAll();
        mapperMock.Received(1).Map<IEnumerable<AdminInfoSummary>>(adminInfos);
        Assert.That(allAdminInfos.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByCode_WhenCalled_ShouldReturnAdminInfoDetail()
    {
        var result = await adminInfoService.GetByCode(Code);

        await adminInfoRepositoryMock.Received(1).GetByCode(Code);
        mapperMock.Received(1).Map<AdminInfoDetail>(adminInfo);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Code, Is.EqualTo(Code));
    }

    [Test]
    public async Task Create_WhenCalled_ShouldReturnAdminInfoDetail()
    {
        var result = await adminInfoService.Create(adminInfoCreate);

        mapperMock.Received().Map<AdminInfo>(adminInfoCreate);

        await adminInfoRepositoryMock.Received().Create(adminInfo);
        await unitOfWorkMock.Received().SaveChangesAsync();
        await adminInfoRepositoryMock.GetById(adminInfo.Id);
        mapperMock.Received().Map<AdminInfoDetail>(adminInfo);

        Assert.That(result, Is.EqualTo(adminInfoDetail));
    }

    [Test]
    public async Task GetOrCreateAdminInfos_WhenExisiting_ShouldNotCallCreate()
    {
        var result = await adminInfoService.GetOrCreateAdminInfos(adminInfoSummary);

        mapperMock.Received(1).Map<AdminInfoDetail>(adminInfo);

        await adminInfoRepositoryMock.Received().GetByCode(Code);

        Assert.That(result, Is.EqualTo(adminInfoDetail));
    }

    [Test]
    public async Task GetAllDetails_WhenCalled_ShouldReturnCollectionDetails()
    {
        var adminInfoDetails = new List<AdminInfoDetail> {
            new()
            {
                Code = adminInfos.First().Code,
                EntryCount = 12
            },
             new()
             {
                Code = adminInfos.Last().Code,
                EntryCount = 0
            }
        };
        mapperMock.Map<IEnumerable<AdminInfoDetail>>(adminInfosWithEntryCount).Returns(adminInfoDetails);

        var result = (await adminInfoService.GetAllDetails()).ToList();
        Assert.That(result, Is.EqualTo(adminInfoDetails));
    }

    [Test]
    public async Task GetDetails_WhenCalled_ShouldReturnAdminInfoUpdate()
    {
        mapperMock.Map<AdminInfoUpdate>(adminInfos.First()).Returns(adminInfoUpdate);

        var result = await adminInfoService.GetDetails(1);
        Assert.That(result, Is.EqualTo(adminInfoUpdate));
    }

    [Test]
    public async Task GetDetails_WhenNotExisting_ShouldThrowEntityNotFoundException()
    {
        adminInfoRepositoryMock.GetById(0).Returns(Task.FromResult<AdminInfo>(null));
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await adminInfoService.GetDetails(0));

        await adminInfoRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task Update_WhenNotExisting_ShouldThrowEntityNotFoundException()
    {
        adminInfoUpdate.Id = 0;
        adminInfoRepositoryMock.GetById(0).Returns(Task.FromResult<AdminInfo>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await adminInfoService.Update(adminInfoUpdate));
        await adminInfoRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task Update_WhenCalled_ShouldReturnAdminInfoDetail()
    {
        var info = adminInfos.First();
        mapperMock.Map<AdminInfo>(adminInfoUpdate).Returns(info);
        mapperMock.Map<AdminInfoDetail>(info).Returns(adminInfoDetail);
        adminInfoRepositoryMock.Update(info, info);

        var result = await adminInfoService.Update(adminInfoUpdate);
        Assert.That(result, Is.EqualTo(adminInfoDetail));
    }

    [Test]
    public async Task Delete_WhenNotExisting_ShouldThrowEntityNotFoundException()
    {
        adminInfoUpdate.Id = 0;
        adminInfoRepositoryMock.GetById(0).Returns(Task.FromResult<AdminInfo>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await adminInfoService.Delete(adminInfoUpdate.Id));
        await adminInfoRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task Delete_WhenCalled_ShouldCallRepository()
    {
        await adminInfoService.Delete(1);

        await adminInfoRepositoryMock.Received(1).GetById(Arg.Is(1));
        adminInfoRepositoryMock.Received(1).Delete(Arg.Any<AdminInfo>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
