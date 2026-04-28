using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to user class assignments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the UserClassController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to manage user class assignments. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to perform database operations such as adding, retrieving, editing, and deleting user class assignments when handling API requests. This setup is essential for the functionality of the various endpoints in this controller, which rely on the database context to manage user class data effectively.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public UserClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Handles POST requests to assign a user to a class. This method accepts a UserClass object in the request body, which contains information about the user, class, and schedule. The method first checks if the user is already assigned to the same class at the same time, and if so, it returns a message indicating that the user is already assigned. It then checks for overlapping class times for the same user, and if any overlap is found, it returns a bad request response indicating that the selected time overlaps with another class. If there are no conflicts, it adds the new user class assignment to the database and saves the changes, returning the created assignment in the response. This endpoint allows for efficient management of user class assignments
        /// </summary>
        /// <param name="userClass">The user class assignment to create.</param>
        /// <returns>The created user class assignment.</returns>
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

        /// <summary>
        /// Handles GET requests to retrieve a list of class assignments
        /// </summary>
        /// <param name="userCode">The code of the user for whom to retrieve class assignments.</param>
        /// <returns>A list of class assignments for the specified user.</returns>
        [HttpGet("{userCode}")]
        public async Task<IActionResult> GetUserClasses(string userCode)
        {
            var userClasses = await _context.UserClasses
                .Include(uc => uc.Class)
                .Where(uc => uc.UserCode == userCode)
                .ToListAsync();
            return Ok(userClasses);
        }


        /// <summary>
        /// Handles PUT requests to edit an existing user class assignment. This method accepts the ID of the user class assignment to edit and a UserClass object in the request body containing the updated information. The method first retrieves the existing user class assignment from the database using the provided ID. If the assignment does not exist, it returns a not found response. It then checks for overlapping class times for the same user, excluding the current assignment being edited, and if any overlap is found, it returns a bad request response indicating that the selected time overlaps with another class. If there are no conflicts, it updates the existing user class assignment with the new information and saves the changes to the database, returning the updated assignment in the response. This endpoint allows for efficient management of user class assignments
        /// </summary>
        /// <param name="id">The ID of the user class assignment to edit.</param>
        /// <param name="userClass">The updated user class assignment information.</param>
        /// <returns>The updated user class assignment.</returns>
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

        /// <summary>
        /// Handles DELETE requests to remove a user class assignment. This method accepts the ID of the user class assignment to delete as a parameter. It first retrieves the user class assignment from the database using the provided ID. If the assignment does not exist, it returns a not found response. If the assignment exists, it removes it from the database and saves the changes, returning an OK response to indicate that the deletion was successful. This endpoint allows for efficient management of user class assignments by enabling the removal of assignments that are no longer needed or were created in error.
        /// </summary>
        /// <param name="id">The ID of the user class assignment to delete.</param>
        /// <returns>The deleted user class assignment.</returns>
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