using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenteeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MenteeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetMentee(string code)
        {
            var mentee = await _context.Mentees.FindAsync(code);
            if (mentee == null)
                return NotFound("Mentee not found.");
            return Ok(mentee);
        }

        [HttpGet("acceptedRequestsCount/{menteeCode}")]
        public async Task<IActionResult> GetAcceptedRequestsCount(string menteeCode)
        {
            var count = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 2 && (r.IsRead == false))
                .CountAsync();
            return Ok(count);
        }

        [HttpGet("acceptedRequests/{menteeCode}")]
        public async Task<IActionResult> GetAcceptedRequests(string menteeCode)
        {
            var notifications = await (
                from req in _context.MentorRequests
                join u in _context.Users on req.MentorCode equals u.Code
                where req.MenteeCode == menteeCode && req.RequestStatusId == 2 && (req.IsRead == false)
                select new {
                    id = req.Id,
                    mentorName = u.Name,
                    mentorLastName = u.LastName,
                    requestDate = req.RequestDate
                }
            ).ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("rejectedRequestsCount/{menteeCode}")]
        public async Task<IActionResult> GetRejectedRequestsCount(string menteeCode)
        {
            var count = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 3 && (r.IsRead == false))
                .CountAsync();
            return Ok(count);
        }

        [HttpGet("rejectedRequests/{menteeCode}")]
        public async Task<IActionResult> GetRejectedRequests(string menteeCode)
        {
            var notifications = await (
                from req in _context.MentorRequests
                join u in _context.Users on req.MentorCode equals u.Code
                where req.MenteeCode == menteeCode && req.RequestStatusId == 3 && (req.IsRead == false)
                select new {
                    id = req.Id,
                    mentorName = u.Name,
                    mentorLastName = u.LastName,
                    requestDate = req.RequestDate,
                    rejectionReason = req.RejectionReason,
                    message = "atmestas"  
                }
            ).ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("markRejectedNotificationsRead/{menteeCode}")]
        public async Task<IActionResult> MarkRejectedNotificationsRead(string menteeCode)
        {
            var notifications = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 3 && (r.IsRead == false))
                .ToListAsync();
            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("markNotificationsRead/{menteeCode}")]
        public async Task<IActionResult> MarkNotificationsRead(string menteeCode)
        {
            var notifications = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 2 && (r.IsRead == false))
                .ToListAsync();
            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("educationalresults/{menteeCode}")]
        public async Task<IActionResult> GetEducationalResults(string menteeCode)
        {
            var semesterPlan = await _context.SemesterPlans
                .Where(sp => sp.MenteeCode == menteeCode)
                .OrderByDescending(sp => sp.SemesterStartDate)
                .FirstOrDefaultAsync();
                
            if (semesterPlan == null)
            {
                return NotFound("No semester plan found for this mentee.");
            }

            var results = await (
                from spt in _context.SemesterPlanTasks
                join t in _context.Tasks on spt.TaskId equals t.Id_Task
                join f in _context.Feedback on spt.Id_SemesterPlanTask equals f.SemesterPlanTaskId into feedbackGroup
                from feedback in feedbackGroup.DefaultIfEmpty()
                where spt.SemesterPlanId == semesterPlan.Id_SemesterPlan
                select new 
                {
                    TaskName = t.Name,
                    Deadline = t.Deadline,
                    CompletionFile = spt.CompletionFile,
                    IsRated = spt.IsRated,
                    FeedbackRating = feedback != null ? feedback.Rating : (int?)null,
                    FeedbackComment = feedback != null ? feedback.Comment : null
                }
            ).ToListAsync();

            double? averageRating = results.Where(r => r.FeedbackRating.HasValue).Any()
                ? results.Where(r => r.FeedbackRating.HasValue).Average(r => r.FeedbackRating.Value)
                : null;

            return Ok(new
            {
                semesterPlan,
                results,
                averageRating
            });
        }

        [HttpGet("mentor/{menteeCode}")]
        public async Task<IActionResult> GetMenteeMentor(string menteeCode)
        {
            var mentee = await _context.Mentees.FindAsync(menteeCode);
            if(mentee == null)
                return NotFound("Mentee not found.");
                
            if(string.IsNullOrEmpty(mentee.MentorCode))
                return Ok(null);
                
            var mentorUser = await _context.Users.FindAsync(mentee.MentorCode);
            if(mentorUser == null)
                return Ok(null);
                
            var result = new {
                Name = mentorUser.Name,
                LastName = mentorUser.LastName
            };
            
            return Ok(result);
        }

        [HttpGet("incompleteTasks/{menteeCode}")]
        public async Task<IActionResult> GetIncompleteTasksCount(string menteeCode)
        {
            // Get the most recent semester plan for the mentee.
            var semesterPlan = await _context.SemesterPlans
                .Where(sp => sp.MenteeCode == menteeCode)
                .OrderByDescending(sp => sp.SemesterStartDate)
                .FirstOrDefaultAsync();

            if (semesterPlan == null)
            {
                return Ok(0);
            }

            // Retrieve tasks for this semester plan into memory.
            var tasks = await _context.SemesterPlanTasks
                .Where(spt => spt.SemesterPlanId == semesterPlan.Id_SemesterPlan)
                .ToListAsync();

            // A task is considered complete if a submission has been provided.
            // So, count tasks as incomplete if CompletionFile is null or whitespace.
            int count = tasks.Count(spt => string.IsNullOrWhiteSpace(spt.CompletionFile));

            return Ok(count);
        }
    }
}