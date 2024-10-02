using CMCCyberSecurity.DTO;
using CMCCyberSecurity.Helpers;
using CMCCyberSecurity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CMCCyberSecurity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CMCCyberSecurityContext _context;

        public AuthController(IConfiguration configuration, CMCCyberSecurityContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO user)
        {
            var userCheck = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userCheck != null)
            {
                throw new HttpStatusException("Email exist", EnumHelper.ECode.BadRequest);
            }

            var keySHA = _configuration.GetSection("CMCCyberSecurity:keySHA").Value
                ?.ToString();
            var HMACSHA256 = HMACUtil.HmacGenerator(user.Password, keySHA, "SHA256");
            await _context.Users.AddAsync(new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = HMACSHA256,
                Role = false
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO user)
        {
            var keySHA = _configuration.GetSection("CMCCyberSecurity:keySHA").Value?.ToString();
            var HMACSHA256 = HMACUtil.HmacGenerator(user.Password, keySHA, "SHA256");

            var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == HMACSHA256)
                ?? throw new HttpStatusException("Invalid email or password", EnumHelper.ECode.BadRequest);
            var token = GenerateJwtToken(userCheck);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role ? "Admin" : "User") // Add role claim
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
