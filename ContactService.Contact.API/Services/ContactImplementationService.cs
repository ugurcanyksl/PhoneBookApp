using AutoMapper;
using ContactService.Contact.API.Repositories;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactService.Contact.API.Services
{
    public class ContactImplementationService : IContactImplementationService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly IKafkaProducerService _kafkaProducerService;

        public ContactImplementationService(IContactRepository contactRepository, IMapper mapper, IKafkaProducerService kafkaProducerService)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
        }

        public async Task<Person> CreateAsync(CreateContactDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrEmpty(dto.FirstName))
                throw new ArgumentException("FirstName cannot be empty.", nameof(dto.FirstName));

            var person = _mapper.Map<Person>(dto);
            person.Id = Guid.NewGuid();

            await _contactRepository.AddAsync(person);
            await _contactRepository.SaveChangesAsync();

            await _kafkaProducerService.SendMessageAsync("contact-created", person.Id.ToString());

            return person;
        }

        public async Task<Person> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(id));

            return await _contactRepository.GetByIdAsync(id);
        }

        public async Task<List<Person>> GetAllAsync()
        {
            return await _contactRepository.GetAllAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(id));

            return await _contactRepository.DeleteAsync(id);
        }

        public async Task<bool> AddContactInfoAsync(Guid personId, ContactInfoDto contactInfoDto)
        {
            if (personId == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

            if (contactInfoDto == null)
                throw new ArgumentNullException(nameof(contactInfoDto));

            var contactInfo = _mapper.Map<ContactInfo>(contactInfoDto);
            return await _contactRepository.AddContactInfoAsync(personId, contactInfo);
        }

        public async Task<bool> RemoveContactInfoAsync(Guid personId, Guid contactInfoId)
        {
            if (personId == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

            if (contactInfoId == Guid.Empty)
                throw new ArgumentException("ContactInfo ID cannot be empty.", nameof(contactInfoId));

            return await _contactRepository.DeleteContactInfoAsync(personId, contactInfoId);
        }

        public async Task<List<Person>> GetByLocationAsync(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("Location cannot be null or empty.", nameof(location));

            return await _contactRepository.GetByLocationAsync(location);
        }

        public async Task<bool> UpdateAsync(Guid personId, UpdateContactDto updateDto)
        {
            if (personId == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var existingPerson = await _contactRepository.GetByIdAsync(personId);
            if (existingPerson == null)
                return false;

            _mapper.Map(updateDto, existingPerson);

            await _contactRepository.UpdateAsync(existingPerson);
            await _contactRepository.SaveChangesAsync();

            await _kafkaProducerService.SendMessageAsync("contact-updated", existingPerson.Id.ToString());

            return true;
        }
    }
}