using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Services;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/mentor")]
    public class MentorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public MentorController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("updateAvailability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] MentorAvailabilityUpdateRequest request)
        {
            var mentor = await _context.Mentors.FindAsync(request.MentorCode);
            if (mentor == null)
            {
                return NotFound();
            }
            mentor.AcceptingMentees = request.AcceptingMentees;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetMentor(string code)
        {
            // First find the mentor record
            var mentor = await _context.Mentors.FindAsync(code);
            if (mentor == null)
            {
                return NotFound();
            }

            // Try to get corresponding user details (if any)
            var userForMentor = await _context.Users.FindAsync(code);

            var result = new {
                Code = mentor.Code,
                Department = mentor.Department,
                // Use user info if available; otherwise fallback to defaults
                Name = userForMentor?.Name ?? "Mentorius",
                LastName = userForMentor?.LastName ?? "",
                Email = userForMentor?.Email ?? "",
                PhoneNumber = userForMentor?.PhoneNumber ?? ""
            };

            return Ok(result);
        }

        [HttpGet("expertise/{mentorCode}")]
        public async Task<IActionResult> GetExpertise(string mentorCode)
        {
            var expertiseList = await _context.MentorExpertises
                .Where(e => e.MentorCode == mentorCode)
                .OrderBy(e => e.Priority)
                .ToListAsync();
            return Ok(expertiseList);
        }

        [HttpPost("updateExpertise")]
        public async Task<IActionResult> UpdateExpertise([FromBody] MentorExpertiseUpdateRequest request)
        {
            // Remove any previous expertise for this mentor
            var existingExpertise = _context.MentorExpertises.Where(e => e.MentorCode == request.MentorCode);
            _context.MentorExpertises.RemoveRange(existingExpertise);

            // Add new items, ensuring Priority orders are set (assuming lower numbers are higher priority)
            foreach (var item in request.ExpertiseList)
            {
                _context.MentorExpertises.Add(new MentorExpertise
                {
                    MentorCode = request.MentorCode,
                    StudyProgram = item.StudyProgram,
                    Specialization = item.Specialization,
                    Priority = item.Priority
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("suggestions/{menteeCode}")]
        public async Task<IActionResult> GetMentorSuggestions(string menteeCode)
        {
            // Get the mentee record (assumes mentee table primary key is menteeCode)
            var mentee = await _context.Mentees.FindAsync(menteeCode);
            if (mentee == null)
                return NotFound("Mentee not found.");

            // Do not return suggestions if mentee already has a mentor assigned
            if (!string.IsNullOrEmpty(mentee.MentorCode))
                return Ok(new List<object>());

            var suggestions = await (
                from m in _context.Mentors
                join u in _context.Users on m.Code equals u.Code
                join me in _context.MentorExpertises on m.Code equals me.MentorCode
                where (m.AcceptingMentees == true) && me.StudyProgram == mentee.StudyProgram
                group new { m, u, me } by new { m.Code, m.Department, u.Name, u.LastName, u.Email, u.PhoneNumber } into g
                select new
                {
                    MentorCode = g.Key.Code,
                    Department = g.Key.Department,
                    MentorName = g.Key.Name,
                    MentorLastName = g.Key.LastName,
                    MentorEmail = g.Key.Email,
                    MentorPhoneNumber = g.Key.PhoneNumber,
                    MinPriority = g.Min(x => x.me.Priority),
                    AssignedCount = _context.Mentees.Count(m => m.MentorCode == g.Key.Code)
                }
            )
            .OrderBy(x => x.MinPriority)
            .ThenBy(x => x.AssignedCount)
            .ToListAsync();

            return Ok(suggestions);
        }

        [HttpGet("requests/{mentorCode}")]
        public async Task<IActionResult> GetRequests(string mentorCode)
        {
            var requests = await (
                from req in _context.MentorRequests
                join status in _context.RequestStatuses on req.RequestStatusId equals status.Id
                join u in _context.Users on req.MenteeCode equals u.Code
                join m in _context.Mentees on req.MenteeCode equals m.Code
                join sp in _context.StudyPrograms on m.StudyProgram equals sp.id_StudyPrograms
                join spec in _context.Specializations on m.Specialization equals spec.id into specGroup
                from spec in specGroup.DefaultIfEmpty()
                where req.MentorCode == mentorCode
                select new 
                {
                    req.Id,
                    req.MenteeCode,
                    req.MentorCode,
                    req.RequestDate,
                    status = status.Name,  // using the joined request status name
                    u.Name,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    StudyProgram = sp.name,
                    Specialization = (spec != null ? spec.name : "-")
                }
            ).ToListAsync();

            return Ok(requests);
        }

        [HttpPost("sendRequest")]
        public async Task<IActionResult> SendRequest([FromBody] MentorRequest request)
        {
            // Prevent duplicate pending requests or if mentee already has a mentor
            var existing = await _context.MentorRequests
                .FirstOrDefaultAsync(r => r.MenteeCode == request.MenteeCode 
                                    && r.RequestStatusId == 1);
            
            // Also, check if mentee already has a mentor assigned.
            var mentee = await _context.Mentees.FindAsync(request.MenteeCode);
            if (mentee != null && !string.IsNullOrEmpty(mentee.MentorCode))
            {
                return BadRequest("Šiam ugdytiniui jau priskirtas mentorius.");
            }
            
            if (existing != null)
            {
                return BadRequest("Jau turite laukiančią užklausą.");
            }

            _context.MentorRequests.Add(request);
            await _context.SaveChangesAsync();
            return Ok(request);
        }

        [HttpPost("acceptRequest/{requestId}")]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var request = await _context.MentorRequests
                .SingleOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return NotFound("Request not found.");

            if (string.IsNullOrEmpty(request.MenteeCode))
                return BadRequest("No MenteeCode found in the request.");

            var mentee = await _context.Mentees
                .SingleOrDefaultAsync(m => m.Code == request.MenteeCode);

            if (mentee == null)
                return NotFound("Mentee not found.");

            // Update the mentee with the mentor's code and change request status to "Priimta" (RequestStatusId = 2)
            mentee.MentorCode = request.MentorCode;
            request.RequestStatusId = 2;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("rejectRequest/{requestId}")]
        public async Task<IActionResult> RejectRequest(int requestId, [FromBody] RejectRequestModel model)
        {
            var request = await _context.MentorRequests.FindAsync(requestId);
            if (request == null)
                return NotFound("Request not found.");
            if (string.IsNullOrEmpty(request.MenteeCode))
                return BadRequest("MenteeCode is missing in the request.");

            // Mark the request as rejected (status 3) and store the rejection reason.
            request.RequestStatusId = 3;
            request.RejectionReason = model.Reason;
            
            await _context.SaveChangesAsync();

            var menteeUser = await _context.Users.FindAsync(request.MenteeCode);
            if (menteeUser != null)
            {
                string subject = "Jūsų prašymas buvo atmestas";
                string plainText = $"Sveiki, jūsų prašymas dėl mentorystės buvo atmestas. Priežastis: {model.Reason}.";
                string htmlContent = $"<p>Sveiki,</p><p>Jūsų prašymas dėl mentorystės buvo atmestas. Priežastis: <strong>{model.Reason}</strong></p>";
                await _emailService.SendEmailAsync(menteeUser.Email, subject, plainText, htmlContent);
            }

            return Ok("Request rejected and notification sent.");
        }

        [HttpGet("mentees/{mentorCode}")]
        public async Task<IActionResult> GetMentees(string mentorCode)
        {
            var mentees = await (from m in _context.Mentees
                         join u in _context.Users on m.Code equals u.Code
                         join sp in _context.StudyPrograms on m.StudyProgram equals sp.id_StudyPrograms into spGroup
                         from sp in spGroup.DefaultIfEmpty()
                         join spec in _context.Specializations on m.Specialization equals spec.id into specGroup
                         from spec in specGroup.DefaultIfEmpty()
                         where m.MentorCode == mentorCode
                         select new {
                             code = m.Code,
                             name = u.Name,
                             lastName = u.LastName,
                             studyProgram = sp != null ? sp.name : m.StudyProgram.ToString(),
                             specialization = spec != null ? spec.name : (m.Specialization != null ? m.Specialization.ToString() : "")
                         }).ToListAsync();

            return Ok(mentees);
        }

        [HttpGet("mentees")]
        public async Task<IActionResult> GetMentees()
        {
            var mentees = await _context.Mentees.ToListAsync();
            return Ok(mentees);
        }

        [HttpGet("mentee/{code}")]
        public async Task<IActionResult> GetMenteeDetails(string code)
        {
            // Get basic user details for the mentee
            var user = await _context.Users.FindAsync(code);
            if(user == null)
            {
                return NotFound("Mentee not found.");
            }
            
            // Optionally, you could also load additional data from the mentee table.
            var result = new {
                name = user.Name,
                lastName = user.LastName
            };
            
            return Ok(result);
        }

        [HttpGet("notRatedTasks/{mentorCode}")]
        public async Task<IActionResult> GetNotRatedTasksCount(string mentorCode)
        {
            var count = await (
                from spt in _context.SemesterPlanTasks
                join sp in _context.SemesterPlans on spt.SemesterPlanId equals sp.Id_SemesterPlan
                join m in _context.Mentees on sp.MenteeCode equals m.Code
                where m.MentorCode == mentorCode && spt.IsRated == false                
                select spt
            ).CountAsync();
                    
            return Ok(count);
        }
    }

    public class MentorExpertiseUpdateRequest
    {
        public string? MentorCode { get; set; }
        public List<ExpertiseItem>? ExpertiseList { get; set; }
    }

    public class ExpertiseItem
    {
        public int? StudyProgram { get; set; }
        public int? Specialization { get; set; }
        public int? Priority { get; set; }
    }

    public class MentorAvailabilityUpdateRequest
    {
        public string? MentorCode { get; set; }
        public bool AcceptingMentees { get; set; }
    }

    public class RejectRequestModel
    {
        public string? Reason { get; set; }
    }
}