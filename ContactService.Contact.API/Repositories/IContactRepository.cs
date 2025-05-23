﻿using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Repositories
{
    public interface IContactRepository
    {
        Task<Person> GetByIdAsync(Guid id);
        Task<List<Person>> GetAllAsync();
        Task AddAsync(Person contact);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AddContactInfoAsync(Guid personId, ContactInfo contactInfo);
        Task<bool> DeleteContactInfoAsync(Guid personId, Guid infoId);
        Task SaveChangesAsync();
        Task<List<Person>> GetByLocationAsync(string location);
        Task<List<Person>> GetAllPagedAsync(int page, int pageSize);
        Task UpdateAsync(Person person);
    }
}
