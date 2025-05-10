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
        private readonly KafkaProducerService _kafkaProducerService;

        public ContactImplementationService(IContactRepository contactRepository, IMapper mapper, KafkaProducerService kafkaProducerService)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task<Person> CreateAsync(CreateContactDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Company))
            {
                throw new ArgumentException("FirstName, LastName ve Company alanları boş olamaz.");
            }

            var person = _mapper.Map<Person>(dto);
            await _contactRepository.AddAsync(person);
            await _contactRepository.SaveChangesAsync();

            var message = $"New contact created: {person.FirstName} {person.LastName} at {person.Company}";
            var topic = "contact-topic";

            await _kafkaProducerService.SendMessageAsync(topic, message);

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
