using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller for handling feedback related operations for semester plan tasks, trainings, and affairs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
         private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for FeedbackController, initializes the database context.
        /// </summary>
        /// <param name="context">The database context.</param>
         public FeedbackController(ApplicationDbContext context)
         {
             _context = context;
         }

        /// <summary>
        /// Creates feedback for a semester plan task. Validates the rating and updates the associated semester plan task to mark it as rated.
        /// </summary>
        /// <param name="request">The feedback request.</param>
        /// <returns>The created feedback.</returns>
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

        /// <summary>
        /// Updates existing feedback for a semester plan task. Validates the rating and updates the feedback entry if it exists.
        /// </summary>
        /// <param name="semesterPlanTaskId">The ID of the semester plan task for which to update feedback.</param>
        /// <param name="request">The feedback update request.</param>
        /// <returns>The updated feedback.</returns>
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

        /// <summary>
        /// Retrieves a list of trainings that have ended but have not yet received feedback from the specified mentee.
        /// </summary>
        /// <param name="menteeCode">The code of the mentee for whom to retrieve awaiting feedback.</param>
        /// <returns>A list of trainings for which feedback is awaited.</returns>
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

        /// <summary>
        /// Creates feedback for a training. Validates the rating, checks that the training has ended, and saves the feedback with a reference to the training and mentee code.
        /// </summary>
        /// <param name="request">The feedback creation request.</param>
        /// <returns>The created feedback.</returns>
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

        /// <summary>
        /// Creates feedback for an affair. Validates the rating, checks that the associated affair's event has ended, prevents duplicate feedback for the same affair and mentee, and saves the feedback with a reference to the affair's event and mentee code.
        /// </summary>
        /// <param name="request">The feedback creation request.</param>
        /// <returns>The created feedback.</returns>
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

        /// <summary>
        /// Retrieves a list of affairs that have ended but have not yet received feedback from the specified mentee. This checks for affairs associated with the mentee's semester plans and filters for those whose events have ended and do not have existing feedback from the mentee.
        /// </summary>
        /// <param name="menteeCode">The code of the mentee for whom to retrieve awaiting feedback.</param>
        /// <returns>A list of affairs for which feedback is awaiting.</returns>
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

        /// <summary>
        /// Retrieves feedback for a specific training. This looks up feedback entries associated with the given training ID and includes the mentee's name and last name by joining with the Users table based on the MenteeCode.
        /// </summary>
        /// <param name="trainingId">The ID of the training for which to retrieve feedback.</param>
        /// <returns>A list of feedback entries for the specified training.</returns>
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

        /// <summary>
        /// Retrieves feedback for a specific affair. This looks up feedback entries associated with the given affair ID (which is stored as fk_Affairid_Affair in the Feedback table) and includes the mentee's name and last name by joining with the Users table based on the MenteeCode.
        /// </summary>
        /// <param name="affairId">The ID of the affair for which to retrieve feedback.</param>
        /// <returns>A list of feedback entries for the specified affair.</returns>
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

    /// <summary>
    /// Request model for creating feedback for a semester plan task. Contains the semester plan task ID, rating, and an optional comment.
    /// </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// The ID of the semester plan task for which to create feedback. This is a nullable integer to allow for flexibility in the feedback creation process, as feedback can also be created for trainings and affairs which do not use this field. When creating feedback for a semester plan task, this field should be provided to associate the feedback with the correct task.
        /// </summary>
         public int? SemesterPlanTaskId { get; set; }

         /// <summary>
         /// The rating for the feedback, which should be an integer value between 1 and 5. This field is required for creating feedback and will be validated to ensure it falls within the acceptable range. The rating represents the mentee's evaluation of the semester plan task, training, or affair for which the feedback is being created.
         /// </summary>
         public int? Rating { get; set; }

         /// <summary>
         /// An optional comment for the feedback. This field allows the mentee to provide additional context or details about their rating. While it is not required, it can be useful for providing more specific feedback on what aspects were particularly good or could be improved. The comment will be stored along with the rating and can be displayed when retrieving feedback for a semester plan task, training, or affair.
         /// </summary>
         public string? Comment { get; set; }
    }

    /// <summary>
    /// Request model for updating existing feedback for a semester plan task. Contains the new rating and an optional comment. The SemesterPlanTaskId is not included in this request model since the feedback entry to be updated is identified by the route parameter in the UpdateFeedback endpoint. This model focuses on the fields that can be updated, which are the rating and comment.
    /// </summary>
    public class FeedbackUpdateRequest
    {
        /// <summary>
        /// The new rating for the feedback, which should be an integer value between 1 and 5. This field is required for updating feedback and will be validated to ensure it falls within the acceptable range. The rating represents the mentee's updated evaluation of the semester plan task, training, or affair for which the feedback is being updated.
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// An optional comment for the feedback update. This field allows the mentee to provide additional context or details about their updated rating. While it is not required, it can be useful for providing more specific feedback on what aspects were particularly good or could be improved based on the mentee's updated perspective. The comment will be stored along with the updated rating and can be displayed when retrieving feedback for a semester plan task, training, or affair.
        /// </summary>
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Request model for creating feedback for a training. Contains the training ID, rating, comment, and mentee code. The training ID is used to associate the feedback with the correct training, while the mentee code allows for tracking which mentee provided the feedback. The rating and comment fields are used to capture the mentee's evaluation of the training.
    /// </summary>
    public class TrainingFeedbackRequest
    {
        /// <summary>
        /// The ID of the training for which to create feedback. This field is required when creating feedback for a training and is used to associate the feedback entry with the correct training in the database. The training ID should correspond to an existing training that has ended, as feedback can only be created for trainings that have already taken place.
        /// </summary>
        public int? TrainingId { get; set; }

        /// <summary>
        /// The rating for the feedback, which should be an integer value between 1 and 5. This field is required for creating feedback and will be validated to ensure it falls within the acceptable range. The rating represents the mentee's evaluation of the training, with 1 being the lowest rating and 5 being the highest.
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// An optional comment for the training feedback. This field allows the mentee to provide additional context or details about their rating of the training. While it is not required, it can be useful for providing more specific feedback on what aspects of the training were particularly good or could be improved. The comment will be stored along with the rating and can be displayed when retrieving feedback for the training.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// The code of the mentee providing the feedback. This field is required for creating feedback for a training, as it allows for tracking which mentee provided the feedback and enables features such as preventing duplicate feedback from the same mentee for the same training. The mentee code should correspond to an existing user in the system who is associated with the training through their semester plan.
        /// </summary>
        public string? MenteeCode { get; set; }
    }

    /// <summary>
    /// Request model for creating feedback for an affair. Contains the affair ID, rating, comment, and mentee code. The affair ID is used to associate the feedback with the correct affair's event, while the mentee code allows for tracking which mentee provided the feedback. The rating and comment fields are used to capture the mentee's evaluation of the affair. This model also includes validation to prevent duplicate feedback for the same affair and mentee combination.
    /// </summary>
    public class AffairFeedbackRequest
    {
        /// <summary>
        /// The ID of the affair for which to create feedback. This field is required when creating feedback for an affair and is used to look up the associated affair and its event in the database. The feedback will be linked to the affair's event ID (fk_Affairid_Affair) to maintain the relationship between the feedback and the affair. The affair ID should correspond to an existing affair that has ended, as feedback can only be created for affairs that have already taken place.
        /// </summary>
        public int? AffairId { get; set; }

        /// <summary>
        /// The rating for the feedback, which should be an integer value between 1 and 5. This field is required for creating feedback and will be validated to ensure it falls within the acceptable range. The rating represents the mentee's evaluation of the affair, with 1 being the lowest rating and 5 being the highest.
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// An optional comment for the affair feedback. This field allows the mentee to provide additional context or details about their rating of the affair. While it is not required, it can be useful for providing more specific feedback on what aspects of the affair were particularly good or could be improved. The comment will be stored along with the rating and can be displayed when retrieving feedback for the affair.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// The code of the mentee providing the feedback. This field is required for creating feedback for an affair, as it allows for tracking which mentee provided the feedback and enables features such as preventing duplicate feedback from the same mentee for the same affair. The mentee code should correspond to an existing user in the system who is associated with the affair through their semester plan.
        /// </summary>
        public string? MenteeCode { get; set; }
    }
}