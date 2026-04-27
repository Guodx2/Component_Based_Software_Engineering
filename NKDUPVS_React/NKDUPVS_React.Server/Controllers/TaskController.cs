using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Task = NKDUPVS_React.Server.Models.Task;

namespace NKDUPVS_React.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userCode}")]
        public async Task<IActionResult> GetTasksByUser(string userCode)
        {
            // Query tasks where CreatedBy equals the provided user code.
            var tasks = await _context.Tasks
                                      .Where(t => t.CreatedBy == userCode)
                                      .ToListAsync();

            return Ok(tasks);
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        [HttpPost("createForMentees")]
        public async System.Threading.Tasks.Task<IActionResult> CreateTaskForMentees([FromBody] TaskCreationRequest request)
        {
            // Explicitly qualify your Task model:
            var taskEntity = new NKDUPVS_React.Server.Models.Task
            {
                Name = request.Name,
                Description = request.Description,
                MaterialLink = request.MaterialLink,
                Deadline = request.Deadline,
                CreatedBy = request.CreatedBy,
                IsAssigned = request.IsAssigned,
                TaskTypeId = request.TaskTypeId
            };

            _context.Tasks.Add(taskEntity);
            await _context.SaveChangesAsync();

            if (request.MenteeCodes != null && request.MenteeCodes.Count > 0)
            {
                foreach (var menteeCode in request.MenteeCodes)
                {
                    var semesterPlan = await _context.SemesterPlans
                        .FirstOrDefaultAsync(sp => sp.MenteeCode == menteeCode);

                    if (semesterPlan != null)
                    {
                        var semesterPlanTask = new SemesterPlanTask
                        {
                            TaskId = taskEntity.Id_Task,
                            SemesterPlanId = semesterPlan.Id_SemesterPlan,
                            CompletionFile = null,
                            IsRated = false
                        };
                        _context.SemesterPlanTasks.Add(semesterPlanTask);
                    }
                }
                taskEntity.IsAssigned = true;
                await _context.SaveChangesAsync();
            }

            return Ok(taskEntity);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskCreationRequest updatedRequest)
        {
            // Find the task to update.
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Update task properties.
            task.Name = updatedRequest.Name;
            task.Description = updatedRequest.Description;
            task.MaterialLink = updatedRequest.MaterialLink;
            task.Deadline = updatedRequest.Deadline;
            task.IsAssigned = updatedRequest.IsAssigned;
            task.TaskTypeId = updatedRequest.TaskTypeId;

            // Update assignments.
            // Get the list of mentee codes submitted.
            var newMenteeCodes = updatedRequest.MenteeCodes ?? new List<string>();

            // Get current assignments with their mentee codes.
            var currentAssignments = await _context.SemesterPlanTasks
                .Join(_context.SemesterPlans,
                      spt => spt.SemesterPlanId,
                      sp => sp.Id_SemesterPlan,
                      (spt, sp) => new { spt, sp.MenteeCode })
                .Where(x => x.spt.TaskId == id)
                .ToListAsync();

            // Remove assignments that are no longer in the new list.
            var assignmentsToRemove = currentAssignments
                .Where(x => !newMenteeCodes.Contains(x.MenteeCode))
                .Select(x => x.spt)
                .ToList();
            if(assignmentsToRemove.Any())
            {
                _context.SemesterPlanTasks.RemoveRange(assignmentsToRemove);
            }

            // For each new mentee code, add an assignment if one does not already exist.
            foreach (var menteeCode in newMenteeCodes)
            {
                bool exists = await _context.SemesterPlanTasks.AnyAsync(spt =>
                    spt.TaskId == id &&
                    _context.SemesterPlans.Any(sp => sp.Id_SemesterPlan == spt.SemesterPlanId && sp.MenteeCode == menteeCode)
                );
                if (!exists)
                {
                    // Find the semester plan for this mentee.
                    var semesterPlan = await _context.SemesterPlans.FirstOrDefaultAsync(sp => sp.MenteeCode == menteeCode);
                    if (semesterPlan != null)
                    {
                        _context.SemesterPlanTasks.Add(new SemesterPlanTask
                        {
                            TaskId = id,
                            SemesterPlanId = semesterPlan.Id_SemesterPlan,
                            CompletionFile = null,
                            IsRated = false
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Get all assignments for the task.
            var relatedAssignments = _context.SemesterPlanTasks.Where(spt => spt.TaskId == id);
            if (relatedAssignments.Any())
            {
                var assignmentIds = await relatedAssignments.Select(spt => spt.Id_SemesterPlanTask).ToListAsync();
                bool hasFeedback = await _context.Feedback.AnyAsync(f => assignmentIds.Contains(f.SemesterPlanTaskId));
                if (hasFeedback)
                {
                    return BadRequest("Užduoties pašalinti negalima, nes ją atliko bent vienas ugdytinis.");
                }
                else
                {
                    _context.SemesterPlanTasks.RemoveRange(relatedAssignments);
                }
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignTask([FromBody] AssignTaskRequest request)
        {
            // Filter out null or empty mentee codes
            var validMenteeCodes = request.MenteeCodes?
                                            .Where(code => !string.IsNullOrWhiteSpace(code))
                                            .ToList() 
                                       ?? new List<string>();

            // If no valid mentee codes provided, then remove any assignments and mark task as unassigned.
            if(validMenteeCodes.Count == 0)
            {
                // Remove all assignments associated with the task.
                var allAssignments = await _context.SemesterPlanTasks
                    .Where(spt => spt.TaskId == request.TaskId)
                    .ToListAsync();
                if(allAssignments.Any())
                {
                    _context.SemesterPlanTasks.RemoveRange(allAssignments);
                }
                
                // Mark the task as not assigned.
                var task = await _context.Tasks.FindAsync(request.TaskId);
                if(task != null)
                {
                    task.IsAssigned = false;
                }

                await _context.SaveChangesAsync();
                return Ok("Task assignments cleared.");
            }

            // Get current assignments for this task (mapping to mentee codes)
            var currentAssignments = await _context.SemesterPlanTasks
                .Join(
                    _context.SemesterPlans,
                    spt => spt.SemesterPlanId,
                    sp => sp.Id_SemesterPlan,
                    (spt, sp) => new { spt, sp.MenteeCode }
                )
                .Where(x => x.spt.TaskId == request.TaskId)
                .Select(x => x.MenteeCode)
                .ToListAsync();

            // Remove assignments that are not in the updated list
            var assignmentsToRemove = await _context.SemesterPlanTasks
                .Join(
                    _context.SemesterPlans,
                    spt => spt.SemesterPlanId,
                    sp => sp.Id_SemesterPlan,
                    (spt, sp) => new { spt, sp.MenteeCode }
                )
                .Where(x => x.spt.TaskId == request.TaskId && !validMenteeCodes.Contains(x.MenteeCode))
                .Select(x => x.spt)
                .ToListAsync();

            if(assignmentsToRemove.Any())
            {
                _context.SemesterPlanTasks.RemoveRange(assignmentsToRemove);
            }

            // For each valid mentee in the new list, add assignment if it doesn't exist
            // Inside the foreach loop for validMenteeCodes:
            foreach(var menteeCode in validMenteeCodes)
            {
                // Look up semester plan for this mentee
                var semesterPlan = await _context.SemesterPlans
                    .FirstOrDefaultAsync(sp => sp.MenteeCode == menteeCode);
                
                // If not found, create one
                if(semesterPlan == null)
                {
                    var now = DateTime.Now;
                    DateTime startDate, endDate;
                    if (now.Month >= 2 && now.Month <= 5)
                    {
                        startDate = new DateTime(now.Year, 2, 1);
                        endDate = new DateTime(now.Year, 5, 31);
                    }
                    else if (now.Month >= 9 && now.Month <= 12)
                    {
                        startDate = new DateTime(now.Year, 9, 1);
                        endDate = new DateTime(now.Year, 12, 31);
                    }
                    else
                    {
                        startDate = new DateTime(now.Year, 2, 1);
                        endDate = new DateTime(now.Year, 5, 31);
                    }
                    
                    semesterPlan = new SemesterPlan
                    {
                        MenteeCode = menteeCode,
                        SemesterStartDate = startDate,
                        SemesterEndDate = endDate
                    };
                    _context.SemesterPlans.Add(semesterPlan);
                    await _context.SaveChangesAsync();  // so that semesterPlan.Id becomes available
                }
                
                // If assignment doesn't exist, add it
                var exists = await _context.SemesterPlanTasks
                    .AnyAsync(spt => spt.TaskId == request.TaskId && spt.SemesterPlanId == semesterPlan.Id_SemesterPlan);
                if(!exists)
                {
                    _context.SemesterPlanTasks.Add(new SemesterPlanTask
                    {
                        TaskId = request.TaskId.Value,
                        SemesterPlanId = semesterPlan.Id_SemesterPlan,
                        CompletionFile = null,
                        IsRated = false   // Set default value!
                    });
                }
            }
            
            // Mark the task as assigned.
            var taskToUpdate = await _context.Tasks.FindAsync(request.TaskId);
            if(taskToUpdate != null)
            {
                taskToUpdate.IsAssigned = true;
            }
            
            await _context.SaveChangesAsync();
            return Ok("Task assigned successfully.");
        }

        [HttpGet("assignments/{taskId}")]
        public async Task<IActionResult> GetTaskAssignments(int taskId)
        {
            var assignmentMenteeCodes = await _context.SemesterPlanTasks
                .Join(_context.SemesterPlans,
                      spt => spt.SemesterPlanId,
                      sp => sp.Id_SemesterPlan,
                      (spt, sp) => new { spt, sp })
                .Where(x => x.spt.TaskId == taskId)
                .Select(x => x.sp.MenteeCode)
                .ToListAsync();

            return Ok(assignmentMenteeCodes);
        }

        [HttpGet("assignedTasks/{menteeCode}")]
        public async Task<IActionResult> GetAssignedTasks(string menteeCode)
        {
            var tasks = await (
                from spt in _context.SemesterPlanTasks
                join sp in _context.SemesterPlans on spt.SemesterPlanId equals sp.Id_SemesterPlan
                where sp.MenteeCode == menteeCode
                join t in _context.Tasks on spt.TaskId equals t.Id_Task
                // Left join with Feedback (if feedback exists for a task)
                join f in _context.Feedback on spt.Id_SemesterPlanTask equals f.SemesterPlanTaskId into fGroup
                from feedback in fGroup.DefaultIfEmpty()
                select new {
                    id_Task = t.Id_Task,
                    name = t.Name,
                    description = t.Description,
                    materialLink = t.MaterialLink,
                    deadline = t.Deadline,
                    completionFile = spt.CompletionFile,
                    semesterPlanTaskId = spt.Id_SemesterPlanTask,
                    isRated = spt.IsRated,
                    feedbackRating = feedback != null ? feedback.Rating : (int?)null,
                    feedbackComment = feedback != null ? feedback.Comment : null,
                    taskTypeId = t.TaskTypeId   // Added to include task type
                }
            ).ToListAsync();

            return Ok(tasks);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> MarkTaskCompleted([FromBody] CompleteTaskRequest request)
        {
            // Find the mentee's semester plan using the provided MenteeCode.
            var semesterPlan = await _context.SemesterPlans
                .FirstOrDefaultAsync(sp => sp.MenteeCode == request.MenteeCode);
            if (semesterPlan == null)
            {
                return NotFound("Semester plan not found for the specified mentee.");
            }
            
            // Find the assigned task record for that semester plan.
            var semesterPlanTask = await _context.SemesterPlanTasks
                .FirstOrDefaultAsync(spt => spt.TaskId == request.TaskId && spt.SemesterPlanId == semesterPlan.Id_SemesterPlan);
            if (semesterPlanTask == null)
            {
                return NotFound("Task assignment not found for this mentee.");
            }
            
            // If no link provided, use a default value to indicate completion.
            if (string.IsNullOrWhiteSpace(request.CompletionFile))
            {
                semesterPlanTask.CompletionFile = "completed";
            }
            else
            {
                semesterPlanTask.CompletionFile = request.CompletionFile;
            }
            
            await _context.SaveChangesAsync();
            return Ok(semesterPlanTask);
        }

        [HttpGet("taskTypes")]
        public async Task<IActionResult> GetTaskTypes()
        {
            var taskTypes = await _context.TaskTypes.ToListAsync();
            return Ok(taskTypes);
        }

        public class CompleteTaskRequest
        {
            public int? TaskId { get; set; }
            public string? MenteeCode { get; set; }
            public string? CompletionFile { get; set; }
        }

        public class AssignTaskRequest
        {
            public int? TaskId { get; set; }
            public string? MentorCode { get; set; }
            public List<string>? MenteeCodes { get; set; }
        }

        public class TaskCreationRequest
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? MaterialLink { get; set; }
            public DateTime? Deadline { get; set; }
            public string? CreatedBy { get; set; }
            public bool? IsAssigned { get; set; }
            public int? TaskTypeId { get; set; }
            public List<string>? MenteeCodes { get; set; }  // Updated to support multiple mentee codes
        }
    }
}