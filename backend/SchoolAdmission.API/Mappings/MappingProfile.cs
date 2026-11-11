
using AutoMapper;
using SchoolAdmission.API.DTOs.Responses;
using SchoolAdmission.Business.Models;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Class mappings
        CreateMap<AdmissionClass, CreateClassResponse>();

        CreateMap<ClassSeatSummary, ClassResponse>();

        // Hold mapping
        CreateMap<AdmissionHold, CreateHoldResponse>()
            .ForMember(
                dest => dest.HoldId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(
                    src => src.Status.ToString().ToUpperInvariant()))
            .ForMember(
                dest => dest.ExpiresAt,
                opt => opt.MapFrom(src => src.ExpiresAtUtc));
    }
}
