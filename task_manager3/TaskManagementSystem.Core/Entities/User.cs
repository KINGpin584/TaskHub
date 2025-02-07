using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TaskManagementSystem.Core.Entities
{

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
       
        public required string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(50)]
        public required string Email { get; set; }
         [Required]
        [StringLength(100)]
        public required string Password {get;set;}
        
        // Navigation property for subscriptions (tasks the user is subscribed to)
        public ICollection<TaskSubscription> TaskSubscriptions { get; set; } = new List<TaskSubscription>();
    }
}
