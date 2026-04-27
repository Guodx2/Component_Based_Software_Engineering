using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignClass([FromBody] UserClass userClass)
        {
            var exists = await _context.UserClasses.AnyAsync(uc =>
                uc.UserCode == userClass.UserCode &&
                uc.ClassCode == userClass.ClassCode &&
                uc.StartTime == userClass.StartTime &&
                uc.EndTime == userClass.EndTime
            );
            if (exists)
            {
                return Ok("Naudotojas jau yra priskirtas šiam užsiėmimui.");
            }

            bool isOverlapping = await _context.UserClasses.AnyAsync(uc =>
                uc.UserCode == userClass.UserCode &&
                userClass.StartTime < uc.EndTime &&
                uc.StartTime < userClass.EndTime
            );
            
            if (isOverlapping)
            {
                return BadRequest("Pasirinktas laikas persidengia su kitu užsiėmimu.");
            }

            _context.UserClasses.Add(userClass);
            await _context.SaveChangesAsync();
            return Ok(userClass);
        }

        [HttpGet("{userCode}")]
        public async Task<IActionResult> GetUserClasses(string userCode)
        {
            var userClasses = await _context.UserClasses
                .Include(uc => uc.Class)
                .Where(uc => uc.UserCode == userCode)
                .ToListAsync();
            return Ok(userClasses);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditUserClass(int id, [FromBody] UserClass userClass)
        {
            var existingUserClass = await _context.UserClasses.FindAsync(id);
            if (existingUserClass == null)
            {
                return NotFound();
            }

            bool isOverlapping = await _context.UserClasses
                .Where(uc => uc.Id != id && uc.UserCode == existingUserClass.UserCode)
                .AnyAsync(uc => userClass.StartTime < uc.EndTime && uc.StartTime < userClass.EndTime);
            
            if (isOverlapping)
            {
                return BadRequest("Pasirinktas laikas persidengia su kitu užsiėmimu.");
            }

            existingUserClass.Department = userClass.Department;
            existingUserClass.Auditorium = userClass.Auditorium;
            existingUserClass.StartTime = userClass.StartTime;
            existingUserClass.EndTime = userClass.EndTime;
            existingUserClass.Teacher = userClass.Teacher;
            existingUserClass.Duration = userClass.Duration;
            existingUserClass.Type = userClass.Type;

            await _context.SaveChangesAsync();
            return Ok(existingUserClass);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserClass(int id)
        {
            var userClass = await _context.UserClasses.FindAsync(id);
            if (userClass == null)
            {
                return NotFound();
            }

            // Only allow deletion if the class has not started yet
            /*if (userClass.StartTime <= DateTime.Now)
            {
                return BadRequest("Cannot delete a class that has already started.");
            }*/

            _context.UserClasses.Remove(userClass);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}