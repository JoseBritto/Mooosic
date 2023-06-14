using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Mooosic.Commands;

public class JoinLeave : InteractionModuleBase
{
    private readonly VoiceControls _controls;
    private readonly DiscordSocketClient _client;

    public JoinLeave(VoiceControls controls, DiscordSocketClient client)
    {
        _controls = controls;
        _client = client;
    }

    [RequireContext(ContextType.Guild)]
    [SlashCommand("join", "Joins the given vc or the vc you are currently in if none is given")]
    public async Task JoinAsync(IVoiceChannel? vc = null)
    {
        if (vc is null)
        {
            var voiceState = Context.User as IVoiceState;
            
            if (voiceState?.VoiceChannel == null)
            {
                await RespondAsync("You are not connected to a voice channel! Please provide a channel to join");
                return;
            }

            vc = voiceState.VoiceChannel;
        }

        await DeferAsync();
        var result = await _controls.JoinAsync(vc, (Context.User as IGuildUser)!);

        await FollowupAsync(result.Response, ephemeral: result.WasSuccess == false);
    }
    
    
    [RequireContext(ContextType.Guild)]
    [SlashCommand("leave", "Leaves the voice channel the bot is currently in")]
    public async Task LeaveAsync()
    {
        await DeferAsync();
        var result = await _controls.LeaveAsync((Context.User as IGuildUser)!);

        await FollowupAsync(result.Response ?? $"Sorry an error occured: {result.Exception?.Message}", ephemeral: result.WasSuccess == false);
    }
}