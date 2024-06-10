using Microsoft.AspNetCore.Identity;

namespace LessonUG.Models
{
    public class User: IdentityUser
    {
        public string Email { get; set; }
        public string SchoolIndex { get; set;}
        public List <Lesson> Lessons { get; set; }

    }
}
