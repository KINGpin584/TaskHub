using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TaskManagementSystem.Core.Entities
{
    public class TaskSubscription
    {
        // Composite Key: UserId + TaskItemId can be configured in the DbContext.
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public required User User { get; set; }
        
        public int TaskItemId { get; set; }
        [ForeignKey("TaskItemId")]
        public required TaskItem TaskItem { get; set; }
        
        public DateTime SubscribedOn { get; set; } = DateTime.UtcNow;
    }
}
