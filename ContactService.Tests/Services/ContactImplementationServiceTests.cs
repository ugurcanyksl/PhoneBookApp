using AutoMapper;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Moq;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactService.Tests.Services
{
    public class ContactImplementationServiceTests
    {
        private readonly Mock<IContactRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IKafkaProducerService> _mockKafkaProducer;
        private readonly ContactImplementationService _service;

        public ContactImplementationServiceTests()
        {
            _mockRepository = new Mock<IContactRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockKafkaProducer = new Mock<IKafkaProducerService>();
            _service = new ContactImplementationService(_mockRepository.Object, _mockMapper.Object, _mockKafkaProducer.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreatePerson_WhenDtoIsValid()
        {
            // Arrange
            var dto = new CreateContactDto { FirstName = "John", LastName = "Doe", Company = "ABC Corp" };
            var person = new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
            _mockMapper.Setup(m => m.Map<Person>(dto)).Returns(person);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(person.Id, result.Id);
            _mockRepository.Verify(r => r.AddAsync(person), Times.Once());
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once());
            _mockKafkaProducer.Verify(k => k.SendMessageAsync("contact-created", person.Id.ToString()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenFirstNameIsEmpty()
        {
            // Arrange
            var dto = new CreateContactDto { FirstName = "", LastName = "Doe" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var person = new Person { Id = personId, FirstName = "John" };
            _mockRepository.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(person);

            // Act
            var result = await _service.GetByIdAsync(personId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personId, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPersonList()
        {
            // Arrange
            var persons = new List<Person> { new Person { Id = Guid.NewGuid(), FirstName = "John" } };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(persons);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(persons.Count, result.Count);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(personId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(personId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(personId), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(Guid.Empty));
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldAddContactInfo_WhenInputIsValid()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto { InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            var contactInfo = new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            _mockMapper.Setup(m => m.Map<ContactInfo>(contactInfoDto)).Returns(contactInfo);
            _mockRepository.Setup(r => r.AddContactInfoAsync(personId, contactInfo)).ReturnsAsync(true);

            // Act
            var result = await _service.AddContactInfoAsync(personId, contactInfoDto);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.AddContactInfoAsync(personId, contactInfo), Times.Once());
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Arrange
            var contactInfoDto = new ContactInfoDto();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddContactInfoAsync(Guid.Empty, contactInfoDto));
        }

        [Fact]
        public async Task AddContactInfoAsync_ShouldThrowArgumentNullException_WhenContactInfoDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddContactInfoAsync(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldRemoveContactInfo_WhenInputIsValid()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteContactInfoAsync(personId, contactInfoId)).ReturnsAsync(true);

            // Act
            var result = await _service.RemoveContactInfoAsync(personId, contactInfoId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteContactInfoAsync(personId, contactInfoId), Times.Once());
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveContactInfoAsync(Guid.Empty, Guid.NewGuid()));
        }

        [Fact]
        public async Task RemoveContactInfoAsync_ShouldThrowArgumentException_WhenContactInfoIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveContactInfoAsync(Guid.NewGuid(), Guid.Empty));
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldReturnPersons_WhenLocationIsValid()
        {
            // Arrange
            var location = "Istanbul";
            var persons = new List<Person> { new Person { Id = Guid.NewGuid(), FirstName = "John" } };
            _mockRepository.Setup(r => r.GetByLocationAsync(location)).ReturnsAsync(persons);

            // Act
            var result = await _service.GetByLocationAsync(location);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(persons.Count, result.Count);
        }

        [Fact]
        public async Task GetByLocationAsync_ShouldThrowArgumentException_WhenLocationIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByLocationAsync(""));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePerson_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var updateDto = new UpdateContactDto { FirstName = "Updated", LastName = "Doe" };
            var existingPerson = new Person { Id = personId, FirstName = "John" };
            _mockRepository.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(existingPerson);
            _mockMapper.Setup(m => m.Map(updateDto, existingPerson));

            // Act
            var result = await _service.UpdateAsync(personId, updateDto);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.UpdateAsync(existingPerson), Times.Once());
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once());
            _mockKafkaProducer.Verify(k => k.SendMessageAsync("contact-updated", personId.ToString()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var updateDto = new UpdateContactDto();
            _mockRepository.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync((Person)null);

            // Act
            var result = await _service.UpdateAsync(personId, updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(Guid.Empty, new UpdateContactDto()));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenUpdateDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(Guid.NewGuid(), null));
        }
    }
}
