using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Common.Resources;
using Bk.Termdat.CrossCutting.Exceptions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using DomainModels = Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Business.Tests.Services;

[TestFixture]
public class OfficeServiceTests
{
    private OfficeService officeService;
    private IOfficeRepository officeRepositoryMock;
    private IMapper mapperMock;
    private IEnumerable<DomainModels.Office> offices;
    private IIdentityService identityServiceMock;
    private IUnitOfWork unitOfWorkMock;
    private ILanguageDescriptionMapper languageDescriptionMapperMock;
    private OfficeCreate officeCreate;
    private OfficeDetail officeDetail;
    private OfficeUpdate officeUpdate;
    private IEnumerable<OfficeWithEntryCount> officesWithEntryCount;
    private readonly string currentEmail = "me@me.com";

    [SetUp]
    public void SetUp()
    {
        mapperMock = Substitute.For<IMapper>();
        officeRepositoryMock = Substitute.For<IOfficeRepository>();
        identityServiceMock = Substitute.For<IIdentityService>();
        unitOfWorkMock = Substitute.For<IUnitOfWork>();
        languageDescriptionMapperMock = Substitute.For<ILanguageDescriptionMapper>();
        officeService = new OfficeService(mapperMock, officeRepositoryMock, identityServiceMock, unitOfWorkMock,
             languageDescriptionMapperMock);

        offices = new List<DomainModels.Office>
        {
            new()
            {
                Id = 1,
                Code = "ABC123",
                Entries = new List<DomainModels.Entry>()
            },
            new()
            {
                Id = 2,
                Code = "DEF456",
            }
        };
        officeCreate = new OfficeCreate
        {
            Code = "Code1",
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionIt = "it",
            DescriptionEn = "en",
            DescriptionRm = "rm"
        };
        officeUpdate = new OfficeUpdate
        {
            Id = 1,
            Code = "abc45",
            DescriptionDe = "de",
            DescriptionFr = "fr",
            DescriptionEn = "en",
            DescriptionRm = "rm",
            DescriptionIt = "it"
        };
        officeDetail = new OfficeDetail
        {
            Id = 1,
            Code = officeCreate.Code,
            CreatedBy = currentEmail,
            ModifiedBy = currentEmail,
            Name = "name1",
            EntryCount = 2
        };
        officesWithEntryCount = offices.Select(o => new OfficeWithEntryCount
        {
            Office = o,
            EntryCount = o.Id == 1 ? 12 : 0
        });

        identityServiceMock.GetCurrentEmail().Returns(currentEmail);

        officeRepositoryMock.GetById(1).Returns(offices.First());
        officeRepositoryMock.GetAllDetails().Returns(officesWithEntryCount);

        mapperMock.Map<DomainModels.Office>(officeCreate).Returns(offices.First());
        mapperMock.Map<OfficeDetail>(offices.First()).Returns(officeDetail);
    }

    [TearDown]
    public void TearDown()
    {
        mapperMock.ClearSubstitute();
        officeRepositoryMock.ClearSubstitute();
        identityServiceMock.ClearSubstitute();
        unitOfWorkMock.ClearSubstitute();
    }

    [Test]
    public async Task CreateShouldReturnOfficeDetail()
    {
        var result = await officeService.Create(officeCreate);
        await officeRepositoryMock.Received().Create(offices.First());
        await unitOfWorkMock.Received().SaveChangesAsync();
        Assert.That(result, Is.EqualTo(officeDetail));
    }

    [Test]
    public async Task GetAllCallsRepository()
    {
        officeRepositoryMock.GetAll().Returns(offices);
        var mappedOffices = new List<OfficeSummary>();
        mapperMock.Map<IEnumerable<OfficeSummary>>(offices).Returns(mappedOffices);

        var result = await officeService.GetAll();

        Assert.That(result, Is.EqualTo(mappedOffices));
    }

    [Test]
    public async Task GetDetailsShouldReturnOfficeUpdate()
    {
        mapperMock.Map<OfficeUpdate>(offices.First()).Returns(officeUpdate);

        var result = await officeService.GetDetails(1);
        Assert.That(result, Is.EqualTo(officeUpdate));
    }

    [Test]
    public async Task GetDetailsShouldThrowEntityNotFoundException()
    {
        officeRepositoryMock.GetById(0).Returns(Task.FromResult<DomainModels.Office>(null));
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await officeService.GetDetails(0));

        await officeRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task GetAllDetailsShouldReturnCollectionDetails()
    {
        var officeDetails = new List<OfficeDetail> {
            new()
            {
                Code = offices.First().Code,
                EntryCount = 12
            },
             new()
             {
                Code = offices.Last().Code,
                EntryCount = 0
            }
        };
        mapperMock.Map<IEnumerable<OfficeDetail>>(officesWithEntryCount).Returns(officeDetails);

        var result = (await officeService.GetAllDetails()).ToList();
        Assert.That(result, Is.EqualTo(officeDetails));
    }

    [Test]
    public async Task DeleteWhenOfficeHasNoEntries()
    {
        await officeService.Delete(1);

        await officeRepositoryMock.Received(1).GetById(Arg.Is(1));
        officeRepositoryMock.GetEntryCountById(1).Returns(0);
        officeRepositoryMock.Received(1).Delete(Arg.Any<DomainModels.Office>());
        await unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task DeleteThrowsWhenOfficeHasEntries()
    {
        officeRepositoryMock.GetById(1).Returns(new DomainModels.Office
        {
            Id = 1,
            Code = "OFFICE1",
            Entries = new List<DomainModels.Entry>
            {
                new()
            }
        });
        officeRepositoryMock.GetEntryCountById(1).Returns(1);

        async Task Act() => await officeService.Delete(1);

        Assert.ThrowsAsync<BusinessValidationException>(Act, BackendTexts.EntryOfficeDeleteFailedHasEntries);
        await officeRepositoryMock.Received(1).GetById(1);
        officeRepositoryMock.DidNotReceiveWithAnyArgs().Delete(Arg.Any<DomainModels.Office>());
    }

    [Test]
    public async Task UpdateShouldThrowEntityNotFoundException()
    {
        officeUpdate.Id = 0;
        officeRepositoryMock.GetById(0).Returns(Task.FromResult<DomainModels.Office>(null));

        Assert.ThrowsAsync<EntityNotFoundException>(async () => await officeService.Update(officeUpdate));
        await officeRepositoryMock.Received().GetById(0);
    }

    [Test]
    public async Task UpdateShouldReturnOfficeDetail()
    {
        var office = offices.First();
        mapperMock.Map<DomainModels.Office>(officeUpdate).Returns(office);
        mapperMock.Map<OfficeDetail>(office).Returns(officeDetail);
        officeRepositoryMock.Update(office, office);

        var result = await officeService.Update(officeUpdate);
        Assert.That(result, Is.EqualTo(officeDetail));
    }
}
