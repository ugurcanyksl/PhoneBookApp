using AutoMapper;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactService.Contact.API.Models;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Tests.Services
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _contactRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<KafkaProducerService> _kafkaProducerServiceMock;
        private readonly ContactImplementationService _contactService;

        public ContactServiceTests()
        {
            _contactRepositoryMock = new Mock<IContactRepository>();
            _mapperMock = new Mock<IMapper>();
            _kafkaProducerServiceMock = new Mock<KafkaProducerService>();
            _contactService = new ContactImplementationService(_contactRepositoryMock.Object, _mapperMock.Object, _kafkaProducerServiceMock.Object);
        }

        // CreateAsync Test
        [Fact]
        public async Task CreateAsync_ShouldReturnPerson_WhenValidData()
        {
            // Arrange
            var dto = new CreateContactDto
            {
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Company = dto.Company
            };

            _mapperMock.Setup(m => m.Map<Person>(It.IsAny<CreateContactDto>())).Returns(person);
            _contactRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
            _contactRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _contactService.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.FirstName, result.FirstName);
            Assert.Equal(dto.LastName, result.LastName);
            Assert.Equal(dto.Company, result.Company);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactService.CreateAsync(null));
        }

        // Edge case test: CreateAsync with missing required fields
        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var dto = new CreateContactDto
            {
                FirstName = string.Empty,  // Missing first name
                LastName = "Doe",
                Company = "ABC Corp"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.CreateAsync(dto));
        }

        // GetByIdAsync Test
        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var person = new Person
            {
                Id = personId,
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ReturnsAsync(person);

            // Act
            var result = await _contactService.GetByIdAsync(personId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personId, result.Id);
            Assert.Equal(person.FirstName, result.FirstName);
            Assert.Equal(person.LastName, result.LastName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ReturnsAsync((Person)null);

            // Act
            var result = await _contactService.GetByIdAsync(personId);

            // Assert
            Assert.Null(result);
        }

        // GetAllAsync Test
        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfPersons()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Company = "ABC Corp" },
                new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Company = "XYZ Ltd" }
            };

            _contactRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(persons);

            // Act
            var result = await _contactService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(persons.Count, result.Count);
        }

        // DeleteAsync Test
        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenPersonDeleted()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _contactRepositoryMock.Setup(repo => repo.DeleteAsync(personId)).ReturnsAsync(true);

            // Act
            var result = await _contactService.DeleteAsync(personId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPersonNotFound()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _contactRepositoryMock.Setup(repo => repo.DeleteAsync(personId)).ReturnsAsync(false);

            // Act
            var result = await _contactService.DeleteAsync(personId);

            // Assert
            Assert.False(result);
        }

        // AddContactInfoAsync Test
        [Fact]
        public async Task AddContactInfoAsync_ShouldReturnTrue_WhenContactInfoAdded()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto
            {
                InfoType = InfoType.PhoneNumber,
                InfoContent = "123456789"
            };

            _mapperMock.Setup(m => m.Map<PhoneBookMicroservices.Shared.Models.ContactInfo>(It.IsAny<ContactInfoDto>())).Returns(new PhoneBookMicroservices.Shared.Models.ContactInfo());
            _contactRepositoryMock.Setup(repo => repo.AddContactInfoAsync(personId, It.IsAny<PhoneBookMicroservices.Shared.Models.ContactInfo>())).ReturnsAsync(true);

            // Act
            var result = await _contactService.AddContactInfoAsync(personId, contactInfoDto);

            // Assert
            Assert.True(result);
        }

        // RemoveContactInfoAsync Test
        [Fact]
        public async Task RemoveContactInfoAsync_ShouldReturnTrue_WhenContactInfoRemoved()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoId = Guid.NewGuid();

            _contactRepositoryMock.Setup(repo => repo.DeleteContactInfoAsync(personId, contactInfoId)).ReturnsAsync(true);

            // Act
            var result = await _contactService.RemoveContactInfoAsync(personId, contactInfoId);

            // Assert
            Assert.True(result);
        }

        // KafkaProducerService Hata Testi
        [Fact]
        public async Task KafkaProducerService_ShouldHandleSendMessageError()
        {
            // Arrange
            var kafkaProducerService = new Mock<KafkaProducerService>();
            kafkaProducerService.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                                .ThrowsAsync(new Exception("Kafka error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => kafkaProducerService.Object.SendMessageAsync("contact-topic", "Test message"));
        }

        // Performans Testi (yavaş çalışan metotlar için)
        [Fact]
        public async Task CreateAsync_ShouldNotTakeTooLong()
        {
            // Arrange
            var dto = new CreateContactDto
            {
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Company = dto.Company
            };

            _mapperMock.Setup(m => m.Map<Person>(It.IsAny<CreateContactDto>())).Returns(person);
            _contactRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
            _contactRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Arrange
            var start = DateTime.Now; // Burada zaman başlıyor

            // Act
            var result = await _contactService.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(DateTime.Now - start < TimeSpan.FromSeconds(1), "Method took too long!");
        }
    }
}
