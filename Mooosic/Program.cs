using Config.Net;
using DiscordBotUtils;
using Serilog;
using Serilog.Events;
using Mooosic;
using static Mooosic.Constants;



Console.WriteLine($"{APP_NAME} is starting up!");
MultiInstanceHandler.EnsureSingleInstance(APP_NAME);

// Build config
var settings = new ConfigurationBuilder<ISettings>()
    .UseJsonFile(CONFIG_FILE)
    .UseEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(LogEventLevel.Verbose)
    .WriteTo.Console(
        outputTemplate: "[{SourceContext}({Timestamp:t})] {Level:u4}: {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.Async(a =>
    {
        a.File(restrictedToMinimumLevel: 
            LogEventLevel.Information,
            outputTemplate: "[{SourceContext}({Timestamp:t})] {Level:u4}: {Message:lj}{NewLine}{Exception}",
            path: settings.LogFile,
            rollingInterval: RollingInterval.Day);
    })
    .Enrich.FromLogContext()
    .CreateLogger();


Log.Information("Application Logging Ready!");

Bot? bot = null;


Console.CancelKeyPress += (sender, eventArgs) => Cleanup(botInstance: bot);


try
{
    bot = new Bot(settings);
    bot.StartAsync().Wait();
    Task.Delay(-1).Wait();
}
catch (Exception e)
{
    Log.Fatal(exception: e, messageTemplate: "Unknown Error Occured!");
}
finally
{
    // Cleanup at the end
    Cleanup(botInstance: bot);
}



void Cleanup(int exitCode = 0, Bot? botInstance = null)
{
    Console.WriteLine("Please wait! Cleaning up...");
    botInstance?.TryStop();
    
    Console.WriteLine("Flushing Logs...");
    Log.CloseAndFlush();

    Console.WriteLine("Quitting...");
    Environment.Exit(exitCode);
}