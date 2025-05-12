using ContactService.Contact.API.Controllers;
using ContactService.Contact.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ContactService.Tests.Controllers
{
    public class ContactControllerTests
    {
        private readonly Mock<IContactRepository> _mockRepository;
        private readonly Mock<ILogger<ContactController>> _mockLogger;
        private readonly ContactController _controller;

        public ContactControllerTests()
        {
            _mockRepository = new Mock<IContactRepository>();
            _mockLogger = new Mock<ILogger<ContactController>>();
            _controller = new ContactController(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateContact_ReturnsCreatedAtAction_WhenDtoIsValid()
        {
            // Arrange
            var dto = new CreateContactDto { FirstName = "John", LastName = "Doe", Company = "ABC Corp" };
            var person = new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Company = "ABC Corp" };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Person>())).Callback<Person>(p => p.Id = person.Id);

            // Act
            var result = await _controller.CreateContact(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Person>(createdResult.Value);
            Assert.Equal(person.Id, returnValue.Id);
            Assert.Equal(nameof(_controller.GetContactById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateContact_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "Required");
            var dto = new CreateContactDto();

            // Act
            var result = await _controller.CreateContact(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetContactById_ReturnsOk_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var person = new Person { Id = personId, FirstName = "John", ContactInfos = new List<ContactInfo>() };
            _mockRepository.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(person);

            // Act
            var result = await _controller.GetContactById(personId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ContactDto>(okResult.Value);
            Assert.Equal(personId, returnValue.Id);
        }

        [Fact]
        public async Task GetContactById_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync((Person)null);

            // Act
            var result = await _controller.GetContactById(personId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllContacts_ReturnsOk_WithPagedResult()
        {
            // Arrange
            var page = 1;
            var pageSize = 10;
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", ContactInfos = new List<ContactInfo>() }
            };
            _mockRepository.Setup(r => r.GetAllPagedAsync(page, pageSize)).ReturnsAsync(persons);

            // Act
            var result = await _controller.GetAllContacts(page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<object>(okResult.Value);
            var dataProperty = returnValue.GetType().GetProperty("Data").GetValue(returnValue) as List<ContactDto>;
            Assert.NotNull(dataProperty);
            Assert.Equal(persons.Count, dataProperty.Count);
            Assert.Equal(page, returnValue.GetType().GetProperty("Page").GetValue(returnValue));
            Assert.Equal(pageSize, returnValue.GetType().GetProperty("PageSize").GetValue(returnValue));
        }

        [Fact]
        public async Task DeleteContact_ReturnsNoContent_WhenPersonExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(personId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteContact(personId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteContact_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteAsync(personId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteContact(personId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsOk_WhenInputIsValid()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto { InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            var contactInfo = new ContactInfo { Id = Guid.NewGuid(), InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            _mockRepository.Setup(r => r.AddContactInfoAsync(personId, It.IsAny<ContactInfo>())).ReturnsAsync(true);

            // Act
            var result = await _controller.AddContactInfo(personId, contactInfoDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ContactInfo>(okResult.Value);
            Assert.Equal(contactInfoDto.InfoContent, returnValue.InfoContent);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var personId = Guid.NewGuid();
            _controller.ModelState.AddModelError("InfoType", "Required");
            var contactInfoDto = new ContactInfoDto();

            // Act
            var result = await _controller.AddContactInfo(personId, contactInfoDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddContactInfo_ReturnsNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var contactInfoDto = new ContactInfoDto { InfoType = InfoType.Email, InfoContent = "john.doe@example.com" };
            _mockRepository.Setup(r => r.AddContactInfoAsync(personId, It.IsAny<ContactInfo>())).ReturnsAsync(false);

            // Act
            var result = await _controller.AddContactInfo(personId, contactInfoDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteContactInfo_ReturnsNoContent_WhenContactInfoExists()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var infoId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteContactInfoAsync(personId, infoId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteContactInfo(personId, infoId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteContactInfo_ReturnsNotFound_WhenContactInfoDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var infoId = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteContactInfoAsync(personId, infoId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteContactInfo(personId, infoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetContactsByLocation_ReturnsOk_WithContactList()
        {
            // Arrange
            var location = "Istanbul";
            var persons = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), FirstName = "John", ContactInfos = new List<ContactInfo>() }
            };
            _mockRepository.Setup(r => r.GetByLocationAsync(location)).ReturnsAsync(persons);

            // Act
            var result = await _controller.GetContactsByLocation(location);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ContactDto>>(okResult.Value);
            Assert.Equal(persons.Count, returnValue.Count);
        }

        [Fact]
        public void HandleException_ReturnsStatusCode500_WithErrorMessage()
        {
            // Arrange
            var exception = new Exception("Test error");

            // Act
            var result = _controller.HandleException(exception);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            var returnValue = Assert.IsAssignableFrom<object>(statusResult.Value);
            Assert.Equal("Test error", returnValue.GetType().GetProperty("message").GetValue(returnValue));
        }

        [Fact]
        public async Task CreateContact_LogsWarning_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "Required");
            var dto = new CreateContactDto();
            _mockLogger.Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid model state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            var result = await _controller.CreateContact(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid model state")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
        }
    }
}