using Coravel;
using Coravel.Scheduling.Schedule.Cron;
using Coravel.Scheduling.Schedule.Interfaces;
using ModMetaDataDownloader.Services;

namespace ModMetaDataDownloader;

public class Program
{
    private static ILogger<Program>? _logger;

    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        // builder.Services.AddHostedService<Worker>();
        builder.Services.AddSingleton<InfluxDbService>();
        builder.Services.AddSingleton<ModFileInfoScrapper>();
        builder.Services.AddScheduler();
        builder.Services.AddTransient<WriteModFileInfoInvocable>();

        var host = builder.Build();
        _logger = host.Services.GetRequiredService<ILogger<Program>>();
        var schedulerConfig = builder.Configuration.GetSection("Scheduler").Get<SchedulerConfig>();

        host.Services.UseScheduler(scheduler =>
        {
            try
            {
                if (schedulerConfig == null) 
                    throw new ArgumentNullException(nameof(schedulerConfig));
                
                scheduler.Schedule<WriteModFileInfoInvocable>()
                    .Cron(schedulerConfig.CronExpr)
                    .PreventOverlapping(nameof(WriteModFileInfoInvocable));
                
                _logger.LogInformation("Scheduling WriteModFileInfoInvocable with cron expression: '{cronExpr}'",
                    schedulerConfig.CronExpr);
            }
            catch (MalformedCronExpressionException mfcee)
            {
                _logger.LogError("Error scheduling WriteModFileInfoInvocable: {e.Message}, malformed cron expression: '{cronExpr}'",
                    mfcee, schedulerConfig.CronExpr);
            }
            catch (ArgumentNullException ane)
            {
                _logger.LogError("Error scheduling WriteModFileInfoInvocable: {e.Message}, no scheduler configuration found", ane);
                _logger.LogInformation("Scheduling WriteModFileInfoInvocable with default hourly schedule");
                scheduler
                    .Schedule<WriteModFileInfoInvocable>()
                    .HourlyAt(0);
            }
        })
        .LogScheduledTaskProgress(host.Services.GetService<ILogger<IScheduler>>())
        .OnError(e =>
        {
            _logger.LogError("Error occurred in scheduled task: {e.Message}", e);
        });
        host.Run();
    }
}