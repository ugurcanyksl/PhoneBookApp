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
            CreateMap<UpdateContactDto, Person>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id güncellenmemeli
                .ForMember(dest => dest.ContactInfos, opt => opt.Ignore()); // ContactInfos güncellenmemeli
            CreateMap<ContactInfoDto, ContactInfo>();

            // Report mappings
            CreateMap<ContactReport, ReportDto>();
            CreateMap<ReportRequestDto, ContactReport>();
        }
    }
}
