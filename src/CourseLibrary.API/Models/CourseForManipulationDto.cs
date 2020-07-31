// using CourseLibrary.API.ValidationAttributes;
using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "O título deve ser diferente da descrição")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "Você deve preencher um título")]
        [MaxLength(100, ErrorMessage = "O título não deve ter mais do que 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "A descrição não deve ter mais 1500 characters.")]
        public virtual string Description { get; set; }
    }
}
