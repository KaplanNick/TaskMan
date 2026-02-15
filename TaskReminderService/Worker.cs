using TaskReminderService.Services;

namespace TaskReminderService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITaskReminderService _taskReminderService;

    public Worker(ILogger<Worker> logger, ITaskReminderService taskReminderService)
    {
        _logger = logger;
        _taskReminderService = taskReminderService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Reminder Service starting");
        // Call the base method but don't await other initialization here
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Task Reminder Service initialized and running");
            
            // Initialize service - this is called in background service context
            // which is already running in a background thread
            await _taskReminderService.StartAsync();
            
            // Keep running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Task Reminder Service shutdown requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Task Reminder Service");
        }
        finally
        {
            try
            {
                await _taskReminderService.StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping service");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Reminder Service stopping");
        await base.StopAsync(cancellationToken);
    }
}

