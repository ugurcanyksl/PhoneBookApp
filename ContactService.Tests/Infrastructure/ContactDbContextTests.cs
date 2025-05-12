using ContactService.Contact.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactService.Tests.Infrastructure
{
    public class ContactDbContextTests : IDisposable
    {
        private readonly ContactDbContext _dbContext;

        public ContactDbContextTests()
        {
            var options = new DbContextOptionsBuilder<ContactDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ContactDbContext(options);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task CanAddAndRetrievePerson()
        {
            // Arrange
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };
            _dbContext.Contacts.Add(person);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedPerson = await _dbContext.Contacts.FindAsync(person.Id);

            // Assert
            Assert.NotNull(retrievedPerson);
            Assert.Equal(person.Id, retrievedPerson.Id);
            Assert.Equal("John", retrievedPerson.FirstName);
            Assert.Equal("Doe", retrievedPerson.LastName);
            Assert.Equal("ABC Corp", retrievedPerson.Company);
        }

        [Fact]
        public async Task CanAddAndRetrieveContactInfo()
        {
            // Arrange
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };
            _dbContext.Contacts.Add(person);
            await _dbContext.SaveChangesAsync();

            var contactInfo = new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            _dbContext.ContactInfos.Add(contactInfo); 
            person.ContactInfos.Add(contactInfo); 
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedPerson = await _dbContext.Contacts.Include(c => c.ContactInfos).FirstAsync(p => p.Id == person.Id);
            var retrievedContactInfo = retrievedPerson.ContactInfos.First();

            // Assert
            Assert.NotNull(retrievedContactInfo);
            Assert.Equal(contactInfo.Id, retrievedContactInfo.Id);
            Assert.Equal(InfoType.Email, retrievedContactInfo.InfoType);
        }
    }
}
