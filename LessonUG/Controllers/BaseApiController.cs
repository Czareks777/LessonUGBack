using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LessonUG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("Lesson")]
    public class BaseApiController:ControllerBase
    {
    }
}
