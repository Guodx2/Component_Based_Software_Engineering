using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to mentees. This includes retrieving mentee information, counting accepted and rejected mentor requests, marking notifications as read, and fetching educational results and mentor information for a specific mentee. The controller interacts with the ApplicationDbContext to perform database operations and returns appropriate responses based on the requested data. Each action method is designed to handle specific endpoints related to mentee functionalities in the application.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MenteeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the MenteeController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to perform CRUD operations related to mentees and their associated data. The context is injected through dependency injection, ensuring that the controller has access to the necessary database context for handling requests and returning appropriate responses based on the data stored in the database.
        /// </summary>
        /// <param name="context"></param>
        public MenteeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a mentee's information based on their unique code. The method takes the mentee's code as a parameter, queries the database for a matching mentee, and returns the mentee's details if found. If no mentee is found with the provided code, it returns a 404 Not Found response with an appropriate message. This endpoint allows clients to fetch specific information about a mentee using their unique identifier.
        /// </summary>
        /// <param name="code">The unique code of the mentee to retrieve.</param>
        /// <returns>The mentee's information if found, otherwise a 404 Not Found response.</returns>
        [HttpGet("{code}")]
        public async Task<IActionResult> GetMentee(string code)
        {
            var mentee = await _context.Mentees.FindAsync(code);
            if (mentee == null)
                return NotFound("Mentee not found.");
            return Ok(mentee);
        }

        /// <summary>
        /// Retrieves the count of accepted mentor requests for a specific mentee that have not been marked as read. The method takes the mentee's code as a parameter, queries the database for mentor requests associated with that mentee where the request status is "accepted" (indicated by RequestStatusId == 2) and the request has not been read (IsRead == false). It then returns the count of such requests. This endpoint allows clients to determine how many accepted mentor requests are pending for a mentee, which can be useful for displaying notifications or alerts in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve accepted requests count.</param>
        /// <returns>The count of accepted mentor requests if found, otherwise a 404 Not Found response.</returns>
        [HttpGet("acceptedRequestsCount/{menteeCode}")]
        public async Task<IActionResult> GetAcceptedRequestsCount(string menteeCode)
        {
            var count = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 2 && (r.IsRead == false))
                .CountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Retrieves a list of accepted mentor requests for a specific mentee that have not been marked as read. The method takes the mentee's code as a parameter, performs a database query to join the MentorRequests and Users tables to fetch details about the mentor associated with each accepted request. It filters the results to include only those requests where the mentee code matches, the request status is "accepted" (RequestStatusId == 2), and the request has not been read (IsRead == false). The response includes the request ID, mentor's name and last name, and the date of the request. This endpoint allows clients to display detailed information about pending accepted mentor requests for a mentee in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve accepted requests.</param>
        /// <returns>The list of accepted mentor requests if found, otherwise a 404 Not Found response.</returns>
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

        /// <summary>
        /// Retrieves the count of rejected mentor requests for a specific mentee that have not been marked as read. The method takes the mentee's code as a parameter, queries the database for mentor requests associated with that mentee where the request status is "rejected" (indicated by RequestStatusId == 3) and the request has not been read (IsRead == false). It then returns the count of such requests. This endpoint allows clients to determine how many rejected mentor requests are pending for a mentee, which can be useful for displaying notifications or alerts in the frontend application regarding rejected requests.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve rejected requests.</param>
        /// <returns>The count of rejected mentor requests if found, otherwise a 404 Not Found response.</returns>
        [HttpGet("rejectedRequestsCount/{menteeCode}")]
        public async Task<IActionResult> GetRejectedRequestsCount(string menteeCode)
        {
            var count = await _context.MentorRequests
                .Where(r => r.MenteeCode == menteeCode && r.RequestStatusId == 3 && (r.IsRead == false))
                .CountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Retrieves a list of rejected mentor requests for a specific mentee that have not been marked as read. The method takes the mentee's code as a parameter, performs a database query to join the MentorRequests and Users tables to fetch details about the mentor associated with each rejected request. It filters the results to include only those requests where the mentee code matches, the request status is "rejected" (RequestStatusId == 3), and the request has not been read (IsRead == false). The response includes the request ID, mentor's name and last name, the date of the request, and the reason for rejection. This endpoint allows clients to display detailed information about pending rejected mentor requests for a mentee in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve rejected requests.</param>
        /// <returns>The list of rejected mentor requests if found, otherwise a 404 Not Found response.</returns>
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


        /// <summary>
        /// Marks all rejected mentor request notifications as read for a specific mentee. The method takes the mentee's code as a parameter, queries the database for mentor requests associated with that mentee where the request status is "rejected" (RequestStatusId == 3) and the request has not been read (IsRead == false). It then iterates through the list of such requests, sets the IsRead property to true for each request, and saves the changes to the database. Finally, it returns an Ok response indicating that the operation was successful. This endpoint allows clients to mark all rejected mentor request notifications as read for a mentee, which can be useful for managing notification states in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to mark rejected notifications as read.</param>
        /// <returns>An Ok response indicating the operation was successful.</returns>
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

        /// <summary>
        /// Marks all accepted mentor request notifications as read for a specific mentee. The method takes the mentee's code as a parameter, queries the database for mentor requests associated with that mentee where the request status is "accepted" (RequestStatusId == 2) and the request has not been read (IsRead == false). It then iterates through the list of such requests, sets the IsRead property to true for each request, and saves the changes to the database. Finally, it returns an Ok response indicating that the operation was successful. This endpoint allows clients to mark all accepted mentor request notifications as read for a mentee, which can be useful for managing notification states in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to mark accepted notifications as read.</param>
        /// <returns>An Ok response indicating the operation was successful.</returns>
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
        
        /// <summary>
        /// Retrieves the educational results for a specific mentee based on their most recent semester plan. The method takes the mentee's code as a parameter, queries the database to find the most recent semester plan for that mentee, and then retrieves the associated tasks and feedback for that semester plan. The response includes details about each task, such as the task name, deadline, completion file, whether it is rated, and any feedback ratings and comments. Additionally, it calculates the average rating for the tasks that have feedback. If no semester plan is found for the mentee, it returns a 404 Not Found response with an appropriate message. This endpoint allows clients to fetch comprehensive educational results for a mentee based on their latest semester plan.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve educational results.</param>
        /// <returns>A response containing the educational results for the mentee.</returns>
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

        /// <summary>
        /// Retrieves the mentor information for a specific mentee. The method takes the mentee's code as a parameter, queries the database to find the mentee, and then checks if the mentee has an associated mentor code. If a mentor code exists, it retrieves the mentor's user information from the database and returns the mentor's name and last name. If no mentor is found or if the mentee does not have a mentor code, it returns null. This endpoint allows clients to fetch basic information about a mentee's mentor, which can be useful for displaying mentor details in the frontend application.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve mentor information.</param>
        /// <returns>A response containing the mentor's name and last name, or null if no mentor is found.</returns>
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

        /// <summary>
        /// Retrieves the count of incomplete tasks for a specific mentee based on their most recent semester plan. The method takes the mentee's code as a parameter, queries the database to find the most recent semester plan for that mentee, and then retrieves the associated tasks for that semester plan. It counts the number of tasks that are considered incomplete, which is determined by checking if the CompletionFile property is null or whitespace. Finally, it returns the count of incomplete tasks. If no semester plan is found for the mentee, it returns a count of 0. This endpoint allows clients to determine how many tasks are still pending for a mentee based on their latest semester plan.
        /// </summary>
        /// <param name="menteeCode">The unique code of the mentee for whom to retrieve incomplete task count.</param>
        /// <returns>The count of incomplete tasks for the specified mentee.</returns>
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