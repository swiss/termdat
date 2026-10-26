using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting.Exceptions;
using Bk.Termdat.Private.Api.Controllers;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Bk.Termdat.Private.Api.Tests.Controllers;

[TestFixture]
internal class EntriesControllerTests
{
    private readonly Faker<GetDetailQuery> getDetailQueryFaker = new();
    private readonly Faker<EntryCreate> entryCreateFaker = new();
    private readonly Faker<EntryUpdate> entryUpdateFaker = new();
    private readonly Faker<EntryDetail> entryDetailFaker = new();
    private readonly Faker<EntryModificationDetail> entryModificationDetailFaker = new();

    private EntriesController entryController;

    private readonly ILogger<EntriesController> loggerMock = Substitute.For<ILogger<EntriesController>>();
    private readonly IEntryService entryServiceMock = Substitute.For<IEntryService>();
    private readonly IEntryHeaderUpdateService entryHeaderUpdateServiceMock = Substitute.For<IEntryHeaderUpdateService>();
    private GetDetailQuery getDetailQuery;
    private EntryCreate entryCreate;
    private EntryUpdate entryUpdate;
    private EntryHeaderUpdate entryHeaderUpdate;
    private EntryCommentUpdate entryCommentUpdate;
    private EntryConceptSystemPositionUpdate entryConceptSystemPositionUpdate;
    private EntrySourceUpdate entrySourceUpdate;
    private EntryDetail entryDetail;
    private EntryModificationDetail entryModificationDetail;

    [SetUp]
    public void SetUp()
    {
        getDetailQuery = getDetailQueryFaker.Generate();
        entryCreate = entryCreateFaker.Generate();
        entryUpdate = entryUpdateFaker.Generate();
        entryHeaderUpdate = new EntryHeaderUpdate
        {
            Ids = new List<int> { 1, 2 },
            CollectionId = 1,
            OfficeId = 2,
            Reliability = Reliability.Tentative,
            Status = Status.Validated
        };
        entryCommentUpdate = new EntryCommentUpdate
        {
            Ids = new List<int> { 34, 46 },
            Comment = "new comment",
        };
        entryConceptSystemPositionUpdate = new EntryConceptSystemPositionUpdate
        {
            Ids = new List<int> { 1, 2 },
            ConceptSystemPositionId = 1,
            Action = MultiEditAction.Set
        };

        entrySourceUpdate = new EntrySourceUpdate
        {
            Ids = new List<int> { 1, 2 },
            UpdateValue = "newSource",
            Action = MultiEditAction.Set,
            TargetFields = ["terminusSource"],
            LanguageIds = [1]
        };
        entryDetail = entryDetailFaker.Generate();
        entryModificationDetail = entryModificationDetailFaker.Generate();

        entryController = new EntriesController(entryServiceMock, entryHeaderUpdateServiceMock, loggerMock);
    }

    [TearDown]
    public void TearDown()
    {
        entryServiceMock.ClearSubstitute();
    }

    [Test]
    public async Task GetDetail_ForValidRequest_ShouldCallEntryService()
    {
        entryServiceMock.GetDetail(1).Returns(entryDetail);

        var result = await entryController.GetDetail(1, getDetailQuery);

        await entryServiceMock.Received().GetDetail(1, getDetailQuery.SourceLanguageIds, getDetailQuery.TargetLanguageIds);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entryDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetDetail_ForNull_ShouldReturnBadRequest()
    {
        var result = await entryController.GetDetail(1, null);

        Assert.That(result, Is.Not.Null);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetModificationDetail_ForValidRequest_ShouldCallEntryService()
    {
        entryServiceMock.GetModificationDetail(1).Returns(entryModificationDetail);

        var result = await entryController.GetModificationDetail(1);

        await entryServiceMock.Received().GetModificationDetail(1);
        Assert.That(result, Is.Not.Null);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entryModificationDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetModificationDetail_OnEntryLockedException_ReturnsBadRequest()
    {
        entryServiceMock.GetModificationDetail(1).Throws(new EntryLockedException("MUST BE CAUGHT"));

        var result = await entryController.GetModificationDetail(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task Create_ForValidRequest_ShouldCallEntryService()
    {
        entryServiceMock.Create(entryCreate).Returns(entryModificationDetail);

        var result = await entryController.Create(entryCreate);

        await entryServiceMock.Received().Create(entryCreate);
        Assert.That(result, Is.Not.Null);

        var response = result as CreatedAtActionResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entryModificationDetail));
        Assert.That(response.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ForNull_ShouldReturnBadRequest()
    {
        entryServiceMock.Create(entryCreate).Returns(entryModificationDetail);

        var result = await entryController.Create(null);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Create_WithForbiddenSubLevelItem_ShouldReturnBadRequest()
    {
        entryServiceMock.Create(entryCreate).Throws<ForbiddenException>();

        var result = await entryController.Create(entryCreate);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ForValidRequest_ShouldCallEntryService()
    {
        entryServiceMock.Update(entryUpdate).Returns(entryModificationDetail);

        var result = await entryController.Update(entryUpdate.Id, entryUpdate);

        await entryServiceMock.Received().Update(entryUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(entryModificationDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Update_ForNull_ShouldReturnBadRequest()
    {
        var result = await entryController.Update(1, null);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_ForRequestWithWrongId_ShouldReturnBadRequest()
    {
        var result = await entryController.Update(entryUpdate.Id + 1, entryUpdate);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Update_WithForbiddenSubLevelItem_ShouldReturnBadRequest()
    {
        entryServiceMock.Update(entryUpdate).Throws<ForbiddenException>();

        var result = await entryController.Update(entryUpdate.Id, entryUpdate);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateMultipleGlossaries_ForValidRequest_ShouldCallEntryHeaderUpdateService()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 }
        };
        var result = await entryController.UpdateMultipleGlossaries(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        await entryHeaderUpdateServiceMock.Received(1)
            .UpdateGlossaries(Arg.Is<EntryChipFieldUpdate>(arg => arg.Equals(dto)));
    }

    [Test]
    public async Task UpdateMultipleGlossaries_ForRequestWithoutIds_ShouldReturnBadRequest()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int>()
        };
        var result = await entryController.UpdateMultipleGlossaries(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryHeaderUpdateServiceMock.DidNotReceive().UpdateGlossaries(Arg.Any<EntryChipFieldUpdate>());
    }

    [Test]
    public async Task UpdateMultipleRelatedCollections_ForValidRequest_ShouldCallEntryHeaderUpdateService()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int> { 1 }
        };
        var result = await entryController.UpdateMultipleRelatedCollections(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        await entryHeaderUpdateServiceMock.Received(1)
            .UpdateRelatedCollections(Arg.Is<EntryChipFieldUpdate>(arg => arg.Equals(dto)));
    }

    [Test]
    public async Task UpdateMultipleRelatedCollections_ForRequestWithoutIds_ShouldReturnBadRequest()
    {
        var dto = new EntryChipFieldUpdate
        {
            Ids = new List<int>()
        };
        var result = await entryController.UpdateMultipleRelatedCollections(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryHeaderUpdateServiceMock.DidNotReceive().UpdateRelatedCollections(Arg.Any<EntryChipFieldUpdate>());
    }

    [Test]
    public async Task UpdateMultipleAdminInfos_ForValidRequest_ShouldCallEntryHeaderUpdateService()
    {
        var dto = new EntryChipFieldInputUpdate
        {
            Ids = new List<int> { 1 }
        };
        var result = await entryController.UpdateMultipleAdminInfos(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        await entryHeaderUpdateServiceMock.Received(1)
            .UpdateAdminInfos(Arg.Is<EntryChipFieldInputUpdate>(arg => arg.Equals(dto)));
    }

    [Test]
    public async Task UpdateMultipleAdminInfos_ForRequestWithoutIds_ShouldReturnBadRequest()
    {
        var dto = new EntryChipFieldInputUpdate
        {
            Ids = new List<int>()
        };
        var result = await entryController.UpdateMultipleAdminInfos(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryHeaderUpdateServiceMock.DidNotReceive().UpdateAdminInfos(Arg.Any<EntryChipFieldInputUpdate>());
    }

    [Test]
    public async Task Delete_ForValidRequest_ShouldCallEntryService()
    {
        var result = await entryController.Delete(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).DeleteMultiple(Arg.Is<int[]>(arr => arr.SequenceEqual(new[] { 1 })));
    }

    [Test]
    public async Task Delete_ForValidRequestWithHardDelete_ShouldCallEntryService()
    {
        var result = await entryController.Delete(1, false);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).DeleteMultiple(Arg.Is<int[]>(arr => arr.SequenceEqual(new[] { 1 })), Arg.Is(false));
    }

    [Test]
    public async Task DeleteMultiple_ForValidRequest_ShouldCallEntryService()
    {
        var entryIds = new[] { 1, 2 };

        var result = await entryController.DeleteMultiple(entryIds);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).DeleteMultiple(Arg.Is(entryIds));
    }

    [Test]
    public async Task DeleteMultiple_ForValidRequestWithHardDelete_ShouldCallEntryService()
    {
        var entryIds = new[] { 1, 2 };

        var result = await entryController.DeleteMultiple(entryIds, false);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).DeleteMultiple(Arg.Is(entryIds), Arg.Is(false));
    }

    [Test]
    public async Task Restore_ForValidRequest_ShouldCallEntryServiceAndReturn()
    {
        var result = await entryController.Restore(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).RestoreMultiple(Arg.Is<int[]>(a => a.Length == 1));
    }

    [Test]
    public async Task RestoreMultiple_ForValidRequest_ShouldCallEntryServiceAndReturn()
    {
        var ids = new[] { 1, 2 };
        var result = await entryController.RestoreMultiple(ids);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        await entryServiceMock.Received(1).RestoreMultiple(Arg.Is(ids));
    }

    [Test]
    public async Task GetReferenceDetail_ForValidRequest_ShouldReturnReferenceDetail()
    {
        var referenceDetail = new EntryReferenceDetail();

        entryServiceMock.GetReferenceDetail(1).Returns(referenceDetail);

        var result = await entryController.GetReferenceDetail(1);

        await entryServiceMock.Received(1).GetReferenceDetail(1);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Value, Is.EqualTo(referenceDetail));
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_ForValidRequest_ShouldCallEntryService()
    {
        var result = await entryController.UpdateMandatoryHeaderFields(entryHeaderUpdate);

        await entryHeaderUpdateServiceMock.Received().UpdateMandatoryHeaderFields(entryHeaderUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_ForNull_ShouldReturnBadRequest()
    {
        var result = await entryController.UpdateMandatoryHeaderFields(null);

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_ForNullIdList_ShouldReturnBadRequest()
    {
        var result = await entryController.UpdateMandatoryHeaderFields(new EntryHeaderUpdate() { Ids = null });

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateMandatoryHeaderFields_ForEmptyIdList_ShouldReturnBadRequest()
    {
        var result = await entryController.UpdateMandatoryHeaderFields(new EntryHeaderUpdate() { Ids = new List<int>() });

        Assert.That(result, Is.Not.Null);
        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateCommentFields_ForValidRequest_ShouldCallEntryService()
    {
        var result = await entryController.UpdateMultipleComment(entryCommentUpdate);

        await entryHeaderUpdateServiceMock.Received().UpdateComment(entryCommentUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMultipleConceptSystemPositions_ForValidRequest_ShouldCallEntryService()
    {
        var result = await entryController.UpdateMultipleConceptSystemPosition(entryConceptSystemPositionUpdate);

        await entryHeaderUpdateServiceMock.Received().UpdateConceptSystemPosition(entryConceptSystemPositionUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMultipleConceptSystemPositions_WhenServiceValidationFails_ShouldReturnBadRequest()
    {
        entryHeaderUpdateServiceMock.UpdateConceptSystemPosition(entryConceptSystemPositionUpdate).Throws(new BusinessValidationException("key", "MUST BE CAUGHT"));

        var result = await entryController.UpdateMultipleConceptSystemPosition(entryConceptSystemPositionUpdate);

        var response = result as BadRequestObjectResult;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(response.Value, Is.EqualTo("MUST BE CAUGHT"));
    }

    [Test]
    public async Task UpdateMultipleConceptSystemPositions_ForInvalidRequest_ShouldReturnBadRequest()
    {
        var invalidDto = new EntryConceptSystemPositionUpdate
        {
            Ids = new List<int> { },
            ConceptSystemPositionId = 1,
            Action = MultiEditAction.Set
        };

        var result = await entryController.UpdateMultipleConceptSystemPosition(invalidDto);

        var response = result as BadRequestResult;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateMultipleSource_ForValidRequest_ShouldCallEntryService()
    {
        var result = await entryController.UpdateSource(entrySourceUpdate);

        await entryHeaderUpdateServiceMock.Received().UpdateSource(entrySourceUpdate);
        Assert.That(result, Is.Not.Null);
        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMultipleSource_WhenServiceValidationFails_ShouldReturnBadRequest()
    {
        entryHeaderUpdateServiceMock.UpdateSource(entrySourceUpdate).Throws(new BusinessValidationException("key", "MUST BE CAUGHT"));

        var result = await entryController.UpdateSource(entrySourceUpdate);

        var response = result as BadRequestObjectResult;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(response.Value, Is.EqualTo("MUST BE CAUGHT"));
    }

    [Test]
    public async Task UpdateMultipleSource_ForInvalidRequest_ShouldReturnBadRequest()
    {
        var invalidDto = new EntrySourceUpdate
        {
            Ids = new List<int> { },
            UpdateValue = "newSource",
            Action = MultiEditAction.Set,
            TargetFields = ["terminusSource"],
            LanguageIds = [1]
        };

        var result = await entryController.UpdateSource(invalidDto);

        var response = result as BadRequestResult;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task MultipleSearchAndReplace_ForValidRequest_ShouldCallEntryHeaderUpdateService()
    {
        var dto = new EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 1, 2 },
            LanguageIds = new List<int> { 1, 2 },
            SearchValue = "value1",
            ReplaceValue = "value2",
            SearchField = "note"
        };
        var result = await entryController.MultipleSearchAndReplace(dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        await entryServiceMock.Received(1)
            .MultipleSearchAndReplace(Arg.Is(dto));
    }

    [Test]
    public async Task MultipleSearchAndReplace_ForRequestWithoutIds_ShouldReturnBadRequest()
    {
        var dto = new EntrySearchAndReplaceUpdate
        {
            Ids = new List<int>(),
            LanguageIds = new List<int> { 1, 2 },
            SearchValue = "value1",
            ReplaceValue = "value2",
            SearchField = "note"
        };
        var result = await entryController.MultipleSearchAndReplace(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryServiceMock.DidNotReceive().MultipleSearchAndReplace(Arg.Any<EntrySearchAndReplaceUpdate>());
    }

    [Test]
    public async Task MultipleSearchAndReplace_ForRequestWithoutSearchField_ShouldReturnBadRequest()
    {
        var dto = new EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 1, 2 },
            LanguageIds = new List<int> { 1, 2 },
            SearchValue = "value1",
            ReplaceValue = "value2",
            SearchField = null
        };
        var result = await entryController.MultipleSearchAndReplace(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryServiceMock.DidNotReceive().MultipleSearchAndReplace(Arg.Any<EntrySearchAndReplaceUpdate>());
    }

    [Test]
    public async Task MultipleSearchAndReplace_ForRequestWithoutSearchValue_ShouldReturnBadRequest()
    {
        var dto = new EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 1, 2 },
            LanguageIds = new List<int> { 1, 2 },
            SearchValue = null,
            ReplaceValue = "value2",
            SearchField = "note"
        };
        var result = await entryController.MultipleSearchAndReplace(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryServiceMock.DidNotReceive().MultipleSearchAndReplace(Arg.Any<EntrySearchAndReplaceUpdate>());
    }

    [Test]
    public async Task MultipleSearchAndReplace_ForRequestWithoutLanguage_ShouldReturnBadRequest()
    {
        var dto = new EntrySearchAndReplaceUpdate
        {
            Ids = new List<int> { 1, 2 },
            LanguageIds = new List<int>(),
            SearchValue = "value1",
            ReplaceValue = "value2",
            SearchField = "note"
        };
        var result = await entryController.MultipleSearchAndReplace(dto);

        var response = result as BadRequestResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(400));
        await entryServiceMock.DidNotReceive().MultipleSearchAndReplace(Arg.Any<EntrySearchAndReplaceUpdate>());
    }

    [Test]
    public async Task Lock_WhenCalled_ShouldCallEntryService()
    {
        entryServiceMock.Lock(1).Returns((uint)10);
        var result = await entryController.Lock(1);

        await entryServiceMock.Received(1).Lock(1);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));

        var lockResult = response.Value as LockResult;
        Assert.That(lockResult, Is.Not.Null);
        Assert.That(lockResult.IsLocked, Is.EqualTo(true));
        Assert.That(lockResult.ConcurrencyToken, Is.EqualTo(10));
    }

    [Test]
    public async Task Unlock_WhenCalled_ShouldCallEntryService()
    {
        entryServiceMock.Unlock(1).Returns((uint)10);
        var result = await entryController.Unlock(1);

        await entryServiceMock.Received(1).Unlock(1);

        var response = result as OkObjectResult;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(200));

        var lockResult = response.Value as LockResult;
        Assert.That(lockResult, Is.Not.Null);
        Assert.That(lockResult.IsLocked, Is.EqualTo(false));
        Assert.That(lockResult.ConcurrencyToken, Is.EqualTo(10));
    }
}
