using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile()
        {
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<Course, CourseForCreationDto>().ReverseMap();          
            CreateMap<Entities.Course, Models.CourseForUpdateDto>().ReverseMap();
        }
    }
}
