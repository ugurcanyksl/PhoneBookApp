using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

namespace ContactService.Contact.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactRepository contactRepository, ILogger<ContactController> logger)
        {
            _contactRepository = contactRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDto createContactDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateContact: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating contact for {FirstName} {LastName}", createContactDto.FirstName, createContactDto.LastName);
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = createContactDto.FirstName,
                LastName = createContactDto.LastName,
                Company = createContactDto.Company,
                ContactInfos = new List<ContactInfo>()
            };

            await _contactRepository.AddAsync(person);
            _logger.LogInformation("Contact created with ID {Id}", person.Id);
            return CreatedAtAction(nameof(GetContactById), new { id = person.Id }, person);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(Guid id)
        {
            _logger.LogInformation("Fetching contact with ID {Id}", id);
            var person = await _contactRepository.GetByIdAsync(id);
            if (person == null)
            {
                _logger.LogWarning("Contact with ID {Id} not found", id);
                return NotFound();
            }

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

        [HttpGet]
        public async Task<IActionResult> GetAllContacts(int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Fetching contacts - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var persons = await _contactRepository.GetAllPagedAsync(page, pageSize);
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

            _logger.LogInformation("Fetched {Count} contacts", contactDtos.Count);
            return Ok(new { Data = contactDtos, Page = page, PageSize = pageSize });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(Guid id)
        {
            _logger.LogInformation("Deleting contact with ID {Id}", id);
            var result = await _contactRepository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Contact with ID {Id} not found for deletion", id);
                return NotFound();
            }

            _logger.LogInformation("Contact with ID {Id} deleted successfully", id);
            return NoContent();
        }

        [HttpPost("{personId}/contact-info")]
        public async Task<IActionResult> AddContactInfo(Guid personId, [FromBody] ContactInfoDto contactInfoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddContactInfo: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Adding contact info for person ID {PersonId}", personId);
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                InfoType = contactInfoDto.InfoType,
                InfoContent = contactInfoDto.InfoContent
            };

            var result = await _contactRepository.AddContactInfoAsync(personId, contactInfo);
            if (!result)
            {
                _logger.LogWarning("Person with ID {PersonId} not found for adding contact info", personId);
                return NotFound();
            }

            _logger.LogInformation("Contact info added for person ID {PersonId}", personId);
            return Ok(contactInfo);
        }

        [HttpDelete("{personId}/contact-info/{infoId}")]
        public async Task<IActionResult> DeleteContactInfo(Guid personId, Guid infoId)
        {
            _logger.LogInformation("Deleting contact info ID {InfoId} for person ID {PersonId}", infoId, personId);
            var result = await _contactRepository.DeleteContactInfoAsync(personId, infoId);
            if (!result)
            {
                _logger.LogWarning("Contact info ID {InfoId} for person ID {PersonId} not found", infoId, personId);
                return NotFound();
            }

            _logger.LogInformation("Contact info ID {InfoId} deleted for person ID {PersonId}", infoId, personId);
            return NoContent();
        }

        [HttpGet("location/{location}")]
        public async Task<IActionResult> GetContactsByLocation(string location)
        {
            _logger.LogInformation("Fetching contacts for location {Location}", location);
            var persons = await _contactRepository.GetByLocationAsync(location);
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

            _logger.LogInformation("Fetched {Count} contacts for location {Location}", contactDtos.Count, location);
            return Ok(contactDtos);
        }

        [NonAction]
        public IActionResult HandleException(Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}
