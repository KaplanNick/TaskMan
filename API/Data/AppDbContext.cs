using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<API.Entities.Task> Tasks => Set<API.Entities.Task>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<API.Entities.Task>(b =>
        {
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            b.Property(x => x.DueDate).IsRequired();
            b.Property(x => x.Priority).IsRequired();
            
            // Add index on UserId for better join performance
            b.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Telephone).HasMaxLength(50).IsRequired();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            
            // Add unique indexes for Email and Telephone
            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => x.Telephone).IsUnique();
        });

        modelBuilder.Entity<Tag>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<TaskTag>(b =>
        {
            b.HasKey(x => new { x.TaskId, x.TagId });

            b.HasOne(x => x.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(x => x.TaskId);

            b.HasOne(x => x.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(x => x.TagId);
        });
    }
}