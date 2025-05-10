using AutoMapper;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;
using ReportService.Report.API.Dtos;

namespace ReportService.Report.API.Infrastructure.AutoMapper
{
    public class ConfigureAutoMapper : Profile
    {
        public ConfigureAutoMapper()
        {
            // Contact mappings
            CreateMap<Person, ContactDto>();
            CreateMap<CreateContactDto, Person>();

            // Report mappings
            CreateMap<ContactReport, ReportDto>();
            CreateMap<ReportRequestDto, ContactReport>();
        }
    }

}
