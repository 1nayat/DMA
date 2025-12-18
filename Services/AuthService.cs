using DMA.Data;
using DMA.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DMA.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly AppDbContext context;
        public AuthService(IConfiguration configuration, AppDbContext context)
        {
            this.configuration = configuration;
            this.context = context;
        }



        public async Task<User?> RegisterAsync(RegisterUserDto request)
        {
            if (await context.Users.AnyAsync(u => u.UserName == request.UserName))
                return null;
            var user = new User();
            user.UserName = request.UserName;
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.Email = request.Email;
           // user.Role = request.Role ?? "User";
            user.CreatedAt = DateTime.UtcNow;
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }



        public async Task<string> LoginAsync(LoginUserDto request)
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user is null)
                return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed
                )
                return null;

            string token = CreateToken(user);

            return token;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.Name,user.UserName),

            new Claim(ClaimTypes.Role,user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
               audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                ); 
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}

 