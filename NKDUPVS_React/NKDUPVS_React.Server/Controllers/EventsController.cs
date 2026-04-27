using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("upcoming/affairs")]
        public IActionResult GetUpcomingAffairs()
        {
            var today = DateTime.Today;
            var upcomingAffairs = _context.Affairs
                .Include(a => a.Event)
                .Where(a => a.Event.startTime >= today)
                .Select(a => new {
                    AffairId = a.id_Affair,
                    EventId = a.Event.id_Event,
                    a.Event.name,
                    a.Event.startTime,
                    a.Event.endTime,
                    a.Event.address,
                    comment = a.Event.comment ?? "" 
                })
                .OrderBy(e => e.startTime)
                .ToList();

            return Ok(upcomingAffairs);
        }

        [HttpGet("upcoming/trainings")]
        public IActionResult GetUpcomingTrainings()
        {
            var today = DateTime.Today;
            var upcomingTrainings = _context.Trainings
                .Include(t => t.Event)
                .Where(t => t.Event.startTime >= today)
                .Select(t => new {
                    TrainingId = t.id_Training,      // added unique training id
                    EventId = t.Event.id_Event,
                    t.Event.name,
                    t.Event.startTime,
                    t.Event.endTime,
                    t.Event.address,
                    comment = t.Event.comment ?? "", // Convert null to an empty string
                    t.fk_ViceDeadForStudiescode
                })
                .OrderBy(e => e.startTime)
                .ToList();

            return Ok(upcomingTrainings);
        }

        [HttpGet("past/trainings")]
        public IActionResult GetPastTrainings()
        {
            var now = DateTime.Now; // Use current date and time
            var pastTrainings = _context.Trainings
                .Include(t => t.Event)
                .Where(t => t.Event.endTime < now) // Compare using current time
                .Select(t => new {
                    TrainingId = t.id_Training,
                    EventId = t.Event.id_Event,
                    t.Event.name,
                    t.Event.startTime,
                    t.Event.endTime,
                    t.Event.address,
                    comment = t.Event.comment ?? "",
                    t.fk_ViceDeadForStudiescode
                })
                .OrderByDescending(e => e.startTime)
                .ToList();

            return Ok(pastTrainings);
        }

        [HttpGet("past/affairs")]
        public IActionResult GetPastAffairs()
        {
            var today = DateTime.Today;
            var pastAffairs = _context.Affairs
                .Include(a => a.Event)
                .Where(a => a.Event.endTime < today)
                .Select(a => new {
                    AffairId = a.id_Affair,
                    EventId = a.Event.id_Event,
                    a.Event.name,
                    a.Event.startTime,
                    a.Event.endTime,
                    a.Event.address,
                    comment = a.Event.comment ?? "" 
                })
                .OrderByDescending(e => e.startTime)
                .ToList();

            return Ok(pastAffairs);
        }

        [HttpPost("trainings")]
        public async Task<IActionResult> CreateTraining([FromBody] CreateTrainingDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid training data.");

            // Validate dates: endTime should be after startTime.
            if (dto.endTime <= dto.startTime)
                return BadRequest("End time must be after start time.");

            var newEvent = new Event
            {
                name = dto.name,
                startTime = dto.startTime,
                endTime = dto.endTime,
                address = dto.address,
                comment = dto.comment
            };

            _context.Add(newEvent);
            await _context.SaveChangesAsync();

            var newTraining = new Training
            {
                event_id = newEvent.id_Event,
                fk_ViceDeadForStudiescode = dto.createdBy,
                Event = newEvent
            };

            _context.Add(newTraining);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Training created successfully.",
                eventId = newEvent.id_Event,
                trainingId = newTraining.id_Training
            });
        }

        [HttpPut("trainings/{trainingId}")]
        public async Task<IActionResult> UpdateTraining(int trainingId, [FromBody] UpdateTrainingDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid training data.");

            if (dto.endTime <= dto.startTime)
                return BadRequest("End time must be after start time.");

            var training = await _context.Trainings
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.id_Training == trainingId);

            if (training == null)
                return NotFound("Training not found.");

            // Update event values
            training.Event.name = dto.name;
            training.Event.startTime = dto.startTime;
            training.Event.endTime = dto.endTime;
            training.Event.address = dto.address;
            training.Event.comment = dto.comment;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Training updated successfully." });
        }

        [HttpDelete("trainings/{trainingId}")]
        public async Task<IActionResult> DeleteTraining(int trainingId)
        {
            var training = await _context.Trainings
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.id_Training == trainingId);
            if (training == null)
                return NotFound("Training not found.");

            var trainingFeedbacks = _context.Feedback
                .Where(f => f.fk_Trainingid_Training == trainingId || f.fk_Affairid_Affair == training.Event.id_Event);
            _context.Feedback.RemoveRange(trainingFeedbacks);

            var registrations = _context.SemesterPlanEvents
                .Where(spe => spe.Fk_Eventid_Event == training.Event.id_Event);
            _context.SemesterPlanEvents.RemoveRange(registrations);

            
            // Remove the training and its associated event
            _context.Trainings.Remove(training);
            _context.Set<Event>().Remove(training.Event);
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Training deleted successfully." });
        }
        
        [HttpPost("affairs")]
        public async Task<IActionResult> CreateAffair([FromBody] CreateAffairDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid affair data.");

            // Validate dates.
            if (dto.endTime <= dto.startTime)
                return BadRequest("End time must be after start time.");

            var newEvent = new Event
            {
                name = dto.name,
                startTime = dto.startTime,
                endTime = dto.endTime,
                address = dto.address,
                comment = dto.comment
            };
            _context.Add(newEvent);
            await _context.SaveChangesAsync();

            var newAffair = new Affair
            {
                event_id = newEvent.id_Event,
                Event = newEvent
            };
            _context.Add(newAffair);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Affair created successfully.",
                eventId = newEvent.id_Event,
                affairId = newAffair.id_Affair
            });
        }

        [HttpPut("affairs/{affairId}")]
        public async Task<IActionResult> UpdateAffair(int affairId, [FromBody] UpdateAffairDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid affair data.");

            if (dto.endTime <= dto.startTime)
                return BadRequest("End time must be after start time.");

            // Find the affair including its event
            var affair = await _context.Affairs
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.id_Affair == affairId);

            if (affair == null)
                return NotFound("Affair not found.");

            // Update event values
            affair.Event.name = dto.name;
            affair.Event.startTime = dto.startTime;
            affair.Event.endTime = dto.endTime;
            affair.Event.address = dto.address;
            affair.Event.comment = dto.comment;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Affair updated successfully." });
        }

        [HttpDelete("affairs/{affairId}")]
        public async Task<IActionResult> DeleteAffair(int affairId)
        {
            var affair = await _context.Affairs
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.id_Affair == affairId);
            if (affair == null)
                return NotFound("Affair not found.");

            var feedbackToDelete = _context.Feedback
                .Where(f => f.fk_Affairid_Affair == affair.Event.id_Event || f.fk_Trainingid_Training == affair.Event.id_Event);
            _context.Feedback.RemoveRange(feedbackToDelete);

            var registrations = _context.SemesterPlanEvents
                .Where(spe => spe.Fk_Eventid_Event == affair.Event.id_Event);
            _context.SemesterPlanEvents.RemoveRange(registrations);

            _context.Affairs.Remove(affair);
            _context.Set<Event>().Remove(affair.Event);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Affair deleted successfully." });
        }

        [HttpPost("register/training")]
        public IActionResult RegisterTraining([FromBody] RegisterTrainingDto dto)
        {
            var today = DateTime.Today;
            var semesterPlan = _context.SemesterPlans
                .FirstOrDefault(sp => sp.MenteeCode == dto.userCode     // Changed Fk_Menteecode to MenteeCode
                                && sp.SemesterStartDate <= today 
                                && sp.SemesterEndDate >= today);
                    
            if (semesterPlan == null)
            {
                return NotFound("Active semester plan not found.");
            }
                    
            // Check for duplicate registration using the SemesterPlanEvents DbSet.
            bool exists = _context.SemesterPlanEvents.Any(spe => spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan &&
                                                                spe.Fk_Eventid_Event == dto.eventId);
            if (exists)
            {
                return BadRequest("Jau esate užsiregistravę.");
            }
                    
            var spe = new SemesterPlanEvent
            {
                Fk_SemesterPlanid_SemesterPlan = semesterPlan.Id_SemesterPlan,
                Fk_Eventid_Event = dto.eventId
            };
            _context.SemesterPlanEvents.Add(spe);
            _context.SaveChanges();
                    
            return Ok(spe);
        }

        [HttpPost("register/affair")]
        public IActionResult RegisterAffair([FromBody] RegisterAffairDto dto)
        {
            // Instead of looking for an active plan only, get the most recent plan.
            var semesterPlan = _context.SemesterPlans
                .Where(sp => sp.MenteeCode == dto.userCode)
                .OrderByDescending(sp => sp.SemesterEndDate)
                .FirstOrDefault();

            if (semesterPlan == null)
                return NotFound("No semester plan found for this mentee.");

            bool exists = _context.SemesterPlanEvents.Any(spe =>
                spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan &&
                spe.Fk_Eventid_Event == dto.eventId);

            if (exists)
                return BadRequest("Jau esate užsiregistravę.");

            var spe = new SemesterPlanEvent
            {
                Fk_SemesterPlanid_SemesterPlan = semesterPlan.Id_SemesterPlan,
                Fk_Eventid_Event = dto.eventId
            };

            _context.SemesterPlanEvents.Add(spe);
            _context.SaveChanges();

            return Ok(spe);
        }

        [HttpGet("registered/trainings")]
        public IActionResult GetRegisteredTrainings(string userCode)
        {
            var today = DateTime.Today;
            var semesterPlan = _context.SemesterPlans
                .FirstOrDefault(sp => sp.MenteeCode == userCode &&
                                    sp.SemesterStartDate <= today &&
                                    sp.SemesterEndDate >= today);
            if (semesterPlan == null)
            {
                return Ok(new object[0]);
            }
            
            var registeredTrainings = _context.SemesterPlanEvents
                .Include(spe => spe.Event)
                .Where(spe => spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan 
                            && _context.Trainings.Any(t => t.event_id == spe.Fk_Eventid_Event))
                .Select(spe => new {
                    eventId = spe.Fk_Eventid_Event,
                    name = spe.Event != null ? spe.Event.name : "",
                    startTime = spe.Event != null ? spe.Event.startTime : DateTime.MinValue,
                    endTime = spe.Event != null ? spe.Event.endTime : DateTime.MinValue,
                    address = spe.Event != null ? spe.Event.address : "",
                    comment = spe.Event != null ? (spe.Event.comment ?? "") : ""
                })
                .OrderBy(e => e.startTime)
                .ToList();

            return Ok(registeredTrainings);
        }

        [HttpGet("registered/affairs")]
        public IActionResult GetRegisteredAffairs(string userCode)
        {
            var today = DateTime.Today;
            var semesterPlan = _context.SemesterPlans
                .FirstOrDefault(sp => sp.MenteeCode == userCode &&
                                    sp.SemesterStartDate <= today &&
                                    sp.SemesterEndDate >= today);
            if (semesterPlan == null)
            {
                return Ok(new object[0]);
            }

            var registeredAffairs = _context.SemesterPlanEvents
                .Include(spe => spe.Event)
                .Where(spe => spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan 
                            && _context.Affairs.Any(a => a.event_id == spe.Fk_Eventid_Event))
                .Select(spe => new {
                    eventId = spe.Fk_Eventid_Event,
                    name = spe.Event != null ? spe.Event.name : "",
                    startTime = spe.Event != null ? spe.Event.startTime : DateTime.MinValue,
                    endTime = spe.Event != null ? spe.Event.endTime : DateTime.MinValue,
                    address = spe.Event != null ? spe.Event.address : "",
                    comment = spe.Event != null ? (spe.Event.comment ?? "") : ""
                })
                .OrderBy(e => e.startTime)
                .ToList();

            return Ok(registeredAffairs);
        }
    }

    public class CreateTrainingDto
    {
        public string? name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? address { get; set; }
        public string? comment { get; set; }
        public string? createdBy { get; set; }
    }

    public class UpdateTrainingDto
    {
        public string? name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? address { get; set; }
        public string? comment { get; set; }
    }

    public class CreateAffairDto
    {
        public string? name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? address { get; set; }
        public string? comment { get; set; }
        public string? createdBy { get; set; }
    }

    public class UpdateAffairDto
    {
        public string? name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? address { get; set; }
        public string? comment { get; set; }
    }

    public class RegisterTrainingDto
    {
        public int? eventId { get; set; }
        public string? userCode { get; set; }
    }

    public class RegisterAffairDto
    {
        public int? eventId { get; set; }
        public string? userCode { get; set; }
    }
}