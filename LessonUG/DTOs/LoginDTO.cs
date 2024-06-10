﻿using System.ComponentModel.DataAnnotations;

namespace LessonUG.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
       
    }
}
