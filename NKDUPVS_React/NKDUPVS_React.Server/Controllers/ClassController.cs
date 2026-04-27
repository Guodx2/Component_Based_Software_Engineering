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
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassController> _logger;

        public ClassController(ApplicationDbContext context, ILogger<ClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.Classes.ToListAsync();
            return Ok(classes);
        }

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