using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller for managing events such as trainings and affairs. This controller provides endpoints for retrieving upcoming and past events, creating new events, updating existing events, deleting events, and registering users for events. It interacts with the database context to perform CRUD operations on events and their related entities, allowing clients to manage and access event information effectively.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the EventsController class with the specified database context. This constructor is used to inject the ApplicationDbContext, which allows the controller to interact with the database and perform operations related to events, such as retrieving, creating, updating, and deleting event records.
        /// </summary>
        /// <param name="context">The database context.</param>
        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of upcoming affairs from the database, including their associated event details. The method filters affairs based on their event's start time, returning only those that are scheduled to occur in the future. The returned data includes the affair ID, event ID, event name, start time, end time, address, and any comments associated with the event. This endpoint is typically used to display upcoming affairs to users, allowing them to see what events are scheduled and plan accordingly.
        /// </summary>
        /// <returns>A list of upcoming affairs.</returns>
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

        /// <summary>
        /// Retrieves a list of upcoming trainings from the database, including their associated event details. The method filters trainings based on their event's start time, returning only those that are scheduled to occur in the future. The returned data includes the training ID, event ID, event name, start time, end time, address, any comments associated with the event, and the code of the vice-dean for studies responsible for the training. This endpoint is typically used to display upcoming trainings to users, allowing them to see what events are scheduled and plan accordingly.
        /// </summary>
        /// <returns>A list of upcoming trainings.</returns>
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

        /// <summary>
        /// Retrieves a list of past trainings from the database, including their associated event details. The method filters trainings based on their event's end time, returning only those that have already occurred. The returned data includes the training ID, event ID, event name, start time, end time, address, any comments associated with the event, and the code of the vice-dean for studies responsible for the training. This endpoint is typically used to display past trainings to users, allowing them to review what events have already taken place.
        /// </summary>
        /// <returns>A list of past trainings.</returns>
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

        /// <summary>
        /// Retrieves a list of past affairs from the database, including their associated event details. The method filters affairs based on their event's end time, returning only those that have already occurred. The returned data includes the affair ID, event ID, event name, start time, end time, address, and any comments associated with the event. This endpoint is typically used to display past affairs to users, allowing them to review what events have already taken place.
        /// </summary>
        /// <returns>A list of past affairs.</returns>
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

        /// <summary>
        /// Creates a new training in the database based on the provided training information. The method first validates the input data, ensuring that the end time is after the start time. It then creates a new event record and saves it to the database, followed by creating a new training record that references the newly created event. Finally, it returns a success message along with the IDs of the created event and training. This endpoint is used to allow users to create new training events for their courses or groups.
        /// </summary>
        /// <param name="dto">The data transfer object containing the training information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Updates an existing training in the database based on the provided training information. The method first validates the input data, ensuring that the end time is after the start time. It then retrieves the existing training record along with its associated event from the database. If the training is found, it updates the event details with the new information provided in the request and saves the changes to the database. Finally, it returns a success message indicating that the training was updated successfully. This endpoint is used to allow users to modify existing training events as needed.
        /// </summary>
        /// <param name="trainingId">The ID of the training to update.</param>
        /// <param name="dto">The data transfer object containing the updated training information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Deletes an existing training from the database based on the provided training ID. The method first retrieves the training record along with its associated event from the database. If the training is found, it also retrieves and deletes any feedback and semester plan event registrations associated with the training's event. Finally, it removes the training and its associated event from the database and saves the changes. This endpoint is used to allow users to delete training events that are no longer needed or were created in error.
        /// </summary>
        /// <param name="trainingId">The ID of the training to delete.</param>
        /// <returns>The result of the operation.</returns>
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
        
        /// <summary>
        /// Creates a new affair in the database based on the provided affair information. The method first validates the input data, ensuring that the end time is after the start time. It then creates a new event record and saves it to the database, followed by creating a new affair record that references the newly created event. Finally, it returns a success message along with the IDs of the created event and affair. This endpoint is used to allow users to create new affair events for their courses or groups.
        /// </summary>
        /// <param name="dto">The data transfer object containing the affair information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Updates an existing affair in the database based on the provided affair information. The method first validates the input data, ensuring that the end time is after the start time. It then retrieves the existing affair record along with its associated event from the database. If the affair is found, it updates the event details with the new information provided in the request and saves the changes to the database. Finally, it returns a success message indicating that the affair was updated successfully. This endpoint is used to allow users to modify existing affair events as needed.
        /// </summary>
        /// <param name="affairId">The ID of the affair to update.</param>
        /// <param name="dto">The data transfer object containing the updated affair information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Deletes an existing affair from the database based on the provided affair ID. The method first retrieves the affair record along with its associated event from the database. If the affair is found, it also retrieves and deletes any feedback and semester plan event registrations associated with the affair's event. Finally, it removes the affair and its associated event from the database and saves the changes. This endpoint is used to allow users to delete affair events that are no longer needed or were created in error.
        /// </summary>
        /// <param name="affairId">The ID of the affair to delete.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Registers a user for a training event based on the provided registration information. The method first retrieves the active semester plan for the user, ensuring that the registration is associated with the correct semester. It then checks if the user has already registered for the specified event to prevent duplicate registrations. If the registration is valid, it creates a new SemesterPlanEvent record linking the user's semester plan to the event and saves it to the database. Finally, it returns a success message along with the details of the registration. This endpoint is used to allow users to sign up for training events that they are interested in attending.
        /// </summary>
        /// <param name="dto">The data transfer object containing the registration information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Registers a user for an affair event based on the provided registration information. The method retrieves the most recent semester plan for the user, ensuring that the registration is associated with the correct semester. It then checks if the user has already registered for the specified event to prevent duplicate registrations. If the registration is valid, it creates a new SemesterPlanEvent record linking the user's semester plan to the event and saves it to the database. Finally, it returns a success message along with the details of the registration. This endpoint is used to allow users to sign up for affair events that they are interested in attending.
        /// </summary>
        /// <param name="dto">The data transfer object containing the registration information.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Retrieves a list of trainings that a user has registered for based on their user code. The method first retrieves the active semester plan for the user, ensuring that the registrations are associated with the correct semester. It then queries the SemesterPlanEvents to find all events that the user has registered for, filtering specifically for training events. The returned data includes the event ID, name, start time, end time, address, and any comments associated with the event. This endpoint is used to allow users to view the training events they have signed up for in the current semester.
        /// </summary>
        /// <param name="userCode">The code of the user for whom to retrieve registered trainings.</param>
        /// <returns>A list of registered training events.</returns>
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

        /// <summary>
        /// Retrieves a list of affairs that a user has registered for based on their user code. The method first retrieves the active semester plan for the user, ensuring that the registrations are associated with the correct semester. It then queries the SemesterPlanEvents to find all events that the user has registered for, filtering specifically for affair events. The returned data includes the event ID, name, start time, end time, address, and any comments associated with the event. This endpoint is used to allow users to view the affair events they have signed up for in the current semester.
        /// </summary>
        /// <param name="userCode">The code of the user for whom to retrieve registered affairs.</param>
        /// <returns>A list of registered affair events.</returns>
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

    /// <summary>
    /// Data transfer object for creating a new training event. This class contains properties that represent the necessary information to create a training, including the name of the training, start and end times, address, any comments, and the code of the vice-dean for studies responsible for the training. This DTO is used to receive data from the client when a new training event is being created through the API.
    /// </summary>
    public class CreateTrainingDto
    {
        /// <summary>
        /// Gets or sets the name of the training event. This property represents the title or name that will be associated with the training event being created. It is used to identify and describe the training in a meaningful way for users who will be viewing or registering for the event.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the start time of the training event. This property represents the date and time when the training is scheduled to begin. It is used to determine when the event will take place and is important for scheduling and registration purposes.
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the training event. This property represents the date and time when the training is scheduled to end. It is used to determine the duration of the event and is important for scheduling and registration purposes, ensuring that users know how long the training will last.
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// Gets or sets the address of the training event. This property represents the location where the training will take place. It is used to provide users with information about where they need to go to attend the training, which can be important for planning and logistics.
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// Gets or sets any comments associated with the training event. This property can be used to provide additional information or notes about the training, such as special instructions, requirements, or other relevant details that attendees should be aware of when registering for or attending the event.
        /// </summary>
        public string? comment { get; set; }

        /// <summary>
        /// Gets or sets the code of the vice-dean for studies responsible for the training event. This property represents the identifier for the vice-dean who is overseeing or responsible for the training. It is used to associate the training with a specific vice-dean, which can be important for administrative purposes and for users who may need to contact the vice-dean regarding the training.
        /// </summary>
        public string? createdBy { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating an existing training event. This class contains properties that represent the information that can be updated for a training, including the name of the training, start and end times, address, and any comments. This DTO is used to receive data from the client when an existing training event is being updated through the API, allowing users to modify the details of a training as needed.
    /// </summary>
    public class UpdateTrainingDto
    {
        /// <summary>
        /// Gets or sets the name of the training event. This property represents the title or name that will be associated with the training event being updated. It is used to identify and describe the training
        /// in a meaningful way for users who will be viewing or registering for the event. When updating a training, this property allows users to change the name of the training if necessary.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the start time of the training event. This property represents the date and time when the training is scheduled to begin. It is used to determine when the event will take place and is important for scheduling and registration purposes. When updating a training, this property allows users to change the start time of the training if necessary.
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the training event. This property represents the date and time when the training is scheduled to end. It is used to determine the duration of the event and is important for scheduling and registration purposes, ensuring that users know how long the training will last. When updating a training, this property allows users to change the end time of the training if necessary.
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// Gets or sets the address of the training event. This property represents the location where the training will take place. It is used to provide users with information about where they need to go to attend the training, which can be important for planning and logistics. When updating a training, this property allows users to change the address of the training if necessary.
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// Gets or sets any comments associated with the training event. This property can be used to provide additional information or notes about the training, such as special instructions, requirements, or other relevant details that attendees should be aware of when registering for or attending the event. When updating a training, this property allows users to change the comments associated with the training if necessary.
        /// </summary>
        public string? comment { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new affair event. This class contains properties that represent the necessary information to create an affair, including the name of the affair, start and end times, address, any comments, and the code of the user who created the affair. This DTO is used to receive data from the client when a new affair event is being created through the API.
    /// </summary>
    public class CreateAffairDto
    {
        /// <summary>
        /// Gets or sets the name of the affair event. This property represents the title or name that will be associated with the affair event being created. It is used to identify and describe the affair in a meaningful way for users who will be viewing or registering for the event.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the start time of the affair event. This property represents the date and time when the affair is scheduled to begin. It is used to determine when the event will take place and is important for scheduling and registration purposes.
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the affair event. This property represents the date and time when the affair is scheduled to end. It is used to determine the duration of the event and is important for scheduling and registration purposes, ensuring that users know how long the affair will last.
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// Gets or sets the address of the affair event. This property represents the location where the affair will take place. It is used to provide users with information about where they need to go to attend the affair, which can be important for planning and logistics.
        /// </summary>
        public string? address { get; set; }
        
        /// <summary>
        /// Gets or sets any comments associated with the affair event. This property can be used to provide additional information or notes about the affair, such as special instructions, requirements, or other relevant details that attendees should be aware of when registering for or attending the event.
        /// </summary>
        public string? comment { get; set; }

        /// <summary>
        /// Gets or sets the code of the user who created the affair event. This property represents the identifier for the user who is responsible for creating the affair. It is used to associate the affair with a specific user, which can be important for administrative purposes and for users who may need to contact the creator regarding the affair.
        /// </summary>
        public string? createdBy { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating an existing affair event. This class contains properties that represent the information that can be updated for an affair, including the name of the affair, start and end times, address, and any comments. This DTO is used to receive data from the client when an existing affair event is being updated through the API, allowing users to modify the details of an affair as needed.
    /// </summary>
    public class UpdateAffairDto
    {
        /// <summary>
        /// Gets or sets the name of the affair event. This property represents the title or name that will be associated with the affair event being updated. It is used to identify and describe the affair in a meaningful way for users who will be viewing or registering for the event. When updating an affair, this property allows users to change the name of the affair if necessary.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the start time of the affair event. This property represents the date and time when the affair is scheduled to begin. It is used to determine when the event will take place and is important for scheduling and registration purposes. When updating an affair, this property allows users to change the start time of the affair if necessary.
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the affair event. This property represents the date and time when the affair is scheduled to end. It is used to determine the duration of the event and is important for scheduling and registration purposes, ensuring that users know how long the affair will last. When updating an affair, this property allows users to change the end time of the affair if necessary.
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// Gets or sets the address of the affair event. This property represents the location where the affair will take place. It is used to provide users with information about where they need to go to attend the affair, which can be important for planning and logistics. When updating an affair, this property allows users to change the address of the affair if necessary.
        /// </summary>
        public string? address { get; set; }

        /// <summary>
        /// Gets or sets any comments associated with the affair event. This property can be used to provide additional information or notes about the affair, such as special instructions, requirements, or other relevant details that attendees should be aware of when registering for or attending the event. When updating an affair, this property allows users to change the comments associated with the affair if necessary.
        /// </summary>
        public string? comment { get; set; }
    }

    /// <summary>
    /// Data transfer object for registering a user for a training or affair event. This class contains properties that represent the necessary information to register for an event, including the ID of the event and the code of the user. This DTO is used to receive data from the client when a user is registering for a training or affair event through the API, allowing users to sign up for events they are interested in attending.
    /// </summary>
    public class RegisterTrainingDto
    {
        /// <summary>
        /// Gets or sets the ID of the event for which the user is registering. This property represents the unique identifier of the training or affair event that the user wants to attend. It is used to link the registration to the specific event in the database, allowing the system to track which users are registered for which events.
        /// </summary>
        public int? eventId { get; set; }

        /// <summary>
        /// Gets or sets the code of the user who is registering for the event. This property represents the identifier for the user who is signing up for the training or affair. It is used to associate the registration with a specific user, which can be important for administrative purposes and for users who may want to view or manage their registrations.
        /// </summary>
        public string? userCode { get; set; }
    }

    /// <summary>
    /// Data transfer object for registering a user for an affair event. This class contains properties that represent the necessary information to register for an affair, including the ID of the event and the code of the user. This DTO is used to receive data from the client when a user is registering for an affair event through the API, allowing users to sign up for events they are interested in attending.
    /// </summary>
    public class RegisterAffairDto
    {
        /// <summary>
        /// Gets or sets the ID of the event for which the user is registering. This property represents the unique identifier of the affair event that the user wants to attend. It is used to link the registration to the specific event in the database, allowing the system to track which users are registered for which events.
        /// </summary>
        public int? eventId { get; set; }

        /// <summary>
        /// Gets or sets the code of the user who is registering for the affair event. This property represents the identifier for the user who is signing up for the affair. It is used to associate the registration with a specific user, which can be important for administrative purposes and for users who may want to view or manage their registrations.
        /// </summary>
        public string? userCode { get; set; }
    }
}