using ContactService.Contact.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Repositories
{
    using PhoneBookMicroservices.Shared.Models;
    using Microsoft.EntityFrameworkCore;

    namespace ContactService.Contact.API.Repositories
    {
        public class ContactRepository : IContactRepository
        {
            private readonly ContactDbContext _dbContext;

            public ContactRepository(ContactDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Person> GetByIdAsync(Guid id) =>
                await _dbContext.Contacts.Include(c => c.ContactInfos).FirstOrDefaultAsync(c => c.Id == id);

            public async Task<List<Person>> GetAllAsync() =>
                await _dbContext.Contacts.Include(c => c.ContactInfos).ToListAsync();

            public async Task AddAsync(Person contact)
            {
                await _dbContext.Contacts.AddAsync(contact);
                await SaveChangesAsync();
            }

            public async Task<bool> DeleteAsync(Guid id)
            {
                var contact = await GetByIdAsync(id);
                if (contact == null) return false;
                _dbContext.Contacts.Remove(contact);
                await SaveChangesAsync();
                return true;
            }

            public async Task<bool> AddContactInfoAsync(Guid personId, ContactInfo contactInfo)
            {
                var person = await _dbContext.Contacts.Include(c => c.ContactInfos)
                                                      .FirstOrDefaultAsync(c => c.Id == personId);
                if (person != null)
                {
                    person.ContactInfos.Add(contactInfo);
                    await SaveChangesAsync();
                    return true;
                }
                return false;
            }

            public async Task<bool> DeleteContactInfoAsync(Guid personId, Guid contactInfoId)
            {
                var person = await _dbContext.Contacts.Include(c => c.ContactInfos)
                                                      .FirstOrDefaultAsync(c => c.Id == personId);
                if (person != null)
                {
                    var contactInfo = person.ContactInfos.FirstOrDefault(ci => ci.Id == contactInfoId);
                    if (contactInfo != null)
                    {
                        person.ContactInfos.Remove(contactInfo);
                        await SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }

            public async Task SaveChangesAsync() =>
                await _dbContext.SaveChangesAsync();
        }
    }


}
