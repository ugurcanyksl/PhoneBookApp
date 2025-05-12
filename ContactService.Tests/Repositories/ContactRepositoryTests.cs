using ContactService.Contact.API.Infrastructure;
using ContactService.Contact.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ContactService.Tests.Repositories
{
    public class ContactRepositoryTests : IDisposable
    {
        private readonly ContactDbContext _dbContext;
        private readonly ContactRepository _contactRepository;

        public ContactRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ContactDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _dbContext = new ContactDbContext(options);
            _contactRepository = new ContactRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private async Task SeedData()
        {
            var person1 = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp",
                ContactInfos = new List<ContactInfo>
                {
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.PhoneNumber, InfoContent = "123456789" },
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Location, InfoContent = "Istanbul" }
                }
            };

            var person2 = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Company = "XYZ Ltd",
                ContactInfos = new List<ContactInfo>
                {
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Email, InfoContent = "jane.smith@example.com" }
                }
            };

            await _dbContext.Contacts.AddRangeAsync(person1, person2);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            await SeedData();
            var person = _dbContext.Contacts.First();
            var result = await _contactRepository.GetByIdAsync(person.Id);
            Assert.NotNull(result);
            Assert.Equal(person.Id, result.Id);
            Assert.Equal(person.FirstName, result.FirstName);
            Assert.Equal(person.ContactInfos.Count, result.ContactInfos.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _contactRepository.GetByIdAsync(nonExistentId);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPersons()
        {
            await SeedData();
            var result = await _contactRepository.GetAllAsync();
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.FirstName == "John");
            Assert.Contains(result, p => p.FirstName == "Jane");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPersonsExist()
        {
            var result = await _contactRepository.GetAllAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPerson()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Brown",
                Company = "DEF Corp",
                ContactInfos = new List<ContactInfo>
                {
                    new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.PhoneNumber, InfoContent = "987654321" }
                }
            };
            await _contactRepository.AddAsync(person);
            await _contactRepository.SaveChangesAsync();
            var result = await _contactRepository.GetByIdAsync(person.Id);
            Assert.NotNull(result);
            Assert.Equal(person.FirstName, result.FirstName);
            Assert.Equal(person.ContactInfos.Count, result.ContactInfos.Count);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePerson()
        {
            await SeedData();
            var person = _dbContext.Contacts.First();
            person.FirstName = "John Updated";
            person.LastName = "Doe Updated";
            await _contactRepository.UpdateAsync(person);
            await _contactRepository.SaveChangesAsync();
            var result = await _contactRepository.GetByIdAsync(person.Id);
            Assert.Equal("John Updated", result.FirstName);
            Assert.Equal("Doe Updated", result.LastName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeletePerson()
        {
            await SeedData();
            var person = _dbContext.Contacts.First();
            var result = await _contactRepository.DeleteAsync(person.Id);
            await _contactRepository.SaveChangesAsync();
            Assert.True(result);
            var deletedPerson = await _contactRepository.GetByIdAsync(person.Id);
            Assert.Null(deletedPerson);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPersonNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _contactRepository.DeleteAsync(nonExistentId);
            Assert.False(result);
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldAddContactInfo()
        {
            await SeedData();
            var personId = _dbContext.Contacts.First().Id;
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = InfoType.Email,
                InfoContent = "john.doe@example.com"
            };
            _dbContext.ContactInfos.Add(contactInfo);
            await _dbContext.SaveChangesAsync();

            var result = await _contactRepository.AddContactInfoAsync(personId, contactInfo);
            Assert.True(result);
            var updatedPerson = await _contactRepository.GetByIdAsync(personId);
            Assert.Contains(updatedPerson.ContactInfos, ci => ci.InfoContent == "john.doe@example.com");
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldReturnFalse_WhenPersonNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = InfoType.Email,
                InfoContent = "test@example.com"
            };
            var result = await _contactRepository.AddContactInfoAsync(nonExistentId, contactInfo);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteContactInfoAsync_ShouldDeleteContactInfo()
        {
            await SeedData();
            var person = _dbContext.Contacts.First();
            var contactInfo = person.ContactInfos.First();
            var result = await _contactRepository.DeleteContactInfoAsync(person.Id, contactInfo.Id);
            await _contactRepository.SaveChangesAsync();
            Assert.True(result);
            var updatedPerson = await _contactRepository.GetByIdAsync(person.Id);
            Assert.DoesNotContain(updatedPerson.ContactInfos, ci => ci.Id == contactInfo.Id);
        }

        [Fact]
        public async Task DeleteContactInfoAsync_ShouldReturnFalse_WhenContactInfoNotFound()
        {
            await SeedData();
            var person = _dbContext.Contacts.First();
            var nonExistentContactInfoId = Guid.NewGuid();
            var result = await _contactRepository.DeleteContactInfoAsync(person.Id, nonExistentContactInfoId);
            Assert.False(result);
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldReturnPersons_WhenLocationExists()
        {
            await SeedData();
            var location = "Istanbul";
            var result = await _contactRepository.GetByLocationAsync(location);
            Assert.Single(result);
            Assert.Equal("John", result.First().FirstName);
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldReturnEmptyList_WhenLocationNotFound()
        {
            await SeedData();
            var location = "Ankara";
            var result = await _contactRepository.GetByLocationAsync(location);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnPagedPersons()
        {
            await SeedData();
            int page = 1, pageSize = 1;
            var result = await _contactRepository.GetAllPagedAsync(page, pageSize);
            Assert.Single(result);
            Assert.Equal("John", result.First().FirstName);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldReturnEmptyList_WhenPageOutOfRange()
        {
            await SeedData();
            int page = 10, pageSize = 1;
            var result = await _contactRepository.GetAllPagedAsync(page, pageSize);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldThrowArgumentException_WhenPageSizeIsNegative()
        {
            int page = 1, pageSize = -1;
            await Assert.ThrowsAsync<ArgumentException>(() => _contactRepository.GetAllPagedAsync(page, pageSize));
        }

        [Fact]
        public async Task GetAllPagedAsync_ShouldThrowArgumentException_WhenPageIsZeroOrNegative()
        {
            int page = 0, pageSize = 1;
            await Assert.ThrowsAsync<ArgumentException>(() => _contactRepository.GetAllPagedAsync(page, pageSize));
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldThrowException_WhenDatabaseFails()
        {
            // Arrange
            var mockContext = new Mock<ContactDbContext>(new DbContextOptions<ContactDbContext>());
            var mockDatabaseFacade = new Mock<DatabaseFacade>(mockContext.Object);
            mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);
            mockDatabaseFacade.Setup(d => d.EnsureDeleted()).Returns(true);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Simulated DB error"));
            var repo = new ContactRepository(mockContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => repo.SaveChangesAsync());
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldThrowArgumentNullException_WhenLocationIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactRepository.GetByLocationAsync(null));
        }
    }
}