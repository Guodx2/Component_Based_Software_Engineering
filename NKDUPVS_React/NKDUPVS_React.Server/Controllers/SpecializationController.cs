using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using System.Threading.Tasks;
using System.Linq;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to specializations. This controller provides an endpoint to retrieve a list of specializations from the database, which can be used by the frontend application to display specialization options to users. The GetSpecializations action method queries the database for all specializations, selects relevant properties (value, label, and fk_id_StudyPrograms), and returns the list of specializations in a format suitable for use in dropdowns or selection components in the frontend. This allows users to easily select their specialization when interacting with the application.
    /// </summary>
    [ApiController]
    [Route("api/specializations")]
    public class SpecializationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the SpecializationController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to retrieve specialization data. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to perform database operations such as querying for specializations when handling API requests. This setup is essential for the functionality of the GetSpecializations endpoint, which relies on the database context to fetch and return specialization information to the frontend application.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public SpecializationController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Handles GET requests to retrieve a list of specializations from the database. This method queries the Specializations table, selects relevant properties (value, label, and fk_id_StudyPrograms), and returns the list of specializations in a format suitable for use in dropdowns or selection components in the frontend application. The value property is set to the specialization's ID as a string, the label property is set to the specialization's name, and the fk_id_StudyPrograms property is included to provide information about the associated study program for each specialization. This endpoint allows users to easily select their specialization when interacting with the application.
        /// </summary>
        /// <returns>A list of specializations.</returns>
        [HttpGet]
        public async Task<IActionResult> GetSpecializations()
        {
            var specs = await _context.Specializations
                .Select(s => new 
                { 
                    value = s.id.ToString(), 
                    label = s.name,
                    fk_id_StudyPrograms = s.fk_id_StudyPrograms.ToString()  
                })
                .ToListAsync();
            return Ok(specs);
        }
    }
}