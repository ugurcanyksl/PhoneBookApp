using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Services
{
    public interface IContactImplementationService
    {
        Task<Person> CreateAsync(CreateContactDto dto);
        Task<Person> GetByIdAsync(Guid id);
        Task<List<Person>> GetAllAsync();
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AddContactInfoAsync(Guid personId, ContactInfoDto contactInfoDto);
        Task<bool> RemoveContactInfoAsync(Guid personId, Guid contactInfoId);
        Task<List<Person>> GetByLocationAsync(string location);
        Task<bool> UpdateAsync(Guid personId, UpdateContactDto updateDto);
    }
}
