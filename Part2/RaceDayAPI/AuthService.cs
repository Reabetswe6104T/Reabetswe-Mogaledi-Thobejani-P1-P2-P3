using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Services
{
    public class AuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(RaceDayDbContext context, IConfiguration config)
        { _context = context; _config = config; }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return new AuthResponseDTO { Success = false, Message = "Email already registered." };
            if (dto.Role != "Participant" && dto.Role != "Organiser")
                return new AuthResponseDTO { Success = false, Message = "Role must be Participant or Organiser." };

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (dto.Role == "Participant")
                _context.Participants.Add(new Participant { UserID = user.UserID, FullName = dto.FullName ?? "Unknown", IDNumber = dto.IDNumber ?? "0000000000000", PhoneNumber = dto.PhoneNumber, DateOfBirth = dto.DateOfBirth, Gender = dto.Gender });
            else
                _context.Organisers.Add(new Organiser { UserID = user.UserID, OrganisationName = dto.OrganisationName ?? "Unknown", RegistrationNumber = dto.RegistrationNumber, ContactPerson = dto.ContactPerson ?? "Unknown", PhoneNumber = dto.PhoneNumber, Email = dto.Email, Website = dto.Website });

            await _context.SaveChangesAsync();
            return new AuthResponseDTO { Success = true, Message = "Registration successful.", UserID = user.UserID, Email = user.Email, Role = user.Role, Token = GenerateToken(user) };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return new AuthResponseDTO { Success = false, Message = "Invalid email or password." };
            if (!user.IsActive) return new AuthResponseDTO { Success = false, Message = "Account is inactive." };
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new AuthResponseDTO { Success = true, Message = "Login successful.", UserID = user.UserID, Email = user.Email, Role = user.Role, Token = GenerateToken(user) };
        }

        private string GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);
            var handler = new JwtSecurityTokenHandler();
            var desc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return handler.WriteToken(handler.CreateToken(desc));
        }
    }
}
