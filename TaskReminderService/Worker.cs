using TaskReminderService.Services;

namespace TaskReminderService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITaskRemainderService _taskRemainderService;

    public Worker(ILogger<Worker> logger, ITaskRemainderService taskRemainderService)
    {
        _logger = logger;
        _taskRemainderService = taskRemainderService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Reminder Service starting at {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _taskRemainderService.StartAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _taskRemainderService.StopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task Reminder Service encountered an error");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Reminder Service stopped at {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}
