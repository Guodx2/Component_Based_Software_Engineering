using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
         private readonly ApplicationDbContext _context;

         public FeedbackController(ApplicationDbContext context)
         {
             _context = context;
         }

         [HttpPost("create")]
         public async System.Threading.Tasks.Task<IActionResult> CreateFeedback([FromBody] FeedbackRequest request)
         {
             if(request.Rating < 1 || request.Rating > 5)
             {
                 return BadRequest("Rating must be between 1 and 5.");
             }
             
             var feedback = new Feedback
             {
                 SemesterPlanTaskId = request.SemesterPlanTaskId,
                 Rating = request.Rating,
                 Comment = request.Comment,
                 SubmissionDate = DateTime.Now
             };

             _context.Feedback.Add(feedback);

             // Find the associated semester plan task and mark it as rated.
             var semesterPlanTask = await _context.SemesterPlanTasks
                                        .FirstOrDefaultAsync(spt => spt.Id_SemesterPlanTask == request.SemesterPlanTaskId);
             if(semesterPlanTask != null)
             {
                 semesterPlanTask.IsRated = true;
             }

             await _context.SaveChangesAsync();
             return Ok(feedback);
         }

         [HttpPut("update/{semesterPlanTaskId}")]
         public async System.Threading.Tasks.Task<IActionResult> UpdateFeedback(int semesterPlanTaskId, [FromBody] FeedbackUpdateRequest request)
         {
             if(request.Rating < 1 || request.Rating > 5)
             {
                 return BadRequest("Rating must be between 1 and 5.");
             }
             
             var feedback = await _context.Feedback
                             .FirstOrDefaultAsync(f => f.SemesterPlanTaskId == semesterPlanTaskId);
             if (feedback == null)
             {
                 return NotFound("Feedback not found.");
             }
             
             feedback.Rating = request.Rating;
             feedback.Comment = request.Comment;
             feedback.SubmissionDate = DateTime.Now;
             
             await _context.SaveChangesAsync();
             return Ok(feedback);
         }

        [HttpGet("training/awaiting/{menteeCode}")]
        public async System.Threading.Tasks.Task<IActionResult> GetAwaitingTrainingFeedback(string menteeCode)
        {
            var semesterPlanIds = await _context.SemesterPlans
                .Where(sp => sp.MenteeCode == menteeCode)
                .Select(sp => sp.Id_SemesterPlan)
                .ToListAsync();

            if (!semesterPlanIds.Any())
            {
                return NotFound("No semester plans found for the given mentee code.");
            }

            var awaitingTrainings =
                (from spe in _context.SemesterPlanEvents
                join training in _context.Trainings on spe.Fk_Eventid_Event equals training.event_id
                join evt in _context.Events on spe.Fk_Eventid_Event equals evt.id_Event
                where semesterPlanIds.Contains(spe.Fk_SemesterPlanid_SemesterPlan) &&
                    evt.endTime < DateTime.Now &&
                    !_context.Feedback.Any(f => 
                        (f.fk_Trainingid_Training ?? 0) == (training.id_Training ?? 0) && 
                            f.MenteeCode == menteeCode)
                orderby evt.startTime descending
                select new 
                {
                    trainingId = training.id_Training,
                    eventId = evt.id_Event,
                    evt.name,
                    evt.startTime,
                    evt.endTime,
                    evt.address,
                    evt.comment
                }).ToList();

            return Ok(await System.Threading.Tasks.Task.FromResult(awaitingTrainings));
        }

         [HttpPost("training/create")]
        public async System.Threading.Tasks.Task<IActionResult> CreateTrainingFeedback([FromBody] TrainingFeedbackRequest request)
        {
            if(request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }
            var training = await _context.Trainings
                            .Include(t => t.Event)
                            .FirstOrDefaultAsync(t => t.id_Training == request.TrainingId);
            if(training == null)
            {
                return NotFound("Training not found.");
            }
            if(training.Event.endTime > DateTime.Now)
            {
                return BadRequest("Training has not ended yet.");
            }
                    
            var feedback = new Feedback
            {
                Rating = request.Rating,
                Comment = request.Comment ?? "",
                SubmissionDate = DateTime.Now,
                fk_Trainingid_Training = request.TrainingId,
                MenteeCode = request.MenteeCode
            };
            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();
            return Ok(feedback);
        }

        [HttpPost("affair/create")]
        public async Task<IActionResult> CreateAffairFeedback([FromBody] AffairFeedbackRequest request)
        {
            if(request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }
            if(request.AffairId == null)
            {
                return BadRequest("AffairId is required.");
            }
            
            // Look up the affair and include its associated event.
            var affair = await _context.Affairs
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.id_Affair == request.AffairId);
            if(affair == null)
            {
                return NotFound("Affair not found.");
            }
            
            // Prevent duplicate affair feedback.
            var existingAffairFeedback = await _context.Feedback.FirstOrDefaultAsync(f => 
                f.fk_Affairid_Affair == affair.Event.id_Event && 
                f.MenteeCode == request.MenteeCode);
            if(existingAffairFeedback != null)
            {
                return BadRequest("Feedback for this affair has already been provided.");
            }
            
            var feedback = new Feedback
            {
                Rating = request.Rating,
                Comment = request.Comment ?? "",
                SubmissionDate = DateTime.Now,
                // Save the affair’s event id so that the FK constraint is met.
                fk_Affairid_Affair = affair.Event.id_Event,
                MenteeCode = request.MenteeCode
            };
            
            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();
            return Ok(feedback);
        }

        [HttpGet("affair/awaiting/{menteeCode}")]
        public async System.Threading.Tasks.Task<IActionResult> GetAwaitingAffairFeedback(string menteeCode)
        {
            var semesterPlanIds = await _context.SemesterPlans
                .Where(sp => sp.MenteeCode == menteeCode)
                .Select(sp => sp.Id_SemesterPlan)
                .ToListAsync();

            if (!semesterPlanIds.Any())
            {
                return NotFound("No semester plans found for the given mentee code.");
            }

            var awaitingAffairs = await (
                from spe in _context.SemesterPlanEvents
                join affair in _context.Affairs on spe.Fk_Eventid_Event equals affair.event_id
                join evt in _context.Events on spe.Fk_Eventid_Event equals evt.id_Event
                where semesterPlanIds.Contains(spe.Fk_SemesterPlanid_SemesterPlan) &&
                    evt.endTime < DateTime.Now &&
                    !_context.Feedback.Any(f => f.fk_Affairid_Affair == evt.id_Event &&
                                                f.MenteeCode == menteeCode)
                orderby evt.startTime descending
                select new {
                    affairId = affair.id_Affair,
                    eventId = evt.id_Event,
                    evt.name,
                    evt.startTime,
                    evt.endTime,
                    evt.address,
                    evt.comment
                }
            ).ToListAsync();

            return Ok(awaitingAffairs);
        }

        [HttpGet("training/feedback/{trainingId}")]
        public async Task<IActionResult> GetTrainingFeedback(int trainingId)
        {
            var feedbacks = await _context.Feedback
                .Where(f => f.fk_Trainingid_Training == trainingId)
                .Select(f => new {
                    f.Id,
                    f.Rating,
                    f.Comment,
                    f.SubmissionDate,
                    // Lookup the mentee's name and lastname from Users table
                    MenteeName = _context.Users.Where(u => u.Code == f.MenteeCode).Select(u => u.Name).FirstOrDefault(),
                    MenteeLastName = _context.Users.Where(u => u.Code == f.MenteeCode).Select(u => u.LastName).FirstOrDefault()
                })
                .ToListAsync();
            return Ok(feedbacks);
        }

        [HttpGet("affair/feedback/{affairId}")]
        public async Task<IActionResult> GetAffairFeedback(int affairId)
        {
            // Look up the affair, including its event.
            var affair = await _context.Affairs.Include(a => a.Event)
                            .FirstOrDefaultAsync(a => a.id_Affair == affairId);
            if(affair == null)
            {
                return NotFound("Affair not found.");
            }
            var feedbacks = await _context.Feedback
                .Where(f => f.fk_Affairid_Affair == affair.Event.id_Event)
                .Select(f => new {
                    f.Id,
                    f.Rating,
                    f.Comment,
                    f.SubmissionDate,
                    MenteeName = _context.Users.Where(u => u.Code == f.MenteeCode).Select(u => u.Name).FirstOrDefault(),
                    MenteeLastName = _context.Users.Where(u => u.Code == f.MenteeCode).Select(u => u.LastName).FirstOrDefault()
                })
                .ToListAsync();
            return Ok(feedbacks);
        }
    }

    public class FeedbackRequest
    {
         public int? SemesterPlanTaskId { get; set; }
         public int? Rating { get; set; }
         public string? Comment { get; set; }
    }

    public class FeedbackUpdateRequest
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class TrainingFeedbackRequest
    {
        public int? TrainingId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? MenteeCode { get; set; }
    }

    public class AffairFeedbackRequest
    {
        public int? AffairId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? MenteeCode { get; set; }
    }
}