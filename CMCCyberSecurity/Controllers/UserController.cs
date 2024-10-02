using CMCCyberSecurity.Helpers;
using CMCCyberSecurity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CMCCyberSecurity.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly CMCCyberSecurityContext _context;
        private readonly IConfiguration _configuration;

        public UserController(CMCCyberSecurityContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new HttpStatusException("User is not found", EnumHelper.ECode.NotFound);
            return user;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                throw new HttpStatusException("User Id not exist", EnumHelper.ECode.BadRequest);
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(ex.Message, EnumHelper.ECode.InternalServerError);
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var keySHA = _configuration.GetSection("CMCCyberSecurity:keySHA").Value?.ToString();
            var HMACSHA256 = HMACUtil.HmacGenerator(user.Password, keySHA, "SHA256");
            user.Password = HMACSHA256;
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new HttpStatusException("User is not found", EnumHelper.ECode.NotFound);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
