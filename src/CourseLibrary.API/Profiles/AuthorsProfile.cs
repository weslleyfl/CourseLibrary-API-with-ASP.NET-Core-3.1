using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(
                   destinationMember: dest => dest.Name,
                   memberOptions: opt => opt.MapFrom(src => $"{src.FirstName } {src.LastName}"))
                .ForMember(
                   destinationMember: dest => dest.Age,
                   memberOptions: opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)))
                .ReverseMap();

            CreateMap<Author, AuthorForCreationDto>().ReverseMap();
            CreateMap<Models.AuthorForCreationWithDateOfDeathDto, Entities.Author>().ReverseMap();
            CreateMap<Entities.Author, Models.AuthorFullDto>().ReverseMap();

        }
    }
}
