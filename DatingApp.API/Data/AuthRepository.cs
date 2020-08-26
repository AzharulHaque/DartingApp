using DatingApp.API.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _db;
        public AuthRepository(DataContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Authenticating existing user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<User> Login(string userName, string password)
        {
            //Get the user from db with provided username
            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserName == userName);

            //If the user is null then no user exist
            if(user == null)
            {
                return null;
            }

            //Gets the hashed password for the user for authenticating the user
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Checks if the password provided is authenticated succesfully
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        /// <returns></returns>
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                bool verificationSuccess = true;
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i])
                    {
                        verificationSuccess = false;
                    }
                }
                return verificationSuccess;
            }
        }

        /// <summary>
        /// Registering the new user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<User> Register(User user , string password)
        {
            byte[] passwordHash, passwordSalt;

            //Craating passwordHash and salt for the password provided by the user
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            //Assiging paswordhash and salt for the user for authenticaiton later
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// //Craating passwordHash and salt for the password provided by the user
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <param name="passwordSalt"></param>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Checks if user exist or not
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> UserExist(string userName)
        {
            if(await _db.Users.AnyAsync(x=>x.UserName == userName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
