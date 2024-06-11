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
using Microsoft.Extensions.Logging;

namespace LessonUG.Controllers
{
    [Route("api/lessons")]
    [ApiController]
    public class LessonController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LessonController> _logger;

        public LessonController(DataContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ILogger<LessonController> logger)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
        {
            try
            {
                return await _context.Lessons
                    .Include(l => l.Users)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while getting lessons.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDTO>> GetLesson(int id)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while getting lesson with id {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> PutLesson(int id, LessonDTO lessonDTO)
        {
            try
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

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError($"Concurrency error occurred while updating lesson with id {id}.");
                    return StatusCode(500, "Internal server error");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while updating lesson with id {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<LessonDTO>> PostLesson(LessonDTO lessonDTO)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while creating a lesson.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while deleting lesson with id {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{lessonId}/join/{userId}")]
        public async Task<IActionResult> JoinLesson(int lessonId, string userId)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while user {userId} joining lesson {lessonId}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{lessonId}/leave/{userId}")]
        public async Task<IActionResult> LeaveLesson(int lessonId, string userId)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while user {userId} leaving lesson {lessonId}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{lessonId}/remove/{userId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RemoveUserFromLesson(int lessonId, string userId)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while removing user {userId} from lesson {lessonId}.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("myLessons")]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetMyLessons()
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while getting lessons for the current user.");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
}
