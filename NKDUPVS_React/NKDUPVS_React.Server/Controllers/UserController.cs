using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using NKDUPVS_React.Server.Services;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public UserController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

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

    // Fixed GetMentees method (duplicate removed and stray comma fixed)
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

    [HttpGet("rejection-history/{userId}")]
    public async Task<IActionResult> GetRejectionHistory(string userId)
    {
        var history = await _context.RejectionHistories
            .Where(r => r.UserCode == userId)
            .OrderByDescending(r => r.RejectedAt)
            .ToListAsync();
        return Ok(history);
    }

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

public class PersonalInfoUpdateRequest
{
    [Required]
    public string Code { get; set; }

    [Required(ErrorMessage = "Vardas privalomas")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Vardas turi būti tarp 2 ir 50 simbolių")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Pavardė privaloma")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Pavardė turi būti tarp 2 ir 50 simbolių")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Telefono numeris privalomas")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "Telefono numeris turi būti tarp 7 ir 15 simbolių")]
    public string PhoneNumber { get; set; }
}

public class MenteeUpdateRequest
{
    public string? Code { get; set; }
    public int? StudyProgram { get; set; }
    public string? PhoneNumber { get; set; }
    public int? Specialization { get; set; }
}

public class MentorUpdateRequest
{
    public string? Code { get; set; }
    public int? Department { get; set; }
    public string? PhoneNumber { get; set; }
}

public class RejectUserRequest
{
    [Required]
    public string? Reason { get; set; }
}