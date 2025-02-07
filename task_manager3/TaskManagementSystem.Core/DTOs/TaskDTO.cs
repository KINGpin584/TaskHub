// Core/DTOs/TaskDTOs.cs
using TaskManagementSystem.Core.Enums;
using System.Text.Json.Serialization;

namespace TaskManagementSystem.Core.DTOs{
    

public class CreateTaskDTO
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public int CategoryId { get; set; }
    public int Priority { get; set; }
    public int UserId { get; set; }
    public TaskState Status { get; set; } = TaskState.Incomplete;
}
public class UpdateTaskStateDTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskState State { get; set; }
}



public class UpdateTaskDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int? CategoryId { get; set; }
    public int? Priority { get; set; }
    
    public TaskState? Status { get; set; }
}

public class TaskResponseDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public TaskState Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<TaskSubscriberDTO> Subscribers { get; set; } = new List<TaskSubscriberDTO>();
}

public class TaskSubscriberDTO
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
}

public class TaskPriorityDTO
{
   
   
    public DateTime DueDate { get; set; }
    public int CategoryId { get; set; }
    public int Priority { get; set; }
 
    public TaskState Status { get; set; } = TaskState.Incomplete;
}

}