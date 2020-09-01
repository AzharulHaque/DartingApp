using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTO;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _authRepository = authRepository;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            //Validate request

            //Validate User
            userDto.Username = userDto.Username.ToLower();
            if (await _authRepository.UserExist(userDto.Username))
            {
                return BadRequest("User already exist");
            }

            //Create user
            var user = new User
            {
                UserName = userDto.Username
            };

            //Register user
            var createUser = await _authRepository.Register(user, userDto.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            //Getting the user from repo once login is successfull
            var userFromRepo = await _authRepository.Login(userLoginDto.UserName.ToLower(), userLoginDto.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            //Creating a secutiy claim based on the UserId and UserName so that once the user is authentictaed
            //next time it can work with authorized token by looking into the token and getting the details 
            //of username and id
            //Crating the cliam
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            //Creating the key for hashing the generated token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            //Signing the key
            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //Adding the expiry for token
            var tokenDecriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credential,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1)
            };

            //token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //Creating the token with the help of token handler
            var token = tokenHandler.CreateToken(tokenDecriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}
