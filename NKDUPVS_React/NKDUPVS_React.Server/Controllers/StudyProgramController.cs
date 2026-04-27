using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using System.Threading.Tasks;
using System.Linq;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/studyprograms")]
    public class StudyProgramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StudyProgramController(ApplicationDbContext context)
        {
            _context = context;
        }
        
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