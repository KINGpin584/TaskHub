using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Services;
using TaskManagementSystem.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace TaskManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagementContext _context;
        private readonly TaskPriorityService _priorityService;
        private readonly IHubContext<TaskHub> _hubContext;

       public TasksController(TaskManagementContext context, TaskPriorityService priorityService, IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _priorityService = priorityService;
            _hubContext = hubContext;
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult> CreateTask([FromBody] CreateTaskDTO taskDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate category exists
                var category = await _context.Categories.FindAsync(taskDto.CategoryId);
                if (category == null)
                    return BadRequest($"Category with ID {taskDto.CategoryId} does not exist");

                var user = await _context.Users.FindAsync(taskDto.UserId);
                if (user == null)
                    return BadRequest($"User with ID {taskDto.UserId} does not exist");

                // Calculate the task priority using category priority and user-assigned priority
                int calculatedPriority = _priorityService.CalculateTaskPriority(
                    (int)category.Priority ,   // Category priority (1-4)
                    taskDto.Priority,     // User-assigned priority (1-5) from DTO
                    taskDto.DueDate       // Due date of the task
                );

                var task = new TaskItem
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    DueDate = taskDto.DueDate,
                    CategoryId = taskDto.CategoryId,
                    Priority = calculatedPriority,
                    Status = TaskState.Incomplete,
                    CreatedAt = DateTime.UtcNow
                };

                 _context.TaskItems.Add(task);
                await _context.SaveChangesAsync();

                // Create task subscription for creator with required properties
                var subscription = new TaskSubscription
                {
                    TaskItemId = task.Id,
                    UserId = taskDto.UserId,
                    User = user,
                    TaskItem = task,
                    SubscribedOn = DateTime.UtcNow
                };

                _context.TaskSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                await _hubContext.Clients.All.SendAsync("TaskCreated", MapToTaskResponse(task));

                return CreatedAtAction(
                    nameof(GetTask),
                    new { id = task.Id },
                    MapToTaskResponse(task)
                );
            }
            catch (Exception)
            {
                transaction.Rollback();
                return StatusCode(500, "An error occurred while creating the task");
            }
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDTO>>>GetTasks(
            [FromQuery] TaskState? state = null,
            [FromQuery] int? priority = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool includeCompleted = true,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = _context.TaskItems
                    .Include(t => t.Category)
                    .Include(t => t.TaskSubscriptions)
                        .ThenInclude(ts => ts.User)
                    .AsQueryable();

                // Apply filters
                if (state.HasValue)
                    query = query.Where(t => t.Status == state.Value);
                
                if (priority.HasValue)
                    query = query.Where(t => t.Priority == priority.Value);
                
                if (categoryId.HasValue)
                    query = query.Where(t => t.CategoryId == categoryId.Value);
                
                if (!includeCompleted)
                    query = query.Where(t => t.Status != TaskState.Completed);
                
                if (fromDate.HasValue)
                    query = query.Where(t => t.DueDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    query = query.Where(t => t.DueDate <= toDate.Value);

                var tasks = await query.ToListAsync();
                var taskDTOs = tasks.Select(MapToTaskResponse);
                return Ok(taskDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }
        // GET: api/tasks/priority
[HttpGet("priority")]
public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> GetTasksByPriority()
{
    try
    {
        var tasks = await _context.TaskItems
            .Include(t => t.Category)
            .Include(t => t.TaskSubscriptions)
                .ThenInclude(ts => ts.User)
            .Where(t => t.Status != TaskState.Completed)  // Exclude completed tasks
            .OrderByDescending(t => t.Priority)           // Sort by priority descending
            .ToListAsync();

        return Ok(tasks.Select(MapToTaskResponse));
    }
    catch (Exception)
    {
        return StatusCode(500, "An error occurred while retrieving tasks by priority");
    }
}

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDTO>> GetTask(int id)
        {
            try
            {
                var task = await _context.TaskItems
                    .Include(t => t.Category)
                    .Include(t => t.TaskSubscriptions)
                    .ThenInclude(ts => ts.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                    return NotFound();

                 return Ok(MapToTaskResponse(task));
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the task");
            }
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDTO updateDto)
{
    try
    {
        // Retrieve the task from database
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound();

        // Update simple properties (e.g., Title, Description, DueDate)
        if (!string.IsNullOrEmpty(updateDto.Title))
            task.Title = updateDto.Title;
        if (!string.IsNullOrEmpty(updateDto.Description))
            task.Description = updateDto.Description;
        if (updateDto.DueDate.HasValue)
            task.DueDate = updateDto.DueDate.Value;

        // Flag to indicate whether we need to re-calc priority
        bool recalcPriority = false;
        int userAssignedPriority = 0;

        // If the user provided a new priority (on a 1–5 scale), use it
        if (updateDto.Priority.HasValue)
        {
            recalcPriority = true;
            userAssignedPriority = updateDto.Priority.Value;
        }
        // Otherwise, if no new user priority was provided, you might choose a default value.
        else
        {
            // Default user-assigned priority; adjust as appropriate.
            userAssignedPriority = 3;
        }

        // If the category is changed, update it and mark for recalculation.
        if (updateDto.CategoryId.HasValue)
        {
            var newCategory = await _context.Categories.FindAsync(updateDto.CategoryId.Value);
            if (newCategory == null)
                return BadRequest("Category not found");
            task.CategoryId = updateDto.CategoryId.Value;
            task.Category = newCategory;
            recalcPriority = true;
        }
        else if (task.Category == null)
        {
            // Ensure that the navigation property is loaded if not already 
            task.Category = await _context.Categories.FindAsync(task.CategoryId);
        }

        // If DueDate was updated, mark for recalculation, too.
        if (updateDto.DueDate.HasValue)
            recalcPriority = true;

        // If any priority-affecting field is updated, recalculate the priority.
        if (recalcPriority)
        {
            // Use the updated due date if provided; otherwise, use existing task due date.
            DateTime dueDateForCalc = updateDto.DueDate.HasValue ? updateDto.DueDate.Value : task.DueDate;
            
            // Ensure task.Category is not null.
            if (task.Category == null)
                return BadRequest("Unable to load category for recalculation.");

            // Cast the category's Priority enum to int.
            int categoryPriority = (int)task.Category.Priority; // (1-4 scale)

            // Call the priority service
            task.Priority = _priorityService.CalculateTaskPriority(categoryPriority, userAssignedPriority, dueDateForCalc);
        }

        // Update the Status if provided
        if (updateDto.Status.HasValue)
        {
            task.Status = updateDto.Status.Value;
            if (task.Status == TaskState.Completed)
                task.CompletedAt = DateTime.UtcNow;
            else
                task.CompletedAt = null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _hubContext.Clients.Group($"Task_{task.Id}")
             .SendAsync("TaskUpdated", MapToTaskResponse(task));


        return NoContent();
    }
    catch (Exception ex)
    {
        // Log the exception as needed
        return StatusCode(500, "An error occurred while updating the task");
    }
}

        // PATCH: api/tasks/{id}/state
       [HttpPatch("{id}/state")]
public async Task<IActionResult> UpdateTaskState(int id, [FromBody] UpdateTaskStateDTO dto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound();

        if (!Enum.IsDefined(typeof(TaskState), dto.State))
            return BadRequest("Invalid task state value");

        task.Status = dto.State;
        task.UpdatedAt = DateTime.UtcNow;
        
        if (dto.State == TaskState.Completed)
            task.CompletedAt = DateTime.UtcNow;
        else
            task.CompletedAt = null;

        await _context.SaveChangesAsync();

        // Notify subscribers through SignalR
        await _hubContext.Clients.Group($"Task_{task.Id}")
            .SendAsync("ReceiveTaskStateUpdate", task.Id, task.Status.ToString());
        Console.WriteLine(task.Status.ToString());
        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred while updating the task state", error = ex.Message });
    }



        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.TaskItems.FindAsync(id);
                if (task == null)
                    return NotFound();

                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group($"Task_{task.Id}").SendAsync("TaskDeleted", task.Id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }

        // Subscribe to task
        [HttpPost("{taskId}/subscribe/{userId}")]
        public async Task<ActionResult> SubscribeToTask(int taskId, int userId)
        {
            try
            {
                var task =await _context.TaskItems.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound($"Task with ID {taskId} does not exist");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} does not exist");
                }

                var existingSubscription = await _context.TaskSubscriptions
                    .AnyAsync(ts => ts.TaskItemId == taskId && ts.UserId == userId);

                if (existingSubscription)
                {
                    return BadRequest("User is already subscribed to this task");
                }

                var subscription = new TaskSubscription
                {
                    TaskItemId = taskId,
                    UserId = userId,
                    User = user,
                    TaskItem = task,
                    SubscribedOn = DateTime.UtcNow
                };
                Console.WriteLine("DEBUG",subscription.ToString());
                _context.TaskSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                return Ok("Successfully subscribed to task");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while subscribing to the task");
            }
        }

        // Unsubscribe from task
        [HttpDelete("{taskId}/unsubscribe/{userId}")]
        public async Task<ActionResult> UnsubscribeFromTask(int taskId, int userId)
        {
            try
            {
                var subscription = await _context.TaskSubscriptions
                    .FirstOrDefaultAsync(ts => ts.TaskItemId == taskId && ts.UserId == userId);

                if (subscription == null)
                {
                    return NotFound("Subscription not found");
                }

                _context.TaskSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();

                return Ok("Successfully unsubscribed from task");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while unsubscribing from the task");
            }
        }

        // Get all subscribers of a task
        [HttpGet("{taskId}/subscribers")]
        public async Task<ActionResult<IEnumerable<TaskSubscriberDTO>>> GetTaskSubscribers(int taskId)
        {
            try
            {
                var task = await _context.TaskItems
                    .Include(t => t.TaskSubscriptions)
                    .ThenInclude(ts => ts.User)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    return NotFound($"Task with ID {taskId} does not exist");
                }

                var subscribers = task.TaskSubscriptions
                    .Select(ts => new TaskSubscriberDTO
                    {
                        UserId = ts.UserId,
                        Username = ts.User.Username
                    })
                    .ToList();

                return Ok(subscribers);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving task subscribers");
            }
        }

        // Map to TaskResponseDTO
        private TaskResponseDTO MapToTaskResponse(TaskItem task)
        {
            return new TaskResponseDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                Subscribers = task.TaskSubscriptions
                    .Select(ts => new TaskSubscriberDTO
                    { 
                        UserId = ts.UserId,
                        Username = ts.User.Username 
                    })
                    .ToList()
            };
        }
    }
}
