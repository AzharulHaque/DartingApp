using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTO
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength =4,ErrorMessage ="Please provide a password between 4-8 charecters")]
        public string Password { get; set; }
    }
}
