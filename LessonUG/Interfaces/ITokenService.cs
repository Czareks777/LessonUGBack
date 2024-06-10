using LessonUG.Models;

namespace LessonUG.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user, TimeSpan expiration);
    }
}
