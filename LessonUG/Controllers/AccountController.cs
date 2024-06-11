using LessonUG.Data;
using LessonUG.DTOs;
using LessonUG.Interfaces;
using LessonUG.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LessonUG.Controllers
{

    public class AccountController :BaseApiController
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(DataContext context, ITokenService tokenService, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    SchoolIndex = model.SchoolIndex
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(result.Errors);
                }

                var assignRoleResult = await _userManager.AddToRoleAsync(user, Roles.Student);

                if (!assignRoleResult.Succeeded)
                {
                    return new BadRequestObjectResult(assignRoleResult.Errors);
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return Ok("Registration successful.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred during registration.");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO model)
        {
            try
            {
                var normalizedEmail = _userManager.NormalizeEmail(model.Email);
                var user = await _userManager.FindByEmailAsync(normalizedEmail);
                if (user == null)
                {
                    return BadRequest(new { message = "Incorrect email." });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Incorrect password." });
                }

                var token = await _tokenService.GenerateJwtToken(user, TimeSpan.FromMinutes(600));

                return Ok(new { Token = token });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred during login.");
                return StatusCode(500, e.Message);
            }
        }
    }
}
