using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
            var email = payload.Email;
            var name = payload.Name;
            var picture = payload.Picture;
            var nameParts = name.Split(' ');
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : string.Empty;
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                string code;
                var random = new Random();
                do
                {
                    var letter = (char)random.Next('A', 'Z' + 1);
                    var number = random.Next(1000, 10000);
                    code = $"{letter}{number}";
                } while (_context.Users.Any(u => u.Code == code));

                user = new User
                {
                    Code = code,
                    Username = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Email = email,
                    Name = firstName,
                    LastName = lastName,
                    PhoneNumber = string.Empty,
                    IsAdmin = false,
                    IsVerified = false,
                    IsSubmitted = false,
                    IsMentor = false // default value; later set via accept when admin approves mentor account
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Login Successful",
                code = user.Code,
                email = user.Email,
                name = user.Name,
                username = user.Username,
                lastName = user.LastName,
                picture = picture,
                phoneNumber = user.PhoneNumber,
                isAdmin = user.IsAdmin,
                isVerified = user.IsVerified,
                isSubmitted = user.IsSubmitted,
                isMentor = user.IsMentor  // include the mentor flag here
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Invalid Google token", Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Retrieve the user from the database
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            // Return user details
            return Ok(new
            {
                Message = "Login Successful",
                UserCode = user.Code, // Return the user Code
                Username = user.Username,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber // Ensure phoneNumber is included
            });
        }
        catch (Exception ex)
        {
            // Log the error details
            Console.WriteLine($"Error in Login: {ex.Message}");
            return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
        }
    }

    // Model for token request
    public class GoogleAuthRequest
    {
        public string? Token { get; set; }
    }

    // Model for login request
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}