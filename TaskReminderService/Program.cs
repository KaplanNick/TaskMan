using TaskReminderService;
using TaskReminderService.Services;
using Microsoft.Extensions.Logging;

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseWindowsService(options =>
        {
            options.ServiceName = "TaskManReminderService";
        })
        .ConfigureServices(services =>
        {
            services.AddHttpClient();
            services.AddSingleton<ITaskRemainderService, global::TaskReminderService.Services.TaskRemainderService>();
            services.AddHostedService<Worker>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex}");
    Environment.Exit(1);
}

