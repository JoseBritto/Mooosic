using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Mooosic;

public class Bot
{
    private readonly ISettings _settings;
    private readonly ILogger _log;
    private readonly ILogger _discordLog;
    
    private IServiceProvider _provider = null!;


    public Bot(ISettings settings)
    {
        _settings = settings;
        _log = Log.Logger.ForContext<Bot>();
        _discordLog = Log.Logger.ForContext("SourceContext", "Discord");
    }

    public async Task StartAsync()
    {
        _log.Information("Preparing services...");
        
        _provider = ConfigureServices(new ServiceCollection());

        await _provider.GetRequiredService<InteractionService>()
            .AddModulesAsync( Assembly.GetAssembly(typeof(Bot)), _provider);

        var client = _provider.GetRequiredService<DiscordSocketClient>();

        client.Log += DiscordLog;
        client.Ready += ClientReady;
        client.InteractionCreated += InteractionCreated;
        client.MessageReceived += ClientMessageReceived;

        _provider.GetRequiredService<InteractionService>().SlashCommandExecuted += PostCommandHandle;

        try
        {
            await client.LoginAsync(TokenType.Bot, _settings.DiscordToken);
        }
        catch (Exception e)
        {
            _log.Fatal(e,"Discord login failed!");
            Environment.Exit(101);
        }

        try
        {
            await client.StartAsync();
        }
        catch (Exception e)
        {
            _log.Fatal(e,"Failed to start discord bot!");
            Environment.Exit(102);
        }

        _log.Information("Bot is connecting...");

    }

    
    private Task ClientMessageReceived(SocketMessage message)
    {
        if(_settings.AdminIds.Contains(message.Author.Id.ToString()) == false) return Task.CompletedTask;
        
        var client = _provider.GetRequiredService<DiscordSocketClient>();
        
        if(message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id) == false) return Task.CompletedTask;
        
        if (message.Content.Contains("refresh guild"))
        {
            if (message.Channel is not SocketGuildChannel channel) return Task.CompletedTask;

            var id = channel.Guild.Id;
            Task.Run(() =>
            {
                _provider.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(id).Wait();
            });
            
            _log.Information("Refreshing commands for {Guild} on request by {User}", channel.Guild, message.Author);
        } 
        else if (message.Content.Contains("refresh global"))
        {
            _log.Information("Refreshing commands globally on request by {User}", message.Author);
            _provider.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();
        }
        else
        {
            return Task.CompletedTask;
        }

        try
        {
          message.AddReactionAsync(new Emoji("👍"));
        }
        catch (Exception e)
        {
            _log.Warning(e,"Failed to add reaction to admin message!");
        }
        
        return Task.CompletedTask;
    }
    
    private Task InteractionCreated(SocketInteraction interaction)
    {
        var client = _provider.GetRequiredService<DiscordSocketClient>();
        
        var context = new InteractionContext(client, interaction, interaction.Channel);

        _log.Verbose("Interaction invoked! Channel: #{Channel} User: {User}", 
            interaction.Channel.Name, interaction.User.Username);
        
        Task.Run(() =>
            _provider.GetRequiredService<InteractionService>().ExecuteCommandAsync(context, _provider)
                .Wait());

        return Task.CompletedTask;
    }
    
    private Task DiscordLog(LogMessage log)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        _discordLog.Write((LogEventLevel)(5 - (int)log.Severity), log.Exception, log.Message);
        
        return Task.CompletedTask;
    }
    
    
    private ServiceProvider ConfigureServices(ServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged
            }))
            .AddSingleton(new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async,
            })
            .AddSingleton<InteractionService>();

        return serviceCollection.BuildServiceProvider();
    }
    
    private Task ClientReady()
    {
        _log.Information("Bot is ready!");
        return Task.CompletedTask;
    }

    public void TryStop()
    {
        _log.Information("Logging out of discord...");
        try
        {
            var client = _provider.GetRequiredService<DiscordSocketClient>();
            client.LogoutAsync().Wait();
            client.StopAsync().Wait();
        }
        catch(Exception e)
        {
            _log.Information(e, "An Error occured while logging out. Giving up..");
        }
        
    }



    private async Task PostCommandHandle(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess) return;

        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand:
                _log.Information("Unknown command encountered: {Command}", info.Name ?? "[failed to get name]");
                break;
            
            case InteractionCommandError.ConvertFailed:
                _log.Warning("Convert failed for command {Command} in channel with id {Id} in guild/dm/context-command {guild}",
                    info.Name, context.Channel?.Id ?? 0, 
                    context.Guild.Name ?? $"{context.User.Username}#{context.User.DiscriminatorValue}");
                break;
            
            case InteractionCommandError.BadArgs:
                _log.Verbose(
                    "Bad args encountered for command {Command} in channel {Id} in guild/dm/context-command {Guild}",
                    info.Name, context.Channel?.Id ?? 0,
                    context.Guild.Name ?? $"{context.User.Username}#{context.User.DiscriminatorValue}");
                break;
            
            case InteractionCommandError.Exception:
                _log.Error("An exception occured in a command: {Error}", result.ErrorReason);
                try
                {
                    await context.Interaction.FollowupAsync($"🤕 An exception occured: {result.ErrorReason}");
                }
                catch
                {
                    // ignored
                }
                break;
            
            case InteractionCommandError.Unsuccessful:
                _log.Warning("Command {Name} failed. Channel id: {Id}", info.Name, context.Channel?.Id ?? 0);
                break;
            
            case InteractionCommandError.UnmetPrecondition:
                _log.Verbose("Command {Name} had an unmet precondition. Channel id: {Id}", 
                    info.Name, context.Channel?.Id ?? 0);
                break;
            
            case InteractionCommandError.ParseFailed:
                _log.Warning("Failed to parse command in channel id: {Id}", context.Channel?.Id ?? 0);
                break;
            
            case null:
            _log.Fatal("Encountered null in log switch case!! This should never have happened!");
                break;
            
            default:
                _log.Fatal("Got an out of range value in log switch case. This should never have happened!");
                throw new ArgumentOutOfRangeException();
        }
    }
}