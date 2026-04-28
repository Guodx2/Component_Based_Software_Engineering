using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to task types. This controller provides an endpoint to retrieve a list of all available task types from the database, which can be used by the frontend application to display task type options to users when creating or managing tasks. The GetTaskTypes action method queries the TaskTypes table in the database and returns the list of task types in the response. This allows users to view the different categories or types of tasks that can be created and assigned within the application, providing them with options for categorizing their tasks effectively.
    /// </summary>
    [ApiController]
    [Route("api/tasktypes")]
    public class TaskTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the TaskTypesController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to retrieve task type data. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to perform database operations such as querying for task types when handling API requests. This setup is essential for the functionality of the GetTaskTypes endpoint, which relies on the database context to fetch and return task type information to the frontend application.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public TaskTypesController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Handles GET requests to retrieve a list of all available task types. This method queries the TaskTypes table in the database and returns the list of task types in the response. This endpoint allows users to view the different categories or types of tasks that can be created and assigned within the application, providing them with options for categorizing their tasks effectively.
        /// </summary>
        /// <returns>A list of task types.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTaskTypes()
        {
            var taskTypes = await _context.TaskTypes.ToListAsync();
            return Ok(taskTypes);
        }
    }
}