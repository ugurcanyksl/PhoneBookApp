using AutoMapper;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Moq;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ContactService.Tests.Services
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _contactRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKafkaProducerService> _kafkaProducerServiceMock;
        private readonly ContactImplementationService _contactService;

        public ContactServiceTests()
        {
            _contactRepositoryMock = new Mock<IContactRepository>();
            _mapperMock = new Mock<IMapper>();
            _kafkaProducerServiceMock = new Mock<IKafkaProducerService>();
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
            _kafkaProducerServiceMock.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

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

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var dto = new CreateContactDto
            {
                FirstName = string.Empty,
                LastName = "Doe",
                Company = "ABC Corp"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenKafkaFails()
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
            _kafkaProducerServiceMock.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Kafka error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.CreateAsync(dto));
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

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.GetByIdAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.GetByIdAsync(personId));
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

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPersonsExist()
        {
            // Arrange
            _contactRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Person>());

            // Act
            var result = await _contactService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _contactRepositoryMock.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.GetAllAsync());
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

        [Fact]
        public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.DeleteAsync(Guid.Empty));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _contactRepositoryMock.Setup(repo => repo.DeleteAsync(personId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.DeleteAsync(personId));
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

            _mapperMock.Setup(m => m.Map<ContactInfo>(It.IsAny<ContactInfoDto>())).Returns(new ContactInfo());
            _contactRepositoryMock.Setup(repo => repo.AddContactInfoAsync(personId, It.IsAny<ContactInfo>())).ReturnsAsync(true);

            // Act
            var result = await _contactService.AddContactInfoAsync(personId, contactInfoDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Arrange
            var contactInfoDto = new ContactInfoDto
            {
                InfoType = InfoType.PhoneNumber,
                InfoContent = "123456789"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.AddContactInfoAsync(Guid.Empty, contactInfoDto));
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldThrowArgumentNullException_WhenContactInfoDtoIsNull()
        {
            // Arrange
            var personId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactService.AddContactInfoAsync(personId, null));
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldReturnFalse_WhenPersonNotFound()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto
            {
                InfoType = InfoType.PhoneNumber,
                InfoContent = "123456789"
            };

            _mapperMock.Setup(m => m.Map<ContactInfo>(It.IsAny<ContactInfoDto>())).Returns(new ContactInfo());
            _contactRepositoryMock.Setup(repo => repo.AddContactInfoAsync(personId, It.IsAny<ContactInfo>())).ReturnsAsync(false);

            // Act
            var result = await _contactService.AddContactInfoAsync(personId, contactInfoDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto
            {
                InfoType = InfoType.PhoneNumber,
                InfoContent = "123456789"
            };

            _mapperMock.Setup(m => m.Map<ContactInfo>(It.IsAny<ContactInfoDto>())).Returns(new ContactInfo());
            _contactRepositoryMock.Setup(repo => repo.AddContactInfoAsync(personId, It.IsAny<ContactInfo>())).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.AddContactInfoAsync(personId, contactInfoDto));
        }

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

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Arrange
            var contactInfoId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.RemoveContactInfoAsync(Guid.Empty, contactInfoId));
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldThrowArgumentException_WhenContactInfoIdIsEmpty()
        {
            // Arrange
            var personId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.RemoveContactInfoAsync(personId, Guid.Empty));
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldReturnFalse_WhenContactInfoNotFound()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoId = Guid.NewGuid();

            _contactRepositoryMock.Setup(repo => repo.DeleteContactInfoAsync(personId, contactInfoId)).ReturnsAsync(false);

            // Act
            var result = await _contactService.RemoveContactInfoAsync(personId, contactInfoId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoId = Guid.NewGuid();

            _contactRepositoryMock.Setup(repo => repo.DeleteContactInfoAsync(personId, contactInfoId)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.RemoveContactInfoAsync(personId, contactInfoId));
        }

        // GetByLocationAsync Test
        [Fact]
        public async Task GetByLocationAsync_ShouldReturnPersons_WhenLocationExists()
        {
            // Arrange
            var location = "Istanbul";
            var persons = new List<Person>
            {
                new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Ali",
                    LastName = "Yilmaz",
                    Company = "XYZ Corp",
                    ContactInfos = new List<ContactInfo>
                    {
                        new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Location, InfoContent = location }
                    }
                }
            };

            _contactRepositoryMock.Setup(repo => repo.GetByLocationAsync(location)).ReturnsAsync(persons);

            // Act
            var result = await _contactService.GetByLocationAsync(location);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(location, result.First().ContactInfos.First().InfoContent);
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldReturnEmptyList_WhenNoPersonsInLocation()
        {
            // Arrange
            var location = "Ankara";
            _contactRepositoryMock.Setup(repo => repo.GetByLocationAsync(location)).ReturnsAsync(new List<Person>());

            // Act
            var result = await _contactService.GetByLocationAsync(location);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldThrowArgumentException_WhenLocationIsNull()
        {
            // Arrange
            string location = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.GetByLocationAsync(location));
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldThrowArgumentException_WhenLocationIsEmpty()
        {
            // Arrange
            var location = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.GetByLocationAsync(location));
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var location = "Istanbul";
            _contactRepositoryMock.Setup(repo => repo.GetByLocationAsync(location)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.GetByLocationAsync(location));
        }

        [Fact]
        public async Task GetByLocationAsync_PerformanceTest_WithLargeDataset()
        {
            // Arrange
            var location = "Istanbul";
            var largeDataset = new List<Person>();
            for (int i = 0; i < 1000; i++)
            {
                largeDataset.Add(new Person
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"Person{i}",
                    LastName = "Test",
                    Company = "Test Corp",
                    ContactInfos = new List<ContactInfo>
                    {
                        new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Location, InfoContent = location }
                    }
                });
            }

            _contactRepositoryMock.Setup(repo => repo.GetByLocationAsync(location)).ReturnsAsync(largeDataset);

            var start = DateTime.Now;

            // Act
            var result = await _contactService.GetByLocationAsync(location);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Count);
            Assert.True(DateTime.Now - start < TimeSpan.FromSeconds(2), "Method took too long for large dataset!");
        }

        // UpdateAsync Test
        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenPersonUpdated()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var updateDto = new UpdateContactDto
            {
                FirstName = "John",
                LastName = "Doe Updated",
                Company = "ABC Corp Updated"
            };

            var existingPerson = new Person
            {
                Id = personId,
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            var updatedPerson = new Person
            {
                Id = personId,
                FirstName = updateDto.FirstName,
                LastName = updateDto.LastName,
                Company = updateDto.Company
            };

            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ReturnsAsync(existingPerson);
            _mapperMock.Setup(m => m.Map(updateDto, existingPerson)).Returns((UpdateContactDto dto, Person person) =>
            {
                person.FirstName = dto.FirstName;
                person.LastName = dto.LastName;
                person.Company = dto.Company;
                return person;
            });
            _contactRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
            _contactRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
            _kafkaProducerServiceMock.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _contactService.UpdateAsync(personId, updateDto);

            // Assert
            Assert.True(result);
            Assert.Equal(updateDto.FirstName, existingPerson.FirstName);
            Assert.Equal(updateDto.LastName, existingPerson.LastName);
            Assert.Equal(updateDto.Company, existingPerson.Company);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Arrange
            var updateDto = new UpdateContactDto
            {
                FirstName = "John",
                LastName = "Doe Updated",
                Company = "ABC Corp Updated"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _contactService.UpdateAsync(Guid.Empty, updateDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenUpdateDtoIsNull()
        {
            // Arrange
            var personId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _contactService.UpdateAsync(personId, null));
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenPersonNotFound()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var updateDto = new UpdateContactDto
            {
                FirstName = "John",
                LastName = "Doe Updated",
                Company = "ABC Corp Updated"
            };

            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ReturnsAsync((Person)null);

            // Act
            var result = await _contactService.UpdateAsync(personId, updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenKafkaFails()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var updateDto = new UpdateContactDto
            {
                FirstName = "John",
                LastName = "Doe Updated",
                Company = "ABC Corp Updated"
            };

            var existingPerson = new Person
            {
                Id = personId,
                FirstName = "John",
                LastName = "Doe",
                Company = "ABC Corp"
            };

            _contactRepositoryMock.Setup(repo => repo.GetByIdAsync(personId)).ReturnsAsync(existingPerson);
            _mapperMock.Setup(m => m.Map(updateDto, existingPerson)).Returns((UpdateContactDto dto, Person person) =>
            {
                person.FirstName = dto.FirstName;
                person.LastName = dto.LastName;
                person.Company = dto.Company;
                return person;
            });
            _contactRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Person>())).Returns(Task.CompletedTask);
            _contactRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
            _kafkaProducerServiceMock.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Kafka error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _contactService.UpdateAsync(personId, updateDto));
        }

        // KafkaProducerService Hata Testi
        [Fact]
        public async Task KafkaProducerService_ShouldHandleSendMessageError()
        {
            // Arrange
            var kafkaProducerService = new Mock<IKafkaProducerService>();
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
            _kafkaProducerServiceMock.Setup(k => k.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Arrange
            var start = DateTime.Now;

            // Act
            var result = await _contactService.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(DateTime.Now - start < TimeSpan.FromSeconds(2), "Method took too long!");
        }
    }
}