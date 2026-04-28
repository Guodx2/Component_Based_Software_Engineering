using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Logging; 

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller for managing class-related operations such as retrieving class information and creating new classes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassController> _logger;

        /// <summary>
        /// Initializes a new instance of the ClassController class with the specified database context and logger.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        public ClassController(ApplicationDbContext context, ILogger<ClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all classes from the database and returns them to the client. This method is typically used to display available classes to users.
        /// </summary>
        /// <returns>A list of all classes.</returns>
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.Classes.ToListAsync();
            return Ok(classes);
        }

        /// <summary>
        /// Creates a new class in the database based on the provided class information. If no code is provided, a unique code will be generated. If a class with the same code already exists, it will return the existing class instead of creating a new one. This method is used to allow users to create new classes for their courses or groups.
        /// </summary>
        /// <param name="newClass">The class to create.</param>
        /// <returns>The created class.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] Class newClass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // If no code was provided, generate a new unique code.
            if (string.IsNullOrWhiteSpace(newClass.Code))
            {
                newClass.Code = "CLS" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }

            // Check if a class with the provided code already exists.
            var existingClass = await _context.Classes.FindAsync(newClass.Code);
            if (existingClass != null)
            {
                // If it exists, just return the existing class.
                return Ok(existingClass);
            }

            try
            {
                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while saving the class.");
                throw;
            }
            return Ok(newClass);
        }
    }
}