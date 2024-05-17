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
        builder.Services.AddSingleton<InfluxDbService>();
        builder.Services.AddSingleton<ModFileInfoScrapper>();
        builder.Services.AddSingleton<ModrinthApiUtil>();
        builder.Services.AddScheduler();
        builder.Services.AddTransient<WriteModrinthModFileInfo>();
        builder.Services.AddTransient<WriteCurseForgeModFileInfo>();

        var host = builder.Build();
        _logger = host.Services.GetRequiredService<ILogger<Program>>();
        var schedulerConfig = builder.Configuration.GetSection("Scheduler").Get<SchedulerConfig>();

        host.Services.UseScheduler(scheduler =>
            {
                try
                {
                    if (schedulerConfig == null) throw new ArgumentNullException(nameof(schedulerConfig));
                    scheduler.Schedule<WriteCurseForgeModFileInfo>()
                        .Cron(schedulerConfig.CronExpr)
                        .PreventOverlapping(nameof(WriteCurseForgeModFileInfo));

                    _logger.LogInformation("Scheduling {Invocable} with cron expression: '{CronExpr}'",
                        nameof(WriteCurseForgeModFileInfo), schedulerConfig.CronExpr);
                }
                catch (MalformedCronExpressionException mfcee)
                {
                    _logger.LogError("{Error}. Error scheduling {Invocable}. Malformed cron expression: '{CronExpr}'",
                        mfcee.Message, nameof(WriteCurseForgeModFileInfo),
                        schedulerConfig == null ? "null" : schedulerConfig.CronExpr);
                }
                catch (ArgumentNullException ane)
                {
                    _logger.LogError("Error scheduling {Invocable}: {Message}, no scheduler configuration found",
                        nameof(WriteCurseForgeModFileInfo), ane.Message);
                    _logger.LogInformation("Scheduling {Invocable} with default hourly schedule",
                        nameof(WriteCurseForgeModFileInfo));
                    scheduler.Schedule<WriteCurseForgeModFileInfo>().HourlyAt(0);
                }

                try
                {
                    if (schedulerConfig == null) throw new ArgumentNullException(nameof(schedulerConfig));
                    scheduler.Schedule<WriteModrinthModFileInfo>()
                        .Cron(schedulerConfig.CronExpr)
                        .PreventOverlapping(nameof(WriteModrinthModFileInfo));

                    _logger.LogInformation("Scheduling {Invocable} with cron expression: '{CronExpr}'",
                        nameof(WriteModrinthModFileInfo), schedulerConfig.CronExpr);
                }
                catch (MalformedCronExpressionException mfcee)
                {
                    _logger.LogError("{Error}. Error scheduling {Invocable}. Malformed cron expression: '{CronExpr}'",
                        mfcee.Message, nameof(WriteModrinthModFileInfo),
                        schedulerConfig == null ? "null" : schedulerConfig.CronExpr);
                }
                catch (ArgumentNullException ane)
                {
                    _logger.LogError("Error scheduling {Invocable}: {Message}, no scheduler configuration found",
                        nameof(WriteModrinthModFileInfo), ane.Message);
                    _logger.LogInformation("Scheduling {Invocable} with default hourly schedule",
                        nameof(WriteModrinthModFileInfo));
                    scheduler.Schedule<WriteModrinthModFileInfo>().HourlyAt(0);
                }
            })
            .LogScheduledTaskProgress(host.Services.GetService<ILogger<IScheduler>>())
            .OnError(e => { _logger.LogError("Error occurred in scheduled task: {Message}", e.Message); });
        host.Run();
    }
}