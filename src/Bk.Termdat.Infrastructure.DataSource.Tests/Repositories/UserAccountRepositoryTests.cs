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
internal class UserAccountRepositoryTests
{
    private UserAccountRepository userAccountRepository;
    private DataContext context;

    private ICollection<Collection> collections;
    private ICollection<Glossary> glossaries;


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        context = DatabaseHelper.PrepareContext(Guid.NewGuid().ToString("N"));
        userAccountRepository = new UserAccountRepository(context);

        var collection1 = context.Collections.First();
        var collection2 = context.Collections.Last();

        var glossary1 = context.Glossaries.First();
        var glossary2 = context.Glossaries.Last();

        collections = new List<Collection> { collection1, collection2 };
        glossaries = new List<Glossary> { glossary1, glossary2 };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        context.Dispose();
    }

    [TestCase]
    public async Task GetAll_WhenCalled_ReturnsAllData()
    {
        var data = (await userAccountRepository.GetAll()).ToList();

        AssertFirst(data.First());
        AssertLast(data.Last());
    }

    [TestCase]
    public async Task GetAllDetailed_WhenCalled_ReturnsAllDataWithDetails()
    {
        var data = (await userAccountRepository.GetAllDetailed()).ToList();

        AssertFirst(data.First());
        AssertLast(data.Last());
    }

    [TestCase("anna.genie@test.it")]
    [TestCase("Anna.Genie@test.it")]
    public async Task GetByEmail_WhenCalles_Returns(string email)
    {
        var data = await userAccountRepository.GetByEmail(email);
        AssertFirst(data);
    }

    [TestCase("anna.genie@test.it")]
    [TestCase("Anna.Genie@test.it")]
    public async Task GetDetailByEmail_WhenCalled_ReturnsAllDataWithDetails(string email)
    {
        var data = await userAccountRepository.GetDetailByEmail(email);

        AssertFirst(data);

        Assert.That(data.AuthorizedCollections, Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries, Is.Not.Null);
        Assert.That(data.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedGlossaries.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 1 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 2 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 1 && y.Code == "Glo1"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 2 && y.Code == "Glo2"), Is.Not.Null);
    }

    [TestCase("anna.genie@test.it")]
    [TestCase("Anna.Genie@test.it")]
    public async Task GetDetailByEmailForUpdate_ShouldReturnWithDetails(string email)
    {
        var data = await userAccountRepository.GetDetailByEmailForUpdate(email);

        AssertFirst(data);

        Assert.That(data.AuthorizedCollections, Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries, Is.Not.Null);
        Assert.That(data.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedGlossaries.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 1 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 2 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 1 && y.Code == "Glo1"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 2 && y.Code == "Glo2"), Is.Not.Null);
    }

    [TestCase]
    public async Task GetDetail_WhenCalled_ReturnsAllDataWithDetails()
    {
        var data = await userAccountRepository.GetDetail(1);

        AssertFirst(data);
    }

    private static void AssertFirst(UserAccount userAccount)
    {
        Assert.That(userAccount.Id, Is.Not.Default);

        Assert.That(userAccount.FirstName, Is.EqualTo("Anna"));
        Assert.That(userAccount.LastName, Is.EqualTo("Genie"));
        Assert.That(userAccount.Email, Is.EqualTo("anna.genie@test.it"));
    }

    private static void AssertLast(UserAccount userAccount)
    {
        Assert.That(userAccount.Id, Is.Not.Default);

        Assert.That(userAccount.FirstName, Is.EqualTo("Franziska"));
        Assert.That(userAccount.LastName, Is.EqualTo("Testerin"));
        Assert.That(userAccount.Email, Is.EqualTo("franziska.testerin@test.ch"));
    }

    [Test]
    public async Task CreateShouldAddUserAccounts()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"));

        var createContext = new DataContext(builder.Options);
        var repository = new UserAccountRepository(createContext);

        var newUserAccount = new UserAccount()
        {
            Email = "new@user.ch",
            FirstName = "New",
            LastName = "User",
            Created = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "test1@test.ch",
            Modified = DateTime.UtcNow.AddDays(-5),
            ModifiedBy = "test2@test.ch",
        };

        await repository.Create(newUserAccount);

        await createContext.SaveChangesAsync();

        Assert.That(newUserAccount.Id, Is.Not.Default);
    }

    [Test]
    public async Task UpdateShouldUpdateUserAccount()
    {
        var databaseUserAccount = await userAccountRepository.GetByEmail("anna.genie@test.it");
        var updatedUserAccount = new UserAccount
        {
            Id = databaseUserAccount.Id,
            Email = "updated@participant.ch",
            FirstName = "Updated",
            LastName = "Participant",
        };

        userAccountRepository.Update(databaseUserAccount, updatedUserAccount);

        await context.SaveChangesAsync();

        var updatedDatabaseUserAccount = await userAccountRepository.GetByEmail("updated@participant.ch");
        Assert.That(updatedDatabaseUserAccount.Email, Is.EqualTo(updatedUserAccount.Email));
        Assert.That(updatedDatabaseUserAccount.FirstName, Is.EqualTo(updatedUserAccount.FirstName));
        Assert.That(updatedDatabaseUserAccount.LastName, Is.EqualTo(updatedUserAccount.LastName));
    }

    [Test]
    public async Task UpdateWithRelations_ValidData_ShouldUpdateUserAccount()
    {
        var databaseUserAccount = await userAccountRepository.GetDetailByIdForUpdate(1);

        var updatedUserAccount = new UserAccount
        {
            Id = databaseUserAccount.Id,
            Email = "updated@participant.ch",
            FirstName = "Updated",
            LastName = "Participant",
            AuthorizedCollections = collections,
            AuthorizedGlossaries = glossaries,
        };

        userAccountRepository.UpdateWithRelations(databaseUserAccount, updatedUserAccount);

        await context.SaveChangesAsync();

        var updatedDatabaseUserAccount = await userAccountRepository.GetByEmail("updated@participant.ch");
        Assert.That(updatedDatabaseUserAccount.Email, Is.EqualTo(updatedUserAccount.Email));
        Assert.That(updatedDatabaseUserAccount.FirstName, Is.EqualTo(updatedUserAccount.FirstName));
        Assert.That(updatedDatabaseUserAccount.LastName, Is.EqualTo(updatedUserAccount.LastName));
        Assert.That(updatedDatabaseUserAccount.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(updatedDatabaseUserAccount.AuthorizedGlossaries.Count, Is.EqualTo(2));
        Assert.That(updatedDatabaseUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 1), Is.Not.Null);
        Assert.That(updatedDatabaseUserAccount.AuthorizedCollections.FirstOrDefault(y => y.Id == 36), Is.Not.Null);
        Assert.That(updatedDatabaseUserAccount.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 1), Is.Not.Null);
        Assert.That(updatedDatabaseUserAccount.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 3), Is.Not.Null);
    }

    [TestCase]
    public async Task GetDetailByIdForUpdate_WhenCalled_ReturnsAllDataWithDetails()
    {
        var data = await userAccountRepository.GetDetailByIdForUpdate(1);

        AssertFirst(data);

        Assert.That(data.AuthorizedCollections, Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries, Is.Not.Null);
        Assert.That(data.AuthorizedCollections.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedGlossaries.Count, Is.EqualTo(2));
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 1 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedCollections.FirstOrDefault(y => y.Id == 2 && y.Code == "ABC11"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 1 && y.Code == "Glo1"), Is.Not.Null);
        Assert.That(data.AuthorizedGlossaries.FirstOrDefault(y => y.Id == 2 && y.Code == "Glo2"), Is.Not.Null);
    }
}
