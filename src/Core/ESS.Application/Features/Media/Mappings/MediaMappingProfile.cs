using AutoMapper;
using ESS.Domain.Entities.Media; // Ensure this is the correct namespace for your Media entity
using ESS.Application.Features.Media.DTOs;

namespace ESS.Application.Features.Media.Mappings;

public class MediaMappingProfile : Profile
{
    public MediaMappingProfile()
    {
        CreateMap<ESS.Domain.Entities.Media.Media, MediaDto>() // Fully qualify the Media type
            .ForMember(dest => dest.FileName,
                opt => opt.MapFrom(src => src.File.FileName))
            .ForMember(dest => dest.FileType,
                opt => opt.MapFrom(src => src.File.FileType))
            .ForMember(dest => dest.MimeType,
                opt => opt.MapFrom(src => src.File.MimeType))
            .ForMember(dest => dest.Size,
                opt => opt.MapFrom(src => src.File.Size));
    }
}