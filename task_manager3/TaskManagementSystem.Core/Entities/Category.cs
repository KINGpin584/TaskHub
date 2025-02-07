using System.Collections.Generic;
using TaskManagementSystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.Core.Entities
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
        [Required]
        [Range(1, 4)]
        public PriorityLevel Priority { get; set; }
        
        // Navigation property to tasks in this category
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
