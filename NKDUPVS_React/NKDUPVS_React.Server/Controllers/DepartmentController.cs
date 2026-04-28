using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using System.Threading.Tasks;
using System.Linq;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller for managing department-related operations such as retrieving department information. This controller provides endpoints for clients to access department data stored in the database, allowing them to display department options or details as needed.
    /// </summary>
    [ApiController]
    [Route("api/departments")]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DepartmentController class with the specified database context. This constructor is used to inject the ApplicationDbContext, which allows the controller to interact with the database and perform operations related to departments.
        /// </summary>
        /// <param name="context">The database context.</param>
        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Retrieves a list of all departments from the database and returns them to the client. The departments are returned in a format suitable for populating dropdowns or selection lists, with each department represented as an object containing a value (the department ID) and a label (the department name). This method is typically used to provide users with options for selecting their department when creating or updating their profile or when filtering data by department.
        /// </summary>
        /// <returns>A list of all departments.</returns>
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new { value = d.id_Departments.ToString(), label = d.name })
                .ToListAsync();
            return Ok(departments);
        }
    }
}