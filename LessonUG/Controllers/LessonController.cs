using Microsoft.AspNetCore.Mvc;
using LessonUG.Data;
using LessonUG.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LessonUG.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LessonUG.Controllers
{
    
    public class LessonController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LessonController(DataContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

   
        [HttpGet]

        public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
        {
            return await _context.Lessons
                .Include(l => l.Users)
                .ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDTO>> GetLesson(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            
            var lessonDto = new LessonDTO
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Date = lesson.Date
            };

            return Ok(lessonDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> PutLesson(int id, LessonDTO lessonDTO)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            
            lesson.Title = lessonDTO.Title;
            lesson.Description = lessonDTO.Description;
            lesson.Date = lessonDTO.Date;

            _context.Entry(lesson).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")] // Autoryzacja tylko dla nauczycieli
        public async Task<ActionResult<LessonDTO>> PostLesson(LessonDTO lessonDTO)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var teacher = await _userManager.FindByEmailAsync(userEmail);

            var lesson = new Lesson
            {
                Title = lessonDTO.Title,
                Description = lessonDTO.Description,
                Date = lessonDTO.Date,
                User = teacher, 
                Users = new List<User>()
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

           

            return Ok(lesson);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost("{lessonId}/join/{userId}")]
        public async Task<IActionResult> JoinLesson(int lessonId, string userId)
        {
            var lesson = await _context.Lessons.Include(l => l.Users).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (lesson.Users.Contains(user))
            {
                return BadRequest("User already joined this lesson");
            }

            lesson.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User joined the lesson");
        }


        [HttpPost("{lessonId}/leave/{userId}")]
        public async Task<IActionResult> LeaveLesson(int lessonId, string userId)
        {
            var lesson = await _context.Lessons.Include(l => l.Users).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!lesson.Users.Contains(user))
            {
                return BadRequest("User is not a member of this lesson");
            }

            lesson.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User left the lesson");
        }


        [HttpDelete("{lessonId}/remove/{userId}")]
        [Authorize(Roles = "Teacher")] // Autoryzacja tylko dla nauczycieli
        public async Task<IActionResult> RemoveUserFromLesson(int lessonId, string userId)
        {
            var lesson = await _context.Lessons.Include(l => l.Users).Include(l => l.User).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _userManager.FindByEmailAsync(userEmail);
            if (lesson.User.Id != currentUser.Id)
            {
                return Forbid("You are not the teacher of this lesson");
            }

            var userToRemove = await _context.Users.FindAsync(userId);
            if (userToRemove == null)
            {
                return NotFound("User not found");
            }

            if (!lesson.Users.Contains(userToRemove))
            {
                return BadRequest("User is not a member of this lesson");
            }

            lesson.Users.Remove(userToRemove);
            await _context.SaveChangesAsync();

            return Ok("User removed from the lesson");
        }

        [HttpGet("myLessons")]

        public async Task<ActionResult<IEnumerable<Lesson>>> GetMyLessons()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var lessons = await _context.Lessons
                .Include(l => l.Users)
                .Where(l => l.Users.Any(u => u.Id == currentUser.Id))
                .ToListAsync();

            return lessons;
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}
