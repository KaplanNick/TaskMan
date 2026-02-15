using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public static class Seed
{
    public static async System.Threading.Tasks.Task SeedData(AppDbContext context)
    {
        // Check if database has any data
        if (await context.Users.AnyAsync() || await context.Tags.AnyAsync())
        {
            Console.WriteLine("Database already contains data. Skipping seed.");
            return;
        }

        // Read seed file
        var seedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedFile.json");
        
        if (!File.Exists(seedFilePath))
        {
            Console.WriteLine($"Seed file not found at: {seedFilePath}");
            return;
        }

        var jsonData = await File.ReadAllTextAsync(seedFilePath);
        var seedData = JsonSerializer.Deserialize<SeedData>(jsonData, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (seedData == null)
        {
            Console.WriteLine("Failed to deserialize seed data.");
            return;
        }

        // Seed Users
        var users = new List<User>();
        if (seedData.Users != null && seedData.Users.Count > 0)
        {
            users = seedData.Users.Select(u => new User
            {
                FullName = u.FullName,
                Email = u.Email.ToLowerInvariant(),
                Telephone = u.Telephone
            }).ToList();

            await context.Users.AddRangeAsync(users);
            Console.WriteLine($"Seeding {users.Count} users...");
        }

        // Seed Tags
        var tags = new List<Tag>();
        if (seedData.Tags != null && seedData.Tags.Count > 0)
        {
            tags = seedData.Tags.Select(t => new Tag
            {
                Name = t.Name
            }).ToList();

            await context.Tags.AddRangeAsync(tags);
            Console.WriteLine($"Seeding {tags.Count} tags...");
        }

        await context.SaveChangesAsync();

        // Seed Tasks (after users and tags are saved so we have IDs)
        if (seedData.Tasks != null && seedData.Tasks.Count > 0)
        {
            var tasks = new List<API.Entities.Task>();
            
            foreach (var taskData in seedData.Tasks)
            {
                // Calculate due date based on offset from today
                var dueDate = DateTime.UtcNow.AddDays(taskData.DueDateDaysOffset);
                
                var task = new API.Entities.Task
                {
                    Title = taskData.Title,
                    Description = taskData.Description,
                    DueDate = dueDate,
                    Priority = (TaskPriority)taskData.Priority,
                    UserId = taskData.UserId
                };

                // Add tags
                if (taskData.TagIds != null)
                {
                    foreach (var tagId in taskData.TagIds)
                    {
                        task.TaskTags.Add(new TaskTag { TagId = tagId });
                    }
                }

                tasks.Add(task);
            }

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeding {tasks.Count} tasks...");
        }

        Console.WriteLine("✓ Seed data successfully added to database.");
    }
}

// DTOs for deserializing seed data
public class SeedData
{
    public List<SeedUser> Users { get; set; } = new();
    public List<SeedTag> Tags { get; set; } = new();
    public List<SeedTask> Tasks { get; set; } = new();
}

public class SeedUser
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telephone { get; set; } = null!;
}

public class SeedTag
{
    public string Name { get; set; } = null!;
}

public class SeedTask
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int DueDateDaysOffset { get; set; }
    public int Priority { get; set; }
    public int UserId { get; set; }
    public List<int> TagIds { get; set; } = new();
}