using ContactService.Contact.API.Repositories;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ContactService.Tests.Repositories
{
    public class ContactRepositoryMockTests
    {
        private readonly Mock<IContactRepository> _mockRepository;
        private readonly IContactRepository _repository;

        public ContactRepositoryMockTests()
        {
            _mockRepository = new Mock<IContactRepository>();
            _repository = _mockRepository.Object;
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PhoneBookMicroservices.Shared.Models.Person)null);

            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once());
        }
    }
}