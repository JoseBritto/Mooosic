using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Mooosic.Commands;

public class Join : InteractionModuleBase
{
    private readonly VoiceControls _controls;
    private readonly DiscordSocketClient _client;

    public Join(VoiceControls controls, DiscordSocketClient client)
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
}