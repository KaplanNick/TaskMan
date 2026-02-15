using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace TaskReminderService.Services;

public interface ITaskRemainderService
{
    Task StartAsync();
    Task StopAsync();
}

public class TaskRemainderService : ITaskRemainderService, IAsyncDisposable
{
    private IConnection? _connection;
    private IModel? _publishChannel;
    private IModel? _consumeChannel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TaskRemainderService> _logger;
    private readonly HttpClient _httpClient;
    private const string QUEUE_NAME = "Remainder";
    private CancellationTokenSource _cancellationTokenSource = new();
    private const int MAX_RETRIES = 5;
    private const int RETRY_DELAY_MS = 2000;
    private bool _disposed = false;
    private readonly ConcurrentDictionary<int, DateTime> _reminded = new();
    private readonly TimeSpan _dedupeWindow = TimeSpan.FromHours(12);

    public TaskRemainderService(
        IConfiguration configuration,
        ILogger<TaskRemainderService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    private async Task ConnectToRabbitMqAsync()
    {
        int retryCount = 0;

        while (retryCount < MAX_RETRIES)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMq:HostName"] ?? "localhost",
                    UserName = _configuration["RabbitMq:UserName"] ?? "guest",
                    Password = _configuration["RabbitMq:Password"] ?? "guest",
                    VirtualHost = _configuration["RabbitMq:VirtualHost"] ?? "/",
                    Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _publishChannel = _connection.CreateModel();
                _consumeChannel = _connection.CreateModel();
                _logger.LogInformation("RabbitMQ connection established successfully");
                return; // Success
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= MAX_RETRIES)
                {
                    _logger.LogError(ex, "Failed to connect to RabbitMQ after {retries} attempts", MAX_RETRIES);
                    throw;
                }

                _logger.LogWarning(
                    "RabbitMQ connection failed (attempt {attempt}/{max}), retrying in {delay}ms: {message}",
                    retryCount, MAX_RETRIES, RETRY_DELAY_MS, ex.Message);

                await Task.Delay(RETRY_DELAY_MS);
            }
        }
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting background tasks");
        
        // Fire off background operations without waiting
        #pragma warning disable CS4014 // Because this call is not awaited, execution continues before the call is completed
        InitializeRabbitMqAsync();
        PollOverdueTasksAsync(_cancellationTokenSource.Token);
        #pragma warning restore CS4014
        
        // Return immediately - tasks continue running in background
        await Task.CompletedTask;
    }

    private async Task InitializeRabbitMqAsync()
    {
        try
        {
            _logger.LogInformation("Initializing RabbitMQ connection...");
            await ConnectToRabbitMqAsync();
            if (GetEnableConsumer())
            {
                SubscribeToQueue();
                _logger.LogInformation("✓ RabbitMQ connected and subscribed to 'Remainder' queue");
            }
            else
            {
                _logger.LogInformation("RabbitMQ connected; consumer disabled by config");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ");
        }
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        _publishChannel?.Close();
        _publishChannel?.Dispose();
        _consumeChannel?.Close();
        _consumeChannel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("Task Reminder Service stopped");
        await Task.CompletedTask;
    }

    private string GetApiBaseUrl() => _configuration["TaskReminder:ApiBaseUrl"] ?? "http://localhost:5000";

    private bool GetEnableConsumer()
    {
        var value = _configuration["TaskReminder:EnableConsumer"];
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (bool.TryParse(value, out var enabled))
        {
            return enabled;
        }

        return true;
    }

    private async Task PollOverdueTasksAsync(CancellationToken cancellationToken)
    {
        var pollingIntervalSeconds = int.Parse(_configuration["TaskReminder:PollingIntervalSeconds"] ?? "300");
        var apiBaseUrl = GetApiBaseUrl();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking for overdue tasks...");

                // Fetch all tasks from API
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/tasks", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch tasks: {response.StatusCode}");
                    await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var tasks = JsonSerializer.Deserialize<List<TaskDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tasks != null && tasks.Any())
                {
                    var todayUtc = DateTime.UtcNow.Date;
                    var overdueTasks = tasks.Where(t => t.DueDate.Date <= todayUtc).ToList();

                    foreach (var task in overdueTasks)
                    {
                        var now = DateTime.UtcNow;

                        // Check deduplication - skip if reminded within window
                        if (_reminded.TryGetValue(task.Id, out var last) && (now - last) < _dedupeWindow)
                        {
                            _logger.LogInformation("Skipping duplicate reminder for TaskId {TaskId} (last reminded {elapsed} ago)", 
                                task.Id, now - last);
                            continue;
                        }

                        // Update reminder timestamp
                        _reminded[task.Id] = now;
                        await PublishTaskRemainderAsync(task, cancellationToken);
                    }

                    if (overdueTasks.Any())
                    {
                        _logger.LogInformation($"Found {overdueTasks.Count} overdue tasks");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when service stops
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling overdue tasks");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task PublishTaskRemainderAsync(TaskDto task, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if channel is available, attempt reconnect if needed
                if (_publishChannel == null || _connection == null || !_connection.IsOpen)
                {
                    _logger.LogWarning("RabbitMQ not available; attempting reconnect...");
                    await ConnectToRabbitMqAsync();
                    if (_publishChannel == null) 
                    {
                        _logger.LogError("Failed to reconnect to RabbitMQ, cannot publish reminder");
                        return;
                    }
                }

                // Fetch user details to get full name
                string userFullName = "Unknown";
                try
                {
                    var apiBaseUrl = GetApiBaseUrl();
                    var userResponse = await _httpClient.GetAsync($"{apiBaseUrl}/api/users/{task.UserId}", cancellationToken);
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                        var userDto = JsonSerializer.Deserialize<UserDto>(userContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (userDto != null)
                        {
                            userFullName = userDto.FullName ?? "Unknown";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to fetch user {task.UserId} details");
                }

                var message = JsonSerializer.Serialize(new
                {
                    TaskId = task.Id,
                    Title = task.Title,
                    DueDate = task.DueDate,
                    UserId = task.UserId,
                    UserFullName = userFullName,
                    Timestamp = DateTime.UtcNow
                });

            // Declare queue
            _publishChannel.QueueDeclare(
                queue: QUEUE_NAME,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Publish message
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _publishChannel.CreateBasicProperties();
            properties.Persistent = true; // Persist message to disk

            _publishChannel.BasicPublish(
                exchange: "",
                routingKey: QUEUE_NAME,
                basicProperties: properties,
                body: body
            );

                _logger.LogInformation($"Task remainder published: Task {task.Id} - {task.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish task remainder for task {task.Id}");
            }
        }

    private void SubscribeToQueue()
    {
        try
        {
            // Check if channel is available
            if (_consumeChannel == null)
            {
                _logger.LogWarning("RabbitMQ channel not available, cannot subscribe to queue");
                return;
            }

            // Declare queue
            _consumeChannel.QueueDeclare(
                queue: QUEUE_NAME,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Set prefetch to 1 for fair dispatch
            _consumeChannel.BasicQos(0, 1, false);

            // Set up consumer
            var consumer = new AsyncEventingBasicConsumer(_consumeChannel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var taskData = JsonSerializer.Deserialize<TaskRemainder>(message,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Log the remainder - THIS IS THE KEY LINE
                    _logger.LogWarning($"✓ Hi {taskData?.UserFullName} your Task is due {taskData?.Title} (Task ID: {taskData?.TaskId})");

                    // Acknowledge message after processing
                    _consumeChannel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue");
                    // Nack and requeue on error
                    _consumeChannel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            // Start consuming (with manual acknowledgment)
            _consumeChannel.BasicConsume(
                queue: QUEUE_NAME,
                autoAck: false,
                consumerTag: "TaskRemainderConsumer",
                consumer: consumer
            );

            _logger.LogInformation($"✓ Subscribed to queue '{QUEUE_NAME}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to queue");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _publishChannel?.Close();
            _publishChannel?.Dispose();
            _consumeChannel?.Close();
            _consumeChannel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("TaskRemainderService resources disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during resource disposal");
        }

        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }

    private class TaskRemainder
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int Priority { get; set; }
        public int UserId { get; set; }
        public List<object> Tags { get; set; } = new();
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
    }
}
