using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LessonUG.Models
{
    public class User: IdentityUser
    {
        [Required]
        [MaxLength(6, ErrorMessage = "School Index must be 6 characters long")]
        [MinLength(6, ErrorMessage = "School Index must be 6 characters long")]
        public string SchoolIndex { get; set;}
        public List <Lesson> Lessons { get; set; }

    }
}
