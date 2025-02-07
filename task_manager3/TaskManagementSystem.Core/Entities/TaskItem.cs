using System;
using System.Collections.Generic;
using TaskManagementSystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TaskManagementSystem.Core.Entities
{
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        
        [StringLength(1000)]
        public string ? Description { get; set; }
         [Required]
        public DateTime DueDate { get; set; }
        
        // This property will be computed based on Category and DueDate, for example.
        [Required]
        [Range(1, 5)]
        public int Priority { get; set; }
        
        // Foreign key for Category
        [Required]
        public int CategoryId { get; set; }
         [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public TaskState Status { get; set; } = TaskState.Incomplete;
        
        // Navigation property for subscriptions (users who are following this task)
        //tofix adding published tasks
        public ICollection<TaskSubscription> TaskSubscriptions { get; set; } = new List<TaskSubscription>();
    }
}
