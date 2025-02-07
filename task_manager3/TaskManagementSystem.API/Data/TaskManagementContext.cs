using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Core.Entities;

namespace TaskManagementSystem.API.Data
{
    public class TaskManagementContext : DbContext
    {
        public TaskManagementContext(DbContextOptions<TaskManagementContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskSubscription> TaskSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();
            
             modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<TaskItem>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

             modelBuilder.Entity<Category>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

            // Configure the composite key for TaskSubscription
            modelBuilder.Entity<TaskSubscription>()
                .HasKey(ts => new { ts.UserId, ts.TaskItemId });

            // Configure relationship between User and TaskSubscription
            modelBuilder.Entity<TaskSubscription>()
                .HasOne(ts => ts.User)
                .WithMany(u => u.TaskSubscriptions)
                .HasForeignKey(ts => ts.UserId);

            // Configure relationship between TaskItem and TaskSubscription
            modelBuilder.Entity<TaskSubscription>()
                .HasOne(ts => ts.TaskItem)
                .WithMany(t => t.TaskSubscriptions)
                .HasForeignKey(ts => ts.TaskItemId);

            // Optionally, you can configure additional details such as table names, indexes, etc.
        }
    }
}
