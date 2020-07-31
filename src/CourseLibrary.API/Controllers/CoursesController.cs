using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CourseLibrary.API.Models;
using CourseLibrary.API.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Controllers
{
    [Route("api/authors/{authorId}/courses")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [Route("", Name = nameof(GetCoursesForAuthor))]
        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courses = _courseLibraryRepository.GetCourses(authorId);

            if (courses == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));

        }

        [Route("{courseId}", Name = nameof(GetCourseForAuthor))]
        [HttpGet]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound()
;
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));

        }

        [Route("", Name = nameof(CreateCourseForAuthor))]
        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            var salvo = _courseLibraryRepository.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);

            return CreatedAtRoute(
                nameof(GetCourseForAuthor),
                new { authorId = authorId, courseId = courseToReturn.Id },
                courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId,
          Guid courseId,
          CourseForUpdateDto course)
        {
            try
            {
                if (!_courseLibraryRepository.AuthorExists(authorId))
                {
                    return NotFound();
                }

                var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

                //if (courseForAuthorFromRepo == null)
                //{
                //    _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                //    return NotFound();
                //}

                if (courseForAuthorFromRepo == null)
                {
                    var courseToAdd = _mapper.Map<Course>(course);
                    courseToAdd.Id = courseId;

                    _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                    _courseLibraryRepository.Save();

                    var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                    return CreatedAtRoute("GetCourseForAuthor",
                        new { authorId, courseId = courseToReturn.Id },
                        courseToReturn);
                }

                // map the entity to a CourseForUpdateDto
                // apply the update field values to that dto
                // map the CourseForUpdateDto back to an entity
                _mapper.Map(course, courseForAuthorFromRepo);

                _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
                bool alterado = _courseLibraryRepository.Save();

                if (alterado == false)
                    return NotFound();

                return Ok(course);
                //return (alterado == true) ? Ok(course) as IActionResult : NotFound() as IActionResult;

            }
            catch (Exception ex)
            {
                // _logger.LogError($"Something went wrong inside UpdateOwner action: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDocument
            )
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            // Add new Course
            if (courseForAuthorFromRepo == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }

                var courseToAdd = _mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute(nameof(GetCourseForAuthor),
                    new[] { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);
            // add validation
            patchDocument.ApplyTo(courseToPatch, modelState: ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseForAuthorFromRepo);

            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

            var alterado = _courseLibraryRepository.Save();

            if (alterado == false)
                return NotFound();

            return Ok(courseForAuthorFromRepo);

        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();

            return new JsonResult(new
            {
                resposta = "deletado",
                cursoId = courseId,
                autorId = authorId
            });
        }

        /// <summary>
        /// Sobrescrevemos o metodo para poder usar a customizaçao feita no startup para descricao de erros
        /// ConfigureApiBehaviorOptions - setupAction.InvalidModelStateResponseFactory
        /// </summary>     
        public override ActionResult ValidationProblem(
           [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

    }
}