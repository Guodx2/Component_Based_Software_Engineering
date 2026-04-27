using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/tasktypes")]
    public class TaskTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TaskTypesController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTaskTypes()
        {
            var taskTypes = await _context.TaskTypes.ToListAsync();
            return Ok(taskTypes);
        }
    }
}