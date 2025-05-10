using AutoMapper;
using ContactService.Contact.API.Repositories;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Services
{
    public class ContactImplementationService : IContactImplementationService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;

        public ContactImplementationService(IContactRepository contactRepository, IMapper mapper)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
        }

        public async Task<Person> CreateAsync(CreateContactDto dto)
        {
            var person = _mapper.Map<Person>(dto);
            await _contactRepository.AddAsync(person);
            await _contactRepository.SaveChangesAsync();
            return person;
        }

        public async Task<Person> GetByIdAsync(Guid id)
        {
            return await _contactRepository.GetByIdAsync(id);
        }

        public async Task<List<Person>> GetAllAsync()
        {
            return await _contactRepository.GetAllAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _contactRepository.DeleteAsync(id);
        }

        public async Task<bool> AddContactInfoAsync(Guid personId, ContactInfoDto contactInfoDto)
        {
            var contactInfo = _mapper.Map<ContactInfo>(contactInfoDto);
            return await _contactRepository.AddContactInfoAsync(personId, contactInfo);
        }

        public async Task<bool> RemoveContactInfoAsync(Guid personId, Guid contactInfoId)
        {
            return await _contactRepository.DeleteContactInfoAsync(personId, contactInfoId);
        }
    }

}
