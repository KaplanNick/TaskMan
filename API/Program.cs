using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TaskMan API",
        Version = "v1",
        Description = "Task Management Application API",
        Contact = new()
        {
            Name = "TaskMan Support",
            Url = new Uri("https://github.com/TaskMan")
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

app.UseHttpsRedirection();

// Enable Swagger/OpenAPI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskMan API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
