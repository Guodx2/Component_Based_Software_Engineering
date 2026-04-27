using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using System.Threading.Tasks;
using System.Linq;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/specializations")]
    public class SpecializationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public SpecializationController(ApplicationDbContext context)
        {
            _context = context;
        }
        
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