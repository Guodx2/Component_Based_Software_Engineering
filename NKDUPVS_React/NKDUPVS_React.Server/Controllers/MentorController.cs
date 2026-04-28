using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Services;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling mentor-related operations in the application. This includes updating mentor availability, retrieving mentor details and expertise, managing mentor requests from mentees, and fetching information about assigned mentees. The controller interacts with the database context to perform CRUD operations on mentor data and uses an email service to send notifications when necessary. It provides various endpoints for both mentors and mentees to facilitate the mentorship process within the application.
    /// </summary>
    [ApiController]
    [Route("api/mentor")]
    public class MentorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Constructor for the MentorController class. It initializes the controller with the necessary dependencies, including the database context for accessing mentor data and the email service for sending notifications. The constructor takes an instance of ApplicationDbContext and IEmailService as parameters and assigns them to private readonly fields for use in the controller's action methods. This setup allows the controller to perform database operations and send emails as needed throughout its various endpoints.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="emailService">The email service for sending notifications.</param>
        public MentorController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Updates the availability status of a mentor to indicate whether they are accepting mentees or not. The method takes a MentorAvailabilityUpdateRequest object as input, which contains the mentor's code and their availability status. It first retrieves the mentor record from the database using the provided mentor code. If the mentor is found, it updates the AcceptingMentees property with the new availability status and saves the changes to the database. If the mentor is not found, it returns a 404 Not Found response. Finally, if the update is successful, it returns an Ok response indicating that the operation was completed successfully. This endpoint allows mentors to easily update their availability status for mentees in the application.
        /// </summary>
        /// <param name="request">The request containing the mentor's code and availability status.</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
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

        /// <summary>
        /// Retrieves the details of a specific mentor based on their unique code. The method takes the mentor's code as a parameter, queries the database to find the corresponding mentor record, and then attempts to fetch additional user details associated with that mentor. If the mentor is found, it constructs a response object containing the mentor's code, department, and user information such as name, last name, email, and phone number. If the mentor is not found, it returns a 404 Not Found response. This endpoint allows clients to fetch comprehensive details about a mentor, which can be useful for displaying mentor profiles in the frontend application.
        /// </summary>
        /// <param name="code">The unique code of the mentor for whom to retrieve details.</param>
        /// <returns>A response containing the mentor's details, or a 404 Not Found response if the mentor is not found.</returns>
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

        /// <summary>
        /// Retrieves the list of expertise areas for a specific mentor based on their unique code. The method takes the mentor's code as a parameter, queries the database to find all mentor expertise records associated with that mentor code, and orders them by their priority. The response includes a list of expertise items, each containing information about the study program, specialization, and priority. This endpoint allows clients to fetch detailed information about a mentor's areas of expertise, which can be useful for matching mentors with mentees based on their academic interests in the frontend application.
        /// </summary>
        /// <param name="mentorCode">The unique code of the mentor for whom to retrieve expertise information.</param>
        /// <returns>A response containing the list of expertise areas, or a 404 Not Found response if the mentor is not found.</returns>
        [HttpGet("expertise/{mentorCode}")]
        public async Task<IActionResult> GetExpertise(string mentorCode)
        {
            var expertiseList = await _context.MentorExpertises
                .Where(e => e.MentorCode == mentorCode)
                .OrderBy(e => e.Priority)
                .ToListAsync();
            return Ok(expertiseList);
        }

        /// <summary>
        /// Updates the expertise areas for a specific mentor based on the provided list of expertise items. The method takes a MentorExpertiseUpdateRequest object as input, which contains the mentor's code and a list of expertise items, each with information about the study program, specialization, and priority. The method first removes any existing expertise records for the specified mentor from the database, and then adds new records based on the provided list of expertise items. Finally, it saves the changes to the database and returns an Ok response indicating that the operation was successful. This endpoint allows mentors to easily update their areas of expertise in the application, ensuring that mentees can find mentors with relevant academic interests.
        /// </summary>
        /// <param name="request">The request object containing the mentor's code and the list of expertise items to update.</param>
        /// <returns>An Ok response indicating the operation was successful.</returns>
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

        /// <summary>
        /// Retrieves a list of mentor suggestions for a specific mentee based on their study program and the mentors' availability. The method takes the mentee's code as a parameter, queries the database to find the corresponding mentee record, and checks if the mentee already has a mentor assigned. If the mentee does not have a mentor, it performs a query to find mentors who are accepting mentees and have expertise in the same study program as the mentee. The results are grouped by mentor and ordered by the minimum priority of their expertise and the number of currently assigned mentees. The response includes details about each suggested mentor, such as their code, department, name, email, phone number, minimum priority of expertise, and the count of assigned mentees. This endpoint allows clients to fetch personalized mentor suggestions for a mentee based on their academic interests and mentor availability in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The code of the mentee for whom to retrieve suggestions.</param>
        /// <returns>A list of suggested mentors based on the mentee's study program and mentor availability.</returns>
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

        /// <summary>
        /// Retrieves the list of mentor requests for a specific mentor based on their unique code. The method takes the mentor's code as a parameter, queries the database to find all mentor requests associated with that mentor code, and joins related data such as request status, mentee user details, mentee study program, and specialization. The response includes details about each request, such as the request ID, mentee code, mentor code, request date, request status name, mentee's name, email, phone number, study program name, and specialization name (if available). This endpoint allows mentors to view all incoming mentorship requests along with relevant information about the requesting mentees in the frontend application.
        /// </summary>
        /// <param name="mentorCode">The code of the mentor for whom to retrieve requests.</param>
        /// <returns>A list of mentor requests associated with the specified mentor.</returns>
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

        /// <summary>
        /// Handles the submission of a new mentor request from a mentee. The method takes a MentorRequest object as input, which contains details about the mentorship request, including the mentee's code and the mentor's code. It first checks if there are any existing pending requests for the same mentee or if the mentee already has a mentor assigned. If either condition is true, it returns a BadRequest response with an appropriate message. If there are no conflicts, it adds the new mentor request to the database and saves the changes. Finally, it returns an Ok response with the created request object. This endpoint allows mentees to submit mentorship requests while ensuring that duplicate requests or requests from mentees who already have mentors are prevented.
        /// </summary>
        /// <param name="request">The mentor request to submit.</param>
        /// <returns>The created mentor request.</returns>
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

        /// <summary>
        /// Handles the acceptance of a mentor request by a mentor. The method takes the request ID as a parameter, retrieves the corresponding mentor request from the database, and checks if it exists and contains a valid mentee code. If the request is found and valid, it retrieves the associated mentee record and updates it by assigning the mentor's code to the mentee and changing the request status to "Priimta" (RequestStatusId = 2). Finally, it saves the changes to the database and returns an Ok response. This endpoint allows mentors to accept mentorship requests from mentees, which results in establishing a mentorship relationship between them in the application.
        /// </summary>
        /// <param name="requestId">The ID of the mentor request to accept.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Handles the rejection of a mentor request by a mentor. The method takes the request ID and a RejectRequestModel containing the rejection reason as parameters. It retrieves the corresponding mentor request from the database, checks if it exists and contains a valid mentee code. If the request is found and valid, it updates the request status to "Atmesta" (RequestStatusId = 3) and stores the provided rejection reason. After saving the changes to the database, it retrieves the mentee's user information and sends an email notification to inform them about the rejection of their mentorship request, including the reason for rejection. Finally, it returns an Ok response indicating that the request was rejected and the notification was sent. This endpoint allows mentors to reject mentorship requests while providing feedback to mentees through email notifications.
        /// </summary>
        /// <param name="requestId">The ID of the mentor request to reject.</param>
        /// <param name="model">The model containing the rejection reason.</param>
        /// <returns>The result of the operation.</returns>
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

        /// <summary>
        /// Retrieves the list of mentees assigned to a specific mentor based on the mentor's unique code. The method takes the mentor's code as a parameter, queries the database to find all mentee records associated with that mentor code, and joins related user details, study program, and specialization information. The response includes details about each mentee, such as their code, name, last name, study program name, and specialization name (if available). This endpoint allows mentors to view all their assigned mentees along with relevant information in the frontend application.
        /// </summary>
        /// <param name="mentorCode">The unique code of the mentor.</param>
        /// <returns>The list of mentees assigned to the mentor.</returns>
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

        /// <summary>
        /// Retrieves the list of all mentees in the system. This method does not filter by mentor code and returns basic information about each mentee, such as their code, name, and last name. This endpoint can be used for administrative purposes or to display a list of all mentees in the application, regardless of their assigned mentors.
        /// </summary>
        /// <returns>The list of all mentees in the system.</returns>
        [HttpGet("mentees")]
        public async Task<IActionResult> GetMentees()
        {
            var mentees = await _context.Mentees.ToListAsync();
            return Ok(mentees);
        }

        /// <summary>
        /// Retrieves the details of a specific mentee based on their unique code. The method takes the mentee's code as a parameter, queries the database to find the corresponding user record for that mentee, and returns basic information such as the mentee's name and last name. If the mentee is not found, it returns a 404 Not Found response. This endpoint allows clients to fetch basic details about a mentee, which can be useful for displaying mentee information in the frontend application or for administrative purposes.
        /// </summary>
        /// <param name="code">The unique code of the mentee.</param>
        /// <returns>The details of the mentee.</returns>
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

        /// <summary>
        /// Retrieves the count of tasks that have not been rated for a specific mentor.
        /// </summary>
        /// <param name="mentorCode">The code of the mentor.</param>
        /// <returns>The count of not-rated tasks.</returns>
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

    /// <summary>
    /// Request model for updating a mentor's expertise areas. This model contains the mentor's unique code and a list of expertise items, where each item includes information about the study program, specialization, and priority. The MentorExpertiseUpdateRequest is used in the UpdateExpertise endpoint of the MentorController to allow mentors to update their areas of expertise in the application. Each expertise item represents a specific area of knowledge or skill that the mentor can offer to mentees, and the priority field can be used to indicate the importance or relevance of each expertise area for matching mentors with mentees based on their academic interests.
    /// </summary>
    public class MentorExpertiseUpdateRequest
    {
        /// <summary>
        /// The unique code of the mentor for whom to update expertise areas. This code is used to identify the mentor in the database and associate the provided expertise items with the correct mentor record. The MentorCode is a crucial part of the request model as it ensures that the expertise updates are applied to the intended mentor in the application.
        /// </summary>
        public string? MentorCode { get; set; }

        /// <summary>
        /// A list of expertise items representing the areas of expertise for the mentor. Each item in the list contains information about a specific study program, specialization, and priority level. The ExpertiseList is used to update the mentor's expertise areas in the database, allowing mentees to find mentors with relevant academic interests when searching for mentorship opportunities in the application.
        /// </summary>
        public List<ExpertiseItem>? ExpertiseList { get; set; }
    }

    /// <summary>
    /// Represents an individual expertise item for a mentor, containing information about the study program, specialization, and priority level. This class is used as part of the MentorExpertiseUpdateRequest to specify the areas of expertise that a mentor has. The StudyProgram property indicates the specific academic program associated with the expertise, while the Specialization property can provide additional details about the mentor's focus within that study program. The Priority property can be used to indicate the importance or relevance of this expertise area when matching mentors with mentees based on their academic interests in the application.
    /// </summary>
    public class ExpertiseItem
    {
        /// <summary>
        /// The unique identifier for the study program associated with the expertise item. This property indicates the specific academic program linked to the expertise area.
        /// </summary>
        public int? StudyProgram { get; set; }

        /// <summary>
        /// The unique identifier for the specialization associated with the expertise item. This property provides additional details about the mentor's focus within the study program, allowing for more specific matching between mentors and mentees based on their academic interests in the application.
        /// </summary>
        public int? Specialization { get; set; }

        /// <summary>
        /// The priority level of the expertise item, which can be used to indicate the importance or relevance of this expertise area when matching mentors with mentees. A lower priority number may indicate a higher level of expertise or a stronger focus in that area, while a higher priority number may indicate a less critical area of expertise for the mentor. This property helps to facilitate more effective matching between mentors and mentees based on their academic interests in the application.
        /// </summary>
        public int? Priority { get; set; }
    }

    /// <summary>
    /// Request model for updating a mentor's availability status. This model contains the mentor's unique code and a boolean value indicating whether the mentor is currently accepting mentees or not. The MentorAvailabilityUpdateRequest is used in the UpdateAvailability endpoint of the MentorController to allow mentors to easily update their availability status in the application. By providing their mentor code and availability status, mentors can ensure that they are accurately represented in the system, allowing mentees to find mentors who are currently accepting new mentees when searching for mentorship opportunities in the frontend application.
    /// </summary>
    public class MentorAvailabilityUpdateRequest
    {
        /// <summary>
        /// The unique code of the mentor for whom to update the availability status. This code is used to identify the mentor in the database and apply the availability update to the correct mentor record. The MentorCode is essential for ensuring that the availability status is updated for the intended mentor in the application, allowing mentees to find mentors who are currently accepting new mentees when searching for mentorship opportunities in the frontend application.
        /// </summary>
        public string? MentorCode { get; set; }

        /// <summary>
        /// A boolean value indicating whether the mentor is currently accepting mentees or not. If AcceptingMentees is set to true, it means that the mentor is open to taking on new mentees, and if it is set to false, it indicates that the mentor is not currently accepting new mentees. This property allows mentors to easily update their availability status in the application, ensuring that mentees can find mentors who are currently accepting new mentees when searching for mentorship opportunities in the frontend application.
        /// </summary>
        public bool AcceptingMentees { get; set; }
    }

    /// <summary>
    /// Request model for rejecting a mentor request. This model contains a single property, Reason, which is a string that allows mentors to provide a reason for rejecting a mentorship request from a mentee. The RejectRequestModel is used in the RejectRequest endpoint of the MentorController to capture the rejection reason when a mentor decides to reject a mentorship request. By providing a reason for rejection, mentors can offer feedback to mentees, which can be helpful for mentees to understand why their request was rejected and potentially improve their chances of being accepted by other mentors in the future. Additionally, this information can be included in email notifications sent to mentees to inform them about the rejection of their mentorship request.
    /// </summary>
    public class RejectRequestModel
    {
        /// <summary>
        /// A string property that allows mentors to provide a reason for rejecting a mentorship request from a mentee. This reason can be used to offer feedback to mentees, helping them understand why their request was rejected and potentially improve their chances of being accepted by other mentors in the future. The Reason property can also be included in email notifications sent to mentees to inform them about the rejection of their mentorship request, providing clarity and transparency in the communication process between mentors and mentees in the application.
        /// </summary>
        public string? Reason { get; set; }
    }
}