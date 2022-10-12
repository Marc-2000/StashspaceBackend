using API.BLL.DTOs;
using API.BLL.RepositoryInterfaces;
using API.DAL.Context;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.BLL.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AccountRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse> Login(UserLoginDTO user)
        {
            User retrievedUser = await _context.Users.Include(pr => pr.UserRoles).ThenInclude(r => r.Role).FirstOrDefaultAsync(x => x.Email.ToLower().Equals(user.Email.ToLower()));

            if (retrievedUser == null || !VerifyPasswordHash(user.Password, retrievedUser.PasswordHash, retrievedUser.PasswordSalt))
            {
                return new ServiceResponse("Credentials are not valid.");
            }
            else
            {
                string token = CreateToken(retrievedUser);
                return new ServiceResponse(retrievedUser, token, "Successfully logged in.");
            }
        }

        public async Task<ServiceResponse> Register(UserRegisterDTO user)
        {
            if (await EmailExists(user.Email))
            {
                return new ServiceResponse("e-mailaddress is already in use.");
            }

            if (await UsernameExists(user.Username))
            {
                return new ServiceResponse("Username is already in use.");
            }

            if (!user.Password.Equals(user.ConfirmPassword))
            {
                return new ServiceResponse("The passwords do not match.");
            }

            //Create password hash with salt
            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User NewUser = new()
            {
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            //Add default role to new user
            Role defaultRole = _context.Roles.FirstOrDefault(x => x.Name.Equals("User"));

            if (defaultRole != null)
            {
                UserRole newPersonRole = new()
                {
                    UserID = NewUser.ID,
                    User = NewUser,
                    RoleID = defaultRole.ID,
                    Role = defaultRole
                };

                //Add user and user-role to database
                await _context.Users.AddAsync(NewUser);
                await _context.UserRoles.AddAsync(newPersonRole);
                await _context.SaveChangesAsync();

                string token = CreateToken(NewUser);

                return new ServiceResponse(NewUser, token, "User registered successfully.");
            }
            else
            {
                return new ServiceResponse("No default role found in database.");
            }

        }

        public async Task<ServiceResponse> Delete(Guid userId)
        {
            User retrievedUser = await _context.Users.FirstOrDefaultAsync(x => x.ID.Equals(userId));

            if (retrievedUser == null)
            {
                return new ServiceResponse("User not found.");
            }
            else
            {
                _context.Users.Remove(retrievedUser);
                await _context.SaveChangesAsync();
                ServiceResponse response = new()
                {
                    Success = true,
                    Message = "Successfully deleted the user."
                };
                return response;
            }
        }

        private async Task<bool> EmailExists(string email)
        {
            //Check if email already exists
            if (await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower())) return true;
            return false;
        }

        private async Task<bool> UsernameExists(string username)
        {
            //Check if username already exists
            if (await _context.Users.AnyAsync(x => x.Email.ToLower() == username.ToLower())) return true;
            return false;
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            //Create password hash with salt
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            //Verify password hash with salt
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private string CreateToken(User user)
        {
            //Set claims for token
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
            };
            if (user.Email != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Email));
            }
            foreach (UserRole role in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            //Add security key to token
            SymmetricSecurityKey key = new(
                Encoding.UTF8.GetBytes(_configuration.GetSection("Auth:Token").Value)
            );

            //Set token credentials
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);

            //Fill token descriptor
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            //Create new tokenHandler and create token including tokenDescriptor
            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            //Return JWT 
            return tokenHandler.WriteToken(token);
        }

        public async Task<User> GetByID(Guid id)
        {
            User retrievedUser = await _context.Users.FirstOrDefaultAsync(x => x.ID == id);
            if (retrievedUser != null)
            {
                User user = new()
                {
                    ID = retrievedUser.ID,
                    Email = retrievedUser.Email
                };
            }
            return retrievedUser;
        }
    }
}
