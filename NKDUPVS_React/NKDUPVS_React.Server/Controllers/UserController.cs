using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using NKDUPVS_React.Server.Services;

/// <summary>
/// Controller responsible for handling API requests related to user management, including updating user information, retrieving lists of mentors and mentees, accepting or rejecting user submissions, and managing user class assignments. This controller interacts with the database to perform CRUD operations on user data and related entities such as mentors, mentees, and user classes. It also includes functionality for sending email notifications when a user's submission is rejected and for maintaining a history of rejections. The endpoints in this controller allow for efficient management of users within the application, enabling administrators to review and verify user submissions, assign users to classes, and maintain accurate records of user activity and status.
/// </summary>
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Constructor for the UserController, which takes instances of ApplicationDbContext and IEmailService as parameters. This allows the controller to interact with the database to manage user data and to send email notifications when necessary. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to perform database operations such as querying for users, updating user information, and managing related entities like mentors and mentees. The IEmailService is also injected to facilitate sending emails, particularly when a user's submission is rejected, allowing for effective communication with users regarding their account status and any actions they may need to take.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="emailService">The email service for sending notifications.</param>
    public UserController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    /// <summary>
    /// Handles POST requests to update a mentee's information. This method accepts a MenteeUpdateRequest object in the request body, which contains the user's code, phone number, study program, and specialization. The method first checks if the request model is valid, and if not, it returns a bad request response with the model state errors. It then retrieves the user from the database using the provided code. If the user does not exist, it returns a not found response. If the user exists, it updates the user's phone number and sets the IsSubmitted flag to true. The method then checks if a mentee record already exists for the user; if it does, it updates the study program and specialization. If it does not exist, it creates a new mentee record with the provided information. Finally, it saves the changes to the database and returns an OK response to indicate that the update was successful.
    /// </summary>
    /// <param name="request">The request containing the updated mentee information.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPost("update/mentee")]
    public async Task<IActionResult> UpdateMentee([FromBody] MenteeUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(request.Code);
        if (user == null)
        {
            return NotFound();
        }

        user.PhoneNumber = request.PhoneNumber;
        user.IsSubmitted = true;

        // Update or create mentee record
        var mentee = await _context.Mentees.FindAsync(request.Code);
        if (mentee != null)
        {
            mentee.StudyProgram = request.StudyProgram;
            mentee.Specialization = request.Specialization;
        }
        else
        {
            var newMentee = new Mentee
            {
                Code = request.Code,
                StudyProgram = request.StudyProgram,
                Specialization = request.Specialization,
            };
            _context.Mentees.Add(newMentee);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Handles POST requests to update a mentor's information. This method accepts a MentorUpdateRequest object in the request body, which contains the user's code, phone number, and department. The method first checks if the request model is valid, and if not, it returns a bad request response with the model state errors. It then retrieves the user from the database using the provided code. If the user does not exist, it returns a not found response. If the user exists, it updates the user's phone number and sets the IsSubmitted flag to true. The method then checks if a mentor record already exists for the user; if it does, it updates the department. If it does not exist, it creates a new mentor record with the provided information. Finally, it saves the changes to the database and returns an OK response to indicate that the update was successful.
    /// </summary>
    /// <param name="request">The request containing the updated mentor information.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPost("update/mentor")]
    public async Task<IActionResult> UpdateMentor([FromBody] MentorUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(request.Code);
        if (user == null)
        {
            return NotFound($"No user found for code '{request.Code}'.");
        }

        user.PhoneNumber = request.PhoneNumber;
        user.IsSubmitted = true;

        var mentor = await _context.Mentors.FindAsync(request.Code);
        if (mentor != null)
        {
            mentor.Department = request.Department;
        }
        else
        {
            var newMentor = new Mentor
            {
                Code = request.Code,
                Department = request.Department,
            };
            _context.Mentors.Add(newMentor);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Handles GET requests to retrieve a list of mentors. This method queries the Users table in the database to find users who are verified, have submitted their information, are not administrators, and have a corresponding record in the Mentors table. It then selects relevant properties (id, name, email, phone, last name, and department) for each mentor and returns the list of mentors in the response. This endpoint allows clients to retrieve information about mentors for display or further processing in the frontend application.
    /// </summary>
    /// <returns>The list of mentors.</returns>
    [HttpGet("mentors")]
    public async Task<IActionResult> GetMentors()
    {
        var mentors = await _context.Users
            .Where(u => (u.IsVerified == true) && (u.IsSubmitted == true) && (u.IsAdmin == false) &&
                _context.Mentors.Any(m => m.Code == u.Code))
            .Select(u => new 
            {
                id = u.Code,
                name = u.Name,
                email = u.Email,
                phone = u.PhoneNumber,
                lastName = u.LastName,
                department = _context.Mentors
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.Department)
                    .FirstOrDefault()
            })
            .ToListAsync();
        return Ok(mentors);
    }

    /// <summary>
    /// Handles GET requests to retrieve a list of mentees. This method queries the Users table in the database to find users who are verified, have submitted their information, are not administrators, and have a corresponding record in the Mentees table. It then selects relevant properties (id, name, email, phone, last name, study program, and specialization) for each mentee and returns the list of mentees in the response. This endpoint allows clients to retrieve information about mentees for display or further processing in the frontend application.
    /// </summary>
    /// <returns>The list of mentees.</returns>
    [HttpGet("mentees")]
    public async Task<IActionResult> GetMentees()
    {
        var mentees = await _context.Users
            .Where(u => (u.IsVerified == true) && (u.IsSubmitted == true) && (u.IsAdmin == false) &&
                _context.Mentees.Any(m => m.Code == u.Code))
            .Select(u => new
            {
                id = u.Code,
                name = u.Name,
                email = u.Email,
                phone = u.PhoneNumber,
                lastName = u.LastName,
                studyProgram = _context.Mentees
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.StudyProgram)
                    .FirstOrDefault(),
                specialization = _context.Mentees
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.Specialization)
                    .FirstOrDefault()
            })
            .ToListAsync();
        return Ok(mentees);
    }

    /// <summary>
    /// Handles GET requests to retrieve a list of mentees for a specific mentor. This method accepts a mentor code as a parameter and queries the Mentees table in the database to find mentees who are assigned to the specified mentor. It then joins the Mentees table with the Users table to select relevant properties (id, name, email, phone, last name, study program, and specialization) for each mentee and returns the list of mentees in the response. This endpoint allows clients to retrieve information about mentees assigned to a specific mentor for display or further processing in the frontend application.
    /// </summary>
    /// <param name="mentorCode">The code of the mentor for whom to retrieve mentees.</param>
    /// <returns>The list of mentees assigned to the specified mentor.</returns>
    [HttpGet("mentees/mentor/{mentorCode}")]
    public async Task<IActionResult> GetMenteesForMentor(string mentorCode)
    {
        var mentees = await _context.Mentees
            .Where(m => (m.MentorCode == mentorCode))
            .Join(_context.Users, 
                  m => m.Code, 
                  u => u.Code,
                  (m, u) => new {
                      id = u.Code,
                      name = u.Name,
                      lastName = u.LastName,
                      email = u.Email,
                      phone = u.PhoneNumber,
                      studyProgram = m.StudyProgram,
                      specialization = m.Specialization
                  })
            .ToListAsync();
        return Ok(mentees);
    }

    /// <summary>
    /// Handles GET requests to retrieve a list of users who have submitted their information but are not yet verified. This method queries the Users table in the database to find users who have the IsSubmitted flag set to true, the IsVerified flag set to false, and are not administrators. It then selects relevant properties (id, name, email, phone, last name, study program, specialization, and department) for each user and returns the list of submitted users in the response. This endpoint allows clients to retrieve information about users who are pending verification for display or further processing in the frontend application.
    /// </summary>
    /// <returns>The list of submitted users.</returns>
    [HttpGet("submitted")]
    public async Task<IActionResult> GetSubmittedUsers()
    {
        var submitted = await _context.Users
            .Where(u => (u.IsSubmitted == true) && (u.IsVerified == false) && (u.IsAdmin == false))
            .Select(u => new
            {
                id = u.Code,
                name = u.Name,
                lastName = u.LastName,
                email = u.Email,
                phone = u.PhoneNumber,
                studyProgram = _context.Mentees
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.StudyProgram)
                    .FirstOrDefault(),
                specialization = _context.Mentees
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.Specialization)
                    .FirstOrDefault(),
                department = _context.Mentors
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.Department)
                    .FirstOrDefault()
            })
            .ToListAsync();
        return Ok(submitted);
    }

    /// <summary>
    /// Handles GET requests to retrieve a list of users who have not submitted their information and are not verified. This method queries the Users table in the database to find users who have the IsSubmitted flag set to false, the IsVerified flag set to false, and are not administrators. It then selects relevant properties (id, name, email, phone, last name, study program, and department) for each user and returns the list of other users in the response. This endpoint allows clients to retrieve information about users who have not yet submitted their information for display or further processing in the frontend application.
    /// </summary>
    /// <returns>The list of other users.</returns>
    [HttpGet("others")]
    public async Task<IActionResult> GetOtherUsers()
    {
        var otherUsers = await _context.Users
            .Where(u => (u.IsSubmitted == false) && (u.IsVerified == false) && (u.IsAdmin == false))
            .Select(u => new
            {
                id = u.Code,
                name = u.Name,
                email = u.Email,
                phone = u.PhoneNumber,
                lastName = u.LastName,
                studyProgram = _context.Mentees
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.StudyProgram)
                    .FirstOrDefault(),
                department = _context.Mentors
                    .Where(m => m.Code == u.Code)
                    .Select(m => m.Department)
                    .FirstOrDefault()
            })
            .ToListAsync();
        return Ok(otherUsers);
    }

    /// <summary>
    /// Handles POST requests to accept a user's submission. This method accepts a user ID as a parameter and retrieves the corresponding user from the database. If the user is not found, it returns a not found response. If the user is found, it sets the IsVerified flag to true. The method then checks if the user has a mentor record with a valid department; if so, it sets the IsMentor flag to true and removes any existing mentee record for the user. If the user does not have a valid mentor record but has a mentee record, it sets the IsMentor flag to false and removes any existing mentor record for the user. Finally, it saves the changes to the database and returns an OK response to indicate that the user has been accepted.
    /// </summary>
    /// <param name="userId">The ID of the user to accept.</param>
    /// <returns>The response indicating the result of the operation.</returns>
    [HttpPost("accept/{userId}")]
    public async Task<IActionResult> AcceptUser(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsVerified = true;

         // Try to find records in both tables
        var mentor = await _context.Mentors.FindAsync(userId);
        var mentee = await _context.Mentees.FindAsync(userId);
        
        if (mentor != null && mentor.Department > 0)
        {
            user.IsMentor = true;
            if (mentee != null)
            {
                _context.Mentees.Remove(mentee);
            }
        }
        else if (mentee != null)
        {
            user.IsMentor = false;
            if (mentor != null)
            {
                _context.Mentors.Remove(mentor);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }


    /// <summary>
    /// Handles POST requests to reject a user's submission. This method accepts a user ID as a parameter and a RejectUserRequest object in the request body, which contains the reason for rejection. The method first checks if the request model is valid, and if not, it returns a bad request response with the model state errors. It then retrieves the user from the database using the provided user ID. If the user is not found, it returns a not found response. If the user is found, it sets the IsSubmitted and IsVerified flags to false. The method then removes any dependent mentor expertise records, mentor records, and mentee records associated with the user to ensure data integrity. After saving these changes to the database, it sends an email notification to the user informing them of the rejection and the reason for it. Finally, it logs the rejection reason in a RejectionHistory table for future reference and returns an OK response to indicate that the user has been rejected.
    /// </summary>
    /// <param name="userId">The ID of the user to reject.</param>
    /// <param name="request">The request containing the rejection reason.</param>
    /// <returns>The response indicating the result of the operation.</returns>
    [HttpPost("reject/{userId}")]
    public async Task<IActionResult> RejectUser(string userId, [FromBody] RejectUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsSubmitted = false;
        user.IsVerified = false;

        // Remove dependent mentor expertise records before removing the mentor record.
        // Adjust "MentorExpertise" to your actual model name.
        var mentorExpertises = _context.MentorExpertises.Where(e => e.MentorCode == userId);
        if (mentorExpertises.Any())
        {
            _context.MentorExpertises.RemoveRange(mentorExpertises);
        }

        var mentor = await _context.Mentors.FindAsync(userId);
        if (mentor != null)
        {
            _context.Mentors.Remove(mentor);
        }

        var mentee = await _context.Mentees.FindAsync(userId);
        if (mentee != null)
        {
            _context.Mentees.Remove(mentee);
        }

        await _context.SaveChangesAsync();

        // (The rest of your email sending and history logging code follows below.)
        var subject = "Jūsų paskyros patvirtinimas atmestas";
        var plainText = $"Sveiki {user.Name},\n\nJūsų paskyra buvo atmesta dėl šios priežasties:\n{request.Reason}\n\nJeigu turite klausimų, kreipkitės į administraciją.";
        var htmlContent = @"
        <html>
          <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
            <div style='max-width:600px; margin:0 auto; background-color:#fff; padding:20px; border-radius:8px;'>
              <h2 style='color:#333;'>Paskyros atmetimas</h2>
              <p>Sveiki " + user.Name + @",</p>
              <p>Jūsų paskyra buvo atmesta dėl šios priežasties:</p>
              <blockquote style='border-left: 4px solid #ccc; padding-left:16px; color:#666;'>
                " + request.Reason + @"
              </blockquote>
              <p>Jeigu turite klausimų, kreipkitės į administraciją.</p>
              <br/>
              <p>Pagarbiai,<br/>Administracija</p>
            </div>
          </body>
        </html>";

        await _emailService.SendEmailAsync(user.Email, subject, plainText, htmlContent);

        var historyEntry = new RejectionHistory
        {
            UserCode = userId,
            Reason = request.Reason,
        };

        _context.RejectionHistories.Add(historyEntry);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Handles GET requests to retrieve the rejection history for a specific user. This method accepts a user ID as a parameter and queries the RejectionHistories table in the database to find all rejection records associated with the specified user. It orders the results by the rejection date in descending order and returns the list of rejection history entries in the response. This endpoint allows clients to retrieve information about past rejections for a user, which can be useful for understanding the reasons for previous rejections and for tracking user status over time.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve rejection history.</param>
    /// <returns>The list of rejection history entries.</returns>
    [HttpGet("rejection-history/{userId}")]
    public async Task<IActionResult> GetRejectionHistory(string userId)
    {
        var history = await _context.RejectionHistories
            .Where(r => r.UserCode == userId)
            .OrderByDescending(r => r.RejectedAt)
            .ToListAsync();
        return Ok(history);
    }

    /// <summary>
    /// Handles DELETE requests to delete a user. This method accepts a user ID as a parameter and retrieves the corresponding user from the database. If the user is not found, it returns a not found response. If the user is found, it first removes any mentor request records that reference the user as a mentee to maintain referential integrity. It then checks if the user has a mentor record; if so, it verifies that there are no mentees assigned to this mentor before allowing deletion. If there are assigned mentees, it returns a bad request response indicating that the mentor cannot be deleted. If the user has a mentee record, it checks for any recent class assignments
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    /// <returns>The response indicating the result of the operation.</returns>
    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        
        // Remove mentor request records referencing the mentee and save changes immediately.
        var mentorRequests = _context.MentorRequests.Where(mr => mr.MenteeCode == userId);
        _context.MentorRequests.RemoveRange(mentorRequests);
        await _context.SaveChangesAsync(); // Remove child rows first

        // Remove mentor record if it exists (after checking dependent mentees)
        var mentor = await _context.Mentors.FindAsync(userId);
        if (mentor != null)
        {
            bool hasMentees = await _context.Mentees.AnyAsync(m => m.MentorCode == userId);
            if (hasMentees)
            {
                return BadRequest("Negalima ištrinti mentoriaus paskyros, nes jam priskirti ugdytiniai.");
            }
            _context.Mentors.Remove(mentor);
        }

        // Remove mentee record if it exists
        var mentee = await _context.Mentees.FindAsync(userId);
        if (mentee != null)
        {
            bool hasRecentClasses = await _context.UserClasses.AnyAsync(uc =>
                uc.UserCode == userId &&
                uc.StartTime >= DateTime.Now.AddYears(-4));
            if (hasRecentClasses)
            {
                return BadRequest("Negalima ištrinti ugdytinio paskyros, nes yra užsiėmimų per pastaruosius 4 metus.");
            }
            _context.Mentees.Remove(mentee);
        }

        // Remove related user classes and rejection histories
        var userClasses = _context.UserClasses.Where(uc => uc.UserCode == userId);
        _context.UserClasses.RemoveRange(userClasses);

        var rejections = _context.RejectionHistories.Where(r => r.UserCode == userId);
        _context.RejectionHistories.RemoveRange(rejections);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Handles POST requests to update a user's personal information. This method accepts a PersonalInfoUpdateRequest object in the request body, which contains the user's code, name, last name, and phone number. The method first checks if the request model is valid, and if not, it returns a bad request response with the model state errors. It then retrieves the user from the database using the provided code. If the user is not found, it returns a not found response. If the user is found, it updates the user's name, last name, and phone number with the values from the request. Finally, it saves the changes to the database and returns an OK response to indicate that the update was successful.
    /// </summary>
    /// <param name="request">The request containing the updated personal information.</param>
    /// <returns>The response indicating the result of the operation.</returns>
    [HttpPost("update/personal")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] PersonalInfoUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(request.Code);
        if (user == null)
        {
            return NotFound();
        }

        user.Name = request.Name;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Handles GET requests to retrieve activity statistics for a specific mentee. This method accepts a mentee code as a parameter and retrieves the most recent semester plan for the mentee, regardless of whether it is active. It then counts the total number of trainings and affairs whose event periods overlap with the semester plan's start and end dates. Additionally, it counts how many of those trainings and affairs the mentee has registered for by checking for corresponding records in the SemesterPlanEvents table. Finally, it returns an object containing the total number of trainings, registered trainings, total number of affairs, and registered affairs in the response. This endpoint allows clients to retrieve information about a mentee's activity and engagement with training and affair events during their semester plan.
    /// </summary>
    /// <param name="menteeCode">The code of the mentee for whom to retrieve activity statistics.</param>
    /// <returns>The activity statistics for the specified mentee.</returns>
    [HttpGet("activityStats/{menteeCode}")]
    public IActionResult GetMenteeActivityStats(string menteeCode)
    {
        // Use the most recent semester plan for the mentee regardless of whether it's active.
        var semesterPlan = _context.SemesterPlans
            .Where(sp => sp.MenteeCode == menteeCode)
            .OrderByDescending(sp => sp.SemesterStartDate)
            .FirstOrDefault();
            
        if (semesterPlan == null)
        {
            return Ok(new {
                totalTrainings = 0,
                registeredTrainings = 0,
                totalAffairs = 0,
                registeredAffairs = 0
            });
        }
        
        // Count trainings whose event period overlaps with the semester.
        int totalTrainings = _context.Trainings
            .Include(t => t.Event)
            .Where(t => t.Event.startTime <= semesterPlan.SemesterEndDate &&
                        t.Event.endTime >= semesterPlan.SemesterStartDate)
            .Count();
        
        // Count affairs similarly.
        int totalAffairs = _context.Affairs
            .Include(a => a.Event)
            .Where(a => a.Event.startTime <= semesterPlan.SemesterEndDate &&
                        a.Event.endTime >= semesterPlan.SemesterStartDate)
            .Count();
        
        // A registration exists if a SemesterPlanEvent record for this semester matches an event in Trainings.
        int registeredTrainings = _context.SemesterPlanEvents
            .Where(spe => spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan &&
                        _context.Trainings.Any(t => t.event_id == spe.Fk_Eventid_Event))
            .Count();
        
        // And for affairs.
        int registeredAffairs = _context.SemesterPlanEvents
            .Where(spe => spe.Fk_SemesterPlanid_SemesterPlan == semesterPlan.Id_SemesterPlan &&
                        _context.Affairs.Any(a => a.event_id == spe.Fk_Eventid_Event))
            .Count();
        
        return Ok(new {
            totalTrainings,
            registeredTrainings,
            totalAffairs,
            registeredAffairs
        });
    }
}

/// <summary>
/// Request model for updating a user's personal information, including their name, last name, and phone number. This model is used in the UpdatePersonalInfo endpoint of the UserController to receive and validate the data sent by the client when a user wants to update their personal details. The Code property is required to identify which user is being updated, while the Name, LastName, and PhoneNumber properties are also required and have validation attributes to ensure that they meet certain criteria (e.g., length constraints). This structured request model helps ensure that the data received from the client is valid and can be processed correctly by the controller.
/// </summary>
public class PersonalInfoUpdateRequest
{
    /// <summary>
    /// The unique code of the user whose personal information is being updated. This property is required to identify the specific user in the database that the update operation should be applied to. The controller will use this code to retrieve the user record and apply the changes to the correct user. Without this code, the controller would not be able to determine which user's information needs to be updated, making it a critical part of the request model for updating personal information.
    /// </summary>
    [Required]
    public string Code { get; set; }

    /// <summary>
    /// The first name of the user. This property is required and must be between 2 and 50 characters in length. The validation attributes ensure that the name provided by the client meets these criteria, which helps maintain data integrity and consistency in the database. When a user submits a request to update their personal information, they must provide a valid name that adheres to these constraints for the update to be successful.
    /// </summary>
    [Required(ErrorMessage = "Vardas privalomas")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Vardas turi būti tarp 2 ir 50 simbolių")]
    public string Name { get; set; }

    /// <summary>
    /// The last name of the user. This property is required and must be between 2 and 50 characters in length. The validation attributes ensure that the last name provided by the client meets these criteria, which helps maintain data integrity and consistency in the database. When a user submits a request to update their personal information, they must provide a valid last name that adheres to these constraints for the update to be successful.
    /// </summary>
    [Required(ErrorMessage = "Pavardė privaloma")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Pavardė turi būti tarp 2 ir 50 simbolių")]
    public string LastName { get; set; }
    
    /// <summary>
    /// The phone number of the user. This property is required and must be between 7 and 15 characters in length. The validation attributes ensure that the phone number provided by the client meets these criteria, which helps maintain data integrity and consistency in the database. When a user submits a request to update their personal information, they must provide a valid phone number that adheres to these constraints for the update to be successful.
    /// </summary>
    [Required(ErrorMessage = "Telefono numeris privalomas")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "Telefono numeris turi būti tarp 7 ir 15 simbolių")]
    public string PhoneNumber { get; set; }
}

/// <summary>
/// Request model for updating a mentee's information, including their study program, specialization, and phone number. This model is used in the UpdateMentee endpoint of the UserController to receive and validate the data sent by the client when a mentee wants to update their information. The Code property is used to identify which mentee is being updated, while the StudyProgram, Specialization, and PhoneNumber properties are optional and can be updated as needed. This structured request model helps ensure that the data received from the client is valid and can be processed correctly by the controller when updating a mentee's information.
/// </summary>
public class MenteeUpdateRequest
{
    /// <summary>
    /// The unique code identifying the mentee whose information is being updated.
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// The study program to which the mentee belongs.
    /// </summary>
    public int? StudyProgram { get; set; }
    /// <summary>
    /// The phone number of the mentee.
    /// </summary>
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// The specialization of the mentee.
    /// </summary>
    public int? Specialization { get; set; }
}

/// <summary>
/// Request model for updating a mentor's information, including their department and phone number. This model is used in the UpdateMentor endpoint of the UserController to receive and validate the data sent by the client when a mentor wants to update their information. The Code property is used to identify which mentor is being updated, while the Department and PhoneNumber properties are optional and can be updated as needed. This structured request model helps ensure that the data received from the client is valid and can be processed correctly by the controller when updating a mentor's information.
/// </summary>
public class MentorUpdateRequest
{
    /// <summary>
    /// The unique code identifying the mentor whose information is being updated.
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// The department to which the mentor belongs.
    /// </summary>
    public int? Department { get; set; }
    /// <summary>
    /// The phone number of the mentor.
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Request model for rejecting a user's submission, containing the reason for rejection. This model is used in the RejectUser endpoint of the UserController to receive and validate the data sent by the client when an administrator wants to reject a user's submission. The Reason property is required and allows the administrator to provide a specific explanation for why the user's submission was rejected. This information can be used for communication with the user and for logging purposes in the system's rejection history.
/// </summary>
public class RejectUserRequest
{
    /// <summary>
    /// The reason for rejecting the user's submission. This property is required and should provide a clear explanation for why the user's submission was rejected. The reason can be communicated to the user in the rejection email and is also stored in the RejectionHistory table for future reference. Providing a reason helps maintain transparency and allows users to understand what they can improve if they wish to resubmit their information in the future.
    /// </summary>
    [Required]
    public string? Reason { get; set; }
}