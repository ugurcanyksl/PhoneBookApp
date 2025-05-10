using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;

        public ContactController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        // Kişi oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDto createContactDto)
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = createContactDto.FirstName,
                LastName = createContactDto.LastName,
                Company = createContactDto.Company,
                ContactInfos = new List<ContactInfo>()
            };

            await _contactRepository.AddAsync(person);
            return CreatedAtAction(nameof(GetContactById), new { id = person.Id }, person);
        }

        // Kişi bilgisi getirme
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(Guid id)
        {
            var person = await _contactRepository.GetByIdAsync(id);
            if (person == null) return NotFound();

            var contactDto = new ContactDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Company = person.Company,
                ContactInfos = person.ContactInfos.Select(c => new ContactInfoDto
                {
                    Id = c.Id,
                    InfoType = c.InfoType,
                    InfoContent = c.InfoContent
                }).ToList()
            };

            return Ok(contactDto);
        }

        // Tüm kişileri listeleme
        [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            var persons = await _contactRepository.GetAllAsync();
            var contactDtos = persons.Select(p => new ContactDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Company = p.Company,
                ContactInfos = p.ContactInfos.Select(c => new ContactInfoDto
                {
                    Id = c.Id,
                    InfoType = c.InfoType,
                    InfoContent = c.InfoContent
                }).ToList()
            }).ToList();

            return Ok(contactDtos);
        }

        // Kişi silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(Guid id)
        {
            var result = await _contactRepository.DeleteAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        // İletişim bilgisi ekleme
        [HttpPost("{personId}/contact-info")]
        public async Task<IActionResult> AddContactInfo(Guid personId, [FromBody] ContactInfoDto contactInfoDto)
        {
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = contactInfoDto.InfoType,
                InfoContent = contactInfoDto.InfoContent
            };

            await _contactRepository.AddContactInfoAsync(personId, contactInfo);
            return Ok(contactInfo);
        }

        // İletişim bilgisi silme
        [HttpDelete("{personId}/contact-info/{infoId}")]
        public async Task<IActionResult> DeleteContactInfo(Guid personId, Guid infoId)
        {
            var result = await _contactRepository.DeleteContactInfoAsync(personId, infoId);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
