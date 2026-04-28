using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Data;
using NKDUPVS_React.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Task = NKDUPVS_React.Server.Models.Task;

namespace NKDUPVS_React.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling API requests related to tasks. This controller provides endpoints for creating, retrieving, updating, and deleting tasks, as well as assigning tasks to mentees and marking tasks as completed. The TaskController interacts with the database through the ApplicationDbContext to perform CRUD operations on tasks and manage task assignments. It also includes functionality to retrieve task types and get assigned tasks for specific mentees, allowing for efficient task management within the application.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor for the TaskController, which takes an instance of ApplicationDbContext as a parameter. This allows the controller to interact with the database to perform CRUD operations on tasks and manage task assignments. The ApplicationDbContext is injected into the controller using dependency injection, enabling it to access the database context for handling API requests related to tasks. This setup is essential for the functionality of the TaskController, as it relies on the database context to fetch, create, update, and delete task data, as well as manage task assignments and retrieve task types for use in the frontend application.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Handles GET requests to retrieve a list of tasks created by a specific user. This method queries the Tasks table in the database, filtering tasks based on the CreatedBy property that matches the provided user code. The resulting list of tasks is returned in the response, allowing users to view and manage their created tasks within the application. This endpoint is essential for providing users with access to their task data and enabling them to perform actions such as updating or deleting their tasks as needed.
        /// </summary>
        /// <param name="userCode">The code of the user for whom to retrieve tasks.</param>
        /// <returns>A list of tasks created by the specified user.</returns>
        [HttpGet("user/{userCode}")]
        public async Task<IActionResult> GetTasksByUser(string userCode)
        {
            // Query tasks where CreatedBy equals the provided user code.
            var tasks = await _context.Tasks
                                      .Where(t => t.CreatedBy == userCode)
                                      .ToListAsync();

            return Ok(tasks);
        }
        
        /// <summary>
        /// Handles POST requests to create a new task. This method accepts a Task object in the request body, adds it to the Tasks table in the database, and saves the changes. Upon successful creation, the newly created task is returned in the response. This endpoint allows users to create new tasks within the application, providing them with the ability to manage their tasks effectively. The Task object should include necessary properties such as Name, Description, MaterialLink, Deadline, CreatedBy, IsAssigned, and TaskTypeId for proper task creation and management.
        /// </summary>
        /// <param name="task">The task to create.</param>
        /// <returns>The created task.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        /// <summary>
        /// Handles POST requests to create a new task and assign it to multiple mentees. This method accepts a TaskCreationRequest object in the request body, which includes task details and a list of mentee codes to whom the task should be assigned. The method creates a new task based on the provided details, saves it to the database, and then iterates through the list of mentee codes to create corresponding SemesterPlanTask entries for each mentee. If any mentee code does not have an associated semester plan, it will be skipped. Finally, the created task is returned in the response. This endpoint allows users to efficiently create tasks and assign them to multiple mentees in one operation, streamlining task management within the application.
        /// </summary>
        /// <param name="request">The request containing task details and mentee codes.</param>
        /// <returns>The created task.</returns>
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

        /// <summary>
        /// Handles PUT requests to update an existing task. This method accepts the task ID as a route parameter and a TaskCreationRequest object in the request body containing the updated task details and a list of mentee codes for assignment. The method first retrieves the existing task from the database using the provided ID. If the task is found, it updates the task's properties with the new values from the request. It then manages task assignments by comparing the new list of mentee codes with the current assignments in the database, removing any assignments that are no longer valid and adding new assignments for any new mentee codes. Finally, it saves the changes to the database and returns the updated task in the response. This endpoint allows users to efficiently update tasks and manage their assignments to mentees within the application.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="updatedRequest">The request containing updated task details and mentee codes.</param>
        /// <returns>The updated task.</returns>
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

        /// <summary>
        /// Handles DELETE requests to remove a task by its ID. This method first retrieves the task from the database using the provided ID. If the task is found, it checks for any related assignments in the SemesterPlanTasks table. If there are related assignments, it further checks if any of those assignments have associated feedback in the Feedback table. If feedback exists for any of the assignments, the method returns a BadRequest response indicating that the task cannot be deleted because it has been completed by at least one mentee. If there are no feedback entries, it proceeds to remove all related assignments and then deletes the task itself from the database. Finally, it saves the changes and returns an Ok response indicating successful deletion. This endpoint ensures that tasks with completed assignments cannot be deleted, preserving important data integrity within the application.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>The result of the delete operation.</returns>
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

        /// <summary>
        /// Handles POST requests to assign a task to multiple mentees. This method accepts an AssignTaskRequest object in the request body, which includes the task ID and a list of mentee codes to whom the task should be assigned. The method first filters out any null or empty mentee codes from the provided list. If there are no valid mentee codes, it removes all existing assignments for the task and marks the task as unassigned. If there are valid mentee codes, it retrieves the current assignments for the task and compares them with the new list of mentee codes. It removes any assignments that are no longer valid and adds new assignments for any new mentee codes. Finally, it marks the task as assigned and saves the changes to the database before returning a success response. This endpoint allows for efficient management of task assignments to mentees within the application.
        /// </summary>
        /// <param name="request">The request object containing the task ID and list of mentee codes.</param>
        /// <returns>The result of the assignment operation.</returns>
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

        /// <summary>
        /// Handles GET requests to retrieve a list of mentee codes that are assigned to a specific task. This method queries the SemesterPlanTasks table joined with the SemesterPlans table to find all assignments for the given task ID and returns a list of mentee codes associated with those assignments. This endpoint allows users to see which mentees are currently assigned to a particular task, providing insight into task distribution and enabling better management of task assignments within the application.
        /// </summary>
        /// <param name="taskId">The ID of the task for which to retrieve assignments.</param>
        /// <returns>A list of mentee codes assigned to the specified task.</returns>
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

        /// <summary>
        /// Handles GET requests to retrieve a list of tasks assigned to a specific mentee. This method queries the SemesterPlanTasks table joined with the SemesterPlans and Tasks tables to find all tasks assigned to the mentee identified by the provided mentee code. It also performs a left join with the Feedback table to include any feedback associated with each task assignment. The resulting list includes task details, assignment information, and feedback (if available) for each task assigned to the mentee. This endpoint allows mentees to view their assigned tasks along with any feedback they may have received, enabling them to manage their tasks effectively within the application.
        /// </summary>
        /// <param name="menteeCode">The code of the mentee for whom to retrieve assigned tasks.</param>
        /// <returns>A list of tasks assigned to the specified mentee.</returns>
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

        /// <summary>
        /// Handles POST requests to mark a task as completed for a specific mentee. This method accepts a CompleteTaskRequest object in the request body, which includes the task ID, mentee code, and an optional completion file link. The method first retrieves the semester plan associated with the provided mentee code, then finds the corresponding task assignment for that semester plan and task ID. If a completion file link is provided, it updates the CompletionFile property of the assignment with that link; otherwise, it sets it to a default value indicating completion. Finally, it saves the changes to the database and returns the updated assignment information in the response. This endpoint allows mentees to mark their tasks as completed and optionally provide evidence of completion through a file link, facilitating better tracking of task progress within the application.
        /// </summary>
        /// <param name="request">The request object containing the task ID, mentee code, and completion file link.</param>
        /// <returns>The updated assignment information.</returns>
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

        /// <summary>
        /// Handles GET requests to retrieve a list of all available task types. This method queries the TaskTypes table in the database and returns the list of task types in the response. This endpoint allows users to view the different categories or types of tasks that can be created and assigned within the application, providing them with options for categorizing their tasks effectively.
        /// </summary>
        /// <returns>A list of task types.</returns>
        [HttpGet("taskTypes")]
        public async Task<IActionResult> GetTaskTypes()
        {
            var taskTypes = await _context.TaskTypes.ToListAsync();
            return Ok(taskTypes);
        }

        /// <summary>
        /// Defines the request model for marking a task as completed. This class includes properties for the task ID, mentee code, and an optional completion file link. The TaskId property identifies the specific task being marked as completed, while the MenteeCode property identifies the mentee who is completing the task. The CompletionFile property allows for an optional link to a file that serves as evidence of task completion. This request model is used in the MarkTaskCompleted endpoint to process requests for marking tasks as completed by mentees within the application.
        /// </summary>
        public class CompleteTaskRequest
        {
            /// <summary>
            /// The ID of the task being marked as completed. This property is used to identify the specific task that the mentee is completing. It is a nullable integer, allowing for the possibility that the task ID may not be provided in the request. However, in typical usage, this property should contain a valid task ID to ensure that the correct task assignment is updated in the database when marking it as completed.
            /// </summary>
            public int? TaskId { get; set; }

            /// <summary>
            /// The code of the mentee who is marking the task as completed. This property is used to identify the specific mentee associated with the task completion request. It is a nullable string, allowing for the possibility that the mentee code may not be provided in the request. However, in typical usage, this property should contain a valid mentee code to ensure that the correct semester plan and task assignment are updated in the database when marking the task as completed.
            /// </summary>
            public string? MenteeCode { get; set; }

            /// <summary>
            /// An optional link to a file that serves as evidence of task completion. This property allows mentees to provide additional information or proof when marking a task as completed. It is a nullable string, meaning that it can be left empty if the mentee does not wish to provide a completion file. If no link is provided, the system will use a default value (e.g., "completed") to indicate that the task has been marked as completed without specific evidence.
            /// </summary>
            public string? CompletionFile { get; set; }
        }

        /// <summary>
        /// Defines the request model for assigning a task to multiple mentees. This class includes properties for the task ID, mentor code, and a list of mentee codes. The TaskId property identifies the specific task being assigned, while the MentorCode property identifies the mentor responsible for the assignment. The MenteeCodes property is a list of strings that contains the codes of the mentees to whom the task should be assigned. This request model is used in the AssignTask endpoint to process requests for assigning tasks to multiple mentees within the application, allowing for efficient management of task assignments.
        /// </summary>
        public class AssignTaskRequest
        {
            /// <summary>
            /// The ID of the task being assigned. This property is used to identify the specific task that is being assigned to mentees. It is a nullable integer, allowing for the possibility that the task ID may not be provided in the request. However, in typical usage, this property should contain a valid task ID to ensure that the correct task is updated in the database when processing the assignment request.
            /// </summary>
            public int? TaskId { get; set; }

            /// <summary>
            /// The code of the mentor responsible for the assignment. This property is used to identify the mentor who is assigning the task to mentees. It is a nullable string, allowing for the possibility that the mentor code may not be provided in the request. However, in typical usage, this property should contain a valid mentor code to ensure that the assignment is properly attributed to the correct mentor within the application.
            /// </summary>
            public string? MentorCode { get; set; }

            /// <summary>
            /// A list of strings that contains the codes of the mentees to whom the task should be assigned. This property allows for multiple mentee codes to be included in the assignment request, enabling the assignment of a single task to multiple mentees in one operation. It is a nullable list, meaning that it can be left empty if no mentee codes are provided. However, in typical usage, this property should contain at least one valid mentee code to ensure that the task is assigned to the intended recipients within the application.
            /// </summary>
            public List<string>? MenteeCodes { get; set; }
        }

        /// <summary>
        /// Defines the request model for creating a new task and assigning it to multiple mentees. This class includes properties for the task name, description, material link, deadline, creator's code, assignment status, task type ID, and a list of mentee codes. The Name property specifies the name of the task, while the Description provides additional details about the task. The MaterialLink property can contain a URL to relevant materials for the task. The Deadline property indicates when the task is due. The CreatedBy property identifies the user who created the task. The IsAssigned property indicates whether the task is currently assigned to any mentees. The TaskTypeId property categorizes the task based on predefined types. Finally, the MenteeCodes property is a list of strings that contains the codes of the mentees to whom the task should be assigned upon creation. This request model is used in the CreateTaskForMentees endpoint to process requests for creating new tasks and assigning them to multiple mentees within the application.
        /// </summary>
        public class TaskCreationRequest
        {
            /// <summary>
            /// The name of the task being created. This property is used to specify the title or name of the task that is being created. It is a nullable string, allowing for the possibility that the task name may not be provided in the request. However, in typical usage, this property should contain a valid task name to ensure that the task is properly identified and displayed within the application.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// A description of the task being created. This property provides additional details or information about the task, allowing users to understand the requirements or objectives of the task. It is a nullable string, meaning that it can be left empty if no description is provided. However, in typical usage, this property should contain relevant information to help users understand the context and expectations for the task within the application.
            /// </summary>
            public string? Description { get; set; }

            /// <summary>
            /// A URL link to relevant materials for the task being created. This property allows users to provide a link to resources, documents, or other materials that are associated with the task. It is a nullable string, meaning that it can be left empty if no material link is provided. However, in typical usage, this property should contain a valid URL to ensure that users can access the necessary materials related to the task within the application.
            /// </summary>
            public string? MaterialLink { get; set; }

            /// <summary>
            /// The deadline for the task being created. This property indicates when the task is due and helps users manage their time effectively. It is a nullable DateTime, allowing for the possibility that a deadline may not be provided in the request. However, in typical usage, this property should contain a valid date and time to ensure that users are aware of the task's due date and can plan accordingly within the application.
            /// </summary>
            public DateTime? Deadline { get; set; }

            /// <summary>
            /// The code of the user who created the task. This property is used to identify the creator of the task, allowing for proper attribution and tracking within the application. It is a nullable string, meaning that it can be left empty if the creator's code is not provided in the request. However, in typical usage, this property should contain a valid user code to ensure that the task is properly associated with its creator within the application.
            /// </summary>
            public string? CreatedBy { get; set; }

            /// <summary>
            /// Indicates whether the task is currently assigned to any mentees. This property is used to track the assignment status of the task, allowing users to quickly see if a task has been assigned or not. It is a nullable boolean, meaning that it can be left empty if the assignment status is not provided in the request. However, in typical usage, this property should contain a value (true or false) to ensure that the task's assignment status is accurately reflected within the application.
            /// </summary>
            public bool? IsAssigned { get; set; }

            /// <summary>
            /// The ID of the task type, categorizing the task based on predefined types. This property allows users to classify tasks according to their nature or purpose, making it easier to organize and filter tasks within the application. It is a nullable integer, allowing for the possibility that a task type may not be provided in the request. However, in typical usage, this property should contain a valid task type ID to ensure that the task is properly categorized and can be easily identified based on its type within the application.
            /// </summary>
            public int? TaskTypeId { get; set; }

            /// <summary>
            /// A list of strings that contains the codes of the mentees to whom the task should be assigned upon creation. This property allows for multiple mentee codes to be included in the task creation request, enabling the assignment of a single task to multiple mentees in one operation. It is a nullable list, meaning that it can be left empty if no mentee codes are provided. However, in typical usage, this property should contain at least one valid mentee code to ensure that the task is assigned to the intended recipients within the application upon creation.
            /// </summary>
            public List<string>? MenteeCodes { get; set; }  // Updated to support multiple mentee codes
        }
    }
}