using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using System.Threading.Tasks;
using System.Linq;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to study programs. This controller provides an endpoint to retrieve a list of study programs from the database, which can be used by the frontend application to display study program options to users. The GetStudyPrograms action method queries the database for all study programs, selects relevant properties (value and label), and returns the list of study programs in a format suitable for use in dropdowns or selection components in the frontend. This allows users to easily select their study program when interacting with the application.
    /// </summary>
    [ApiController]
    [Route("api/studyprograms")]
    public class StudyProgramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the StudyProgramController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to retrieve study program data. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to perform database operations such as querying for study programs when handling API requests. This setup is essential for the functionality of the GetStudyPrograms endpoint, which relies on the database context to fetch and return study program information to the frontend application.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public StudyProgramController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Handles GET requests to retrieve a list of study programs from the database. This method queries the StudyPrograms table, selects relevant properties (value and label), and returns the list of study programs in a format suitable for use in dropdowns or selection components in the frontend application. The value property is set to the study program's ID as a string, and the label property is set to the study program's name. This endpoint allows users to easily select their study program when interacting with the application.
        /// </summary>
        /// <returns>A list of study programs.</returns>
        [HttpGet]
        public async Task<IActionResult> GetStudyPrograms()
        {
            var programs = await _context.StudyPrograms
                .Select(sp => new { value = sp.id_StudyPrograms.ToString(), label = sp.name })
                .ToListAsync();
            return Ok(programs);
        }
    }
}